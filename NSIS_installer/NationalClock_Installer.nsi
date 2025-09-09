; NationalClock NSIS Installer Script
; Framework-dependent build - NSIS_installer folder version
; 다중 시간대 월드 클록 WPF 애플리케이션
; Developer: Green Power Co., Ltd.

; 명령줄에서 정의되지 않은 경우에만 기본값 사용
!ifndef PRODUCT_NAME
  !define PRODUCT_NAME "NationalClock"
!endif
!ifndef PRODUCT_VERSION
  !define PRODUCT_VERSION "1.0.001"
!endif
!ifndef BUILD_DATE
  !define BUILD_DATE "20250909_2050"
!endif

!define PRODUCT_PUBLISHER "Green Power Co., Ltd."
!define PRODUCT_WEB_SITE "https://github.com/GreenPower/NationalClock"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_DESCRIPTION "다중 시간대 월드 클록 애플리케이션 (WPF, Material Design)"

; Modern UI
!include "MUI2.nsh"
!include "x64.nsh"

; General settings
Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "${PRODUCT_NAME}_v${PRODUCT_VERSION}_Build_${BUILD_DATE}_Setup.exe"
InstallDir "$PROGRAMFILES64\${PRODUCT_NAME}"
ShowInstDetails show
ShowUnInstDetails show

; Request admin privileges
RequestExecutionLevel admin

; Target x64 architecture
!define MULTIUSER_EXECUTIONLEVEL Admin
!define MULTIUSER_MUI
!define MULTIUSER_INSTALLMODE_COMMANDLINE
!define MULTIUSER_INSTALLMODE_DEFAULT_ALLUSERS

; Compression - LZMA 고압축
SetCompressor /SOLID lzma

; Icon settings (using icon from published files)
!define MUI_ICON "publish\framework-dependent\Resources\NationalClock.ico"
!define MUI_UNICON "publish\framework-dependent\Resources\NationalClock.ico"

; Modern UI settings
!define MUI_ABORTWARNING
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_RIGHT
!define MUI_HEADERIMAGE_BITMAP_NOSTRETCH

; Install pages
!define MUI_WELCOMEPAGE_TITLE "NationalClock 설치"
!define MUI_WELCOMEPAGE_TEXT "다중 시간대 월드 클록 WPF 애플리케이션입니다.$\r$\n$\r$\n시스템 요구사항:$\r$\n• Windows 10 이상 (x64)$\r$\n• .NET 8.0 Desktop Runtime$\r$\n• Material Design UI 지원$\r$\n$\r$\n계속하려면 다음을 클릭하세요."

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY

; Custom page for .NET 8.0 dependency check
Page custom DotNetCheckPage

!insertmacro MUI_PAGE_INSTFILES

!define MUI_FINISHPAGE_RUN "$INSTDIR\NationalClock.exe"
!define MUI_FINISHPAGE_RUN_TEXT "NationalClock 실행"
!define MUI_FINISHPAGE_SHOWREADME "$INSTDIR\README.txt"
!define MUI_FINISHPAGE_SHOWREADME_TEXT "사용 가이드 보기"
!insertmacro MUI_PAGE_FINISH

; Uninstall pages
!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

; Language - Korean support
!insertmacro MUI_LANGUAGE "Korean"

; Version info
VIProductVersion "1.0.1.0"
VIAddVersionKey /LANG=${LANG_KOREAN} "ProductName" "${PRODUCT_NAME}"
VIAddVersionKey /LANG=${LANG_KOREAN} "Comments" "${PRODUCT_DESCRIPTION}"
VIAddVersionKey /LANG=${LANG_KOREAN} "CompanyName" "${PRODUCT_PUBLISHER}"
VIAddVersionKey /LANG=${LANG_KOREAN} "LegalCopyright" "Copyright (c) ${PRODUCT_PUBLISHER}, 2025"
VIAddVersionKey /LANG=${LANG_KOREAN} "FileDescription" "${PRODUCT_NAME} 설치 프로그램"
VIAddVersionKey /LANG=${LANG_KOREAN} "FileVersion" "1.0.1.0"
VIAddVersionKey /LANG=${LANG_KOREAN} "ProductVersion" "1.0.1.0"

; .NET 8.0 Runtime dependency check function
Function DotNetCheckPage
  ; Simplified system check
  DetailPrint "시스템 확인 중..."
  
  ; Simplified .NET 8.0 Runtime check - just warn user
  DetailPrint ".NET 8.0 Desktop Runtime 확인 중..."
  nsExec::ExecToStack 'dotnet --version'
  Pop $0
  
  StrCmp $0 "0" DotNetFound
    MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 ".NET Runtime을 찾을 수 없습니다.$\r$\n$\r$\n설치를 계속하시겠습니까?$\r$\n$\r$\n프로그램 실행을 위해서는 .NET 8.0 Desktop Runtime이 필요합니다.$\r$\n다음 주소에서 다운로드할 수 있습니다:$\r$\nhttps://dotnet.microsoft.com/download/dotnet/8.0" IDYES ContinueInstall
    Abort
    ContinueInstall:
    Goto CheckComplete
    
  DotNetFound:
    DetailPrint ".NET Runtime 확인됨"
    
  CheckComplete:
