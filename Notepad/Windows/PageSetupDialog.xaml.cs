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
    /// Interaction logic for PageSetupDialog.xaml
    /// </summary>
    public partial class PageSetupDialog : Window
    {
        public PageSetupDialog()
        {
            InitializeComponent();
            SetTheme();

            // Restore Page Setting Settings
            RestoreSettings();
        }

        /// <summary>
        /// Gets or sets the selected paper size (e.g., A3, A4) for the page setup dialog.
        /// </summary>
        public string PaperSize { get; private set; }

        /// <summary>
        /// Gets or sets the selected paper source (e.g., Automatically select, Upper Paper Tray) for the page setup dialog.
        /// </summary>
        public string PaperSource { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the page orientation is portrait (true) or landscape (false) for the page setup dialog.
        /// </summary>
        public bool IsPortrait { get; private set; }

        /// <summary>
        /// Gets or sets the left margin value (in inches) for the page setup dialog.
        /// </summary>
        public double LeftMargin { get; private set; }

        /// <summary>
        /// Gets or sets the right margin value (in inches) for the page setup dialog.
        /// </summary>
        public double RightMargin { get; private set; }

        /// <summary>
        /// Gets or sets the top margin value (in inches) for the page setup dialog.
        /// </summary>
        public double TopMargin { get; private set; }

        /// <summary>
        /// Gets or sets the bottom margin value (in inches) for the page setup dialog.
        /// </summary>
        public double BottomMargin { get; private set; }

        /// <summary>
        /// Gets or sets the header text for the page setup dialog.
        /// </summary>
        public string Header { get; private set; }

        /// <summary>
        /// Gets or sets the footer text for the page setup dialog.
        /// </summary>
        public string Footer { get; private set; }

        /// <summary>
        /// Event handler for the OK button click.
        /// Captures the values from the dialog controls and sets the properties accordingly.
        /// Closes the dialog and returns true to indicate success.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Capture values from the dialog controls

            // Retrieve the selected paper size from the ComboBox and convert it to string
            PaperSize = ((ComboBoxItem)paperSizeComboBox.SelectedItem).Content.ToString();

            // Retrieve the selected paper source from the ComboBox and convert it to string
            PaperSource = ((ComboBoxItem)sourceComboBox.SelectedItem).Content.ToString();

            // Check if the PortraitRadioButton is checked to determine the page orientation
            IsPortrait = PortraitRadioButton.IsChecked == true;

            // Parse the text entered in the left margin TextBox to a double value
            LeftMargin = double.Parse(leftMarginTextBox.Text);

            // Parse the text entered in the right margin TextBox to a double value
            RightMargin = double.Parse(rightMarginTextBox.Text);

            // Parse the text entered in the top margin TextBox to a double value
            TopMargin = double.Parse(topMarginTextBox.Text);

            // Parse the text entered in the bottom margin TextBox to a double value
            BottomMargin = double.Parse(bottomMarginTextBox.Text);

            // Retrieve the text entered in the header TextBox
            Header = headerTextBox.Text;

            // Retrieve the text entered in the footer TextBox
            Footer = footerTextBox.Text;

            // Save Current Page Setup settings
            SaveSettings();

            // Close the dialog and return true to indicate success
            DialogResult = true;
        }

        /// <summary>
        /// Event handler for the Cancel button click.
        /// Closes the dialog and returns false to indicate cancellation.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Close the dialog and return false to indicate cancellation
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
        /// Restores the settings from the application settings.
        /// </summary>
        private void RestoreSettings()
        {
            PaperSize = Settings.Default.PaperSize;
            PaperSource = Settings.Default.PaperSource;
            IsPortrait = Settings.Default.IsPortrait;
            LeftMargin = Settings.Default.LeftMargin;
            RightMargin = Settings.Default.RightMargin;
            TopMargin = Settings.Default.TopMargin;
            BottomMargin = Settings.Default.BottomMargin;
            Header = Settings.Default.Header;
            Footer = Settings.Default.Footer;

            // Update the dialog controls with the restored settings
            paperSizeComboBox.SelectedItem = paperSizeComboBox.Items.Cast<ComboBoxItem>()
                                            .FirstOrDefault(item => item.Content.ToString() == PaperSize);
            sourceComboBox.SelectedItem = sourceComboBox.Items.Cast<ComboBoxItem>()
                                            .FirstOrDefault(item => item.Content.ToString() == PaperSource);
            PortraitRadioButton.IsChecked = IsPortrait;
            LandscapeRadioButton.IsChecked = !IsPortrait;
            leftMarginTextBox.Text = LeftMargin.ToString();
            rightMarginTextBox.Text = RightMargin.ToString();
            topMarginTextBox.Text = TopMargin.ToString();
            bottomMarginTextBox.Text = BottomMargin.ToString();
            headerTextBox.Text = Header;
            footerTextBox.Text = Footer;
        }

        /// <summary>
        /// Saves the current settings to the application settings.
        /// </summary>
        private void SaveSettings()
        {
            Settings.Default.PaperSize = PaperSize;
            Settings.Default.PaperSource = PaperSource;
            Settings.Default.IsPortrait = IsPortrait;
            Settings.Default.LeftMargin = LeftMargin;
            Settings.Default.RightMargin = RightMargin;
            Settings.Default.TopMargin = TopMargin;
            Settings.Default.BottomMargin = BottomMargin;
            Settings.Default.Header = Header;
            Settings.Default.Footer = Footer;
            Settings.Default.Save();
        }
    }
}
