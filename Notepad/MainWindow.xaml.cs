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

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            
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
    }
}
