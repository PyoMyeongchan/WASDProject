# 데일리 행운 슬롯 (Daily Luck Slot)

Unity 2D 모바일 캐주얼 미니게임. 토스처럼 앱 안에서 짧게 즐기는 하루 한 번 행운 뽑기.

---

## 주요 기능

- **하루 1회 슬롯 스핀** — PlayerPrefs로 날짜를 저장해 중복 실행 방지
- **3릴 슬롯 애니메이션** — 행운 물건 / 행운 숫자 / 행운 문구 순차 공개
- **결과 카드 페이드인** — 뽑기 결과를 카드 형태로 애니메이션과 함께 표시
- **히스토리 패널** — 최근 30일 뽑기 기록을 JSON으로 로컬 저장 및 조회
- **공유 기능** — 결과 텍스트 클립보드 복사 + 화면 캡처 (NativeShare 연동 준비)
- **서버 시드 지원** — UnityWebRequest로 서버에서 일일 시드를 받아 결과 결정론적 고정
- **오디오** — 스핀 시작 / 릴 정지 / 카드 공개 / 버튼 클릭 효과음
- **한글 폰트** — Maplestory Bold SDF (Dynamic Atlas, 한글 음절 11,172자 지원)

---

## 아키텍처

```
Assets/
├── Scripts/
│   ├── GameManager.cs           # 최상위 조율자 — 일일 체크, 스핀 흐름 총괄
│   ├── SlotMachineController.cs # 릴 3개 오케스트레이션, OnSpinComplete 이벤트
│   ├── SlotReel.cs              # 개별 릴 텍스트 순환 애니메이션
│   ├── ResultCardUI.cs          # 결과 카드 표시/숨김, 페이드인
│   ├── LuckDataSO.cs            # ScriptableObject — 아이템/숫자/문구 데이터
│   ├── SaveManager.cs           # PlayerPrefs 래퍼 — 날짜·결과 저장
│   ├── LuckRecord.cs            # 기록 데이터 모델 (Serializable)
│   ├── LuckHistoryManager.cs    # JSON 기록 저장·불러오기 (persistentDataPath)
│   ├── HistoryPanel.cs          # 히스토리 UI 패널
│   ├── HistoryItemUI.cs         # 히스토리 리스트 항목 UI
│   ├── AudioManager.cs          # 싱글턴, DontDestroyOnLoad, 효과음 재생
│   ├── ShareManager.cs          # 공유 기능 (클립보드 / NativeShare)
│   └── ServerLuckManager.cs     # 서버 시드 UnityWebRequest 통신
│
├── Editor/
│   ├── LuckSlotSceneBuilder.cs  # 씬 전체 자동 생성 Editor 스크립트
│   └── LuckSlotFontFixer.cs     # Maplestory Bold SDF 에셋 자동 생성
│
├── Data/
│   └── LuckData.asset           # LuckDataSO 인스턴스
│
├── Fonts/
│   ├── Maplestory Bold.ttf
│   └── Maplestory Bold SDF.asset
│
└── Prefabs/
    └── HistoryItemUI.prefab
```

### 데이터 흐름

```
GameManager.Start()
  └─ SaveManager.HasPlayedToday()
       ├─ true  → ResultCardUI.ShowCard(저장된 결과)
       └─ false → [SpinButton 활성화]
                    └─ SlotMachineController.SpinSequence()
                         ├─ SlotReel × 3 (애니메이션 → StopSpin)
                         └─ OnSpinComplete 이벤트
                              ├─ SaveManager.SaveResult()
                              ├─ LuckHistoryManager.AddRecord()
                              └─ ResultCardUI.ShowCard()
```

---

## 에디터 도구 (Unity 메뉴 → LuckSlot)

| 메뉴 항목 | 설명 |
|---|---|
| 전체 씬 자동 생성 ★ | Canvas 계층 전체를 코드로 빌드 |
| 한글 폰트 에셋 재생성 ★ | Maplestory Bold SDF Dynamic 에셋 생성 |
| EventSystem 모듈 교체 | New Input System 호환 모듈로 교체 |
| 테스트: 하루 제한 초기화 | PlayerPrefs + JSON 기록 초기화 |

---

## 개발 환경

- Unity 6 (6000.x)
- Universal Render Pipeline (URP)
- TextMeshPro
- New Input System
- 타겟 플랫폼: Android / iOS
