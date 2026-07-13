using System;
using System.IO;

namespace WinFormsApp1.Services
{
    /// <summary>
    /// Handles persistence of the username using a local flat file.
    /// Implements the "Remember Me" feature as per the requirements document.
    /// Design: Single Responsibility — this class only manages username storage.
    ///
    /// DCR (Design Change Request): Storage folder is now injected via constructor.
    /// This makes the class testable in isolation without touching %AppData%.
    /// Production code continues to use the default parameterless constructor.
    /// </summary>
    internal class UserService
    {
        private readonly string _appDataFolder;
        private readonly string _userFilePath;

        /// <summary>
        /// Production constructor — stores data in %AppData%\AwesomeChat.
        /// </summary>
        public UserService()
            : this(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AwesomeChat"))
        { }

        /// <summary>
        /// Testable constructor — accepts a custom storage folder.
        /// Used by unit tests to redirect I/O to a temporary directory.
        /// </summary>
        internal UserService(string storageFolder)
        {
            _appDataFolder = storageFolder;
            _userFilePath = Path.Combine(_appDataFolder, "user.txt");
        }

        /// <summary>
        /// Saves the username to a flat file for future pre-population.
        /// Uses atomic write (temp file + rename) to prevent file corruption on crash.
        /// </summary>
        public void SaveUsername(string username)
        {
            Directory.CreateDirectory(_appDataFolder);

            // Atomic write: write to temp, then rename — prevents corruption if app crashes mid-write
            string tempPath = _userFilePath + ".tmp";
            File.WriteAllText(tempPath, username.Trim());
            File.Move(tempPath, _userFilePath, overwrite: true);
        }

        /// <summary>
        /// Loads the previously saved username.
        /// Returns an empty string if no username has been saved.
        /// </summary>
        public string LoadUsername()
        {
            if (!File.Exists(_userFilePath))
                return string.Empty;

            try
            {
                return File.ReadAllText(_userFilePath).Trim();
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
            if (File.Exists(_userFilePath))
                File.Delete(_userFilePath);
        }
    }
}
