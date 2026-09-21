#define MyAppName "Panel"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Panel"
#define MyAppExeName "Panel.exe"

[Setup]
AppId={{5A5C2E9E-C34D-4A83-9C39-1434A10B86E8}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Panel
DefaultGroupName=Panel
DisableProgramGroupPage=yes
OutputDir=..\release
OutputBaseFilename=Panel-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
SetupIconFile=..\Panel\Panel.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Shortcuts:"; Flags: unchecked

[Files]
Source: "..\dist\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Panel"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Panel"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Open Panel"; Flags: nowait postinstall skipifsilent
