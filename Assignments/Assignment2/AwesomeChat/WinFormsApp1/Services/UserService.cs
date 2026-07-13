using System;
using System.IO;
using Serilog;

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
            Log.Debug("Username saved: {Username}", username.Trim());
        }

        public string LoadUsername()
        {
            if (!File.Exists(_userFilePath))
            {
                Log.Debug("No saved username found");
                return string.Empty;
            }

            try
            {
                string name = File.ReadAllText(_userFilePath).Trim();
                Log.Debug("Loaded saved username: {Username}", name);
                return name;
            }
            catch (IOException ex)
            {
                Log.Warning(ex, "Failed to read username file");
                return string.Empty;
            }
        }

        public void ClearUsername()
        {
            if (File.Exists(_userFilePath))
            {
                File.Delete(_userFilePath);
                Log.Debug("Saved username cleared");
            }
        }
    }
}
