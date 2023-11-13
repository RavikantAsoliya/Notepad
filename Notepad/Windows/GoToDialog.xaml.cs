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
    /// Interaction logic for GoToDialog.xaml
    /// </summary>
    public partial class GoToDialog : Window
    {
        /// <summary>
        /// The line number to which you want to jump using the GoTo dialog.
        /// </summary>
        public string LineNumber { get; set; } = "";

        public GoToDialog()
        {
            InitializeComponent();
            SetTheme();
            // Set focus to the LineNumberTextBox when the dialog is loaded.
            LineNumberTextBox.Focus();
        }

        /// <summary>
        /// Handles the Click event for the GoToButton. This method sets the LineNumber based on the input, and then closes the dialog with a result of true.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments containing information about the event.</param>
        private void GoToButton_Click(object sender, RoutedEventArgs e)
        {
            // Set the LineNumber property to the text entered in the LineNumberTextBox.
            LineNumber = LineNumberTextBox.Text;

            // Set the DialogResult of the dialog to true, indicating successful completion.
            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event for the CancelButton. This method closes the dialog with a result of false.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments containing information about the event.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Set the DialogResult of the dialog to false, indicating cancellation.
            DialogResult = false;
        }

        /// <summary>
        /// Handles the Loaded event for the window. This method sets the LineNumberTextBox text and selects all of its content.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments containing information about the event.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the text in the LineNumberTextBox to the value of LineNumber.
            LineNumberTextBox.Text = LineNumber;

            // Select all the content in the LineNumberTextBox.
            LineNumberTextBox.SelectAll();
        }

        /// <summary>
        /// Handles the KeyDown event for the LineNumberTextBox, allowing only numeric keys (0-9) and backspace (Delete or Back).
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Key event arguments containing information about the pressed key.</param>
        private void LineNumberTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the pressed key is not a number (0-9) or backspace (Delete or Back).
            if (!IsNumericKey(e.Key) && e.Key != Key.Back)
            {
                // If the key is not a number or backspace, mark it as handled to ignore it.
                e.Handled = true;
            }
        }

        /// <summary>
        /// Checks if the specified key is a numeric key, which includes the keys 0-9 (both from the main keyboard and the numeric keypad).
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is numeric; otherwise, false.</returns>
        private bool IsNumericKey(Key key)
        {
            // Check if the key falls within the range of numeric keys on the main keyboard (0-9) or the numeric keypad (NumPad0-NumPad9).
            return (key >= Key.D0 && key <= Key.D9) || (key >= Key.NumPad0 && key <= Key.NumPad9);
        }
        #region Theme Management
        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);
        private void SetTheme()
        {
            if (Settings.Default.DarkTheme)
            {
                DwmSetWindowAttribute(new WindowInteropHelper(this).EnsureHandle(), 20, new[] { 1 }, 4);
                LabelLineNumber.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff"));
                return;
            }
            this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f0f0"));
        }
        #endregion
    }
}