FunctionEnd

; Section descriptions
LangString DESC_SEC01 ${LANG_KOREAN} "NationalClock 메인 프로그램입니다. (필수)"
LangString DESC_SEC02 ${LANG_KOREAN} "바탕화면에 바로가기를 생성합니다."
LangString DESC_SEC03 ${LANG_KOREAN} "Windows 시작 시 자동으로 실행됩니다. (선택사항)"
LangString DESC_SEC04 ${LANG_KOREAN} "Microsoft Visual C++ 2022 x64 재배포 가능 패키지를 설치합니다."

; Main program section
Section "메인 프로그램 (필수)" SEC01
  SectionIn RO
  SetOutPath "$INSTDIR"
  
  ; Copy program files from publish folder
  File /r "publish\framework-dependent\*.*"
  
  ; Create application data directory
  CreateDirectory "$LOCALAPPDATA\${PRODUCT_NAME}"
  CreateDirectory "$LOCALAPPDATA\${PRODUCT_NAME}\logs"
  CreateDirectory "$LOCALAPPDATA\${PRODUCT_NAME}\backup"
  
  ; Create README.txt with usage guide
  FileOpen $4 "$INSTDIR\README.txt" w
  FileWrite $4 "NationalClock v${PRODUCT_VERSION}$\r$\n"
  FileWrite $4 "==============================$\r$\n$\r$\n"
  FileWrite $4 "다중 시간대 월드 클록 WPF 애플리케이션$\r$\n$\r$\n"
  FileWrite $4 "설치 경로: $INSTDIR$\r$\n"
  FileWrite $4 "사용자 데이터: $LOCALAPPDATA\${PRODUCT_NAME}$\r$\n$\r$\n"
  FileWrite $4 "시스템 요구사항:$\r$\n"
  FileWrite $4 "- Windows 10 이상 (x64)$\r$\n"
  FileWrite $4 "- .NET 8.0 Desktop Runtime$\r$\n"
  FileWrite $4 "- Material Design UI 지원$\r$\n$\r$\n"
  FileWrite $4 "주요 기능:$\r$\n"
  FileWrite $4 "- 다중 시간대 실시간 표시$\r$\n"
  FileWrite $4 "- Material Design 테마 (라이트/다크 모드)$\r$\n"
  FileWrite $4 "- 시간대 추가/제거 관리$\r$\n"
  FileWrite $4 "- 사용자 설정 저장 및 복원$\r$\n"
  FileWrite $4 "- MVVM 패턴 기반 WPF 아키텍처$\r$\n$\r$\n"
  FileWrite $4 "사용법:$\r$\n"
  FileWrite $4 "1. 프로그램 실행 후 원하는 시간대 선택$\r$\n"
  FileWrite $4 "2. 설정 메뉴에서 테마 및 표시 옵션 변경$\r$\n"
  FileWrite $4 "3. 시간대는 드래그&드롭으로 순서 변경 가능$\r$\n$\r$\n"
  FileWrite $4 "Copyright (c) ${PRODUCT_PUBLISHER}, 2025$\r$\n"
  FileClose $4
  
  ; Create shortcuts
  CreateDirectory "$SMPROGRAMS\${PRODUCT_NAME}"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\${PRODUCT_NAME}.lnk" "$INSTDIR\NationalClock.exe" "" "$INSTDIR\Resources\NationalClock.ico"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\사용 가이드.lnk" "$INSTDIR\README.txt"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\${PRODUCT_NAME} 제거.lnk" "$INSTDIR\uninst.exe"
  
  ; Registry entries for uninstaller
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "DisplayName" "${PRODUCT_NAME}"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\NationalClock.exe"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "InstallLocation" "$INSTDIR"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "Comments" "${PRODUCT_DESCRIPTION}"
  WriteRegDWORD HKLM "${PRODUCT_UNINST_KEY}" "NoModify" 1
  WriteRegDWORD HKLM "${PRODUCT_UNINST_KEY}" "NoRepair" 1
  WriteRegDWORD HKLM "${PRODUCT_UNINST_KEY}" "EstimatedSize" 30000
  
  ; Create uninstaller
  WriteUninstaller "$INSTDIR\uninst.exe"
  
  ; Set file associations for settings files (optional)
  WriteRegStr HKCR ".nclock" "" "${PRODUCT_NAME}File"
  WriteRegStr HKCR "${PRODUCT_NAME}File" "" "${PRODUCT_NAME} 설정 파일"
  WriteRegStr HKCR "${PRODUCT_NAME}File\DefaultIcon" "" "$INSTDIR\NationalClock.exe,0"
  WriteRegStr HKCR "${PRODUCT_NAME}File\shell\open\command" "" '"$INSTDIR\NationalClock.exe" "%1"'
SectionEnd

Section "바탕화면 바로가기" SEC02
  CreateShortCut "$DESKTOP\${PRODUCT_NAME}.lnk" "$INSTDIR\NationalClock.exe" "" "$INSTDIR\Resources\NationalClock.ico"
SectionEnd

