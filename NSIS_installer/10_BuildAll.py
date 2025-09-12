#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
NationalClock 전체 빌드 스크립트
프로젝트 업데이트부터 NSIS 설치파일 생성까지 전체 과정을 자동으로 수행합니다.

Framework-dependent (.NET 8.0 Runtime 필요)
파일명: NationalClock_v1.0.001_Build_20250909_2006_Setup.exe
아키텍처: x64 최적화, LZMA 고압축
설치 경로: C:\\Program Files\\NationalClock
"""

import os
import sys
import subprocess
import shutil
from pathlib import Path
from datetime import datetime
import time

# 출력 인코딩 설정
if sys.stdout.encoding != 'utf-8':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'replace')
    sys.stderr = codecs.getwriter('utf-8')(sys.stderr.buffer, 'replace')

# ==========================================
# 설정 (필요시 수정)
# ==========================================
PRODUCT_NAME = "NationalClock"
PRODUCT_VERSION = "1.0.001"
BUILD_DATE = "20250912_1702"
PROJECT_FILE = "NationalClock.csproj"
NSIS_SCRIPT = "NationalClock_Installer.nsi"

# NSIS 경로 후보들
NSIS_PATHS = [
    r"C:\Program Files (x86)\NSIS\makensis.exe",
    r"C:\Program Files\NSIS\makensis.exe",
    r"D:\Program Files (x86)\NSIS\makensis.exe",
    r"D:\Program Files\NSIS\makensis.exe"
]

def print_header():
    """헤더 출력"""
    print("=" * 80)
    print(">> NationalClock 전체 빌드 프로세스 <<")
    print("프로젝트 업데이트 → NSIS 설치파일 생성")
    print("=" * 80)
    print()
    print(f"제품명: {PRODUCT_NAME}")
    print(f"버전: {PRODUCT_VERSION}")
    print(f"빌드 일시: {BUILD_DATE}")
    print(f"현재 디렉터리: {os.getcwd()}")
    print(f"타겟: x64 Framework-dependent")
    print(f"압축: LZMA 고압축")
    print(f"설치 경로: C:\\Program Files\\{PRODUCT_NAME}")
    print()

def check_prerequisites():
    """사전 요구사항 확인"""
    print("🔍 사전 요구사항 확인 중...")
    print("=" * 40)
    
    errors = []
    
    # 1. .NET SDK 확인
    print("1. .NET 8.0 SDK 확인 중...")
    try:
        result = subprocess.run(["dotnet", "--version"], capture_output=True, text=True, encoding='utf-8', errors='replace')
        if result.returncode == 0:
            version = result.stdout.strip()
            print(f"   ✓ .NET SDK 확인됨: {version}")
            if not version.startswith("8."):
                print(f"   ⚠ .NET 8.0이 아님: {version}")
        else:
            print("   ❌ .NET SDK를 찾을 수 없습니다.")
            errors.append(".NET 8.0 SDK가 설치되어 있지 않습니다.")
    except FileNotFoundError:
        print("   ❌ dotnet 명령을 찾을 수 없습니다.")
        errors.append(".NET 8.0 SDK가 설치되어 있지 않습니다.")
    
    # 2. 프로젝트 파일 확인
    print("2. 프로젝트 파일 확인 중...")
    project_path = Path("..") / "NationalClock" / PROJECT_FILE
    if project_path.exists():
        print(f"   ✓ 프로젝트 파일 확인됨: {project_path}")
    else:
        print(f"   ❌ 프로젝트 파일을 찾을 수 없습니다: {project_path}")
        errors.append(f"프로젝트 파일을 찾을 수 없습니다: {project_path}")
    
    # 3. NSIS 확인
    print("3. NSIS 설치 확인 중...")
    nsis_found = False
    for nsis_path in NSIS_PATHS:
        if Path(nsis_path).exists():
            print(f"   ✓ NSIS 확인됨: {nsis_path}")
            nsis_found = True
            break
    
    if not nsis_found:
        print("   ❌ NSIS를 찾을 수 없습니다.")
        print("   다음 경로를 확인했습니다:")
        for path in NSIS_PATHS:
            print(f"   • {path}")
        errors.append("NSIS가 설치되어 있지 않습니다.")
    
    # 4. 스크립트 파일 확인
    print("4. 빌드 스크립트 확인 중...")
    scripts = ["11_UpdateFromProject.py", "12_BuildInstaller.py"]
    for script in scripts:
        if Path(script).exists():
            print(f"   ✓ {script}")
        else:
            print(f"   ❌ {script}")
            errors.append(f"필수 스크립트가 없습니다: {script}")
    
    print()
    
    if errors:
        print("❌ 사전 요구사항 확인 실패:")
        for i, error in enumerate(errors, 1):
            print(f"   {i}. {error}")
        print()
        print("해결 방법:")
        print("• .NET 8.0 SDK: https://dotnet.microsoft.com/download/dotnet/8.0")
        print("• NSIS 3.x: https://nsis.sourceforge.io/Download")
        print("• 프로젝트 경로가 올바른지 확인")
        return False
    
    print("✅ 모든 사전 요구사항이 충족되었습니다!")
    print()
    return True

def run_step(step_name, script_name, description):
    """단계별 스크립트 실행"""
    print(f"🔧 {step_name}: {description}")
    print("=" * 60)
    
    try:
        # Python 스크립트 실행
        result = subprocess.run([sys.executable, script_name], 
                              capture_output=False, text=True, encoding='utf-8', errors='replace')
        
        if result.returncode == 0:
            print(f"✅ {step_name} 완료!")
            return True
        else:
            print(f"❌ {step_name} 실패! (종료 코드: {result.returncode})")
            return False
            
    except Exception as e:
        print(f"❌ {step_name} 중 오류 발생: {str(e)}")
        return False

def cleanup_old_files():
    """이전 빌드 파일 정리"""
    print("🗑️ 이전 빌드 파일 정리 중...")
    print("=" * 40)
    
    # 이전 설치파일들 찾기
    pattern = f"{PRODUCT_NAME}_v*_Build_*_Setup.exe"
    old_installers = list(Path(".").glob(pattern))
    
    if old_installers:
        print(f"   발견된 이전 설치파일: {len(old_installers)}개")
        for installer in old_installers:
            try:
                installer.unlink()
                print(f"   ✓ 삭제됨: {installer.name}")
            except Exception as e:
                print(f"   ⚠ 삭제 실패: {installer.name} ({e})")
    else:
        print("   • 정리할 이전 설치파일이 없습니다.")
    
    # 임시 폴더 정리
    temp_dirs = ["publish", "Tmp"]
    for temp_dir in temp_dirs:
        temp_path = Path(temp_dir)
        if temp_path.exists():
            try:
                shutil.rmtree(temp_path)
                print(f"   ✓ 임시 폴더 삭제: {temp_dir}")
            except Exception as e:
                print(f"   ⚠ 임시 폴더 삭제 실패: {temp_dir} ({e})")
    
    print("   ✅ 정리 완료!")
    print()

def verify_final_result():
    """최종 결과 검증"""
    print("🔍 최종 결과 검증 중...")
    print("=" * 40)
    
    # 설치파일 패턴으로 검색
    installer_pattern = f"{PRODUCT_NAME}_v{PRODUCT_VERSION}_Build_*_Setup.exe"
    installer_files = list(Path(".").glob(installer_pattern))
    
    if not installer_files:
        print(f"   ❌ 설치파일을 찾을 수 없습니다: {installer_pattern}")
        return False
    
    # 가장 최근 파일 선택
    installer_path = max(installer_files, key=lambda f: f.stat().st_mtime)
    installer_name = installer_path.name
    
    # 파일 정보 출력
    file_size = installer_path.stat().st_size
    size_mb = file_size // 1024 // 1024
    
    print(f"   ✅ 설치파일: {installer_name}")
    print(f"   📦 파일 크기: {size_mb} MB ({file_size:,} bytes)")
    
    # 생성 시간 확인
    creation_time = installer_path.stat().st_mtime
    creation_datetime = datetime.fromtimestamp(creation_time)
    print(f"   📅 생성 시간: {creation_datetime.strftime('%Y-%m-%d %H:%M:%S')}")
    
    # 관련 파일들 확인
    info_file = f"{installer_name}_INFO.txt"
    if Path(info_file).exists():
        print(f"   📄 정보 파일: {info_file}")
    
    if Path("VERSION.txt").exists():
        print(f"   📄 버전 파일: VERSION.txt")
    
    if Path("BUILD_INFO.txt").exists():
        print(f"   📄 빌드 정보: BUILD_INFO.txt")
    
    print()
    return True

def generate_build_report():
    """빌드 보고서 생성"""
    print("📊 빌드 보고서 생성 중...")
    print("=" * 40)
    
    # 설치파일 패턴으로 검색
    installer_pattern = f"{PRODUCT_NAME}_v{PRODUCT_VERSION}_Build_*_Setup.exe"
    installer_files = list(Path(".").glob(installer_pattern))
    
    if installer_files:
        # 가장 최근 파일 선택
        installer_path = max(installer_files, key=lambda f: f.stat().st_mtime)
        installer_name = installer_path.name
        file_size = installer_path.stat().st_size
        size_mb = file_size // 1024 // 1024
        creation_time = installer_path.stat().st_mtime
        creation_datetime = datetime.fromtimestamp(creation_time)
        
        report_content = f"""==================================================
{PRODUCT_NAME} 빌드 보고서
==================================================

