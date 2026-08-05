# DCR — Design Change Request: Testability Improvements

**Project:** AwesomeChat (SODV2452-01 Application Development — Project 4)  
**Author:** Eddie Chongtham  
**Date:** July 2026  
**Branch:** `feature/project3-testing`

---

## Summary

Two design changes were made to the existing codebase to improve testability while keeping production behaviour identical. Neither change alters what the app does from the user's perspective — only *how* the code is internally structured.

---

## DCR-1 — UserService: Injectable Storage Path

### Problem

`UserService` used `static readonly` fields to compute the data path at class-load time:

```csharp
// Before — path baked in at class load, impossible to redirect in tests
private static readonly string AppDataFolder = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "AwesomeChat");
```

This meant unit tests couldn't control where files were written — every test would read/write the real `%AppData%\AwesomeChat\user.txt`, causing:
- Tests interfering with each other
- Tests dirtying the developer's actual saved username
- No way to run tests in CI without side effects

### Change

Added a secondary `internal` constructor that accepts a custom folder path. The default constructor still resolves to `%AppData%\AwesomeChat` — production code is unchanged.

```csharp
// After — production default preserved; test can inject a temp path
public UserService()
    : this(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AwesomeChat"))
{ }

internal UserService(string storageFolder)   // ← new, test-only entry point
{
    _appDataFolder = storageFolder;
    _userFilePath  = Path.Combine(_appDataFolder, "user.txt");
}
```

### How Tests Use It

```csharp
// Each test creates its own isolated temp folder
_testFolder = Path.Combine(Path.GetTempPath(), $"AwesomeChatTests_{Guid.NewGuid()}");
_sut = new UserService(_testFolder);   // ← inject; no %AppData% touched
```

The `IDisposable.Dispose()` method deletes the temp folder after each test, keeping the machine clean.

---

## DCR-2 — ChatService: Extracted Pure Helper Methods

### Problem

The message wire format and validation rules were buried inside `SendMessageAsync`:

```csharp
// Before — formatting is an inline string interpolation, not testable
string fullMessage = $"{senderName}: {message}";
byte[] messageBytes = Encoding.Unicode.GetBytes(fullMessage);
```

There was no validation of message content before a TCP connection was opened, meaning:
- Empty messages would silently open a connection to the server
- Messages over any length would silently succeed (up to buffer limits)
- Tests could not verify the "Name: message" wire format without a live server

### Change

Two `internal static` methods were extracted — they are pure functions (no I/O, no side effects), so tests can call them directly without a server:

```csharp
internal static string FormatMessage(string senderName, string message)
    => $"{senderName}: {message}";

internal static string? ValidateMessage(string message)
{
    if (string.IsNullOrWhiteSpace(message))
        return "Message cannot be empty.";
    if (message.Length > MaxMessageLength)
        return $"Message is too long ({message.Length} characters). Maximum allowed is {MaxMessageLength}.";
    return null; // null → valid
}
```

`SendMessageAsync` now calls both helpers before opening the socket:

```csharp
string? validationError = ValidateMessage(message);
if (validationError != null)
    throw new ArgumentException(validationError, nameof(message));

string fullMessage = FormatMessage(senderName, message);
```

`ChatForm` was also updated to catch `ArgumentException` and show a user-friendly dialog.

### Before / After: What the Tests Can Now Assert

| Scenario | Before DCR | After DCR |
|---|---|---|
| Wire format "Name: msg" | Not testable without TCP | Directly testable via `FormatMessage` |
| Empty message rejected | No validation — silently sent | `ValidateMessage` returns error |
| Message > 2000 chars rejected | No validation — silently sent | `ValidateMessage` returns error |
| Boundary: exactly 2000 chars | Not validated | Accepted correctly |

---

## Test Results

Run via: `dotnet test --collect:"XPlat Code Coverage"`

| Test Class | Tests | Passed | Failed |
|---|---|---|---|
| `UserServiceTests` | 9 | 9 | 0 |
| `ChatServiceTests` | 16 | 16 | 0 |
| **Total** | **25** | **25** | **0** |

### Code Coverage (WinFormsApp1)

| Class | Line Coverage |
|---|---|
| `UserService` | ~78% |
| `ChatService` (pure methods + validation path) | ~53% overall |

> Overall: **53% line / 64% branch** — within the 50–60% mastery target.
> `SendOverTcp` is excluded from unit coverage because it requires a live TCP connection. It is covered by manual integration testing (see `DebuggingNotes.md`).

---

## Impact Assessment

| Area | Risk | Mitigation |
|---|---|---|
| Production `UserService` | None — default constructor unchanged | Existing `LoginForm` code requires no edits |
| `ChatService.SendMessageAsync` | Low — now throws `ArgumentException` for bad input | `ChatForm` updated to catch and display error |
| `ChatForm` UI flow | Low — extra catch clause added | Empty-message guard still present before service call |
