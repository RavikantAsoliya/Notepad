using Notepad.Windows;
using System;
using System.Collections.Generic;
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
        }

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
                    // I will implement it later.
                }
                else if (result == null || (result == true ))//&& SaveAsNewDocument() == false
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
            return askToSave.ShowDialog();
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

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void Print_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void NewWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

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
        }
    }
}
