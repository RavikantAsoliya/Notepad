using Notepad.Properties;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Notepad.Windows
{
    /// <summary>
    /// Interaction logic for FontDialog.xaml
    /// </summary>
    public partial class FontDialog : Window
    {
        public FontDialog()
        {
            InitializeComponent();
            // Set default font properties based on user settings.
            SetDefaultFontProperties();
            // Select the current font style, weight, and size.
            SelectCurrentItems();
            // Set focus to the font list box.
            FontListBox.Focus();
        }

        /// <summary>
        /// Set default font properties for sample text based on user settings.
        /// </summary>
        private void SetDefaultFontProperties()
        {
            // Set the FontFamily of the sample label to the one stored in user settings.
            SampleText.FontFamily = new FontFamily(Settings.Default.FontFamily);
            // Set the FontSize of the sample label to the one stored in user settings.
            SampleText.FontSize = Settings.Default.FontSize;
            // Set the FontStyle of the sample label based on whether italics is enabled in user settings.
            SampleText.FontStyle = Settings.Default.FontItalic ? FontStyles.Italic : FontStyles.Normal;
            // Set the FontWeight of the sample label based on whether bold is enabled in user settings.
            SampleText.FontWeight = Settings.Default.FontBold ? FontWeights.Bold : FontWeights.Normal;
        }

        /// <summary>
        /// Selects the current items for FontListBox, FontStylesListBox, and FontSizeListBox based on the properties of SampleText or TextArea.
        /// </summary>
        private void SelectCurrentItems()
        {
            // Set the selected font family in the FontListBox
            FontListBox.SelectedItem = SampleText.FontFamily;

            // Default index for FontStylesListBox
            int index = 0;

            // Check if the font style is italic
            if (SampleText.FontStyle == FontStyles.Italic)
            {
                // If italic, check if the font weight is bold
                index = SampleText.FontWeight == FontWeights.Bold ? 3 : 1;
            }
            // If not italic, check if the font weight is bold
            else if (SampleText.FontWeight == FontWeights.Bold)
            {
                index = 2;
            }

            // Set the selected index in FontStylesListBox
            FontStylesListBox.SelectedIndex = index;

            // Set the selected font size in FontSizeListBox
            FontSizeListBox.SelectedItem = SampleText.FontSize;
        }



        /// <summary>
        /// Event handler for font selection change.
        /// </summary>
        private void FontList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Set the FontFamily of the sample label to the selected font family from the font list box.
            SampleText.FontFamily = (FontFamily)FontListBox.SelectedItem;
        }


        /// <summary>
        /// Event handler for font style selection change.
        /// </summary>
        private void FontStyleList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get the selected font style as a string.
            string selectedStyle = FontStylesListBox.SelectedItem.ToString();
            // Set the FontStyle of the sample label based on whether the selected style contains "Italic".
            SampleText.FontStyle = selectedStyle.Contains("Italic") ? FontStyles.Italic : FontStyles.Normal;
            // Set the FontWeight of the sample label based on whether the selected style contains "Bold".
            SampleText.FontWeight = selectedStyle.Contains("Bold") ? FontWeights.Bold : FontWeights.Normal;
        }


        /// <summary>
        /// Event handler for font size selection change.
        /// </summary>
        private void FontSizeList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Set the FontSize of the sample label to the selected font size from the font size list box.
            SampleText.FontSize = (double)FontSizeListBox.SelectedItem;
        }


        /// <summary>
        /// Event handler for OK button click.
        /// </summary>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Save font settings and close the dialog with a true result.
            SaveFontSettings();
            CloseDialog(true);
        }

        /// <summary>
        /// Event handler for Cancel button click.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the dialog with a false result.
            CloseDialog(false);
        }

        /// <summary>
        /// Save font settings to user preferences.
        /// </summary>
        private void SaveFontSettings()
        {
            // Save the selected font family to user settings.
            Settings.Default.FontFamily = SampleText.FontFamily.ToString();
            // Save the selected font size to user settings.
            Settings.Default.FontSize = SampleText.FontSize;
            // Save whether the selected font weight is bold to user settings.
            Settings.Default.FontBold = SampleText.FontWeight == FontWeights.Bold;
            // Save whether the selected font style is italic to user settings.
            Settings.Default.FontItalic = SampleText.FontStyle == FontStyles.Italic;
        }


        /// <summary>
        /// Close the dialog with specified dialog result.
        /// </summary>
        private void CloseDialog(bool dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
