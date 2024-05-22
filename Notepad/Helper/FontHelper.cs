using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Notepad.Helper
{
    /// <summary>
    /// Helper class for managing fonts.
    /// </summary>
    public static class FontHelper
    {
        /// <summary>
        /// Checks if a font with the specified font family name is installed on the system.
        /// </summary>
        /// <param name="fontFamilyName">The name of the font family to check.</param>
        /// <returns>True if the font is installed, otherwise false.</returns>
        public static bool IsFontInstalled(string fontFamilyName)
        {
            // Initialize an InstalledFontCollection to access the installed fonts.
            InstalledFontCollection installedFontCollection = new InstalledFontCollection();

            // Check if the installedFontCollection object is not null and if it contains any font families.
            if (installedFontCollection != null && installedFontCollection.Families != null)
            {
                // Check if any font family in the collection has a name that matches the specified font family name.
                return installedFontCollection.Families
                    .Any(fontFamily => fontFamily.Name.Equals(fontFamilyName, StringComparison.OrdinalIgnoreCase));
            }

            // If the installedFontCollection object is null or does not contain any font families, return false.
            return false;
        }

        /// <summary>
        /// Installs a font from the specified source path to the system's font directory.
        /// </summary>
        /// <param name="fontSourcePath">The path to the font file to install.</param>
        public static void InstallFont(string fontSourcePath)
        {
            // Get the type information for the Shell.Application COM object.
            var shellAppType = Type.GetTypeFromProgID("Shell.Application");

            // Create an instance of the Shell.Application COM object.
            var shell = Activator.CreateInstance(shellAppType);

            // Get the font folder from the system's font directory.
            var fontFolder = (Shell32.Folder)shellAppType.InvokeMember("NameSpace", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { Environment.GetFolderPath(Environment.SpecialFolder.Fonts) });

            // Check if the font source file exists.
            if (File.Exists(fontSourcePath))
            {
                // Copy the font file to the system's font directory.
                fontFolder.CopyHere(fontSourcePath);
            }
        }

        /// <summary>
        /// Extracts a font file from embedded resources and saves it to a temporary location.
        /// </summary>
        /// <param name="resourceName">The name of the embedded resource containing the font file.</param>
        /// <returns>The file path of the extracted font file.</returns>
        public static string ExtractFontFromResources(string resourceName)
        {
            // Get the executing assembly.
            Assembly executingAssembly = Assembly.GetExecutingAssembly();

            // Open the embedded resource stream.
            using (Stream resourceStream = executingAssembly.GetManifestResourceStream(resourceName))
            {
                // Check if the resource stream is null.
                if (resourceStream == null)
                {
                    // Throw an exception if the resource is not found.
                    throw new Exception($"Resource '{resourceName}' not found.");
                }

                // Define the file path for the extracted font file in the temporary directory.
                string fontFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Segoe Fluent Icons.ttf");

                // Create a file stream to write the font file.
                using (FileStream fileStream = File.Create(fontFilePath))
                {
                    // Copy the contents of the resource stream to the file stream.
                    resourceStream.CopyTo(fileStream);
                }

                // Return the file path of the extracted font file.
                return fontFilePath;
            }
        }

        /// <summary>
        /// Retrieves the names of all installed fonts on the system.
        /// </summary>
        /// <returns>An array of font names.</returns>
        public static string[] GetInstalledFonts()
        {
            // Create an instance of InstalledFontCollection to access installed fonts.
            InstalledFontCollection installedFontCollection = new InstalledFontCollection();

            // Get an array of FontFamily objects representing installed fonts.
            FontFamily[] fontFamilies = installedFontCollection.Families;

            // Extract font names from FontFamily objects and store them in an array.
            string[] fontNames = fontFamilies.Select(fontFamily => fontFamily.Name).ToArray();

            // Return the array of font names.
            return fontNames;
        }

        /// <summary>
        /// Uninstalls a font by its name.
        /// </summary>
        /// <param name="fontName">The name of the font to uninstall.</param>
        public static void UninstallFontByName(string fontName)
        {
            // Get the path to the Fonts folder.
            string fontsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));

            // Get an array of font files in the Fonts folder with the .ttf extension.
            string[] fontFiles = Directory.GetFiles(fontsFolder, "*.ttf", SearchOption.TopDirectoryOnly);

            // Iterate through each font file.
            foreach (string fontFile in fontFiles)
            {
                // Extract the file name (without extension) from the font file path.
                string fileName = Path.GetFileNameWithoutExtension(fontFile);

                // Check if the font file name matches the specified font name (case-insensitive).
                if (fileName.Equals(fontName, StringComparison.OrdinalIgnoreCase))
                {
                    // Delete the font file.
                    File.Delete(fontFile);

                    // Print a success message to the console.
                    Console.WriteLine($"Font '{fontName}' uninstalled successfully.");
                }
            }

            // Print a message to the console if the font was not found.
            Console.WriteLine($"Font '{fontName}' not found.");
        }

        /// <summary>
        /// Installs a font if it is not already installed.
        /// </summary>
        /// <param name="fontFamilyName">The name of the font family to check/install.</param>
        /// <param name="resourceName">The name of the resource containing the font file.</param>
        public static void InstallFontIfNotInstalled(string fontFamilyName, string resourceName)
        {
            try
            {
                // Check if the font is already installed.
                bool fontInstalled = IsFontInstalled(fontFamilyName);

                // If the font is not installed, prompt the user to install it.
                if (!fontInstalled)
                {
                    MessageBoxResult messageBoxResult = MessageBox.Show($"The font '{fontFamilyName}' is not installed. Installing the font now to continue.", "Font Request", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    // If the user agrees to install the font, proceed with the installation.
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        // Extract the font file from the application's resources.
                        string fontFilePath = ExtractFontFromResources(resourceName);

                        // Install the font.
                        InstallFont(fontFilePath);

                        // Check if the font was installed successfully.
                        fontInstalled = IsFontInstalled(fontFamilyName);
                        if (fontInstalled)
                        {
                            // Display a success message if the font was installed.
                            MessageBox.Show($"Font '{fontFamilyName}' installed successfully.", "Font Installed", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Delete the temporary font file.
                            File.Delete(fontFilePath);
                        }
                    }
                    else
                    {
                        // If the user declines to install the font, shut down the application.
                        Application.Current.Shutdown();
                    }
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs during font installation.
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
