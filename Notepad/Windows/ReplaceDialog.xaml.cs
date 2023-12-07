using Notepad.Helper;
using Notepad.Properties;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Notepad.Windows
{
    /// <summary>
    /// Interaction logic for ReplaceDialog.xaml
    /// </summary>
    public partial class ReplaceDialog : Window
    {
        /// <summary>
        /// Gets or sets the associated TextFinder instance for text searching functionality.
        /// </summary>
        private TextFinder textFinder;

        /// <summary>
        /// Gets or sets the text to find in the text replacement process.
        /// </summary>
        public string TextToFind { get; set; }
        public string TextToReplace { get; set; }

        /// <summary>
        /// Initializes a new instance of the ReplaceDialog class with a TextFinder instance.
        /// </summary>
        /// <param name="finder">The TextFinder instance to be associated with the dialog.</param>
        public ReplaceDialog(TextFinder finder)
        {
            // Initialize the dialog's components and set the associated TextFinder instance.
            InitializeComponent();
            SetTheme();
            textFinder = finder;

            // Set focus to the FindTextBox for a smoother user experience.
            FindTextBox.Focus();
        }

        /// <summary>
        /// Handles the Window_Loaded event. Sets up the initial state of the dialog.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FindTextBox.Text = TextToFind;
            ReplaceTextBox.Text = TextToReplace;
            // Update the button status based on the FindTextBox's content.
            UpdateButtonStatus();

            // Select all text in the FindTextBox to make it easier for the user to replace or search again.
            FindTextBox.SelectAll();
        }

        /// <summary>
        /// Handles the TextChanged event for the FindTextBox, updating button status.
        /// </summary>
        private void FindTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Update the button status based on the content of the FindTextBox when the text changes.
            UpdateButtonStatus();
        }

        /// <summary>
        /// Updates the enabled status of the FindNext, Replace, and ReplaceAll buttons based on the FindTextBox's text.
        /// </summary>
        private void UpdateButtonStatus()
        {
            // Check if the FindTextBox's text is not empty.
            bool textNotEmpty = !string.IsNullOrEmpty(FindTextBox.Text);

            // Set the enabled status of FindNextButton, ReplaceButton, and ReplaceAllButton based on the textNotEmpty condition.
            FindNextButton.IsEnabled = ReplaceButton.IsEnabled = ReplaceAllButton.IsEnabled = textNotEmpty;
        }

        /// <summary>
        /// Initiates a search operation using the TextFinder based on the settings, starting from the current location.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Event arguments associated with the event.</param>
        private void FindNextButton_Click(object sender, RoutedEventArgs e)
        {
            // Update the TextFinder settings based on the values entered in the dialog.
            UpdateTextFinderSettings();

            // Trigger the FindNext method of the TextFinder to find the next occurrence of the specified text.
            textFinder.Find();
        }


        /// <summary>
        /// Updates TextFinder settings and replaces the currently selected text in the TextBox.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Event arguments associated with the event.</param>
        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            // Update the TextFinder settings based on the values entered in the dialog.
            UpdateTextFinderSettings();

            // Set the direction to false (downward) to replace text from the current position.
            textFinder.DirectionUp = false;

            // Replace the currently selected text in the TextBox with the specified replacement text.
            textFinder.ReplaceText(ReplaceTextBox.Text);
        }


        /// <summary>
        /// Updates TextFinder settings and replaces all occurrences of the search text in the TextBox.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Event arguments associated with the event.</param>
        private void ReplaceAllButton_Click(object sender, RoutedEventArgs e)
        {
            // Update the TextFinder settings based on the values entered in the dialog.
            UpdateTextFinderSettings();

            // Replace all occurrences of the search text in the TextBox with the specified replacement text.
            textFinder.ReplaceAllText(ReplaceTextBox.Text);
        }


        /// <summary>
        /// Updates the TextFinder settings based on the values entered in the dialog.
        /// </summary>
        private void UpdateTextFinderSettings()
        {
            // Update the FindText property of the TextFinder with the text entered in the FindTextBox.
            textFinder.FindText = FindTextBox.Text;

            // Update the MatchCase property of the TextFinder based on the state of the MatchCaseCheckBox.
            textFinder.MatchCase = MatchCaseCheckBox.IsChecked ?? false;

            // Update the WrapAround property of the TextFinder based on the state of the WrapAroundCheckBox.
            textFinder.WrapAround = WrapAroundCheckBox.IsChecked ?? false;
        }


        /// <summary>
        /// Handles the click event for the CancelButton, closing the dialog.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the dialog window.
            Close();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        #region Theme Management
        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);
        private void SetTheme()
        {
            if (Settings.Default.DarkTheme)
            {
                DwmSetWindowAttribute(new WindowInteropHelper(this).EnsureHandle(), 20, new[] { 1 }, 4);
                return;
            }
            this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f0f0"));
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.LastFindWord = FindTextBox.Text;
            Settings.Default.LastReplaceWord = ReplaceTextBox.Text;
            Settings.Default.Save();
        }
    }
}
