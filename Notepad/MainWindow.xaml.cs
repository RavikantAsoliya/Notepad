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

        public MainWindow()
        {
            InitializeComponent();
            TextArea.Focus();
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

        #region

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

        #endregion
        private void Replace_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void GoTo_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void TimeDate_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

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




        #endregion

    }
}
