using UnityEngine;

namespace DoorScript
{
    public class Door : MonoBehaviour
    {
        [Header("문 상태")]
        public bool open = false;              // 문이 열려 있는지 여부
        public float smooth = 1.0f;            // 문 열리고 닫히는 속도
        public float openAngle = -90.0f;       // 문이 열릴 각도

        [Header("잠금 설정")]
        public bool isLocked = true;           // 문 잠김 여부
        public string requiredKeyName = "문 열쇠"; // 문을 여는 데 필요한 열쇠 이름

        private Quaternion closedRotation;     // 닫힌 상태 회전값
        private Quaternion openRotation;       // 열린 상태 회전값

        void Start()
        {
            // 게임 시작 시 현재 문의 회전값을 닫힌 상태로 저장
            closedRotation = transform.localRotation;

            // 닫힌 상태 기준으로 openAngle만큼 회전한 값을 열린 상태로 저장
            openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
        }

        void Update()
        {
            Quaternion target;

            if (open)
            {
                target = openRotation;
            }
            else
            {
                target = closedRotation;
            }

            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                target,
                Time.deltaTime * 5 * smooth
            );
        }

        public void OpenDoor()
        {
            // 문이 잠겨 있는 경우
            if (isLocked)
            {
                // 인벤토리에 필요한 열쇠가 있는지 확인
                if (InventoryManager.Instance != null &&
                    InventoryManager.Instance.HasItem(requiredKeyName))
                {
                    // 열쇠가 있으면 잠금 해제
                    isLocked = false;

                    if (GameMessageUI.Instance != null)
                    {
                        GameMessageUI.Instance.ShowMessage("문 잠금을 풀었다.");
                    }
                }
                else
                {
                    // 열쇠가 없으면 문을 열지 않고 메시지만 출력
                    if (GameMessageUI.Instance != null)
                    {
                        GameMessageUI.Instance.ShowMessage("문이 잠겨 있다. 열쇠가 필요하다.");
                    }

                    return;
                }
            }

            // 잠겨 있지 않으면 문 열기/닫기
            open = !open;
        }
    }
}