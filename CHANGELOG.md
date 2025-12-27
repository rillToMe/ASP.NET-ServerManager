# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned
- Keyboard shortcuts support
- Dark mode theme toggle
- Export/import project configurations
- Custom port configuration
- Environment variables management

---

## [1.1.0] - 2025-12-28

### 🎉 Major Features Added
- **Build Project** - Build ASP.NET projects directly from the app
- **Clean Project** - Clean build artifacts with one click
- **Build State Tracking** - Intelligent detection of project build status
- **Smart Button Management** - Dynamic button states based on project status

### ✨ Enhancements
- Added horizontal scrollbar for responsive button layout
- Improved button spacing and visual hierarchy
- Added build status validation before running servers
- Enhanced console output formatting for build/clean operations
- Added success/failure indicators (✓/✗) for build operations
- Implemented auto-check for existing builds on project import

### 🐛 Bug Fixes
- Fixed UI freeze during build and clean operations (async implementation)
- Fixed button state inconsistency after cleaning projects
- Fixed responsive layout issues on smaller screens
- Improved process tracking reliability

### 🔧 Technical Improvements
- Converted build/clean operations to async/await pattern
- Added proper button disable/enable states during operations
- Enhanced error handling for build processes
- Improved project state persistence
- Added `IsBuilt` property to ServerProject model

### 📝 Changes
- Updated button layout with better spacing
- Enhanced workflow: Build → Run / Clean → Build → Run
- Added build validation prompt when trying to run unbuild projects
- Improved Material Design icon usage

---

## [1.0.0] - 2025-12-27

### 🎉 Initial Release

### ✨ Features
- **Multiple Project Management** - Manage unlimited ASP.NET projects
- **Run Server** - Start development servers with one click
- **Stop Server** - Gracefully stop running servers
- **Live Console Output** - Real-time terminal-style logs
- **Auto URL Detection** - Automatically detect and extract server URLs
- **Visit Web** - Open running servers in default browser
- **Project CRUD Operations**
  - Add new projects
  - Edit project names
  - Edit project paths
  - Delete projects from list
- **Process Tracking** - Track running servers across app restarts
- **PID Management** - Reconnect to background processes
- **Data Persistence** - Auto-save projects and running processes
- **Material Design UI** - Modern, clean interface with Material Design
- **Font Awesome Icons** - Beautiful icon integration
- **About Dialog** - Developer information and social links

### 🎨 UI/UX
- Left sidebar with project list
- Right panel with project details and controls
- Status indicators (Running/Stopped with color coding)
- Empty state messaging
- Responsive layout
- Settings button with gear icon
- Tooltip support on hover

### 🔐 Safety Features
- Warning dialog when closing with running servers
- Confirmation prompts for destructive actions
- Process validation before operations
- Path existence checks

### 💾 Data Management
- Projects stored in `%AppData%/ServerManager/projects.json`
- Running processes tracked in `running_processes.json`
- Automatic state restoration on app launch

### 🛠️ Technical Stack
- WPF (Windows Presentation Foundation)
- .NET 8.0
- Material Design In XAML Toolkit
- Font Awesome Sharp
- C# with async/await patterns
- JSON serialization for data storage

---

## Version Naming Convention

- **Major version** (1.0.0): Breaking changes or major feature overhauls
- **Minor version** (1.1.0): New features and enhancements

## Links

- [GitHub Repository](https://github.com/rillToMe/ASP.NET-ServerManager)
- [Issue Tracker](https://github.com/rillToMe/ASP.NET-ServerManager/issues)
- [Pull Requests](https://github.com/rillToMe/ASP.NET-ServerManager/pulls)

---

**Note**: Replace `2025-12-28` with actual release dates when publishing versions.