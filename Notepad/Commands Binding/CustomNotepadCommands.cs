using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Notepad.Commands_Binding
{
    /// <summary>
    /// Defines a collection of custom commands for a Notepad application.
    /// </summary>
    internal class CustomNotepadCommands
    {
        /// <summary>
        /// Represents a custom command to open a new window.
        /// </summary>
        public static RoutedUICommand NewWindow { get; private set; }

        /// <summary>
        /// Represents a custom command to save a document as.
        /// </summary>
        public static RoutedUICommand SaveAs { get; private set; }

        /// <summary>
        /// Represents a custom command to search with Bing.
        /// </summary>
        public static RoutedUICommand SearchWithBing { get; private set; }

        /// <summary>
        /// Represents a custom command to find the next occurrence.
        /// </summary>
        public static RoutedUICommand FindNext { get; private set; }

        /// <summary>
        /// Represents a custom command to find the previous occurrence.
        /// </summary>
        public static RoutedUICommand FindPrevious { get; private set; }

        /// <summary>
        /// Represents a custom command to go to a specific location.
        /// </summary>
        public static RoutedUICommand GoTo { get; private set; }

        /// <summary>
        /// Represents a custom command to insert the current time and date.
        /// </summary>
        public static RoutedUICommand TimeDate { get; private set; }

        static CustomNotepadCommands()
        {
            // Create a custom command to open a new window with the key gesture Ctrl+Shift+N
            NewWindow = new RoutedUICommand("New Window", "NewWindow", typeof(CustomNotepadCommands), new InputGestureCollection
            {
                new KeyGesture(Key.N, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+N")
            });

            // Create a custom command to save a document as with the key gesture Ctrl+Shift+S
            SaveAs = new RoutedUICommand("Save As...", "SaveAs", typeof(CustomNotepadCommands), new InputGestureCollection
            {
                new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+S")
            });

            // Create a custom command to search with Bing with the key gesture Ctrl+E
            SearchWithBing = new RoutedUICommand("Search with Bing...", "SearchWithBing", typeof(CustomNotepadCommands), new InputGestureCollection
            {
                new KeyGesture(Key.E, ModifierKeys.Control, "Ctrl+E")
            });

            // Create a custom command to find the next occurrence with the key gesture F3
            FindNext = new RoutedUICommand("Find Next", "FindNext", typeof(CustomNotepadCommands), new InputGestureCollection
            {
                new KeyGesture(Key.F3, ModifierKeys.None, "F3")
            });

            // Create a custom command to find the previous occurrence with the key gesture Shift+F3
            FindPrevious = new RoutedUICommand("Find Previous", "FindPrevious", typeof(CustomNotepadCommands), new InputGestureCollection
            {
                new KeyGesture(Key.F3, ModifierKeys.Shift, "Shift+F3")
            });

            // Create a custom command to go to a specific location with the key gesture Ctrl+G
            GoTo = new RoutedUICommand("Go To...", "GoTo", typeof(CustomNotepadCommands), new InputGestureCollection
            {
                new KeyGesture(Key.G, ModifierKeys.Control, "Ctrl+G")
            });

            // Create a custom command to insert the current time and date with the key gesture F5
            TimeDate = new RoutedUICommand("Time/Date", "TimeDate", typeof(CustomNotepadCommands), new InputGestureCollection
            {
                new KeyGesture(Key.F5, ModifierKeys.None, "F5")
            });

        }
    }
}
