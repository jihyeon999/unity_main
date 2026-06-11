using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour, Vector3, Time 등)을 사용하기 위한 네임스페이스

public class DrawerInteract : MonoBehaviour // 서랍 오브젝트에 부착하여 열고 닫는 동작과 잠금 처리를 담당하는 클래스
{
    [Header("서랍 설정")] // 인스펙터에서 아래 변수들을 "서랍 설정" 그룹으로 표시
    public Transform drawer; // 실제로 움직일 서랍 본체의 Transform (보통 자기 자신이나 자식 오브젝트)
    public float openOffset = 0.5f; // 서랍이 열렸을 때 닫힌 위치에서 X축으로 이동할 거리
    public float openSpeed = 3f; // 서랍이 열리고 닫힐 때의 이동 속도(보간 계수)
    public bool isLocked = false; // 서랍이 잠겨 있는지 여부

    [Header("잠금 설정")] // 인스펙터에서 아래 변수를 "잠금 설정" 그룹으로 표시
    public string requiredKeyName = "서랍 열쇠"; // 서랍 잠금을 해제하는 데 필요한 열쇠 아이템 이름

    private Vector3 closedPos; // 서랍이 닫혀 있을 때의 로컬 위치 (시작 시 저장)
    private Vector3 openPos; // 서랍이 열렸을 때의 로컬 위치 (closedPos + openOffset)
    private bool isOpen = false; // 현재 서랍이 열린 상태인지 여부
    private bool isMoving = false; // 현재 서랍이 이동(애니메이션) 중인지 여부

    public bool IsOpen // 외부에서 서랍의 열림 여부를 읽기 전용으로 확인할 수 있는 프로퍼티
    {
        get { return isOpen; } // isOpen 값을 그대로 반환
    }

    void Start() // 씬이 시작될 때 한 번 호출되어 닫힘/열림 위치를 계산하는 함수
    {
        closedPos = drawer.localPosition; // 현재(에디터에 배치된) 위치를 "닫힘" 기준 위치로 저장
        openPos = closedPos + new Vector3(openOffset, 0f, 0f); // 닫힘 위치에서 X축으로 openOffset만큼 이동한 위치를 "열림" 위치로 계산
    }

    void Update() // 매 프레임마다 호출되어 서랍의 이동 애니메이션을 처리하는 함수
    {
        if (isMoving) // 서랍이 현재 이동 중이라면
        {
            Vector3 target; // 이번 프레임에 도달해야 할 목표 위치를 저장할 변수

            if (isOpen) // 서랍이 열리는 중이라면
            {
                target = openPos; // 목표 위치를 "열림" 위치로 설정
            }
            else // 서랍이 닫히는 중이라면
            {
                target = closedPos; // 목표 위치를 "닫힘" 위치로 설정
            }

            drawer.localPosition = Vector3.Lerp(drawer.localPosition, target, Time.deltaTime * openSpeed); // 현재 위치에서 목표 위치로 선형 보간하여 부드럽게 이동

            if (Vector3.Distance(drawer.localPosition, target) < 0.001f) // 목표 위치와의 거리가 거의 0(0.001 미만)이 되면
            {
                drawer.localPosition = target; // 위치를 목표 위치로 정확히 고정하여 오차 누적 방지
                isMoving = false; // 이동이 끝났으므로 이동 상태를 false로 변경
            }
        }
    }

    public void TryInteract() // 플레이어가 서랍과 상호작용을 시도할 때 호출되는 함수
    {
        if (isLocked) // 서랍이 잠겨 있다면
        {
            if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(requiredKeyName)) // 인벤토리 매니저가 존재하고 필요한 열쇠를 보유하고 있다면
            {
                Unlock(); // 서랍 잠금을 해제하는 함수 호출
            }
            else // 필요한 열쇠가 없다면
            {
                Debug.Log("잠겨있다. 열쇠가 필요할 것 같다."); // 디버그 콘솔에 상태 로그 출력

                if (GameMessageUI.Instance != null) // 메시지 UI 싱글톤이 존재하면
                {
                    GameMessageUI.Instance.ShowSequence( // 여러 줄의 메시지를 순서대로 표시
                        "잠겨있다. 열쇠가 필요할 것 같다.", // 1. 서랍이 잠겨있음을 알림
                        "이 근처 어딘가에 맞는 열쇠가 있을 텐데..." // 2. 힌트 대사
                    );
                }

                return; // 잠금이 해제되지 않았으므로 서랍을 열지 않고 함수 종료
            }
        }

        isOpen = !isOpen; // 서랍의 열림 상태를 반전(토글)
        isMoving = true; // 위치 이동 애니메이션을 시작하도록 이동 상태를 true로 설정
    }

    public void Unlock() // 서랍의 잠금을 해제하는 함수
    {
        isLocked = false; // 잠금 상태를 false로 변경

        if (InventoryManager.Instance != null) // 인벤토리 매니저가 존재하면
        {
            InventoryManager.Instance.RemoveItem(requiredKeyName); // 사용한 열쇠 아이템을 인벤토리에서 제거
        }

        Debug.Log("잠금을 풀었다."); // 디버그 콘솔에 잠금 해제 로그 출력

        if (GameMessageUI.Instance != null) // 메시지 UI 싱글톤이 존재하면
        {
            GameMessageUI.Instance.ShowSequence( // 여러 줄의 메시지를 순서대로 표시
                "잠금을 풀었다.", // 1. 잠금 해제 안내
                "좋아, 이제 안을 확인해보자." // 2. 다음 행동 유도 대사
            );
        }
    }
}
