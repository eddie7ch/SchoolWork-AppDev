using System;
using System.IO;

namespace WinFormsApp1.Services
{
    /// <summary>
    /// Handles persistence of the username using a local flat file.
    /// Implements the "Remember Me" feature. Design: Single Responsibility — storage only.
    /// </summary>
    internal class UserService
    {
        private readonly string _appDataFolder;
        private readonly string _userFilePath;

        // Default: %AppData%\AwesomeChat
        public UserService()
            : this(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AwesomeChat"))
        { }

        // Testable: inject a custom folder so unit tests avoid touching %AppData%
        internal UserService(string storageFolder)
        {
            _appDataFolder = storageFolder;
            _userFilePath = Path.Combine(_appDataFolder, "user.txt");
        }

        public void SaveUsername(string username)
        {
            Directory.CreateDirectory(_appDataFolder);
            // Atomic write — prevents corruption if the app crashes mid-write
            string tempPath = _userFilePath + ".tmp";
            File.WriteAllText(tempPath, username.Trim());
            File.Move(tempPath, _userFilePath, overwrite: true);
        }

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
                return string.Empty;
            }
        }

        public void ClearUsername()
        {
            if (File.Exists(_userFilePath))
                File.Delete(_userFilePath);
        }
    }
}
