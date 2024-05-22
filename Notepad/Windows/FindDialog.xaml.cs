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
using Notepad.Helper;

namespace Notepad.Windows
{
    /// <summary>
    /// Interaction logic for FindDialog.xaml
    /// </summary>
    public partial class FindDialog : Window
    {
        /// <summary>
        /// Gets or sets the associated TextFinder instance for text searching functionality.
        /// </summary>
        private TextFinder textFinder;

        /// <summary>
        /// Gets or sets the text to find in the associated TextFinder during text search.
        /// </summary>
        public string TextToFind { get; set; }

        /// <summary>
        /// Initializes a new instance of the FindDialog class.
        /// </summary>
        /// <param name="finder">The associated TextFinder instance.</param>
        public FindDialog(TextFinder finder)
        {
            InitializeComponent();
            SetTheme();
            textFinder = finder;
            FindTextBox.Focus();
        }
        
        /// <summary>
        /// Handles the click event of the "Find Next" button. Updates the TextFinder settings based on the dialog controls and initiates the search for the next occurrence.
        /// </summary>
        private void FindNextButton_Click(object sender, RoutedEventArgs e)
        {
            // Update the TextFinder settings and find the next occurrence.
            UpdateTextFinderSettings();
            textFinder.Find();
        }

        /// <summary>
        /// Handles the "Window Loaded" event. Initializes the dialog and sets focus on the FindTextBox.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize the dialog and set focus.
            FindTextBox.Text = TextToFind;
            UpdateButtonStatus();
            FindTextBox.SelectAll();
        }

        /// <summary>
        /// Handles the text change event of the FindTextBox. Enables or disables the "Find Next" button based on text changes.
        /// </summary>
        private void FindTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Enable or disable the FindNextButton based on text changes.
            UpdateButtonStatus();
        }

        /// <summary>
        /// Handles the click event of the "Cancel" button. Closes the FindDialog window.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the FindDialog window.
            this.Close();
        }

        /// <summary>
        /// Updates the settings of the TextFinder based on the values in the dialog controls, including the search text, match case, wrap around, and search direction.
        /// </summary>
        private void UpdateTextFinderSettings()
        {
            // Set the search text in the TextFinder to the text entered in the FindTextBox.
            textFinder.FindText = FindTextBox.Text;

            // Set the match case option in the TextFinder based on the state of the MatchCaseCheckBox.
            textFinder.MatchCase = MatchCaseCheckBox.IsChecked ?? false;

            // Set the wrap around option in the TextFinder based on the state of the WrapAroundCheckBox.
            textFinder.WrapAround = WrapAroundCheckBox.IsChecked ?? false;

            // Set the search direction in the TextFinder based on the state of the UpRadioButton.
            textFinder.DirectionUp = UpRadioButton.IsChecked ?? false;
        }

        /// <summary>
        /// Updates the button status, enabling or disabling the FindNextButton based on whether the FindTextBox contains text.
        /// </summary>
        private void UpdateButtonStatus()
        {
            // Check if the FindTextBox contains text.
            bool textNotEmpty = !string.IsNullOrEmpty(FindTextBox.Text);

            // Enable or disable the FindNextButton based on whether the text is empty.
            FindNextButton.IsEnabled = textNotEmpty;
        }

        /// <summary>
        /// Overrides the OnSourceInitialized method to remove the icon from the window.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
            this.Topmost = true;
        }

        #region Theme Management

        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        /// <summary>
        /// Sets the theme of the PageSetupDialog based on the user's preference.
        /// </summary>
        private void SetTheme()
        {
            if (Settings.Default.DarkTheme)
            {
                // Set dark theme
                DwmSetWindowAttribute(new WindowInteropHelper(this).EnsureHandle(), 20, new[] { 1 }, 4);
                return;
            }
            // Set default light theme
            this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f0f0"));
        }

        #endregion

        /// <summary>
        /// Handles the Closing event of the Window to save the last search word and application settings.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data containing cancel information.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save the text from the FindTextBox into the application settings for future use.
            Settings.Default.LastFindWord = FindTextBox.Text;

            // Save the updated settings to persistent storage.
            Settings.Default.Save();
        }

    }
}
