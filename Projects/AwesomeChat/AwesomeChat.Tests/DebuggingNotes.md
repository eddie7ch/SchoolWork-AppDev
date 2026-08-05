# Debugging Notes — AwesomeChat (Project 4)

**Project:** AwesomeChat (SODV2452-01 Application Development — Project 4)  
**Author:** Eddie Chongtham  
**Purpose:** Documents debugging activities performed using VS Code / Visual Studio tools.  
This satisfies Project 4's debugging/logging evidence (set breakpoints, step into code, observe variables).

---

## Debugging Session 1 — UserService Atomic Write Verification

### Goal
Verify that `SaveUsername` uses an atomic write (write to `.tmp` → rename) and that `LoadUsername` reads back the correct trimmed value.

### Breakpoints Set
| File | Line | Purpose |
|---|---|---|
| `UserService.cs` | `File.WriteAllText(tempPath, ...)` | Observe `tempPath` and `username.Trim()` value before write |
| `UserService.cs` | `File.Move(tempPath, _userFilePath, ...)` | Confirm rename happens after temp write |
| `UserService.cs` | `return File.ReadAllText(_userFilePath).Trim()` | Inspect raw file content before trim |

### Execution Steps (F10 / Step Over)
1. Set breakpoint at `File.WriteAllText`.
2. Called `SaveUsername("  Eddie  ")` from LoginForm.
3. Observed: `username.Trim()` evaluated to `"Eddie"` (whitespace stripped before write).
4. Stepped to `File.Move` — confirmed `tempPath` ends in `.tmp`.
5. Stepped through `LoadUsername` — observed `File.ReadAllText` returns `"Eddie"`.

### Variables Observed
```
username        = "  Eddie  "
username.Trim() = "Eddie"
tempPath        = "C:\Users\eddie\AppData\Roaming\AwesomeChat\user.txt.tmp"
_userFilePath   = "C:\Users\eddie\AppData\Roaming\AwesomeChat\user.txt"
```

### Finding
The atomic write works correctly. The `.tmp` file is created and then atomically renamed. If the app crashed between write and rename, the original `user.txt` would be unmodified — preventing partial writes from corrupting the saved username.

---

## Debugging Session 2 — ChatService ValidateMessage Boundary

### Goal
Step through `ValidateMessage` to confirm the 2000-character boundary behaves correctly at exactly 2000 and at 2001 characters.

### Breakpoints Set
| File | Line | Purpose |
|---|---|---|
| `ChatService.cs` | `if (string.IsNullOrWhiteSpace(message))` | Confirm empty check hits first |
| `ChatService.cs` | `if (message.Length > MaxMessageLength)` | Observe `message.Length` vs `MaxMessageLength` |
| `ChatService.cs` | `return null;` | Confirm null returned for valid message |

### Execution Steps
1. Set breakpoints in `ValidateMessage`.
2. Called from test runner with 2000-char message.
3. Stepped through — `message.Length == 2000`, `MaxMessageLength == 2000` → condition `> 2000` is false → returned `null` (valid).
4. Called again with 2001-char message.
5. Observed `message.Length == 2001 > 2000` → error string returned containing "2001".

### Variables Observed (2001-char case)
```
message.Length  = 2001
MaxMessageLength = 2000
error           = "Message is too long (2001 characters). Maximum allowed is 2000."
```

### Finding
Boundary is exclusive of 2001 (i.e., 2000 is allowed, 2001 is not). This matches the unit test `ValidateMessage_ExactlyAtMaxLength_ReturnsNull` and `ValidateMessage_OneBeyondMaxLength_ReturnsError`.

---

## Debugging Session 3 — ChatForm Error Handling Flow

### Goal
Verify that `ArgumentException` from `ChatService.SendMessageAsync` surfaces correctly in `ChatForm` and shows the right dialog.

### Breakpoints Set
| File | Line | Purpose |
|---|---|---|
| `ChatForm.cs` | `string? validationError = ValidateMessage(message)` (inside `SendMessageAsync`) | Observe error string |
| `ChatForm.cs` | `catch (ArgumentException ex)` | Confirm correct catch block is hit |
| `ChatForm.cs` | `SetStatus("Message not sent — validation failed.", ...)` | Confirm UI status updated |

### Execution Steps
1. Started app with server running.
2. Typed a 2001-character message into the chat text box.
3. Clicked Send.
4. Observed breakpoint at `ValidateMessage` — `validationError` contained the too-long message.
5. Execution jumped to `throw new ArgumentException(validationError, ...)`.
6. Caught by `catch (ArgumentException ex)` in `ChatForm.SendMessageAsync`.
7. `SetStatus` called with red colour — label updated in UI.
8. `MessageBox.Show` triggered with "Invalid Message" title.

### Variables Observed
```
message.Length   = 2001
validationError  = "Message is too long (2001 characters). Maximum allowed is 2000."
ex.Message       = "Message is too long (2001 characters). Maximum allowed is 2000."
ex.ParamName     = "message"
```

### Finding
The full error handling pipeline works end-to-end. The message is rejected before any TCP connection is attempted, saving network resources and giving the user a clear explanation.

---

## Debugging Session 4 — LoginForm Remember Me Flow

### Goal
Trace the full login flow — from form open, to pre-population of a saved name, through to the ChatForm opening — to confirm the Remember Me feature works end-to-end.

### Breakpoints Set
| File | Line | Purpose |
|---|---|---|
| `LoginForm.cs` | `string savedName = _userService.LoadUsername()` | Observe whether a saved name is returned |
| `LoginForm.cs` | `txtName.Text = savedName` | Confirm pre-population happens |
| `LoginForm.cs` | `string username = txtName.Text.Trim()` | Observe the trimmed name on login click |
| `LoginForm.cs` | `_userService.SaveUsername(username)` | Confirm save is called when Remember Me is checked |
| `LoginForm.cs` | `var chatForm = new ChatForm(username)` | Confirm correct username passed to ChatForm |

### Execution Steps
1. Saved a username ("Eddie") in a previous run so `user.txt` exists.
2. Launched app — breakpoint hit at `LoadUsername()` → returned `"Eddie"`.
3. Stepped through: `txtName.Text` set to `"Eddie"`, `chkRememberMe.Checked` set to `true`.
4. Clicked Login with Remember Me checked — breakpoint hit at `SaveUsername("Eddie")`.
5. Stepped to `new ChatForm("Eddie")` — confirmed `username` was `"Eddie"` (not empty, not with whitespace).
6. ChatForm opened with `lblUserInfo` showing `"Logged in as: Eddie"`.

### Variables Observed
```
savedName     = "Eddie"
username      = "Eddie"
chkRememberMe.Checked = true
```

### Finding
The Remember Me feature works correctly end-to-end. Loading, pre-population, and re-saving all happen in the expected order. The `ChatForm` receives the exact trimmed username from `LoginForm`, confirming no data is lost between forms.

---

## Tools Used

| Tool | Usage |
|---|---|
| VS Code Debugger (F5) | Launch AwesomeChat with debugger attached |
| Breakpoints (F9) | Pause execution at specific lines |
| Step Over (F10) | Move to next statement without entering called methods |
| Step Into (F11) | Enter method body to trace internal execution |
| Watch Window | Monitor variable values (`_userFilePath`, `message.Length`, `validationError`, `username`) |
| Locals Panel | Inspect all local variables at current stack frame |
| Call Stack | Confirm execution path from `btnSend_Click` → `SendMessageAsync` → `ChatService` |
| dotnet test output | Verify all 21 unit tests pass with 0 failures |
