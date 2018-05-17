; Based on Modular InnoSetup Dependency Installer:
; github.com/stfx/innodependencyinstaller
; codeproject.com/Articles/20868/NET-Framework-1-1-2-0-3-5-Installer-for-InnoSetup

#define QuestJSVersion '6.4.4'
#define SetupVersion '644'

[Setup]
AppName=QuestJS
AppVersion={#QuestJSVersion}
AppVerName=Quest {#QuestJSVersion}
AppCopyright=Copyright © 2017-2018 KV, 2013 Alex Warren
VersionInfoVersion={#QuestJSVersion}
AppPublisher=KV
AppPublisherURL=http://github.com/KVOnGit/questjs
AppSupportURL=http://github.com/KVOnGit/questjs/wiki
AppUpdatesURL=http://github.com/KVOnGit/questjs
OutputBaseFilename=questJs_{#SetupVersion}-alpha
DefaultGroupName=QuestJS
DefaultDirName={pf}\QuestJS
UninstallDisplayIcon={app}\QuestCompiler.exe
OutputDir=bin
SourceDir=.
AllowNoIcons=yes
SolidCompression=yes
PrivilegesRequired=admin
ChangesAssociations=yes
MinVersion=6.0
UsePreviousSetupType=no

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\QuestCompiler\bin\Release\*.*"; Excludes: "*.vshost.*,*.pdb,\*.xml"; DestDir: "{app}"; Flags: recursesubdirs
Source: "..\Dependencies\*"; DestDir: "{tmp}"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Icons]
Name: "{group}\QuestJS"; Filename: "{app}\QuestCompiler.exe"
Name: "{commondesktop}\QuestJS"; Filename: "{app}\QuestCompiler.exe"; Tasks: desktopicon; WorkingDir: {app}

[Run]
Filename: "{app}\QuestCompiler.exe"; Description: "Launch QuestJS"; Flags: nowait postinstall skipifsilent


#include "scripts\products.iss"
#include "scripts\products\stringversion.iss"
#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"
#include "scripts\products\dotnetfxversion.iss"
#include "scripts\products\dotnetfx45.iss"

[CustomMessages]
win_sp_title=Windows %1 Service Pack %2

[Code]
function InitializeSetup(): boolean;
begin
	//init windows version
	initwinversion();



	Result := true;
end;
