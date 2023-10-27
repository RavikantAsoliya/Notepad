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
    /// Interaction logic for SaveMessageBox.xaml
    /// </summary>
    public partial class SaveMessageBox : Window
    {
        /// <summary>
        /// Gets or sets the message displayed in the SaveMessageBox, prompting the user for confirmation.
        /// </summary>
        public string Message { get; set; } = "Do you want to changes to Untitled?";

        public SaveMessageBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Window Loaded event, setting the text of the SaveMessageBox to the provided message.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SaveMessage.Text = Message; // Set the text of the SaveMessageBox to the provided message.
        }

        /// <summary>
        /// Handles the Click event of the SaveButton, setting the DialogResult to true and closing the window.
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; // Set the DialogResult to true.
            this.Close(); // Close the window.
        }

        /// <summary>
        /// Handles the Click event of the DontSaveButton, setting the DialogResult to false and closing the window.
        /// </summary>
        private void DontSaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Set the DialogResult to false.
            this.Close(); // Close the window.
        }

        /// <summary>
        /// Handles the Click event of the CancelButton, setting the DialogResult to null and closing the window.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = null; // Set the DialogResult to null.
            this.Close(); // Close the window.
        }

    }
}
