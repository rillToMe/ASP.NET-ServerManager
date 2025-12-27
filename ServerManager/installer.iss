#define AppName "ASP.NET Server Manager"
#define AppVersion "1.1.0"
#define AppPublisher "DitDev"
#define AppExeName "ServerManager.exe"

[Setup]
AppId={{E2F4A7D9-9E1D-4A5C-BB11-123456789ABC}}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}

DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}

OutputDir=Output
OutputBaseFilename=ServerManager_Setup_{#AppVersion}
Compression=lzma
SolidCompression=yes

SetupIconFile=Assets\hotaru_icon.ico
WizardStyle=modern

LicenseFile=bin\Release\net8.0-windows\win-x64\publish\Assets\LICENSE.txt

DisableProgramGroupPage=yes
AllowNoIcons=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "bin\Release\net8.0-windows\win-x64\publish\*"; \
    DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; Flags: unchecked

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; \
    Description: "Launch {#AppName}"; \
    Flags: nowait postinstall skipifsilent
