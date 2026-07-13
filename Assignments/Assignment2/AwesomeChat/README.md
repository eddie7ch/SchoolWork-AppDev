# AWE Chat — Windows Forms Chat Application

## Overview
AWE Chat is a Windows Forms desktop application built in C# (.NET 10) that lets users enter their name and exchange messages through a central TCP server.

## Architecture & Design Principles

### 1. Separation of Concerns (SoC)
```
WinFormsApp1/
├── Forms/
│   ├── LoginForm.cs/.Designer.cs   ← UI: login screen
│   └── ChatForm.cs/.Designer.cs    ← UI: chat screen
├── Services/
│   ├── ChatService.cs              ← Network: TCP communication
│   └── UserService.cs              ← Persistence: username storage
└── Program.cs                      ← Entry point + Serilog initialisation
Server/
└── TCPServer.cs                    ← Server simulator (starter code, do not modify)
```

### 2. Single Responsibility Principle (SRP)
- `UserService` — only reads/writes the username flat file
- `ChatService` — only handles TCP send/receive + message validation
- `LoginForm` — only handles login UI and validation
- `ChatForm` — only handles chat UI and message display

### 3. DRY (Don't Repeat Yourself)
- Validation in one place (`ChatService.ValidateMessage`)
- History append in one place (`ChatForm.AppendToHistory`)
- Status update in one place (`ChatForm.SetStatus`)

---

## Features

| Feature | Status |
|---|---|
| Login form with name validation | ✅ |
| Remember Me (flat-file persistence) | ✅ |
| Chat form with message history (colour-coded) | ✅ |
| Send on button click or Enter key | ✅ |
| Async TCP (non-blocking UI) | ✅ |
| Message validation (empty / too long) | ✅ |
| Error dialogs for each failure type | ✅ |
| Structured file logging (Serilog) | ✅ |

---

## Installation

### Option A — Pre-built Installer (recommended)
1. Run `Installer/Output/AwesomeChatSetup.exe`
2. Follow the wizard (choose install folder, optional desktop shortcut)
3. The app is self-contained — **no separate .NET runtime required**
4. To uninstall: **Control Panel → Programs → AwesomeChat → Uninstall**

### Option B — Build from Source
```bash
# Prerequisites: .NET 10 SDK, Windows 10+

# 1. Clone repo
git clone https://github.com/eddie7ch/SchoolWork-AppDev.git
cd SchoolWork-AppDev/Assignments/Assignment2/AwesomeChat

# 2. Start the server
cd Server && dotnet run
# Expected: "Started listening requests at: 127.0.0.1:1234"

# 3. Start the client (new terminal)
cd WinFormsApp1 && dotnet run
```

### Rebuild the Installer
```bash
# Publish single-file exe
dotnet publish WinFormsApp1/WinFormsApp1.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o Installer/publish

# Compile installer (requires Inno Setup 6)
"C:\Users\<you>\AppData\Local\Programs\Inno Setup 6\ISCC.exe" Installer/AwesomeChat.iss
# Output: Installer/Output/AwesomeChatSetup.exe
```

---

## User Manual

### First Launch
1. Start the **Server** first (see Build from Source above, or ensure it runs separately)
2. Launch **AwesomeChat** (from the Start Menu, desktop shortcut, or `dotnet run`)
3. Enter your name → click **Login** (or press Enter)
4. Check **Remember Me** to skip the name prompt next time

### Sending Messages
- Type a message in the text box at the bottom of the chat window
- Click **Send** or press **Enter**
- Your messages appear in **blue**; server responses appear in **green**
- Messages over 2,000 characters are rejected with a prompt to shorten them

### Data stored on your machine
| Location | Contents |
|---|---|
| `%AppData%\AwesomeChat\user.txt` | Saved username (Remember Me) |
| `%AppData%\AwesomeChat\logs\app-YYYYMMDD.log` | Application log (daily rolling) |

---

## Library: Serilog

**Package:** `Serilog` 4.4.0 + `Serilog.Sinks.File` 7.0.0  
**Why chosen over `Microsoft.Extensions.Logging`:** Serilog works out of the box with a static `Log` class, requiring no dependency injection setup — ideal for a WinForms app without a DI container.

### Pros
- Fluent, single-line setup (`new LoggerConfiguration().WriteTo.File(...).CreateLogger()`)
- Structured logging — log properties are stored as key-value pairs, not just plain strings, making filtering easier
- Automatic daily rolling log files (`app-20260713.log`, etc.) with zero extra config
- Widely adopted in .NET — extensive documentation, community support, and compatible sinks

### Cons
- Split packages — `Serilog` core and each sink (`Serilog.Sinks.File`, `Serilog.Sinks.Console`) are separate NuGet installs
- Static `Log` class is a global singleton — if two parts of the app configure it differently, they will conflict
- Structured logging benefits are lost if developers use string interpolation (`$"User {name}"`) instead of message templates (`"User {Name}", name`)

### Limitations
- Log files are plain text — **do not log passwords, tokens, or personal data**
- Rolling is time-based (daily), not size-based — a very chatty app can create large single-day files
- No built-in log viewer; files must be opened in a text editor or log analysis tool

---

## Log Analysis (Criterion 2)

Logs are written to `%AppData%\AwesomeChat\logs\app-<date>.log`.

### Sample log output
```
2026-07-13 14:22:01.003 [INF] AwesomeChat starting
2026-07-13 14:22:01.451 [DBG] No saved username found
2026-07-13 14:22:09.112 [INF] User Eddie logged in
2026-07-13 14:22:09.118 [DBG] Username saved: Eddie
2026-07-13 14:22:15.874 [DBG] Sending message from Eddie (12 chars)
2026-07-13 14:22:15.931 [INF] Message delivered; server response 2097152 bytes
2026-07-13 14:22:31.004 [ERR] TCP connection failed — server may be offline
System.Net.Sockets.SocketException (10061): No connection could be made because the target machine actively refused it.
   at System.Net.Sockets.TcpClient.Connect(String hostname, Int32 port)
   at WinFormsApp1.Services.ChatService.SendOverTcp(Byte[] messageBytes)
2026-07-13 14:22:41.217 [INF] AwesomeChat shutting down
```

### Root cause analysis example
If a user reports "messages aren't sending", search the log for `[ERR]`:

- **`SocketException (10061)`** — *connection actively refused* → the server process is not running on port 1234. Fix: start the Server project.
- **`SocketException (10060)`** — *connection timed out* → a firewall is blocking port 1234. Fix: allow inbound TCP on port 1234 in Windows Firewall.
- **`TimeoutException`** — server accepted the connection but never replied → server is running but hung. Fix: restart the server.

The structured log format (`{Sender}`, `{Length}`, `{Timeout}`) lets you correlate exactly which user triggered an error and when, without reading through raw text.

---

## Code Review
Pull requests are used for code review. See the open PR on GitHub for the full review history and inline comments.

