using System;
using System.IO;

namespace WinFormsApp1.Services
{
    /// <summary>
    /// Handles persistence of the username using a local flat file.
    /// Implements the "Remember Me" feature as per the requirements document.
    /// Design: Single Responsibility — this class only manages username storage.
    /// </summary>
    internal class UserService
    {
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AwesomeChat");

        private static readonly string UserFilePath = Path.Combine(AppDataFolder, "user.txt");

        /// <summary>
        /// Saves the username to a flat file for future pre-population.
        /// Uses atomic write (temp file + rename) to prevent file corruption on crash.
        /// </summary>
        public void SaveUsername(string username)
        {
            Directory.CreateDirectory(AppDataFolder);

            // Atomic write: write to temp, then rename — prevents corruption if app crashes mid-write
            string tempPath = UserFilePath + ".tmp";
            File.WriteAllText(tempPath, username.Trim());
            File.Move(tempPath, UserFilePath, overwrite: true);
        }

        /// <summary>
        /// Loads the previously saved username.
        /// Returns an empty string if no username has been saved.
        /// </summary>
        public string LoadUsername()
        {
            if (!File.Exists(UserFilePath))
                return string.Empty;

            try
            {
                return File.ReadAllText(UserFilePath).Trim();
            }
            catch (IOException)
            {
                // If the file cannot be read, return empty — app continues without pre-population
                return string.Empty;
            }
        }

        /// <summary>
        /// Deletes the saved username file when "Remember Me" is unchecked.
        /// </summary>
        public void ClearUsername()
        {
            if (File.Exists(UserFilePath))
                File.Delete(UserFilePath);
        }
    }
}
