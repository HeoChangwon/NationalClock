#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
NationalClock NSIS 설치파일 빌드 스크립트
NSIS 컴파일러를 사용하여 Windows 설치파일을 생성합니다.

파일명 형식: NationalClock_v1.0.001_Build_20250909_2006_Setup.exe
아키텍처: x64 최적화
압축: LZMA 고압축 적용
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
NSIS_SCRIPT = "NationalClock_Installer.nsi"
NSIS_PATH = r"C:\Program Files (x86)\NSIS\makensis.exe"

# Alternative NSIS paths
NSIS_PATHS = [
    r"C:\Program Files (x86)\NSIS\makensis.exe",
    r"C:\Program Files\NSIS\makensis.exe",
    r"D:\Program Files (x86)\NSIS\makensis.exe",
    r"D:\Program Files\NSIS\makensis.exe"
]

def print_header():
    """헤더 출력"""
    print("=" * 70)
    print("NationalClock NSIS 설치파일 빌드")
    print("Framework-dependent x64 최적화 버전")
    print("=" * 70)
    print()
    print(f"제품명: {PRODUCT_NAME}")
    print(f"버전: {PRODUCT_VERSION}")
    print(f"빌드 일시: {BUILD_DATE}")
    print(f"현재 디렉터리: {os.getcwd()}")
    print()

def find_nsis():
    """NSIS 설치 경로 찾기"""
    print("1. NSIS 설치 확인 중...")
    
    for nsis_path in NSIS_PATHS:
        if Path(nsis_path).exists():
            print(f"   ✓ NSIS 발견: {nsis_path}")
            return nsis_path
    
    print("   ❌ NSIS를 찾을 수 없습니다.")
    print("   다음 경로를 확인했습니다:")
    for path in NSIS_PATHS:
        print(f"   • {path}")
    print()
    print("   해결방법:")
    print("   1. NSIS 3.x를 다운로드하여 설치하세요:")
    print("      https://nsis.sourceforge.io/Download")
    print("   2. 또는 다른 경로에 설치된 경우 스크립트의 NSIS_PATHS를 수정하세요.")
    
    return None

def check_nsis_script():
    """NSIS 스크립트 파일 확인"""
    print("2. NSIS 스크립트 확인 중...")
    
    if not Path(NSIS_SCRIPT).exists():
        print(f"   ❌ NSIS 스크립트를 찾을 수 없습니다: {NSIS_SCRIPT}")
        print("   11_UpdateFromProject.py를 먼저 실행하세요.")
        return False
    
    print(f"   ✓ NSIS 스크립트 확인: {NSIS_SCRIPT}")
    return True

def check_publish_folder():
    """게시 폴더 확인"""
    print("3. 게시 폴더 확인 중...")
    
    publish_path = Path("publish") / "framework-dependent"
    if not publish_path.exists():
        print(f"   ❌ 게시 폴더를 찾을 수 없습니다: {publish_path}")
        print("   11_UpdateFromProject.py를 먼저 실행하세요.")
        return False
    
    # 필수 파일 확인
    required_files = [
        "NationalClock.exe",
        "NationalClock.dll"
    ]
    
    missing_files = []
    for file_name in required_files:
        file_path = publish_path / file_name
        if not file_path.exists():
            missing_files.append(file_name)
    
    if missing_files:
        print(f"   ❌ 필수 파일이 누락되었습니다: {missing_files}")
        print("   11_UpdateFromProject.py를 먼저 실행하세요.")
        return False
    
    print(f"   ✓ 게시 폴더 확인됨: {publish_path}")
    return True

def build_installer(nsis_exe_path):
    """NSIS 설치파일 빌드"""
    print("4. NSIS 설치파일 빌드 중...")
    
    try:
        # NSIS 컴파일 실행
        cmd = [
            nsis_exe_path,
            f"/DBUILD_DATE={BUILD_DATE}",
            f"/DPRODUCT_NAME={PRODUCT_NAME}",
            f"/DPRODUCT_VERSION={PRODUCT_VERSION}",
            NSIS_SCRIPT
        ]
        
        print(f"   • 명령: {' '.join(cmd)}")
        
        result = subprocess.run(cmd, capture_output=True, text=True, encoding='utf-8', errors='replace')
        
        if result.returncode != 0:
            print("   ❌ NSIS 컴파일 실패:")
            print(f"   stdout: {result.stdout}")
            print(f"   stderr: {result.stderr}")
            return False
        
        print("   ✓ NSIS 컴파일 완료")
        
        # 컴파일 결과 출력 (정보성)
        if result.stdout:
            lines = result.stdout.split('\n')
            for line in lines:
                if line.strip():
                    if "Total size:" in line or "Compressed" in line or "Output" in line:
                        print(f"   • {line.strip()}")
        
        return True
        
    except Exception as e:
        print(f"   ❌ NSIS 컴파일 중 오류 발생: {str(e)}")
        return False

