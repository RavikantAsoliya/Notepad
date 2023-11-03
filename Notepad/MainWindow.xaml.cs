using Notepad.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        /// Gets or sets a flag indicating whether there are unsaved changes in the current document.
        /// </summary>
        public bool ShouldSave { get; set; } = false;

        /// <summary>
        /// Indicates whether WordWrap is checked in the menu.
        /// </summary>
        private bool IsWordWrapChecked { get; set; } = false;

        readonly double currentFontSize; // Declare a variable to store the current font size.

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


        public MainWindow()
        {
            InitializeComponent();
            TextArea.Focus();
            currentFontSize = TextArea.FontSize; // Assign the current font size of the TextArea to the variable.
            textFinder = new TextFinder(ref TextArea);
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
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), // Set the initial directory for the file dialog to the user's Documents folder.
                RestoreDirectory = true // Restore the directory to its previously selected location
            };

            // Show the open file dialog and check if the user selected a file.
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName; // Get the full path of the selected file.
                FileName = System.IO.Path.GetFileNameWithoutExtension(FilePath); // Extract the file name without its extension.
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

        private void PageSetup_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Page Setup Feature
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
        }

        /// <summary>
        /// Determines whether the application should exit, taking into account unsaved changes and user decisions.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> if the application should exit; otherwise, <see langword="false"/>.
        /// </returns>
        private bool ShouldExitApplication()
        {
            // Check if there are unsaved changes.
            if (!ShouldSave)
                return true; // No changes to save, it's safe to exit.

            // Prompt the user to save changes and get their choice (true for save, false for discard, null for cancel).
            bool? result = AskToSaveFile();

            // Check the user's choice.
            if (result == true && File.Exists(FilePath))
            {
                // User chose to save, and the file exists, so save the current document.
                SaveOldDocument();
            }
            else if (result == null || (result == true && SaveAsNewDocument() == false))
            {
                // User canceled or encountered an error while saving, or they chose not to save the current document.
                // In any of these cases, it's not safe to exit the application.
                return false;
            }

            // If none of the previous conditions are met, it's safe to exit.
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
                    TextToFind = TextArea.SelectedText.Length > 0 ? TextArea.SelectedText : ""
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
                    TextToFind = TextArea.SelectedText.Length > 0 ? TextArea.SelectedText : ""
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

        // TODO: Font Dialog and Its Implementation

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
        public double ZoomLevel { get; set; } = 1.0;

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
            TextArea.FontSize = currentFontSize * ZoomLevel; // current font size is the initial font size

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
        private void HideScrollbarsMenuItem_Checked(object sender, RoutedEventArgs e)
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
        private void HideScrollbarsMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            // Restore the previous state of WordWrap.
            WordWrap.IsChecked = IsWordWrapChecked;

            // Enable WordWrap menu item and show vertical scrollbars.
            WordWrap.IsEnabled = true;
            TextArea.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
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

            // Update the cursor position display
            GetCursorPosition();
        }

        /// <summary>
        /// Retrieves and displays the current cursor position in the text area.
        /// </summary>
        private void GetCursorPosition()
        {
            int lineNumber = 1; // Initialize with the first line.
            int columnNumber = 1;

            // Get the current cursor position.
            int caretIndex = TextArea.CaretIndex;

            // Split the text into lines based on line breaks (e.g., "\r\n" or "\n").
            string[] lines = TextArea.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            // Loop through each line to find the cursor's line and column.
            foreach (string line in lines)
            {
                // Check if the caret index is within the current line.
                if (caretIndex <= line.Length)
                {
                    columnNumber = caretIndex + 1; // Column number is 1-based.
                    break;
                }

                // Move to the next line.
                caretIndex -= line.Length + Environment.NewLine.Length;
                lineNumber++;
            }

            // Display the cursor's position.
            CursorLocationStatusBarItem.Content = $"Ln {lineNumber}, Col {columnNumber}";
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

        
    }
}
