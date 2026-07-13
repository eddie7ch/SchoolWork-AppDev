using System;
using System.IO;
using WinFormsApp1.Services;

namespace AwesomeChat.Tests
{
    /// <summary>
    /// Unit tests for UserService — save, load, clear, and edge cases.
    /// Uses the internal constructor to redirect I/O to a temp folder (no %AppData% touched).
    /// </summary>
    public class UserServiceTests : IDisposable
    {
        // Each test instance gets its own isolated temp folder
        private readonly string _testFolder;
        private readonly UserService _sut; // System Under Test

        public UserServiceTests()
        {
            _testFolder = Path.Combine(Path.GetTempPath(), $"AwesomeChatTests_{Guid.NewGuid()}");
            // DCR: inject the temp path instead of relying on %AppData%
            _sut = new UserService(_testFolder);
        }

        // ── Save Username ──────────────────────────────────────────────────────

        [Fact]
        public void SaveUsername_ValidName_CreatesFile()
        {
            // Act
            _sut.SaveUsername("Eddie");

            // Assert — file must exist after save
            string expectedPath = Path.Combine(_testFolder, "user.txt");
            Assert.True(File.Exists(expectedPath), "user.txt should exist after SaveUsername");
        }

        [Fact]
        public void SaveUsername_ValidName_FileContainsCorrectName()
        {
            // Arrange
            string username = "Sohaib";

            // Act
            _sut.SaveUsername(username);
            string saved = _sut.LoadUsername();

            // Assert
            Assert.Equal(username, saved);
        }

        [Fact]
        public void SaveUsername_TrimsWhitespace_BeforeSaving()
        {
            // Arrange — name with leading/trailing spaces
            _sut.SaveUsername("  Alice  ");

            // Assert — stored value should be trimmed
            Assert.Equal("Alice", _sut.LoadUsername());
        }

        [Fact]
        public void SaveUsername_CalledTwice_OverwritesPreviousName()
        {
            // Arrange
            _sut.SaveUsername("FirstName");

            // Act — second save should overwrite
            _sut.SaveUsername("SecondName");

            // Assert
            Assert.Equal("SecondName", _sut.LoadUsername());
        }

        [Fact]
        public void SaveUsername_SpecialCharacters_SavedCorrectly()
        {
            // Arrange — names with spaces and accents are valid
            string username = "José O'Brien";

            // Act
            _sut.SaveUsername(username);

            // Assert
            Assert.Equal(username, _sut.LoadUsername());
        }

        // ── Load Username ──────────────────────────────────────────────────────

        [Fact]
        public void LoadUsername_WhenNoFileExists_ReturnsEmptyString()
        {
            // Arrange — fresh UserService with no saved data

            // Act
            string result = _sut.LoadUsername();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void LoadUsername_AfterSave_ReturnsSavedName()
        {
            // Arrange
            string expected = "TestUser";
            _sut.SaveUsername(expected);

            // Act
            string result = _sut.LoadUsername();

            // Assert
            Assert.Equal(expected, result);
        }

        // ── Clear Username ─────────────────────────────────────────────────────

        [Fact]
        public void ClearUsername_AfterSave_LoadUsernameReturnsEmpty()
        {
            // Arrange
            _sut.SaveUsername("Eddie");

            // Act
            _sut.ClearUsername();

            // Assert
            Assert.Equal(string.Empty, _sut.LoadUsername());
        }

        [Fact]
        public void ClearUsername_WhenNoFileExists_DoesNotThrow()
        {
            // Act & Assert — clearing when nothing saved should not throw
            var ex = Record.Exception(() => _sut.ClearUsername());
            Assert.Null(ex);
        }

        [Fact]
        public void ClearUsername_CalledTwice_DoesNotThrow()
        {
            // Arrange
            _sut.SaveUsername("Eddie");
            _sut.ClearUsername();

            // Act & Assert — second clear should be a no-op
            var ex = Record.Exception(() => _sut.ClearUsername());
            Assert.Null(ex);
        }

        // ── Cleanup ────────────────────────────────────────────────────────────

        public void Dispose()
        {
            // Delete the temp test folder after each test
            if (Directory.Exists(_testFolder))
                Directory.Delete(_testFolder, recursive: true);
        }
    }
}
