#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
NationalClock 프로젝트 업데이트 스크립트
프로젝트를 빌드하고 NSIS 설치파일 생성을 위한 publish 폴더를 생성합니다.

Framework-dependent (.NET 8.0 Runtime 필요)
"""

import os
import sys
import subprocess
import shutil
from pathlib import Path
from datetime import datetime

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
BUILD_DATE = "20250909_2125"
PROJECT_FILE = "NationalClock.csproj"

def print_header():
    """헤더 출력"""
    print("=" * 60)
    print("NationalClock 프로젝트 업데이트")
    print("Framework-dependent 빌드 (x64 최적화)")
    print("=" * 60)
    print()
    print(f"제품명: {PRODUCT_NAME}")
    print(f"버전: {PRODUCT_VERSION}")
    print(f"빌드 일시: {BUILD_DATE}")
    print(f"현재 디렉터리: {os.getcwd()}")
    print()

def clean_publish_folder():
    """기존 publish 폴더 정리"""
    print("1. 이전 빌드 정리 중...")
    
    publish_dir = Path("publish")
    if publish_dir.exists():
        shutil.rmtree(publish_dir)
        print("   * 기존 publish 폴더를 삭제했습니다.")
    
    # 디렉터리 생성
    publish_dir.mkdir()
    (publish_dir / "framework-dependent").mkdir()
    print("   * 새로운 publish 폴더를 생성했습니다.")
    print()

def check_project_file():
    """프로젝트 파일 확인"""
    print("2. 프로젝트 파일 확인 중...")
    
    project_path = Path("..") / "NationalClock" / PROJECT_FILE
    if not project_path.exists():
        print(f"   ❌ 프로젝트 파일을 찾을 수 없습니다: {project_path}")
        return False
        
    print(f"   ✓ 프로젝트 파일 확인됨: {project_path}")
    return True

def build_project():
    """프로젝트 빌드 (.NET 8.0)"""
    print("3. NationalClock 프로젝트 빌드 중...")
    
    project_path = Path("..") / "NationalClock"
    os.chdir(project_path)
    
    try:
        # Clean 빌드
        print("   • Clean 빌드 수행 중...")
        result = subprocess.run([
            "dotnet", "clean",
            "--configuration", "Release",
            "--verbosity", "quiet"
        ], capture_output=True, text=True, encoding='utf-8', errors='replace')
        
        if result.returncode != 0:
            print(f"   ❌ Clean 실패: {result.stderr}")
            return False
        print("   ✓ Clean 완료")
        
        # 빌드
        print("   • 프로젝트 빌드 중...")
        result = subprocess.run([
            "dotnet", "build",
            "--configuration", "Release",
            "--verbosity", "quiet"
        ], capture_output=True, text=True, encoding='utf-8', errors='replace')
        
        if result.returncode != 0:
            print(f"   ❌ 빌드 실패:")
            print(f"   {result.stderr}")
            return False
        print("   ✓ 빌드 완료")
        
        return True
        
    except FileNotFoundError:
        print("   ❌ dotnet 명령을 찾을 수 없습니다.")
        print("   .NET 8.0 SDK가 설치되어 있는지 확인하세요.")
        return False
    except Exception as e:
        print(f"   ❌ 빌드 중 오류 발생: {str(e)}")
        return False

def publish_project():
    """Framework-dependent 방식으로 게시"""
    print("4. Framework-dependent 게시 중...")
    
    publish_path = Path("..") / "NSIS_installer" / "publish" / "framework-dependent"
    
    try:
        # Publish 실행
        result = subprocess.run([
            "dotnet", "publish",
            "--configuration", "Release",
            "--runtime", "win-x64",
            "--self-contained", "false",
            "--output", str(publish_path.absolute()),
            "--verbosity", "quiet"
        ], capture_output=True, text=True, encoding='utf-8', errors='replace')
        
        if result.returncode != 0:
            print(f"   ❌ 게시 실패:")
            print(f"   {result.stderr}")
            return False
            
        print(f"   ✓ 게시 완료: {publish_path}")
        return True
        
    except Exception as e:
        print(f"   ❌ 게시 중 오류 발생: {str(e)}")
        return False

def verify_published_files():
    """게시된 파일 검증"""
    print("5. 게시 파일 검증 중...")
    
    publish_path = Path("..") / "NSIS_installer" / "publish" / "framework-dependent"
    
    # 필수 파일 확인
    required_files = [
        "NationalClock.exe",
        "NationalClock.dll",
        "NationalClock.deps.json",
        "NationalClock.runtimeconfig.json"
    ]
    
    missing_files = []
    for file_name in required_files:
        file_path = publish_path / file_name
        if not file_path.exists():
            missing_files.append(file_name)
        else:
            print(f"   ✓ {file_name}")
    
    if missing_files:
        print(f"   ❌ 필수 파일이 누락되었습니다: {missing_files}")
        return False
    
    # Resources 폴더 확인 및 복사
    resources_path = publish_path / "Resources"
    if not resources_path.exists():
        print("   • Resources 폴더를 찾을 수 없어 복사합니다.")
        
        # 프로젝트의 Resources 폴더에서 복사
        project_resources = Path("..") / "NationalClock" / "Resources"
        if project_resources.exists():
            shutil.copytree(project_resources, resources_path)
            print("   ✓ Resources 폴더 복사됨")
        else:
            print("   ⚠ 프로젝트 Resources 폴더를 찾을 수 없습니다.")
    else:
        print("   ✓ Resources 폴더 확인됨")
        
    # 아이콘 파일 확인
    icon_file = resources_path / "NationalClock.ico"
    if icon_file.exists():
        print("   ✓ NationalClock.ico 확인됨")
    else:
        print("   ⚠ NationalClock.ico 파일을 찾을 수 없습니다.")
    
    # 게시 파일 크기 확인
    total_size = 0
    file_count = 0
    for file_path in publish_path.rglob("*"):
        if file_path.is_file():
            total_size += file_path.stat().st_size
            file_count += 1
    
    print(f"   ✓ 총 {file_count}개 파일, 크기: {total_size // 1024 // 1024} MB")
    return True

def update_version_info():
    """VERSION.txt 파일 업데이트"""
    print("6. 버전 정보 업데이트 중...")
    
    os.chdir(Path("..") / "NSIS_installer")
    
    version_content = f"""{PRODUCT_NAME} v{PRODUCT_VERSION}
