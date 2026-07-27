# AWE Chat — Windows Forms Chat Application

## Overview
AWE Chat is a Windows Forms desktop application built in C# (.NET 7) that allows users to enter their name and exchange messages with others through a central TCP server.

## Architecture & Design Principles

This project follows three key design principles (selected for Mastery — Project 2):

### 1. Separation of Concerns (SoC)
The codebase is split into distinct layers — each folder has one responsibility:

```
WinFormsApp1/
├── Forms/
│   ├── LoginForm.cs/.Designer.cs   ← UI layer: Login screen
│   └── ChatForm.cs/.Designer.cs    ← UI layer: Chat screen
├── Services/
│   ├── ChatService.cs              ← Network layer: TCP communication
│   └── UserService.cs              ← Persistence layer: username storage
├── Program.cs                      ← Entry point + backend team's sendMessage interface
Server/
└── TCPServer.cs                    ← Server simulator (provided by backend team, do not modify)
```

### 2. Single Responsibility Principle (SRP)
- `UserService` — only reads/writes the username flat file
- `ChatService` — only handles TCP send/receive
- `LoginForm` — only handles login UI and validation
- `ChatForm` — only handles chat UI and message display

### 3. DRY (Don't Repeat Yourself)
- Validation logic is in one place (`LoginForm.btnLogin_Click`)
- History append logic is in one place (`ChatForm.AppendToHistory`)
- Status update logic is in one place (`ChatForm.SetStatus`)

## Features Implemented

| Feature | Requirement | Status |
|---|---|---|
| Login Form with Name field | FR-01, FR-02 | ✅ |
| Login button validates name (empty = error) | FR-03, NFR-07 | ✅ |
| Opens Chat window on success | FR-03 | ✅ |
| Remember Me — saves name to flat file | FR-04, FR-05 | ✅ |
| Pre-populates name on next launch | FR-05 | ✅ |
| Chat Form with Message input | FR-06 | ✅ |
| Send button (+ Enter key) | FR-06 | ✅ |
| Read-only History field | FR-07 | ✅ |
| TCP server communication | FR-08 | ✅ |
| Error dialog on connection failure | FR-10 | ✅ |
| Async I/O (non-blocking UI) | NFR-05 | ✅ |
| Resource disposal (using statements) | NFR-10 | ✅ |

## Data Persistence
Username is stored in a plain text flat file:
```
%AppData%\AwesomeChat\user.txt
```
Uses atomic write (temp file + rename) to prevent file corruption if the app crashes during a write.

## Getting Started

### Prerequisites
- Windows 10 or higher
- .NET 7 SDK (or Visual Studio 2022)

### Run the Server (required before starting the client)
1. Open `WinFormsApp1.sln` in Visual Studio 2022
2. Right-click the **Server** project → **Set as Startup Project**
3. Press **F5** — you should see: `Started listening requests at: 127.0.0.1:1234`

### Run the Client
1. Right-click the **WinFormsApp1** project → **Set as Startup Project**
2. Press **F5**
3. Enter your name and click **Login**
4. Type a message and click **Send** (or press Enter)

### Run via Command Line
```bash
# Terminal 1 — start the server first
cd Server
dotnet run

# Terminal 2 — start the client
cd WinFormsApp1
dotnet run
```

## Code Review
Pull requests are used for code review. See the open PR on GitHub for the full review history and inline comments.