def verify_installer():
    """생성된 설치파일 검증"""
    print("5. 설치파일 검증 중...")
    
    installer_name = f"{PRODUCT_NAME}_v{PRODUCT_VERSION}_Build_{BUILD_DATE}_Setup.exe"
    installer_path = Path(installer_name)
    
    if not installer_path.exists():
        print(f"   ❌ 설치파일을 찾을 수 없습니다: {installer_name}")
        return False
    
    file_size = installer_path.stat().st_size
    size_mb = file_size // 1024 // 1024
    
    print(f"   ✓ 설치파일 생성됨: {installer_name}")
    print(f"   ✓ 파일 크기: {size_mb} MB ({file_size:,} bytes)")
    
    # 파일 크기 체크 (대략적인 범위)
    if size_mb < 1:
        print("   ⚠ 파일 크기가 매우 작습니다. 정상적으로 생성되었는지 확인하세요.")
    elif size_mb > 100:
        print("   ⚠ 파일 크기가 매우 큽니다. 불필요한 파일이 포함되었는지 확인하세요.")
    
    return True

def create_installer_info():
    """설치파일 정보 텍스트 생성"""
    print("6. 설치파일 정보 생성 중...")
    
    installer_name = f"{PRODUCT_NAME}_v{PRODUCT_VERSION}_Build_{BUILD_DATE}_Setup.exe"
    installer_path = Path(installer_name)
    
    if installer_path.exists():
        file_size = installer_path.stat().st_size
        size_mb = file_size // 1024 // 1024
        
        info_content = f"""==================================================
{PRODUCT_NAME} 설치파일 정보
==================================================

파일명: {installer_name}
빌드 일시: {BUILD_DATE}
파일 크기: {size_mb} MB ({file_size:,} bytes)

제품 정보:
- 제품명: {PRODUCT_NAME}
- 버전: {PRODUCT_VERSION}
- 아키텍처: x64
- 배포 형식: Framework-dependent
- 압축: LZMA

설치 정보:
- 설치 경로: C:\\Program Files\\{PRODUCT_NAME}
- 사용자 데이터: %LocalAppData%\\{PRODUCT_NAME}
- 바로가기: 데스크톱, 시작메뉴
- 시작프로그램: 선택사항

시스템 요구사항:
- Windows 10 이상 (x64)
- .NET 8.0 Desktop Runtime
- 관리자 권한 (설치 시)

개발 정보:
- 개발사: Green Power Co., Ltd.
- 프레임워크: WPF (.NET 8.0)
- UI 디자인: Material Design
- 아키텍처: MVVM Pattern

사용법:
1. 관리자 권한으로 설치파일 실행
2. 설치 마법사 따라 진행
3. .NET 8.0 Desktop Runtime 설치 안내 확인
4. 설치 완료 후 바로가기로 실행

주의사항:
- 이전 버전이 설치된 경우 업그레이드/제거 선택 가능
- 사용자 설정 보존 옵션 제공
- 방화벽 설정에서 차단되지 않도록 주의
"""
        
        info_file = f"{installer_name}_INFO.txt"
        with open(info_file, "w", encoding="utf-8") as f:
            f.write(info_content)
        
        print(f"   ✓ 설치파일 정보 생성: {info_file}")

def main():
    """메인 실행 함수"""
    print_header()
    
    # 현재 위치를 NSIS_installer로 변경
    script_dir = Path(__file__).parent
    os.chdir(script_dir)
    
    try:
        # 1. NSIS 설치 확인
        nsis_exe = find_nsis()
        if not nsis_exe:
            return 1
        
        # 2. NSIS 스크립트 확인
        if not check_nsis_script():
            return 1
        
        # 3. 게시 폴더 확인
        if not check_publish_folder():
            return 1
        
        # 4. 설치파일 빌드
        if not build_installer(nsis_exe):
            return 1
        
        # 5. 설치파일 검증
        if not verify_installer():
            return 1
        
        # 6. 설치파일 정보 생성
        create_installer_info()
        
        print()
        print("=" * 70)
        print("✅ NSIS 설치파일 빌드 완료!")
        print("=" * 70)
        print()
        
        installer_name = f"{PRODUCT_NAME}_v{PRODUCT_VERSION}_Build_{BUILD_DATE}_Setup.exe"
        print(f"생성된 파일: {installer_name}")
        print()
        print("다음 단계:")
        print("1. 설치파일을 테스트해보세요")
        print("2. 배포 전에 다양한 환경에서 설치 테스트 수행")
        print("3. 바이러스 스캔 및 디지털 서명 고려")
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