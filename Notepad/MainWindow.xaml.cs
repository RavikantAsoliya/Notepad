using Microsoft.Win32;
using Notepad.Helper;
using Notepad.Properties;
using Notepad.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Notepad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Gets or sets the content data of the current document.
        /// </summary>
        public string FileData { get; set; } = "";

        /// <summary>
        /// Gets or sets the name of the current document file.
        /// </summary>
        public string FileName { get; set; } = "Untitled";

        /// <summary>
        /// Gets or sets the full path to the current document file.
        /// </summary>
        public string FilePath { get; set; } = "";

        /// <summary>
        ///  Gets or sets the last opened location when opening a folder.
        /// </summary>
        public string LastOpenLocation { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // Default save location is documents

        /// <summary>
        /// Gets or sets a flag indicating whether there are unsaved changes in the current document.
        /// </summary>
        public bool ShouldSave { get; set; } = false;

        /// <summary>
        /// Indicates whether WordWrap is checked in the menu.
        /// </summary>
        private bool IsWordWrapChecked { get; set; } = false;

        /// <summary>
        /// Declare a variable to store the current font size.
        /// </summary>
        private double CurrentFontSize { get; set; } = Settings.Default.FontSize;

        /// <summary>
        /// Gets or sets the FindDialog, used for finding text within the application.
        /// </summary>
        private FindDialog findDialog;

        /// <summary>
        /// Gets or sets the ReplaceDialog, used for replacing text within the application.
        /// </summary>
        private ReplaceDialog replaceDialog;

        /// <summary>
        /// Gets or sets the TextFinder, responsible for text searching and manipulation.
        /// </summary>
        private readonly TextFinder textFinder;

        private readonly DispatcherTimer autoSaveTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            // Initialize the main window components.
            InitializeComponent();

            // Ensure the required font is installed.
            FontHelper.InstallFontIfNotInstalled("Segoe Fluent Icons", "Notepad.Resources.Segoe Fluent Icons.ttf");

            // Initialize the current Windows theme.
            InitializeCurrentWindowsTheme();

            // Set focus to the text area.
            TextArea.Focus();

            // Set initial status for file path display.
            FilePathStatusBar.Content = "Untitled Document";

            // Initialize the text finder.
            textFinder = new TextFinder(ref TextArea);

            // Log window dimensions for debugging purposes.
            Debug.WriteLine($"Left:{Left}, Top: {Top}, Height: {Height}, Width: {Width}");

            // Initialize and configure the auto-save timer.
            autoSaveTimer = new DispatcherTimer();
            autoSaveTimer.Interval = TimeSpan.FromSeconds(1);
            autoSaveTimer.Tick += AutoSaveTimer_Tick;
        }

        #region File Menu Command's Code Implementation

        /// <summary>
        /// Handles the execution of the "New" command, which creates a new empty document.
        /// If there are unsaved changes in the current document, it prompts the user to save or discard them.
        /// </summary>
        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Check if there are unsaved changes in the current document.
            if (ShouldSave)
            {
                // Prompt the user to save changes and get their choice (true for save, false for discard, null for cancel).
                bool? result = AskToSaveFile();

                if (result == true && File.Exists(FilePath))
                {
                    // Save the current document before creating a new one.
                    SaveOldDocument();
                }
                else if (result == null || (result == true && SaveAsNewDocument() == false))
                {
                    // User canceled or encountered an error while saving, so do not create a new document.
                    return;
                }
            }

            // Create a new empty document.
            CreateNewDocument();

            // Update Status Bar
            FilePathStatusBar.Content = "Untitled Document";
        }

        /// <summary>
        /// Opens a custom dialog to ask the user if they want to save changes to the current document.
        /// </summary>
        /// <returns>
        ///   <para><c>true</c> if the user chooses to save changes,</para>
        ///   <para><c>false</c> if the user chooses to discard changes,</para>
        ///   <para><c>null</c> if the user cancels the operation.</para>
        /// </returns>
        private bool? AskToSaveFile()
        {
            // Create a new instance of the SaveMessageBox, a custom dialog for asking to save changes.
            SaveMessageBox askToSave = new SaveMessageBox
            {
                // Set the message text based on whether a file path (FilePath) exists or not.
                Message = File.Exists(FilePath)
                    ? "Do you want to save changes to " + FilePath + "?" // File path exists, use it in the message.
                    : "Do you want to save changes to " + FileName + "?", // File path doesn't exist, use the file name.
                Owner = this // Set the owner of the dialog to this current window.
            };

            // Show the dialog to the user and Return the result of the user's choice (true for save, false for discard, null for cancel).
            askToSave.ShowDialog();
            return askToSave.Result;
        }

        /// <summary>
        /// Creates a new, empty document by clearing the text area and resetting document properties.
        /// </summary>
        private void CreateNewDocument()
        {
            // Clear the text area, removing any existing content.
            TextArea.Clear();

            // Set the document properties for a new, untitled document.
            FileName = "Untitled";  // Set the default file name.
            FilePath = FileData = "";  // Clear file-related data as it's a new document.

            // Update the application title to reflect the new document state.
            ShouldSave = false;  // No unsaved changes in the new document.
            this.Title = FileName + " - Notepad";  // Update the window title.
        }

        /// <summary>
        /// Executes the command to open a new instance of the Notepad application window.
        /// </summary>
        private void NewWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // First Way:

            // Get the filename of the current running process (your application executable).
            //string currentProcessFileName = Process.GetCurrentProcess().MainModule.FileName;

            // Start a new instance of the same application, effectively opening a new window.
            //Process.Start(currentProcessFileName);

            // Second Way:

            // Create a new instance of the MainWindow class to open a new Notepad window
            MainWindow newWindow = new MainWindow();

            // Show the new window to the user
            newWindow.Show();
        }

        /// <summary>
        /// Executes the Open command, allowing the user to open an existing text document.
        /// </summary>
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Check if there are unsaved changes (ShouldSave) and if the user chooses to save them (AskToSaveFile() == true).
            if (ShouldSave && AskToSaveFile() == true)
            {
                // If the file already exists, save the changes to the current document.
                if (File.Exists(FilePath))
                    SaveOldDocument();
                // If the file does not exist, prompt the user to save changes as a new document.
                else
                    SaveAsNewDocument();
            }

            // Open an existing text document using the OpenFile method.
            OpenFile();

            // Update Status Bar
            FilePathStatusBar.Content = FilePath;
        }

        /// <summary>
        /// Opens a file dialog to select and load a text document into the Notepad application.
        /// </summary>
        private void OpenFile()
        {
            // Create a new OpenFileDialog instance for opening files.
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text Document (*.txt)|*.txt|All Files (*.*)|*.*", // Define the file type filters for the dialog.
                Title = "Open", // Set the dialog's title.
                Multiselect = false, // Allow only single file selection.
                InitialDirectory = LastOpenLocation, // Set the initial location to the location last opened from.
                RestoreDirectory = true // Restore the directory to its previously selected location
            };

            // Show the open file dialog and check if the user selected a file.
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName; // Get the full path of the selected file.
                FileName = System.IO.Path.GetFileNameWithoutExtension(FilePath); // Extract the file name without its extension.
                LastOpenLocation = System.IO.Path.GetDirectoryName(FilePath); // Save the opened directory to LastOpenLocation
                FileData = File.ReadAllText(FilePath); // Read the content of the selected file and store it in FileData.
                TextArea.Text = FileData; // Set the TextArea's text to the content of the selected file.
                this.Title = FileName + " - Notepad"; // Update the window title to reflect the file name.
                ShouldSave = false; // Mark that there are no unsaved changes in the current document.
            }
        }

        /// <summary>
        /// Executes the Save command, allowing the user to save the current document.
        /// </summary>
        /// <param name="sender">The sender of the command.</param>
        /// <param name="e">The event arguments.</param>
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Check if the current document already exists on the file system and save it.
            if (File.Exists(FilePath))
                SaveOldDocument();
            // If the document is new or has not been saved before, use SaveAsNewDocument to specify a location.
            else
                SaveAsNewDocument();
        }

        /// <summary>
        /// Saves the contents of the text area to the previously opened file.
        /// Updates the FileData, ShouldSave, and window title accordingly.
        /// </summary>
        private void SaveOldDocument()
        {
            // Write the text in the text area to the file at FilePath.
            File.WriteAllText(FilePath, TextArea.Text);

            // Update the FileData to match the saved content.
            FileData = TextArea.Text;

            // The document is now saved, so ShouldSave is set to false.
            ShouldSave = false;

            // Update the window title to reflect the saved state.
            this.Title = FileName + " - Notepad";
        }

        /// <summary>
        /// Executes the Save As command, allowing the user to specify a new location for the current document.
        /// </summary>
        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Check if the current document already exists on the file system.
            if (File.Exists(FilePath))
                SaveAsNewDocument(System.IO.Path.GetExtension(FilePath)); // If it exists, use the existing file's extension as the default when saving with a new name.
            else
                SaveAsNewDocument(); // If the document is new or has not been saved before, allow the user to specify a location.
        }

        /// <summary>
        /// Opens a Save File dialog for the user to save the current document as a new file.
        /// </summary>
        /// <param name="defaultExtension">The default file extension to use for the new file.</param>
        /// <returns>
        /// A <see cref="bool"/> representing the dialog result. 
        /// <see langword="true"/> if the document was saved successfully, 
        /// <see langword="false"/> if the user canceled the operation, 
        /// and <see langword="null"/> if an error occurred.
        /// </returns>
        private bool? SaveAsNewDocument(string defaultExtension = ".txt")
        {
            // Create a new SaveFileDialog instance for saving the document.
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Save As", // Set the dialog title.
                FileName = FileName + defaultExtension, // Set the default file name using the specified default extension.
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), // Set the initial directory to the user's documents folder.
                RestoreDirectory = true, // Allow the dialog to restore the last used directory.
                DefaultExt = defaultExtension, // Set the default file extension.
                Filter = "Text Document (*.txt)|*.txt|All Files (*.*)|*.*" // Define the filter for the file types in the dialog.
            };

            // Show the SaveFileDialog and store the result (true for success, false for failure, null for cancel).
            bool? result = saveFileDialog.ShowDialog();

            // Check if the user selected a file and clicked the "Save" button.
            if (result == true)
            {
                File.WriteAllText(saveFileDialog.FileName, TextArea.Text); // Write the content of the TextArea to the selected file.
                FilePath = saveFileDialog.FileName; // Update the FilePath to the selected file's path.
                FileName = System.IO.Path.GetFileNameWithoutExtension(FilePath); // Extract the file name without its extension.
                FileData = TextArea.Text; // Update the stored document data.
                this.Title = FileName + " - Notepad"; // Update the window title to reflect the new file name.
                ShouldSave = false; // Reset the "ShouldSave" flag as the document is now saved.
            }
            // Return the result (true for success, false for failure, null for cancel).
            return result;
        }

        /// <summary>
        /// Represents the PageSetupDialog instance used to configure page setup settings.
        /// </summary>
        private PageSetupDialog PageSetupDialog;

        /// <summary>
        /// Event handler for the Page Setup button click.
        /// Opens the PageSetupDialog window and captures page setup settings if the dialog is closed with a true result.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void PageSetup_Click(object sender, RoutedEventArgs e)
        {
            // Instantiate a new PageSetupDialog window
            PageSetupDialog = new PageSetupDialog();

            // Display the PageSetupDialog window and capture the result
            if (PageSetupDialog.ShowDialog() == true)
            {
                // Page setup settings captured, no further action needed here
                Console.WriteLine("Result is OK");
            }
            else
            {
                Console.WriteLine("Result is Cancel or Close");
            }
        }
        
        private void Print_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: Implement this Print Feature
        }

        /// <summary>
        /// Handles the click event of the Exit menu item or button to close the application.
        /// </summary>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // Close the application window.
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Check if the application should exit based on unsaved changes and user decisions.
            e.Cancel = !ShouldExitApplication();
            SaveState();
        }

        /// <summary>
        /// Checks whether the application should exit, considering whether to save changes.
        /// </summary>
        /// <returns>True if the application should exit, false otherwise.</returns>
        private bool ShouldExitApplication()
        {
            // Check if there are no unsaved changes or if changes were successfully saved.
            if (!ShouldSave || SaveChanges() == true)
                return true;

            // Return false if there are unsaved changes and the user chooses not to save.
            return false;
        }

        /// <summary>
        /// Prompts the user to save changes and handles the response accordingly.
        /// </summary>
        /// <returns>True if changes were successfully saved or no changes needed saving, false otherwise.</returns>
        private bool? SaveChanges()
        {
            // Ask the user whether to save changes.
            bool? result = AskToSaveFile();

            // Prompt the user to save changes and get their choice (true for save, false for discard, null for cancel).
            if (result == true && File.Exists(FilePath))
            {
                // User chose to save, and the file exists, so save the current document.
                SaveOldDocument();
            }
            else if (result == null || (result == true && SaveAsNewDocument() == false))
            {
                // User canceled or encountered an error while saving, or they choose not to save the current document.
                // In any of these cases, it's not safe to exit the application.
                return false;
            }
            // Return true if changes were successfully saved or no changes needed saving.
            return true;
        }

        #endregion

        #region Edit Menu Command's Code Implementation

        /// <summary>
        /// Determines whether the Paste command can be executed.
        /// </summary>
        /// <param name="sender">The command source.</param>
        /// <param name="e">The event data.</param>
        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Check if there is text data in the clipboard.
            if (System.Windows.Clipboard.ContainsText())
                e.CanExecute = true; // Allow execution if there is text in the clipboard.
            else
                e.CanExecute = false; // Disallow execution if the clipboard does not contain text.
        }

        /// <summary>
        /// Determines whether the Delete command can be executed.
        /// </summary>
        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Check if there is selected text in the TextArea.
            if (TextArea.SelectedText.Length > 0)
                e.CanExecute = true; // Allow execution if there is selected text.
            else
                e.CanExecute = false; // Disallow execution if no text is selected.
        }

        /// <summary>
        /// Executes the "Delete" command, which removes the selected text in the text area.
        /// </summary>
        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Delete the selected text by setting it to an empty string.
            TextArea.SelectedText = "";
        }

        /// <summary>
        /// Determines whether the Search with Bing command can be executed.
        /// </summary>
        private void SearchWithBing_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Check if there is selected text in the TextArea.
            if (TextArea.SelectedText.Length > 0)
                e.CanExecute = true; // Allow execution if there is selected text.
            else
                e.CanExecute = false; // Disallow execution if no text is selected.
        }

        /// <summary>
        /// Executes the "Search with Bing" command, which opens a web browser and performs a Bing search using the selected text as the query.
        /// </summary>
        private void SearchWithBing_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Construct the Bing search URL with the selected text as the query, and Open the default web browser to perform the Bing search.
            //Process.Start($"https://www.bing.com/search?q={Uri.EscapeDataString(TextArea.SelectedText)}");

            // Construct the Bing search URL with the selected text as the query, and Open the Microsoft Edge web browser to perform the Bing search.
            Process.Start($"microsoft-edge:https://www.bing.com/search?q={Uri.EscapeDataString(TextArea.SelectedText)}");
        }

        /// <summary>
        /// Determines whether the Find command can be executed.
        /// </summary>
        private void Find_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Check if there is any text in the TextArea.
            if (TextArea.Text.Length > 0)
                e.CanExecute = true; // Allow execution if there is text.
            else
                e.CanExecute = false; // Disallow execution if there is no text.
        }

        /// <summary>
        /// Executes the "Find" command, which opens the FindDialog to search for text in the TextArea.
        /// If the ReplaceDialog is open, it is closed to avoid having both dialogs open simultaneously.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The executed routed event arguments.</param>
        private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Close the replace dialog if it is open.
            if (replaceDialog != null)
                CloseReplaceDialog();

            // Close the find dialog if it is open.
            if (findDialog == null)
            {
                // Create a new instance of the FindDialog with appropriate settings.
                findDialog = new FindDialog(textFinder)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    // Initialize the TextToFind property based on selected text (if any).
                    TextToFind = TextArea.SelectedText.Length > 0 ? TextArea.SelectedText : Settings.Default.LastFindWord
                };

                // Dispose the dialog when it's closed.
                findDialog.Closed += (s, args) => findDialog = null;

                // Show the FindDialog.
                findDialog.Show();
            }
        }

        /// <summary>
        /// Closes the custom replace dialog if it is open and sets the dialog variable to null.
        /// </summary>
        private void CloseReplaceDialog()
        {
            // Check if the Replace dialog is open.
            if (replaceDialog != null)
            {
                replaceDialog.Close(); // Close the replace dialog.
                replaceDialog = null; // Set the dialog variable to null to indicate it's closed.
            }
        }

        /// <summary>
        /// Executes the "Find Next" command, which instructs the TextFinder to search for the next occurrence of text in the TextArea.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The executed routed event arguments.</param>
        private void FindNext_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Set the search direction to forward and call the FindNext method from the TextFinder.
            textFinder.Direction(findNext: true);
            textFinder.Find();
        }

        /// <summary>
        /// Executes the "Find Previous" command, which instructs the TextFinder to search for the previous occurrence of text in the TextArea.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The executed routed event arguments.</param>
        private void FindPrevious_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Set the search direction to backward and call the FindNext method from the TextFinder.
            textFinder.Direction(findNext: false);
            textFinder.Find();
        }

        /// <summary>
        /// Executes the "Replace" command, which opens the ReplaceDialog to search for and replace text in the TextArea.
        /// If the FindDialog is open, it is closed to avoid having both dialogs open simultaneously.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The executed routed event arguments.</param>
        private void Replace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Close the find dialog if it is open.
            if (findDialog != null)
                CloseFindDialog();

            // Close the replace dialog if it is open.
            if (replaceDialog == null)
            {
                // Create a new instance of the ReplaceDialog with appropriate settings.
                replaceDialog = new ReplaceDialog(textFinder)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    // Initialize the TextToFind property based on selected text (if any).
                    TextToFind = TextArea.SelectedText.Length > 0 ? TextArea.SelectedText : Settings.Default.LastFindWord,
                    TextToReplace = Settings.Default.LastReplaceWord.Length > 0 ? Settings.Default.LastReplaceWord : ""
                };

                // Dispose the dialog when it's closed.
                replaceDialog.Closed += (s, args) => replaceDialog = null;

                // Show the ReplaceDialog.
                replaceDialog.Show();
            }
        }

        /// <summary>
        /// Closes the custom Find dialog if it is open.
        /// </summary>
        private void CloseFindDialog()
        {
            // Check if the Find dialog is open.
            if (findDialog != null)
            {
                findDialog.Close(); // Close the Find dialog.
                findDialog = null; // Set the reference to null to indicate that it's closed.
            }
        }

        /// <summary>
        /// Executes the "Go To" command, which allows the user to navigate to a specific line in the text document.
        /// </summary>
        private void GoTo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Create a new GoToDialog to prompt the user for a line number input.
            GoToDialog goToDialog = new GoToDialog
            {
                LineNumber = TextArea.LineCount.ToString(), // Set the default line number to the total line count.
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            // If the user confirms the dialog, proceed to navigate to the specified line number.
            if (goToDialog.ShowDialog() == true)
            {
                // Parse the user's input into an integer representing the line number.
                int lineNumber = int.Parse(goToDialog.LineNumber);

                // Check if the line number is within a valid range (1 to the total line count).
                if (lineNumber >= 1 && lineNumber <= TextArea.LineCount)
                {
                    // Move the cursor to the specified line.
                    MoveCursorToLine(lineNumber);
                }
                else
                {
                    // Display an error message if the line number is invalid.
                    System.Windows.MessageBox.Show("Invalid line number.");
                }
            }
        }

        /// <summary>
        /// Moves the cursor to the specified line in the text area.
        /// </summary>
        /// <param name="lineNumber">The line number to navigate to.</param>
        private void MoveCursorToLine(int lineNumber)
        {
            // Calculate the character index for the start of the specified line.
            int lineStartIndex = TextArea.GetCharacterIndexFromLineIndex(lineNumber - 1);

            // Set the cursor position and selection length.
            TextArea.SelectionStart = lineStartIndex;
            TextArea.SelectionLength = 0; // Optional: Remove selection if any.

            // Scroll the text area to make the selected line visible.
            TextArea.ScrollToLine(lineNumber - 1);
        }

        /// <summary>
        /// Executes the "Select All" command to select all text in the text area.
        /// </summary>
        /// <param name="sender">The sender of the command.</param>
        /// <param name="e">The command event arguments.</param>
        private void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Select all text in the text area.
            TextArea.SelectAll();
        }

        /// <summary>
        /// Inserts the current date and time at the cursor position or replaces the selected text with it.
        /// </summary>
        private void TimeDate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Get the current date and time.
            DateTime currentDateTime = DateTime.Now;

            // Format the date and time as "h:mm tt M/d/yyyy".
            string formattedTime = currentDateTime.ToString("h:mm tt M/d/yyyy");

            // Check if text is selected in the TextArea.
            if (TextArea.SelectedText.Length > 0)
            {
                // Replace the selected text with the formatted date and time.
                TextArea.SelectedText = formattedTime;
            }
            else
            {
                // Get the cursor position.
                int cursorPosition = TextArea.SelectionStart;

                // Insert the formatted date and time at the cursor position.
                TextArea.Text = TextArea.Text.Insert(cursorPosition, formattedTime);
            }
        }

        #endregion

        #region Format Menu Command's Code Implementation


        /// <summary>
        /// Handles the Click event of the FontMenuItem to display a font selection dialog
        /// and apply the selected font settings to the text area.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void FontMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Create and show a new FontDialog with the main window as its owner.
            FontDialog fontDialog = new FontDialog
            {
                Owner = this
            };
            fontDialog.ShowDialog();

            // Check if the dialog result is true, indicating the user confirmed the selection.
            if (fontDialog.DialogResult == true)
            {
                // Apply the selected font family to the text area.
                string fontName = Settings.Default.FontFamily; // Get the font family name from settings.
                TextArea.FontFamily = new System.Windows.Media.FontFamily(fontName); // Set the font family.

                // Apply the selected font size to the text area.
                TextArea.FontSize = Settings.Default.FontSize; // Set the font size.
                CurrentFontSize = Settings.Default.FontSize; // Update the current font size.

                // Apply bold formatting based on the setting.
                if (Settings.Default.FontBold)
                    TextArea.FontWeight = FontWeights.Bold; // Set font weight to bold.
                else
                    TextArea.FontWeight = FontWeights.Normal; // Set font weight to normal.

                // Apply italic formatting based on the setting.
                if (Settings.Default.FontItalic)
                    TextArea.FontStyle = FontStyles.Italic; // Set font style to italic.
                else
                    TextArea.FontStyle = FontStyles.Normal; // Set font style to normal.
            }
        }

        /// <summary>
        /// Enables word wrapping for the text area.
        /// </summary>
        private void WordWrap_Checked(object sender, RoutedEventArgs e)
        {
            // Enable word wrapping.
            TextArea.TextWrapping = TextWrapping.WrapWithOverflow;
        }

        /// <summary>
        /// Disables word wrapping for the text area.
        /// </summary>
        private void WordWrap_Unchecked(object sender, RoutedEventArgs e)
        {
            // Disable word wrapping.
            TextArea.TextWrapping = TextWrapping.NoWrap;
        }

        #endregion

        #region View Menu Command's Code Implementation

        /// <summary>
        /// Gets or sets the current zoom level, which determines the scaling of content.
        /// </summary>
        public double ZoomLevel { get; set; } = Settings.Default.ZoomPercentage;

        /// <summary>
        /// Gets or sets the step size used to adjust the zoom level, typically in increments like 10%.
        /// </summary>
        public double ZoomStep { get; set; } = 0.1;

        /// <summary>
        /// Increases the zoom level by a specified step, limiting it to a maximum of 500%.
        /// </summary>
        private void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomLevel += ZoomStep; // Increase the zoom level by the zoom step
            if (ZoomLevel > 5.0) // Limit zoom to a maximum of 500%
            {
                ZoomLevel = 5.0;
            }
            RoundZoomLevel(); // Round the zoom level to a specified number of decimal places
            ApplyZoom(); // Apply the updated zoom level to the TextArea
        }

        /// <summary>
        /// Decreases the zoom level by a specified step, limiting it to a minimum of 10%.
        /// </summary>
        private void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomLevel -= ZoomStep; // Decrease the zoom level by the zoom step
            if (ZoomLevel < 0.1) // Limit zoom to a minimum of 10%
            {
                ZoomLevel = 0.1;
            }
            RoundZoomLevel(); // Round the zoom level to a specified number of decimal places
            ApplyZoom(); // Apply the updated zoom level to the TextArea
        }

        /// <summary>
        /// Restores the zoom level to the default 100%.
        /// </summary>
        private void RestoreDefaultZoom_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomLevel = 1.0; // Reset to the default zoom level (100%)
            ApplyZoom(); // Apply the default zoom level to the TextArea
        }

        /// <summary>
        /// Applies the current zoom level to the TextArea and updates the status bar's zoom percentage display.
        /// </summary>
        private void ApplyZoom()
        {
            // Update the font size of the TextArea based on the zoomLevel
            TextArea.FontSize = CurrentFontSize * ZoomLevel; // current font size is the initial font size

            // Update the StatusBar ZoomPercentage to reflect the current zoom level
            ZoomPercentage.Content = $"{(int)(ZoomLevel * 100)}%";
        }

        /// <summary>
        /// Rounds the current zoom level to a specified number of decimal places (1 for 10% increments).
        /// </summary>
        private void RoundZoomLevel()
        {
            // Round the zoomLevel to a specific number of decimal places (1 for 10% increments)
            int decimalPlaces = 1;
            ZoomLevel = Math.Round(ZoomLevel, decimalPlaces);
        }

        /// <summary>
        /// Shows the status bar if it is not null.
        /// </summary>
        private void StatusBar_Checked(object sender, RoutedEventArgs e)
        {
            // Check if the status bar element is not null.
            if (NotepadStatusBar != null)
            {
                // Set the visibility of the status bar to be visible.
                NotepadStatusBar.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Hides the status bar by setting its visibility to collapsed.
        /// </summary>
        private void StatusBar_Unchecked(object sender, RoutedEventArgs e)
        {
            // Set the visibility of the status bar to be collapsed, hiding it.
            NotepadStatusBar.Visibility = Visibility.Collapsed;
        }


        #region Additional Features Implementation

        /// <summary>
        /// Handles the Checked event for hiding scrollbars. 
        /// </summary>
        private void HideScrollbars_Checked(object sender, RoutedEventArgs e)
        {
            // Store the current state of WordWrap.
            IsWordWrapChecked = WordWrap.IsChecked;

            // Ensure WordWrap is checked.
            if (!IsWordWrapChecked)
                WordWrap.IsChecked = true;

            // Hide vertical scrollbars and disable WordWrap menu item.
            TextArea.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            WordWrap.IsEnabled = false;
        }

        /// <summary>
        /// Handles the Unchecked event for showing scrollbars.
        /// </summary>
        private void HideScrollbars_Unchecked(object sender, RoutedEventArgs e)
        {
            // Restore the previous state of WordWrap.
            WordWrap.IsChecked = IsWordWrapChecked;

            // Enable WordWrap menu item and show vertical scrollbars.
            WordWrap.IsEnabled = true;
            TextArea.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
        }

        /// <summary>
        /// Enables spell checking in the TextArea.
        /// </summary>
        private void SpellChecking_Checked(object sender, RoutedEventArgs e)
        {
            // Enable spell checking.
            TextArea.SpellCheck.IsEnabled = true;
        }

        /// <summary>
        /// Disables spell checking in the TextArea.
        /// </summary>
        private void SpellChecking_Unchecked(object sender, RoutedEventArgs e)
        {
            // Disable spell checking.
            TextArea.SpellCheck.IsEnabled = false;
        }

        /// <summary>
        /// Opens the file's location in the file explorer if it exists; otherwise, displays an error message.
        /// </summary>
        private void ShowFileInFileExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(FilePath))
            {
                // Open the file's location in the file explorer.
                Process.Start("explorer.exe", $"/select, \"{FilePath}\"");
            }
            else
            {
                // Show an error message if the file does not exist.
                System.Windows.MessageBox.Show("Please save the file first", "Notepad", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Shows the file path in the status bar if the file exists; otherwise, displays "Untitled Document."
        /// </summary>
        private void ShowFilePath_Checked(object sender, RoutedEventArgs e)
        {
            if (File.Exists(FilePath))
            {
                // Set the status bar content to the file path.
                FilePathStatusBar.Content = FilePath;
            }
            else
            {
                // Set the status bar content to "Untitled Document" if the file does not exist.
                FilePathStatusBar.Content = "Untitled Document";
            }

            // Make the status bar visible.
            FilePathStatusBar.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides the file path in the status bar.
        /// </summary>
        private void ShowFilePath_Unchecked(object sender, RoutedEventArgs e)
        {
            // Hide the status bar content.
            FilePathStatusBar.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Inserts the file path or "[Untitled Document]" at the cursor position or replaces the selected text with it.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void InsertFilePath_Click(object sender, RoutedEventArgs e)
        {
            // Determine the text to insert based on whether the file exists or not.
            string textToInsert = File.Exists(FilePath) ? FilePath : "[Untitled Document]";

            // Check if there is selected text in the TextArea.
            if (TextArea.SelectedText.Length > 0)
            {
                // Replace the selected text with the determined text.
                TextArea.SelectedText = textToInsert;
            }
            else
            {
                // Get the cursor position.
                int cursorPosition = TextArea.SelectionStart;

                // Insert the determined text at the cursor position.
                TextArea.Text = TextArea.Text.Insert(cursorPosition, textToInsert);
            }
        }

        /// <summary>
        /// Displays a confirmation message box for changing the application theme.
        /// </summary>
        /// <param name="themeName">The name of the theme being switched to.</param>
        /// <returns>True if the user confirms the theme change; otherwise, false.</returns>
        private bool ConfirmThemeChange(string themeName)
        {
            // Display a message box with a warning about the theme change.
            MessageBoxResult result = MessageBox.Show($"Switching to {themeName} theme requires restarting the application. Continue?", "Theme Change", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            // Return true if the user clicks OK, indicating confirmation.
            return result == MessageBoxResult.OK;
        }

        /// <summary>
        /// Changes the theme of the application and restarts it if necessary.
        /// </summary>
        /// <param name="lightTheme">True if switching to the light theme, false otherwise.</param>
        /// <param name="darkTheme">True if switching to the dark theme, false otherwise.</param>
        /// <param name="themeName">The name of the theme being changed to.</param>
        private void ChangeTheme(bool lightTheme, bool darkTheme, string themeName)
        {
            // Check if the desired theme is already active.
            if ((NightMode.IsChecked && darkTheme) || (DayMode.IsChecked && lightTheme))
                return;

            // Confirm the theme change with the user.
            if (ConfirmThemeChange(themeName))
            {
                // Save changes and restart the application.
                if (ShouldSave && SaveChanges() == false)
                    return;

                Settings.Default.DetectTheme = false;
                Settings.Default.LightTheme = lightTheme;
                Settings.Default.DarkTheme = darkTheme;
                Settings.Default.Save();

                Application.Current.Shutdown();
                Process.Start(Application.ResourceAssembly.Location);
            }
        }

        /// <summary>
        /// Handles the click event for the Night Mode toggle button.
        /// </summary>
        /// <param name="sender">The event sender (Night Mode toggle button).</param>
        /// <param name="e">The event arguments.</param>
        private void NightMode_Click(object sender, RoutedEventArgs e)
        {
            // Check and change the theme to Night Mode.
            if (!NightMode.IsChecked)
                ChangeTheme(false, true, "Night");
        }

        /// <summary>
        /// Handles the click event for the Day Mode toggle button.
        /// </summary>
        /// <param name="sender">The event sender (Day Mode toggle button).</param>
        /// <param name="e">The event arguments.</param>
        private void DayMode_Click(object sender, RoutedEventArgs e)
        {
            // Check and change the theme to Day Mode.
            if (!DayMode.IsChecked)
                ChangeTheme(true, false, "Day");
        }

        /// <summary>
        /// Handles the tick event of the auto-save timer.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            // Save the document automatically.
            SaveOldDocument();
        }

        /// <summary>
        /// Handles the checked event of the auto-save checkbox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AutoSave_Checked(object sender, RoutedEventArgs e)
        {
            // Check if the file exists.
            if (File.Exists(FilePath))
            {
                // Start the auto-save timer.
                autoSaveTimer.Start();
            }
            else
            {
                // Display an error message if the file does not exist.
                MessageBox.Show("The autosave feature cannot start as the document does not exist. Please create or load a document to enable autosave functionality.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Uncheck the auto-save checkbox.
                ((MenuItem)sender).IsChecked = false;
            }
        }

        /// <summary>
        /// Handles the unchecked event of the auto-save checkbox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AutoSave_Unchecked(object sender, RoutedEventArgs e)
        {
            // Stop the auto-save timer.
            autoSaveTimer.Stop();
        }

        /// <summary>
        /// Handles the checked event of the hide menu bar checkbox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void HideMenuBar_Checked(object sender, RoutedEventArgs e)
        {
            // Hide the menu bar and adjust the text area border thickness.
            Menubar.Visibility = Visibility.Collapsed;
            TextArea.BorderThickness = new Thickness(0, 0, 0, 0);
        }

        /// <summary>
        /// Handles the unchecked event of the hide menu bar checkbox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void HideMenuBar_Unchecked(object sender, RoutedEventArgs e)
        {
            // Show the menu bar and adjust the text area border thickness.
            Menubar.Visibility = Visibility.Visible;
            TextArea.BorderThickness = new Thickness(0, 1, 0, 0);
        }

        /// <summary>
        /// Handles the mouse enter event of the window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            // Show the menu bar and adjust the text area border thickness if the hide menu bar checkbox is checked.
            if (HideMenuBar.IsChecked)
            {
                Menubar.Visibility = Visibility.Visible;
                TextArea.BorderThickness = new Thickness(0, 1, 0, 0);
            }
        }

        /// <summary>
        /// Handles the mouse leave event of the window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            // Hide the menu bar and adjust the text area border thickness if the hide menu bar checkbox is checked.
            if (HideMenuBar.IsChecked)
            {
                Menubar.Visibility = Visibility.Collapsed;
                TextArea.BorderThickness = new Thickness(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Handles the Checked event to set the window to be always on top.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void AlwaysOnTop_Checked(object sender, RoutedEventArgs e)
        {
            // Set the window to be always on top
            Topmost = true;
        }

        /// <summary>
        /// Handles the Unchecked event to disable the "always on top" behavior of the window.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void AlwaysOnTop_Unchecked(object sender, RoutedEventArgs e)
        {
            // Disable the "always on top" behavior of the window
            Topmost = false;
        }

        #endregion


        #endregion

        #region TextArea and StausBar Functionality

        /// <summary>
        /// Handles the TextChanged event of the text area, updating the window title and the unsaved changes flag.
        /// </summary>
        private void TextArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Check if the current text matches the loaded file data
            if (TextArea.Text == FileData)
            {
                // If the text matches, set the window title to the file name without an asterisk
                this.Title = FileName + " - Notepad";

                // Mark that there's no need to save changes
                ShouldSave = false;
            }
            else
            {
                // If the text has changed, set the window title with an asterisk to indicate unsaved changes
                this.Title = "*" + FileName + " - Notepad";

                // Mark that there are unsaved changes
                ShouldSave = true;
            }

            // Update the line and column index in status bar
            GetLineAndColumnPosition();
        }

        /// <summary>
        /// Event handler for the selection changed event in the TextArea control.
        /// </summary>
        /// <param name="sender">The event sender (TextArea).</param>
        /// <param name="e">The event arguments.</param>
        private void TextArea_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // When the selection in the TextArea changes, update the displayed line and column position.
            GetLineAndColumnPosition();
        }

        /// <summary>
        /// Updates and displays the current line and column position based on the text selection
        /// within the TextArea control.
        /// </summary>
        private void GetLineAndColumnPosition()
        {
            // Calculate the current line index (0-based) of the selection and add 1 to display it as 1-based.
            int line = TextArea.GetLineIndexFromCharacterIndex(TextArea.SelectionStart) + 1;

            // Calculate the column position by subtracting the starting character index of the current line from the selection's start position, and add 1 to make it 1-based.
            int column = TextArea.SelectionStart - TextArea.GetCharacterIndexFromLineIndex(line - 1) + 1;

            // Update the user interface to show the line and column position.
            LineAndColumnPosition.Content = $"Ln {line}, Col {column}";
        }

        /// <summary>
        /// Handles the Checked event of the right-to-left reading order context menu item.
        /// Sets the text area's flow direction to RightToLeft for right-to-left reading order 
        /// and maintains the custom context menu's flow direction as LeftToRight for consistency.
        /// </summary>
        private void RightToLeftReadingOrder_Checked(object sender, RoutedEventArgs e)
        {
            // Set the text area's flow direction to RightToLeft for right-to-left reading order.
            TextArea.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            // Set the custom context menu's flow direction to LeftToRight to maintain consistency.
            CustomContextMenu.FlowDirection = System.Windows.FlowDirection.LeftToRight;
        }

        /// <summary>
        /// Handles the Unchecked event of the right-to-left reading order context menu item.
        /// Reverts the text area's flow direction to LeftToRight and ensures the custom context
        /// menu's flow direction remains as LeftToRight for consistency.
        /// </summary>
        private void RightToLeftReadingOrder_Unchecked(object sender, RoutedEventArgs e)
        {
            // Revert the text area's flow direction to the default, which is LeftToRight.
            TextArea.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            // Restore the custom context menu's flow direction to LeftToRight for consistency.
            CustomContextMenu.FlowDirection = System.Windows.FlowDirection.LeftToRight;
        }

        #endregion

        #region Theme Management

        /// <summary>
        /// Changes the theme of the application based on user preferences.
        /// </summary>
        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        /// <summary>
        /// Registry key path for Windows theme settings.
        /// </summary>
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        /// <summary>
        /// Registry value name for Windows theme settings.
        /// </summary>
        private const string RegistryValueName = "AppsUseLightTheme";

        /// <summary>
        /// Enum representing the available Windows themes.
        /// </summary>
        private enum WindowsTheme
        {
            Light,
            Dark
        }

        /// <summary>
        /// Retrieves the current Windows theme from the registry.
        /// </summary>
        /// <returns>The current Windows theme, either Light or Dark.</returns>
        private static WindowsTheme GetWindowsTheme()
        {
            // Open the registry key for the current user's theme settings.
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
            {
                // Retrieve the value from the registry.
                object registryValueObject = key?.GetValue(RegistryValueName);

                // If the value is not present, default to the Light theme.
                if (registryValueObject == null)
                {
                    return WindowsTheme.Light;
                }

                // Convert the registry value to an integer.
                int registryValue = (int)registryValueObject;

                // Return Light theme if the registry value is greater than 0, otherwise Dark theme.
                return registryValue > 0 ? WindowsTheme.Light : WindowsTheme.Dark;
            }
        }

        /// <summary>
        /// Initializes the theme of the application based on user settings.
        /// </summary>
        private void InitializeCurrentWindowsTheme()
        {
            // Determine the theme based on user preferences.
            WindowsTheme theme = Settings.Default.DetectTheme ? GetWindowsTheme() :
                (Settings.Default.LightTheme ? WindowsTheme.Light : WindowsTheme.Dark);

            // Set the window attribute to enable dark or light theme.
            // Apply theme-specific attributes to the window based on Windows build number
            if (GetWindowsBuildNumber() > 19044)
            {
                // Using DwmSetWindowAttribute to set window attribute based on theme
                DwmSetWindowAttribute(new WindowInteropHelper(this).EnsureHandle(),
                    theme == WindowsTheme.Light ? 0 : 20, new[] { 1 }, 4);
            }
            else
            {
                // Using DwmSetWindowAttribute to set window attribute based on theme (different value for older Windows builds)
                DwmSetWindowAttribute(new WindowInteropHelper(this).EnsureHandle(),
                    theme == WindowsTheme.Light ? 19 : 20, new[] { 1 }, 4);
            }

            // Clear existing resource dictionaries and add the selected theme.
            System.Windows.Application.Current.Resources.MergedDictionaries.Clear();
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri(theme == WindowsTheme.Light ? "Themes/DayMode.xaml" : "Themes/NightMode.xaml", UriKind.Relative)
            });

            // Update application settings.
            Settings.Default.DetectTheme = false;
            Settings.Default.LightTheme = theme == WindowsTheme.Light;
            Settings.Default.DarkTheme = theme == WindowsTheme.Dark;
            Settings.Default.Save();

            // Update theme selection checkboxes.
            DayMode.IsChecked = theme == WindowsTheme.Light;
            NightMode.IsChecked = theme == WindowsTheme.Dark;
        }

        /// <summary>
        /// Retrieves the current Windows build number from the registry.
        /// </summary>
        /// <returns>The current Windows build number as an integer.</returns>
        static int GetWindowsBuildNumber()
        {
            // Open registry key to access Windows version information
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                // Check if the key is accessible
                if (key != null)
                {
                    // Retrieve the value of the "CurrentBuildNumber" registry entry
                    object buildNumberValue = key.GetValue("CurrentBuildNumber");
                    // Check if the build number value exists
                    if (buildNumberValue != null)
                    {
                        // Parse and return the build number as an integer
                        return int.Parse(buildNumberValue.ToString());
                    }
                }
            }
            // Return 0 if the build number cannot be retrieved
            return 0;
        }

        #endregion

        #region Save and Restore Window State

        /// <summary>
        /// Handles the source initialized event of the window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            // Restore the previous state of the application.
            RestoreState();
        }

        /// <summary>
        /// Restores the previous state of the application.
        /// </summary>
        private void RestoreState()
        {
            // Set window dimensions and position.
            this.Width = Settings.Default.WindowWidth;
            this.Height = Settings.Default.WindowHeight;
            this.Left = Settings.Default.WindowLeft;
            this.Top = Settings.Default.WindowTop;

            // Restore various UI settings.
            WordWrap.IsChecked = Settings.Default.WordWrapState;
            StatusBar.IsChecked = Settings.Default.StatusBarState;
            HideScrollbars.IsChecked = Settings.Default.HideScrollBarState;
            SpellChecking.IsChecked = Settings.Default.SpellCheckingState;
            ShowFilePath.IsChecked = Settings.Default.ShowFilePathState;
            HideMenuBar.IsChecked = Settings.Default.HideMenubarState;
            AlwaysOnTop.IsChecked = Settings.Default.AlwaysOnTopState;
            LastOpenLocation = Settings.Default.LastOpenLocation;

            TextArea.FontFamily = new FontFamily(Settings.Default.FontFamily);
            TextArea.FontSize = Settings.Default.FontSize;
            if (Settings.Default.FontBold)
                TextArea.FontWeight = FontWeights.Bold;
            else
                TextArea.FontWeight = FontWeights.Normal;
            if (Settings.Default.FontItalic)
                TextArea.FontStyle = FontStyles.Italic;
            else
                TextArea.FontStyle = FontStyles.Normal;

            ApplyZoom();
        }

        /// <summary>
        /// Saves the current state of the application.
        /// </summary>
        private void SaveState()
        {
            // Save window dimensions and position.
            Settings.Default.WindowWidth = this.Width;
            Settings.Default.WindowHeight = this.Height;
            Settings.Default.WindowLeft = this.Left;
            Settings.Default.WindowTop = this.Top;

            // Save various UI settings.
            Settings.Default.WordWrapState = WordWrap.IsChecked;
            Settings.Default.StatusBarState = StatusBar.IsChecked;
            Settings.Default.HideScrollBarState = HideScrollbars.IsChecked;
            Settings.Default.SpellCheckingState = SpellChecking.IsChecked;
            Settings.Default.ShowFilePathState = ShowFilePath.IsChecked;
            Settings.Default.HideMenubarState = HideMenuBar.IsChecked;
            Settings.Default.AlwaysOnTopState = AlwaysOnTop.IsChecked;
            Settings.Default.LastOpenLocation = LastOpenLocation;
            Settings.Default.FontSize = CurrentFontSize;
            Settings.Default.ZoomPercentage = ZoomLevel;

            // Save settings to persistent storage.
            Settings.Default.Save();
        }

        #endregion

    }
}
