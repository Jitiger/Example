# Balloon Fight Prototype

Unity 6에서 바로 실행되는 벌룬파이트 스타일 프로토타입입니다.

## 실행

1. 프로젝트를 Unity 6000.3.22f1로 엽니다.
2. 기존 `Assets/Scenes/SampleScene.unity`를 열어도 됩니다.
3. 별도 오브젝트나 프리팹을 만들 필요 없이 Play를 누릅니다.
4. `BalloonFightRuntime`이 실행 시 카메라, 발판, 플레이어, 적, 풍선, HUD를 자동 생성합니다.

## 코드 구조

- `Scripts/Core`: 게임 흐름, 스테이지 및 캐릭터 생성
- `Scripts/Actors`: 플레이어, 적, 풍선 충돌과 공통 캐릭터 로직
- `Scripts/Config`: 조작감과 밸런스 값을 관리하는 `ScriptableObject`
- `Scripts/Pooling`: 적 오브젝트 풀과 풀 수명주기
- `Scripts/UI`: HUD 표시
- `Scripts/Visual`: 임시 런타임 픽셀 그래픽 생성

`BalloonGameConfig` 에셋을 `Assets/Resources/BalloonGameConfig.asset` 경로에 만들면 밸런스 값을 Inspector에서 관리할 수 있습니다. 에셋이 없는 현재 샘플 씬에서는 동일한 기본값을 가진 런타임 설정을 사용합니다.

## 조작

- 좌우 이동: `A / D` 또는 `← / →`
- 날갯짓: `Space`, `Z`, `↑`
- 게임 오버 / 전체 클리어 후 재시작: `R`

## 현재 구현

- 플레이어 2개 풍선
- 적 1개 풍선
- 날갯짓 Impulse 방식 비행
- 공중 수평 관성
- 좌우 화면 래핑
- 플레이어 몸으로 적 풍선 터뜨리기
- 적이 플레이어 풍선 터뜨리기
- 풍선이 하나 남으면 플레이어 상승력 감소
- 적 간단 추적 AI
- 점수 / 목숨 / 3 Phase
- 적 오브젝트 풀링
- `ScriptableObject` 기반 게임 밸런스 설정
- 런타임 픽셀 스타일 임시 그래픽

## 참고 방향

동작 설계는 다음 공개 프로젝트를 참고하되 코드는 새로 작성했습니다.

- LuigiBlood/balloonfight_dis: 원작 NES 상태/속도/풍선 구조 참고
- SoIncredible/BalloonFight_UnityDemo: 공중 수평 관성과 화면 래핑 방식 참고
- BenjixD/BalloonFightClone: 버튼 1회 입력에 Impulse를 주는 플랩 이동 방식 참고

현재 그래픽은 임시 런타임 생성 이미지입니다. 실제 과제용 스프라이트가 준비되면 `RetroFactory` 부분만 교체하면 게임 로직은 유지할 수 있습니다.