빌드 정보:
- 제품명: {PRODUCT_NAME}
- 버전: {PRODUCT_VERSION}
- 빌드 일시: {BUILD_DATE}
- 빌드 완료: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}

생성된 파일:
- 설치파일: {installer_name}
- 파일 크기: {size_mb} MB ({file_size:,} bytes)
- 생성 시간: {creation_datetime.strftime('%Y-%m-%d %H:%M:%S')}

기술 사양:
- 플랫폼: .NET 8.0 WPF
- 아키텍처: x64
- 배포 방식: Framework-dependent
- UI 프레임워크: Material Design
- 압축 방식: LZMA
- 패턴: MVVM

시스템 요구사항:
- Windows 10 이상 (x64)
- .NET 8.0 Desktop Runtime
- 관리자 권한 (설치 시)

설치 정보:
- 설치 경로: C:\\Program Files\\{PRODUCT_NAME}
- 사용자 데이터: %LocalAppData%\\{PRODUCT_NAME}
- 바로가기: 데스크톱, 시작메뉴
- 자동 시작: 선택사항

빌드 과정:
1. 사전 요구사항 확인 ✅
2. 이전 빌드 정리 ✅
3. 프로젝트 업데이트 ✅
4. NSIS 설치파일 생성 ✅
5. 최종 결과 검증 ✅

