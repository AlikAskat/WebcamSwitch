#define MyAppName "WebcamSwitch"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "WinUnbury"
#define MyAppExeName "WebcamSwitch.exe"

[Setup]
AppId={{8B5F6E0D-7D9E-4D4D-A7D5-9F0D8E5A6C21}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\WinUnbury\WebcamSwitch
DefaultGroupName=WinUnbury
OutputDir=.
OutputBaseFilename=WebcamSwitch-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\WebcamSwitch.exe

[Files]
Source: "..\READY\WebcamSwitch.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "WebcamSwitch-Task.ps1"; DestDir: "{app}"; Flags: ignoreversion
Source: "InstallTask.ps1"; DestDir: "{app}"; Flags: ignoreversion
Source: "UninstallTask.ps1"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\WebcamSwitch"; Filename: "{app}\WebcamSwitch.exe"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "WebcamSwitch"; ValueData: """{app}\WebcamSwitch.exe"""; Flags: uninsdeletevalue

[Run]
Filename: "powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\InstallTask.ps1"""; Flags: runhidden waituntilterminated

Filename: "{app}\{#MyAppExeName}"; Description: "Launch WebcamSwitch"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\UninstallTask.ps1"""; Flags: runhidden waituntilterminated
