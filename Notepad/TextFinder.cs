using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Notepad
{
    public class TextFinder
    {
        /// <summary>
        /// Gets or sets the text to find in the TextBox.
        /// </summary>
        public string FindText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the search is case-sensitive.
        /// </summary>
        public bool MatchCase { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the search wraps around to the beginning when the end of the text is reached.
        /// </summary>
        public bool WrapAround { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the search direction (up or down).
        /// </summary>
        public bool DirectionUp { get; set; }

        /// <summary>
        /// The TextBox where text replacement operations are performed.
        /// </summary>
        private readonly TextBox TextArea;

        /// <summary>
        /// The starting index for text replacement within the TextArea.
        /// Initialized to -1 as no replacement is in progress initially.
        /// </summary>
        private int replaceStartIndex = -1;

        /// <summary>
        /// Initializes a new instance of the TextFinder class with the provided TextBox.
        /// </summary>
        /// <param name="textBox">The TextBox to be associated with the TextFinder instance.</param>
        public TextFinder(ref TextBox textBox)
        {
            // Assign the provided TextBox to the TextArea property for text finding operations.
            TextArea = textBox;
        }



        /// <summary>
        /// Sets the search direction.
        /// </summary>
        /// <param name="findNext">True for searching up, false for searching down.</param>
        public void Direction(bool findNext)
        {
            // Set the DirectionUp property to the specified direction.
            DirectionUp = !findNext;
        }

        /// <summary>
        /// Replaces all occurrences of the FindText with the specified replaceText in the TextArea.
        /// </summary>
        /// <param name="replaceText">The text to replace the found text with.</param>
        public void ReplaceAllText(string replaceText)
        {
            // Determine the regular expression options based on MatchCase setting.
            RegexOptions regexOptions = MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase;

            // Use the Regex.Replace method to perform a global replacement of FindText with replaceText in the TextArea's text.
            TextArea.Text = Regex.Replace(TextArea.Text, FindText, replaceText, regexOptions);
        }


        /// <summary>
        /// Finds the next occurrence of the specified text in the TextArea.
        /// </summary>
        /// <returns>True if a match is found; otherwise, false.</returns>
        public bool Find()
        {
            // Check if the FindText is empty.
            if (string.IsNullOrEmpty(FindText))
            {
                // Display a message to the user and return false since the search term is empty.
                //MessageBox.Show("Please enter a search term.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            // Determine the string comparison type based on the MatchCase setting.
            StringComparison comparisonType = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            // Calculate the search starting index based on the search direction.
            int startIndex = DirectionUp ? TextArea.SelectionStart - 1 : TextArea.SelectionStart + TextArea.SelectionLength;

            // If searching upwards and the index goes below zero, handle wraparound behavior.
            if (DirectionUp && startIndex < 0)
            {
                if (!WrapAround)
                {
                    // Display a message to the user and return false since text is not found.
                    MessageBox.Show("Text not found.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
                startIndex = DirectionUp ? TextArea.Text.Length - 1 : 0;
            }

            // Perform a case-sensitive or case-insensitive search based on MatchCase.
            int index = DirectionUp ? TextArea.Text.LastIndexOf(FindText, startIndex, comparisonType) : TextArea.Text.IndexOf(FindText, startIndex, comparisonType);

            // If a match is found, select the text, focus on the TextArea, and store the start index for replacement.
            if (index != -1)
            {
                TextArea.Select(index, FindText.Length);
                TextArea.Focus();
                replaceStartIndex = index;
                return true;
            }

            // If wraparound is enabled, try to find a match in the opposite direction.
            if (WrapAround)
            {
                int wrapIndex = DirectionUp ? TextArea.Text.LastIndexOf(FindText, comparisonType) : TextArea.Text.IndexOf(FindText, comparisonType);

                if (wrapIndex != -1)
                {
                    // Select the text, focus on the TextArea, and store the start index for replacement.
                    TextArea.Select(wrapIndex, FindText.Length);
                    TextArea.Focus();
                    replaceStartIndex = wrapIndex;
                    return true;
                }
            }

            // Display a message to the user and return false since text is not found.
            MessageBox.Show("Text not found.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }


        /// <summary>
        /// Replaces the currently selected text in the TextArea with the specified replacement text.
        /// </summary>
        /// <param name="replaceText">The text to replace the currently selected text with.</param>
        public void ReplaceText(string replaceText)
        {
            // Disable the "DirectionUp" flag to ensure replacement occurs in the expected direction.
            DirectionUp = false;

            // If there is no valid start index for replacement, perform a search for the next occurrence.
            if (replaceStartIndex == -1)
            {
                Find();
                return;
            }

            // Replace the text in the TextArea by removing the old text and inserting the replacement text.
            TextArea.Text = TextArea.Text.Remove(replaceStartIndex, FindText.Length).Insert(replaceStartIndex, replaceText);

            // Select the replaced text in the TextArea and focus on it.
            TextArea.Select(replaceStartIndex, replaceText.Length);
            TextArea.Focus();

            // Reset the start index for replacement to its default value.
            replaceStartIndex = -1;
        }
    }
}