주의사항:
- 설치 전 이전 버전 제거 권장
- .NET 8.0 Desktop Runtime 필수
- 관리자 권한으로 설치 실행
- 방화벽 설정 확인 필요

배포 체크리스트:
□ 다양한 Windows 버전에서 설치 테스트
□ .NET Runtime이 없는 환경에서 테스트
□ 업그레이드 설치 테스트
□ 제거 후 재설치 테스트
□ 바이러스 스캔 수행
□ 디지털 서명 적용 (선택사항)

개발 정보:
- 개발사: Green Power Co., Ltd.
- 빌드 도구: Python + NSIS
- 프로젝트 구조: MVVM Pattern
- 설정 관리: JSON 기반
- 테마 시스템: Material Design
"""
        
        report_file = f"{PRODUCT_NAME}_Build_Report_{BUILD_DATE}.txt"
        with open(report_file, "w", encoding="utf-8") as f:
            f.write(report_content)
        
        print(f"   ✅ 빌드 보고서 생성: {report_file}")
        print()

def main():
    """메인 실행 함수"""
    start_time = time.time()
    
    print_header()
    
    # 현재 위치를 NSIS_installer로 변경
    script_dir = Path(__file__).parent
    os.chdir(script_dir)
    
    try:
        # 1. 사전 요구사항 확인
        if not check_prerequisites():
            return 1
        
        # 2. 이전 빌드 파일 정리
        cleanup_old_files()
        
        # 3. 프로젝트 업데이트 단계
        print("📤 1단계: 프로젝트 업데이트 및 게시")
        if not run_step("1단계", "11_UpdateFromProject.py", 
                       "프로젝트 빌드 및 게시 폴더 생성"):
            print("❌ 프로젝트 업데이트 실패!")
            return 1
        
        print("\n" + "="*80 + "\n")
        
        # 4. 설치파일 생성 단계
        print("📦 2단계: NSIS 설치파일 생성")
        if not run_step("2단계", "12_BuildInstaller.py", 
                       "NSIS 설치파일 컴파일"):
            print("❌ 설치파일 생성 실패!")
            return 1
        
        print("\n" + "="*80 + "\n")
        
        # 5. 최종 결과 검증
        if not verify_final_result():
            print("❌ 최종 검증 실패!")
            return 1
        
        # 6. 빌드 보고서 생성
        generate_build_report()
        
        # 빌드 완료 메시지
        elapsed_time = time.time() - start_time
        minutes = int(elapsed_time // 60)
        seconds = int(elapsed_time % 60)
        
        print("🎉 전체 빌드 프로세스 완료!")
        print("=" * 80)
        print()
        
        # 실제 생성된 설치파일 찾기
        installer_pattern = f"{PRODUCT_NAME}_v{PRODUCT_VERSION}_Build_*_Setup.exe"
        installer_files = list(Path(".").glob(installer_pattern))
        if installer_files:
            installer_path = max(installer_files, key=lambda f: f.stat().st_mtime)
            installer_name = installer_path.name
        else:
            installer_name = "설치파일을 찾을 수 없음"
        
        print(f"📦 생성된 설치파일: {installer_name}")
        print(f"⏱️  소요 시간: {minutes:02d}분 {seconds:02d}초")
        print()
        
        print("🚀 다음 단계:")
        print("1. 설치파일 테스트 실행")
        print("2. 다양한 환경에서 설치 검증")
        print("3. 사용자 매뉴얼 업데이트")
        print("4. 배포 준비 (바이러스 스캔, 디지털 서명)")
        print()
        
        print("📋 생성된 파일 목록:")
        files_created = [
            installer_name,
            f"{installer_name}_INFO.txt",
            "VERSION.txt",
            "BUILD_INFO.txt"
        ]
        
        # 빌드 리포트 파일도 패턴으로 찾기
        report_pattern = f"{PRODUCT_NAME}_Build_Report_*.txt"
        report_files = list(Path(".").glob(report_pattern))
        if report_files:
            latest_report = max(report_files, key=lambda f: f.stat().st_mtime)
            files_created.append(latest_report.name)
        
        for file_name in files_created:
            if Path(file_name).exists():
                print(f"   ✓ {file_name}")
        
        print()
        print("✅ 모든 작업이 성공적으로 완료되었습니다!")
        
        return 0
        
    except KeyboardInterrupt:
        print("\n❌ 사용자에 의해 중단되었습니다.")
        return 1
    except Exception as e:
        print(f"\n❌ 예기치 않은 오류 발생: {str(e)}")
        import traceback
        traceback.print_exc()
        return 1

if __name__ == "__main__":
    sys.exit(main())