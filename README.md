# Multi Telegram Manager - Unlimited Telegram Instances

**Professional Windows application** for running unlimited Telegram instances with complete session isolation and automatic state management.

## 🚀 Key Features

- ✅ **Native Windows Application** (C# WinForms + WebView2)
- ✅ **Complete Session Isolation** - Each instance is completely independent
- ✅ **Automatic State Saving** - Sessions persist between app restarts
- ✅ **Smart Instance Management** - Save, restore, and organize your accounts
- ✅ **Advanced Device Bypass** - Unique sessions and User-Agent per instance
- ✅ **Unlimited Accounts** - No restrictions on number of Telegram accounts
- ✅ **Beautiful Modern UI** - Professional interface with statistics
- ✅ **System Tray Support** - Minimize to tray, background operation
- ✅ **Hotkeys Support** (Ctrl+N for new, Ctrl+R for restore)
- ✅ **Memory Optimization** - Smart resource management
- ✅ **Window Position Memory** - Each instance remembers its position
- ✅ **Context Menus** - Right-click management for each instance

## 📋 System Requirements

- **Windows 10/11** (Required)
- **.NET 6.0 Runtime** or newer
- **Microsoft Edge WebView2** (Usually pre-installed)
- **4+ GB RAM** (Recommended 8GB for multiple instances)
- **Internet Connection**

## 🔧 Quick Start

### Option 1: Ready-to-Use Executable (Recommended)
1. Download **MultiTelegram.exe** from Releases
2. Run the file (No installation required!)
3. Click "➕ Create New Instance" 
4. Login to your Telegram account
5. Repeat for additional accounts

### Option 2: Build from Source
```bash
git clone https://github.com/tim1363/MiniAppTg
cd MiniAppTg
build_release.bat
```

## 🎮 How to Use

### Creating Instances
1. **Launch MultiTelegram.exe** - Main manager window opens
2. **Click "➕ Create New Instance"** - New Telegram window appears
3. **Login to your account** - Enter phone number and confirmation code
4. **Repeat as needed** - Create unlimited instances

### Managing Sessions
- **🔄 Restore Saved** - Restore all previously created instances
- **Right-click on instance** - Show/Hide/Close individual instances
- **Double-click** - Toggle instance visibility
- **System Tray** - Minimize app to tray for background operation

### 🔥 Hotkeys
- `Ctrl+N` - Create new Telegram instance
- `Ctrl+R` - Restore saved instances
- **Double-click** on instance - Show/Hide window
- **Right-click** on instance - Context menu

## 🔒 Advanced Privacy Features

The application uses cutting-edge techniques for complete detection bypass:

1. **Isolated WebView2 Sessions** - Each instance runs in completely separate environment
2. **Unique User Agents** - Random Chrome/Firefox/Edge identifiers per instance
3. **Persistent Data Folders** - Separate user data in Windows AppData
4. **Random IP Headers** - Simulates connections from different devices
5. **Session Persistence** - Automatic save/restore of all login sessions
6. **Memory Isolation** - Complete separation of browser profiles

## 📊 Session Management

- **Automatic Saving** - All instances are automatically saved
- **Smart Restoration** - Restore sessions with original window positions
- **Data Persistence** - Sessions survive app restarts and system reboots
- **Memory Tracking** - Monitor resource usage per instance
- **Session Statistics** - View data folder sizes and activity

## ⚠️ Important Notes

- This application is for legitimate use only
- Each instance uses ~120MB RAM when active
- Recommended limit: 10-15 instances simultaneously
- First launch may take time for WebView2 initialization
- All session data is stored locally and securely

## 🛠️ For Developers

### Development Setup
```bash
git clone https://github.com/tim1363/MiniAppTg
cd MiniAppTg
dotnet restore MultiTelegram.sln
dotnet run --project MultiTelegram
```

### Project Structure
```
├── MultiTelegram/
│   ├── MainForm.cs           # Main application window
│   ├── TelegramInstance.cs   # Individual Telegram instance
│   ├── Settings.cs           # Configuration and state management
│   ├── Program.cs            # Application entry point
│   └── MultiTelegram.csproj  # Project configuration
├── build_release.bat         # Automated build script
└── README.md                 # This file
```

### Building Release
```bash
build_release.bat              # Windows batch script
# OR manually:
dotnet publish MultiTelegram -c Release -r win-x64 --self-contained true
```

## 📝 Technical Details

- **Platform**: .NET 6.0 Windows Forms
- **Web Engine**: Microsoft Edge WebView2
- **Telegram Interface**: Official web.telegram.org/k/
- **Isolation Method**: Separate UserDataFolder per instance
- **Architecture**: Self-contained Windows executable
- **Data Storage**: Windows AppData folder with JSON configuration

## 🐛 Troubleshooting

### Instance won't start
- Check internet connection
- Ensure web.telegram.org is accessible
- Verify WebView2 is installed (usually automatic)

### High memory usage
- Close unused instances via right-click menu
- Use "❌ Close All" to reset all instances
- Restart application if needed

### Sessions not saving
- Check Windows AppData folder permissions
- Ensure antivirus isn't blocking the application
- Run as administrator if needed

### WebView2 errors
- Install WebView2 Runtime from Microsoft website
- Update Windows to latest version
- Check Windows compatibility

## 🆕 What's New in v1.0

- **Smart State Management** - Automatic session save/restore
- **Enhanced UI** - Modern design with statistics and status indicators
- **System Tray Support** - Background operation capability
- **Memory Optimization** - Better resource management
- **Advanced Instance Control** - Individual window management
- **Persistent Sessions** - Sessions survive restarts
- **Developer Tools** - Built-in debugging support

## 📞 Support

For issues or questions:
1. Check troubleshooting section above
2. Ensure you have the latest version
3. Create an issue on GitHub with error details

---

**Author**: KoTim  
**Version**: 1.0.0  
**License**: MIT  
**Build Date**: 2024

**🎯 Perfect for**: Social Media Managers, Developers, Customer Support Teams, Power Users