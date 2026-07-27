using System;
using WinFormsApp1.Services;

namespace AwesomeChat.Tests
{
    /// <summary>
    /// Unit tests for ChatService.FormatMessage and ChatService.ValidateMessage.
    /// SendMessageAsync (TCP) is covered by manual integration testing in DebuggingNotes.md.
    /// </summary>
    public class ChatServiceTests
    {
        // ── FormatMessage ──────────────────────────────────────────────────────

        [Fact]
        public void FormatMessage_ValidInputs_ReturnsCorrectFormat()
        {
            // Arrange
            string sender = "Eddie";
            string message = "Hello!";

            // Act
            string result = ChatService.FormatMessage(sender, message);

            // Assert — must follow the "Name: message" wire format
            Assert.Equal("Eddie: Hello!", result);
        }

        [Fact]
        public void FormatMessage_NameWithSpaces_FormatsCorrectly()
        {
            // Arrange
            string sender = "John Smith";
            string message = "How's it going?";

            // Act
            string result = ChatService.FormatMessage(sender, message);

            // Assert
            Assert.Equal("John Smith: How's it going?", result);
        }

        [Fact]
        public void FormatMessage_LongMessage_IncludedInFull()
        {
            // Arrange
            string sender = "Alice";
            string message = new string('a', 1000); // 1000-character message

            // Act
            string result = ChatService.FormatMessage(sender, message);

            // Assert — full message must be preserved, not truncated
            Assert.StartsWith("Alice: ", result);
            Assert.Equal(sender.Length + 2 + message.Length, result.Length); // "Alice: " = +2 for ": "
        }

        // ── ValidateMessage — null / empty ─────────────────────────────────────

        [Fact]
        public void ValidateMessage_NullMessage_ReturnsError()
        {
            // Act
            string? error = ChatService.ValidateMessage(null!);

            // Assert — null should fail validation
            Assert.NotNull(error);
        }

        [Fact]
        public void ValidateMessage_EmptyString_ReturnsError()
        {
            // Act
            string? error = ChatService.ValidateMessage(string.Empty);

            // Assert
            Assert.NotNull(error);
        }

        [Fact]
        public void ValidateMessage_WhitespaceOnly_ReturnsError()
        {
            // Arrange — spaces and tabs are not a meaningful message
            string message = "   \t  ";

            // Act
            string? error = ChatService.ValidateMessage(message);

            // Assert
            Assert.NotNull(error);
        }

        // ── ValidateMessage — valid ────────────────────────────────────────────

        [Fact]
        public void ValidateMessage_NormalMessage_ReturnsNull()
        {
            // Arrange
            string message = "Hello, everyone!";

            // Act
            string? error = ChatService.ValidateMessage(message);

            // Assert — null means valid
            Assert.Null(error);
        }

        [Fact]
        public void ValidateMessage_SingleCharacter_ReturnsNull()
        {
            // Arrange — a single non-whitespace character is a valid message
            string message = "?";

            // Act
            string? error = ChatService.ValidateMessage(message);

            // Assert
            Assert.Null(error);
        }

        [Fact]
        public void ValidateMessage_ExactlyAtMaxLength_ReturnsNull()
        {
            // Arrange — boundary test: exactly 2000 characters should be valid
            string message = new string('x', ChatService.MaxMessageLength);

            // Act
            string? error = ChatService.ValidateMessage(message);

            // Assert
            Assert.Null(error);
        }

        // ── ValidateMessage — too long (DCR boundary) ─────────────────────────

        [Fact]
        public void ValidateMessage_OneBeyondMaxLength_ReturnsError()
        {
            // Arrange — boundary test: 2001 characters should fail
            string message = new string('x', ChatService.MaxMessageLength + 1);

            // Act
            string? error = ChatService.ValidateMessage(message);

            // Assert
            Assert.NotNull(error);
        }

        [Fact]
        public void ValidateMessage_FarOverMaxLength_ReturnsErrorMentioningLength()
        {
            // Arrange
            string message = new string('x', 5000);

            // Act
            string? error = ChatService.ValidateMessage(message);

            // Assert — error message should mention "5000" or "too long"
            Assert.NotNull(error);
            Assert.Contains("5000", error!);
        }

        // ── SendMessageAsync — validation rejects before TCP ──────────────────
        // These tests exercise the early-exit path in SendMessageAsync without
        // needing a live server — validation throws ArgumentException first.

        [Fact]
        public async Task SendMessageAsync_EmptyMessage_ThrowsArgumentException()
        {
            // Arrange
            var svc = new ChatService();

            // Act & Assert — validation fires before any TCP connection
            await Assert.ThrowsAsync<ArgumentException>(
                () => svc.SendMessageAsync("Eddie", string.Empty));
        }

        [Fact]
        public async Task SendMessageAsync_WhitespaceMessage_ThrowsArgumentException()
        {
            // Arrange
            var svc = new ChatService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => svc.SendMessageAsync("Eddie", "   "));
        }

        [Fact]
        public async Task SendMessageAsync_NullMessage_ThrowsArgumentException()
        {
            // Arrange
            var svc = new ChatService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => svc.SendMessageAsync("Eddie", null!));
        }

        [Fact]
        public async Task SendMessageAsync_TooLongMessage_ThrowsArgumentException()
        {
            // Arrange — message exceeds MaxMessageLength
            var svc = new ChatService();
            string tooLong = new string('x', ChatService.MaxMessageLength + 1);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => svc.SendMessageAsync("Eddie", tooLong));
        }
    }
}
