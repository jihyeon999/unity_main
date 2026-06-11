using UnityEngine;

public class DrawerInteract : MonoBehaviour
{
    [Header("서랍 설정")]
    public Transform drawer; //열릴 서랍 오브젝트(cabinet1 or cabinet2)
    public float openOffset = 0.5f; //서랍이 열릴 x 거리(서랍이 어디까지 열릴지를 정함)
    public float openSpeed = 3f; //열리는 속도
    public bool isLocked = false; //잠김 여부

    // 이 서랍을 여는 데 필요한 열쇠 이름
    [Header("잠금 설정")]
    public string requiredKeyName = "서랍 열쇠";

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool isOpen = false;
    private bool isMoving = false;

    public bool IsOpen
    {
        get
        {
            return isOpen; //isOpen을 호출하면 isOpen 값을 돌려줌
        }
    }

    void Start()
    {
        closedPos = drawer.localPosition; //시작할 때 서랍의 현재 위치를 닫힌 위치로 저장함
        openPos = closedPos + new Vector3(openOffset, 0f, 0f); //닫힌 위치에서 X축 방향으로 openOffset만큼 이동한 위치를 열린 위치로 정함
    }

    void Update()
    {
        if (isMoving) //서랍 열기/닫기 애니메이션
        {
            Vector3 target;

            if (isOpen)
            {
                target = openPos;
            }
            else
            {
                target = closedPos;
            }
            drawer.localPosition = Vector3.Lerp(drawer.localPosition, target, Time.deltaTime * openSpeed);

            if (Vector3.Distance(drawer.localPosition, target) < 0.001f)
            {
                drawer.localPosition = target;
                isMoving = false;
            }
        }
    }

    // PlayerInteraction.cs에서 호출
    public void TryInteract()
    {
        // 1. 잠겨있는 경우
        if (isLocked)
        {
            // 인벤토리에 열쇠가 있는지 확인 (싱글톤 매니저 호출)
            if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(requiredKeyName))
            {
                Unlock(); // 열쇠가 있으면 잠금 해제
            }
            else
            {
                Debug.Log("잠겨있다. 열쇠가 필요할 것 같다.");
                if (GameMessageUI.Instance != null)
                {
                    GameMessageUI.Instance.ShowMessage("잠겨있다. 열쇠가 필요할 것 같다.");
                }
                return;
            }
        }

        // 2. 잠겨있지 않거나 방금 해제했다면 열기/닫기 실행
        isOpen = !isOpen;
        isMoving = true;
    }

    public void Unlock()
    {
        isLocked = false;
        // 인벤토리 매니저에게 사용한 열쇠를 지우라고 명령합니다.
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveItem(requiredKeyName);
        }
        Debug.Log("잠금을 풀었다.");
        if (GameMessageUI.Instance != null)
        {
            GameMessageUI.Instance.ShowMessage("잠금을 풀었다.");
        }
    }
}