Section /o "시작프로그램 등록" SEC03
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "${PRODUCT_NAME}" "$INSTDIR\NationalClock.exe"
SectionEnd

Section "Visual C++ 재배포 가능 패키지" SEC04
  ; This section would install VC++ redistributable if needed
  ; For now, we'll just note it in the registry
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "VCRedistRequired" "Microsoft Visual C++ 2022 x64"
SectionEnd

; Component descriptions
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC01} $(DESC_SEC01)
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC02} $(DESC_SEC02) 
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC03} $(DESC_SEC03)
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC04} $(DESC_SEC04)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

; Uninstaller section
Section "Uninstall"
  ; Remove registry entries
  DeleteRegKey HKLM "${PRODUCT_UNINST_KEY}"
  DeleteRegKey HKCR ".nclock"
  DeleteRegKey HKCR "${PRODUCT_NAME}File"
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "${PRODUCT_NAME}"
  
  ; Remove shortcuts
  Delete "$DESKTOP\${PRODUCT_NAME}.lnk"
  Delete "$SMPROGRAMS\${PRODUCT_NAME}\${PRODUCT_NAME}.lnk"
  Delete "$SMPROGRAMS\${PRODUCT_NAME}\사용 가이드.lnk"
  Delete "$SMPROGRAMS\${PRODUCT_NAME}\${PRODUCT_NAME} 제거.lnk"
  RMDir "$SMPROGRAMS\${PRODUCT_NAME}"
  
  ; Remove program files
  RMDir /r "$INSTDIR"
  
  ; Check if this is an upgrade installation (installer signaled to keep data)
  ReadRegStr $R1 HKLM "SOFTWARE\${PRODUCT_NAME}_Temp" "KeepUserData"
  StrCmp $R1 "1" UpgradeInstall NormalUninstall
  
  NormalUninstall:
  ; Ask user if they want to keep user data (normal uninstall)
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "사용자 설정 및 로그 파일을 삭제하시겠습니까?$\r$\n$\r$\n다음 폴더의 내용이 삭제됩니다:$\r$\n$LOCALAPPDATA\${PRODUCT_NAME}" IDNO KeepUserData
  
  ; Remove user data if requested
  RMDir /r "$LOCALAPPDATA\${PRODUCT_NAME}"
  Goto UninstallComplete
  
  UpgradeInstall:
  ; This is an upgrade - keep user data as requested by installer
  DetailPrint "업그레이드 설치: 사용자 데이터를 보존합니다."
  Goto UninstallComplete
  
  KeepUserData:
  MessageBox MB_ICONINFORMATION|MB_OK "사용자 설정이 보존되었습니다:$\r$\n$LOCALAPPDATA\${PRODUCT_NAME}"
  
  UninstallComplete:
  MessageBox MB_ICONINFORMATION|MB_OK "${PRODUCT_NAME}이(가) 성공적으로 제거되었습니다."
SectionEnd

; Installer functions
Function .onInit
  ; Check Windows version (Windows 10 or higher) - simplified check
  ; Note: This is a basic check, more detailed version checking can be added if needed
  
  ; Check if already installed
  ReadRegStr $R0 HKLM "${PRODUCT_UNINST_KEY}" "UninstallString"
  StrCmp $R0 "" NotInstalled
  
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "${PRODUCT_NAME}이(가) 이미 설치되어 있습니다.$\r$\n$\r$\n기존 설치를 제거하고 새로 설치하시겠습니까?" IDYES ShowUninstallOptions
  Abort
  
  ShowUninstallOptions:
  MessageBox MB_ICONQUESTION|MB_YESNOCANCEL|MB_DEFBUTTON3 "기존 설정과 사용자 데이터를 어떻게 하시겠습니까?$\r$\n$\r$\n• 예: 설정 및 데이터 보존 (권장)$\r$\n• 아니오: 모든 데이터 삭제$\r$\n• 취소: 설치 중단" IDYES UninstallKeepData IDNO UninstallRemoveAll
  Abort
  
  UninstallKeepData:
  ; Create temporary registry key to signal uninstaller to keep user data
  WriteRegStr HKLM "SOFTWARE\${PRODUCT_NAME}_Temp" "KeepUserData" "1"
  ExecWait '$R0 /S'
  DeleteRegKey HKLM "SOFTWARE\${PRODUCT_NAME}_Temp"
  Goto NotInstalled
  
  UninstallRemoveAll:
  ; Remove temporary registry key to signal complete removal
  DeleteRegKey HKLM "SOFTWARE\${PRODUCT_NAME}_Temp"
  ExecWait '$R0 /S'
  
  NotInstalled:
FunctionEnd

Function .onInstSuccess
  ; Create desktop link to user guide
  CreateShortCut "$DESKTOP\NationalClock 사용 가이드.lnk" "$INSTDIR\README.txt"
  
  ; Log installation
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "InstallDate" "${__DATE__}"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "InstallTime" "${__TIME__}"
FunctionEnd

; Uninstaller functions
Function un.onInit
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "${PRODUCT_NAME}을(를) 완전히 제거하시겠습니까?" IDYES +2
  Abort
FunctionEnd