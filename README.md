## 플레이 사진
<img width="1260" height="706" alt="image" src="https://github.com/user-attachments/assets/64828964-e1da-440a-a2fe-9d6e38bb3b17" />


## 구조 요약

- **MirrorManager** (싱글톤) : Mirror 생성/삭제/선택/편집 담당. Mirror 상태 변경 시 `OnMirrorChanged` 호출.
- **MirrorController** : 개별 Mirror의 EditMode에 따른 Material Setting 관리.
- **LaserManager** : 반사 경로 계산 및 Receiver 적중 시, Target Receiver의 `SetState()` 호출.
- **ReceiverManager** : Idle/Success 상태 관리. 연출은 `HighlightEffect`사용.
- **HighlightEffect** : 색상 및 Shake 연출 컴포넌트. (Receiver 및 Laser 공통 사용)
- **GameManager** (싱글톤) : UI 및 Receiver 관리. All Receiver Success 판정의 연출 담당.
- **ToastController** (싱글톤) : 2D UI 토스트 메시지 출력 담당.

***Mirror 조작 → OnMirrorChanged → Laser 경로 재계산 → Receiver 적중/이탈 → GameManager 판정***

## 조작 방법

좌측 상단 Help Button 클릭으로 확인 가능
<img width="367" height="278" alt="image" src="https://github.com/user-attachments/assets/85e4bc0b-b7fc-4f2a-a0c8-4ce529b59eb6" />

| 입력 | 동작 |
| --- | --- |
| `M` | Mirror 생성 |
| `R` | 선택된 Mirror 회전 초기화 |
| `Delete` | 선택된 Mirror 삭제 |
| `C` | 모든 Mirror 삭제 |
| `Tab` | 회전 모드 전환 (Pitch / Yaw) |
| 좌클릭 | Mirror 선택 |
| 좌클릭 드래그 | 위치 이동 |
| 우클릭 드래그 | 회전 (X / Y) |
| 마우스 휠 | 회전 (Z) |

## 소요 시간
약 **8시간** 소요

| 항목 | 시간 |
| --- | --- |
| 아키텍처 설계 | 30분 |
| UI Sprite 제작 | 30분 |
| 구현 및 버그 수정 | 약 6시간 |
| 테스트 & 문서 작성 | 약 1시간 |
