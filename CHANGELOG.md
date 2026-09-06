# 변경 내역

## v2.1.3 (2026-09-07)

### Fixed

- VRChat Avatars SDK 3.10.0 미만 버전에서 컴파일 실패 문제
- 프리셋의 무릎 깊이 변경 사항이 인스펙터와 빌드에 반영되지 않는 문제
- 프리셋 추가·삭제 및 표시 이름 변경 시 인스펙터 목록 갱신 누락

## v2.1.2 (2026-08-27)

### Changed

- Manuka 프리셋 표시 이름
    - `Manuka (マヌカ)` → `Manuka (マヌカ) - Style A`
- Manuka v2 프리셋 표시 이름
    - `Manuka (マヌカ) v2` → `Manuka (マヌカ) - Style B`
- 라이선스
    - `MIT` → `MIT-0`

### Removed

- VRC Constraint 재사용 로직

### Fixed

- `EditorOnly` 태그가 지정된 오브젝트 및 하위 오브젝트의 Knee Fixer까지 포함하는 중복 판정 문제

## v2.1.1 (2026-08-25)

### Fixed

- 중복으로 적용되지 않는 Knee Fixer의 인스펙터 설정 잠금 누락

## v2.1.0 (2026-08-22)

### Added

- Manuka v2 프리셋
- 인스펙터에서 여러 Knee Fixer 동시 설정
- Knee Fixer 중복 경고 및 해당 오브젝트 선택 버튼

### Changed

- VPM 패키지화
- 중복 Knee Fixer 중 아바타 루트에 가장 가까운 하나만 적용

## v2.0.2 (2026-07-23)

### Fixed

- 아바타 루트의 위치·회전·스케일에 따른 무릎 위치 오류
    - Modular Avatar의 Manual Bake
    - AvaPo!의 Clone

## v2.0.1 (2026-07-23)

### Added

- Milfy 프리셋

### Changed

- 불러오기에 실패한 프리셋은 목록에서 제외하고 경고 표시

## v2.0.0 (2026-07-13)

### Added

- Setup 메뉴
- 커스텀 프리셋 에셋 및 생성 메뉴
- 인스펙터에 Knee Fixer 버전 표시

### Changed

- 자동 설정 적용 시점을 아바타 빌드 과정으로 전환
- 프리셋 저장 방식
    - 고정 목록 → 개별 에셋

### Removed

- Modular Avatar 의존성

## v1.4.0 (2026-05-05)

### Changed

- 일부 파일 이름 및 경로
- 인스펙터의 본 참조 항목 제목
    - `Debug` → `Bone References`

## v1.3.4 (2026-03-18)

### Added

- Ichigo 프리셋
- Riru 프리셋

## v1.3.3 (2026-03-07)

### Added

- Manuka 프리셋

### Changed

- 수동 설정 프리셋 이름
    - `Custom` → `None`
- 아바타 프리셋 이름에 일본어 이름을 함께 표시

### Removed

- 프리셋 프리팹

### Fixed

- AAO: Avatar Optimizer의 ‘알 수 없는 컴포넌트’ 경고 문제

## v1.3.2 (2026-02-01)

### Fixed

- VRChat Worlds SDK의 컴파일 실패 문제

## v1.3.1 (2026-01-31)

### Fixed

- VRChat Avatars SDK의 AutoFix 경고 문제

## v1.3.0 (2026-01-31)

### Added

- 자동 설정 기능

### Removed

- README

## v1.2.0 (2026-01-20)

### Added

- Plum 프리셋 프리팹

## v1.1.0 (2025-12-26)

### Added

- Rinasciita 프리셋 프리팹

## v1.0.1 (2025-12-25)

### Changed

- README

## v1.0.0 (2025-12-25)

### Added

- 최초 출시