빌드 일시: {BUILD_DATE}
설치파일: {PRODUCT_NAME}_v{PRODUCT_VERSION}_Build_{BUILD_DATE}_Setup.exe
아키텍처: x64
배포 형식: Framework-dependent
요구사항: .NET 8.0 Desktop Runtime
설치 경로: C:\\Program Files\\{PRODUCT_NAME}
압축 방식: LZMA
개발사: Green Power Co., Ltd.
목적: 다중 시간대 월드 클록 WPF 애플리케이션
"""
    
    with open("VERSION.txt", "w", encoding="utf-8") as f:
        f.write(version_content)
    
    print("   ✓ VERSION.txt 업데이트 완료")

def update_build_info():
    """BUILD_INFO.txt 파일 업데이트"""
    print("7. 빌드 정보 업데이트 중...")
    
    build_info_content = f"""==================================================
{PRODUCT_NAME} 빌드 정보
==================================================

제품명: {PRODUCT_NAME}
버전: {PRODUCT_VERSION}
빌드 일시: {BUILD_DATE}
설치파일: {PRODUCT_NAME}_v{PRODUCT_VERSION}_Build_{BUILD_DATE}_Setup.exe

기술 정보:
- 플랫폼: .NET 8.0
- 아키텍처: x64
- 배포: Framework-dependent
- UI: WPF with Material Design
- 압축: LZMA

설치 정보:
- 설치 경로: C:\\Program Files\\{PRODUCT_NAME}
- 사용자 데이터: %LocalAppData%\\{PRODUCT_NAME}
- 바로가기: 데스크톱, 시작메뉴
- 자동 시작: 선택사항

시스템 요구사항:
- Windows 10 이상 (x64)
- .NET 8.0 Desktop Runtime
- Material Design UI 지원

주요 기능:
- 다중 시간대 실시간 표시
- Material Design 테마 (라이트/다크 모드)
- 시간대 추가/제거 관리
- 사용자 설정 저장 및 복원
- MVVM 패턴 기반 WPF 아키텍처
"""
    
    with open("BUILD_INFO.txt", "w", encoding="utf-8") as f:
        f.write(build_info_content)
    
    print("   ✓ BUILD_INFO.txt 업데이트 완료")

def main():
    """메인 실행 함수"""
    print_header()
    
    # 현재 위치를 NSIS_installer로 변경
    script_dir = Path(__file__).parent
    os.chdir(script_dir)
    
    try:
        # 1. 폴더 정리
        clean_publish_folder()
        
        # 2. 프로젝트 파일 확인
        if not check_project_file():
            return 1
        
        # 3. 프로젝트 빌드
        if not build_project():
            return 1
        
        # 4. 프로젝트 게시
        if not publish_project():
            return 1
        
        # 5. 게시 파일 검증
        if not verify_published_files():
            return 1
        
        # NSIS_installer 디렉터리로 이동
        os.chdir(script_dir)
        
        # 6. 버전 정보 업데이트
        update_version_info()
        
        # 7. 빌드 정보 업데이트
        update_build_info()
        
        print()
        print("=" * 60)
        print("✅ 프로젝트 업데이트 완료!")
        print("=" * 60)
        print()
        print("다음 단계:")
        print("1. python 12_BuildInstaller.py - 설치파일 생성")
        print("2. python 10_BuildAll.py - 전체 빌드 프로세스")
        print()
        
        return 0
        
    except KeyboardInterrupt:
        print("\n❌ 사용자에 의해 중단되었습니다.")
        return 1
    except Exception as e:
        print(f"\n❌ 예기치 않은 오류 발생: {str(e)}")
        return 1

if __name__ == "__main__":
    sys.exit(main())