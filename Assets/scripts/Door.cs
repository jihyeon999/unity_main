using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour, Quaternion, Time 등)을 사용하기 위한 네임스페이스

namespace DoorScript // 문 관련 스크립트를 담는 네임스페이스
{
    public class Door : MonoBehaviour // 문 오브젝트에 부착하여 열림/닫힘 동작과 잠금 처리를 담당하는 클래스
    {
        [Header("문 상태")] // 인스펙터에서 아래 변수들을 "문 상태" 그룹으로 표시
        public bool open = false;                  // 문이 열려 있는지 여부
        public float smooth = 1.0f;                // 문 열리고 닫히는 속도
        public float openAngle = -90.0f;           // 문이 열릴 각도

        [Header("잠금 설정")] // 인스펙터에서 아래 변수들을 "잠금 설정" 그룹으로 표시
        public bool isLocked = true;               // 문 잠김 여부
        public string requiredKeyName = "문 열쇠"; // 문을 여는 데 필요한 열쇠 이름

        private Quaternion closedRotation;         // 닫힌 상태 회전값
        private Quaternion openRotation;           // 열린 상태 회전값

        private bool firstLockedMessageShown = false; // 처음 잠긴 문을 조사했는지 여부
        //private bool escapeMessageShown = false;       // 탈출 대사를 이미 보여줬는지 여부

        void Start() // 씬이 시작될 때 한 번 호출되어 회전값 기준점을 초기화하는 함수
        {
            // 게임 시작 시 현재 문의 회전값을 닫힌 상태로 저장
            closedRotation = transform.localRotation; // 현재(에디터에서 배치된) 로컬 회전값을 "닫힘" 상태 기준으로 저장

            // 닫힌 상태 기준으로 openAngle만큼 회전한 값을 열린 상태로 저장
            openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0); // 닫힌 회전값에 Y축으로 openAngle만큼 추가 회전한 값을 "열림" 상태로 계산하여 저장
        }

        void Update() // 매 프레임마다 호출되어 문의 회전을 목표 상태로 보간하는 함수
        {
            Quaternion target; // 이번 프레임에 도달해야 할 목표 회전값을 저장할 변수

            if (open) // 문이 열린 상태라면
            {
                target = openRotation; // 목표 회전값을 "열림" 회전값으로 설정
            }
            else // 문이 닫힌 상태라면
            {
                target = closedRotation; // 목표 회전값을 "닫힘" 회전값으로 설정
            }

            // 현재 회전값에서 목표 회전값까지 부드럽게 회전
            transform.localRotation = Quaternion.Slerp( // 현재 회전값과 목표 회전값을 구면 선형 보간(Slerp)하여 적용
                transform.localRotation, // 보간 시작값: 현재 로컬 회전값
                target, // 보간 목표값: 위에서 계산한 target (열림 또는 닫힘 회전값)
                Time.deltaTime * 5 * smooth // 보간 비율: 프레임 시간에 비례하며 smooth 값이 클수록 빠르게 회전
            );
        }

        public void OpenDoor() // 외부(예: CameraOpenDoor)에서 문을 열거나 닫도록 요청할 때 호출되는 함수
        {
            // 문이 잠겨 있는 경우
            if (isLocked) // 현재 문이 잠금 상태인지 확인
            {
                // 인벤토리에 필요한 열쇠가 있는지 확인
                if (InventoryManager.Instance != null && // 인벤토리 매니저 싱글톤이 존재하고
                    InventoryManager.Instance.HasItem(requiredKeyName)) // 인벤토리에 필요한 열쇠 아이템을 보유하고 있다면
                {
                    // 열쇠가 있으면 잠금 해제
                    isLocked = false; // 문의 잠금 상태를 해제

                    if (GameMessageUI.Instance != null) // 메시지 UI 싱글톤이 존재하면
                    {
                        GameMessageUI.Instance.ShowSequence( // 여러 줄의 메시지를 순서대로 보여주는 함수 호출
                            "문이 열렸다.", // 첫 번째로 표시될 메시지
                            "드디어 집에 갈 수 있겠어!" // 두 번째로 표시될 메시지
                        );
                    }
                }
                else // 필요한 열쇠가 없는 경우
                {
                    // 열쇠가 없으면 문을 열지 않고 메시지만 출력
                    if (GameMessageUI.Instance != null) // 메시지 UI 싱글톤이 존재하면
                    {
                        if (!firstLockedMessageShown) // 잠긴 문을 처음 조사하는 경우라면
                        {
                            GameMessageUI.Instance.ShowSequence( // 여러 줄의 안내 메시지를 순서대로 표시
                                "문이 잠겨 있다.", // 1. 문이 잠겨있다는 사실 안내
                                "어? 잠겨있네...", // 2. 플레이어의 반응
                                "분명 어딘가에 열쇠가 있을 텐데.", // 3. 단서 제공
                                "일단 연구실 안을 좀 찾아보자." // 4. 다음 행동 유도
                            );

                            firstLockedMessageShown = true; // 처음 잠긴 문 메시지를 표시했음을 기록하여 다음부터는 다른 메시지가 나오게 함
                        }
                        else // 이미 처음 메시지를 본 적이 있다면
                        {
                            GameMessageUI.Instance.ShowMessage("아직 열쇠가 없다."); // 짧은 안내 메시지만 다시 표시
                        }
                    }

                    return; // 열쇠가 없으므로 문을 열지 않고 함수 종료
                }
            }

            // 잠겨 있지 않으면 문 열기/닫기
            open = !open; // 문의 열림 상태를 반전(토글)시켜 열려있으면 닫고, 닫혀있으면 엶
        }
    }
}
