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
    /// Interaction logic for SaveMessageBox.xaml
    /// </summary>
    public partial class SaveMessageBox : Window
    {
        /// <summary>
        /// Gets or sets the message displayed in the SaveMessageBox, prompting the user for confirmation.
        /// </summary>
        public string Message { get; set; } = "Do you want to changes to Untitled?";

        /// <summary>
        /// Gets or sets the result of the user's choice in the SaveMessageBox dialog.
        /// This property is a nullable boolean (bool?) that represents the user's decision.
        /// </summary>
        public bool? Result { get; set; }

        public SaveMessageBox()
        {
            InitializeComponent();
            SetTheme();
        }

        /// <summary>
        /// Handles the Window Loaded event, setting the text of the SaveMessageBox to the provided message.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SaveMessage.Text = Message; // Set the text of the SaveMessageBox to the provided message.
        }

        /// <summary>
        /// Handles the Click event of the SaveButton, setting the Result to true and closing the window.
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Result = true; // Set the Result to true.
            this.Close(); // Close the window.
        }

        /// <summary>
        /// Handles the Click event of the DontSaveButton, setting the Result to false and closing the window.
        /// </summary>
        private void DontSaveButton_Click(object sender, RoutedEventArgs e)
        {
            Result = false; // Set the Result to false.
            this.Close(); // Close the window.
        }

        /// <summary>
        /// Handles the Click event of the CancelButton, setting the Result to null and closing the window.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = null; // Set the Result to null.
            this.Close(); // Close the window.
        }

        #region Theme Management
        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);
        private void SetTheme()
        {
            if (Settings.Default.DarkTheme)
            {
                DwmSetWindowAttribute(new WindowInteropHelper(this).EnsureHandle(), 20, new[] { 1 }, 4);
            }
        }
        #endregion

    }
}
