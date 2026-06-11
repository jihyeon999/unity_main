using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour, Input, Cursor 등)을 사용하기 위한 네임스페이스
using System.Collections.Generic; // List<T> 등 제네릭 컬렉션을 사용하기 위한 네임스페이스

public class InventoryManager : MonoBehaviour // 플레이어의 아이템 인벤토리와 관련 UI를 관리하는 클래스
{
    public static InventoryManager Instance; // 어디서든 접근 가능하게 싱글톤 설정
    public List<Item> items = new List<Item>(); // 플레이어가 획득한 아이템 저장소

    [Header("UI 설정")] // 인스펙터에서 아래 변수들을 "UI 설정" 그룹으로 표시
    public GameObject inventoryUI; // 인벤토리 UI 오브젝트
    public Transform slotParent;   // 슬롯들이 모여 있는 부모 오브젝트
    private InventorySlot[] slots; // 모든 슬롯 리스트

    private bool isInventoryOpen = false; // 현재 인벤토리 창이 열려 있는지 여부

    void Awake() // 오브젝트가 생성될 때 가장 먼저 호출되는 초기화 함수
    {
        Instance = this; // 자기 자신을 싱글톤 인스턴스로 등록하여 다른 스크립트에서 접근 가능하게 함
    }

    void Start() // 씬이 시작될 때 호출되어 슬롯 목록을 초기화하는 함수
    {
        slots = slotParent.GetComponentsInChildren<InventorySlot>(true); // slotParent의 자식들(비활성 포함)에서 InventorySlot 컴포넌트를 모두 찾아 배열로 저장
        UpdateUI(); // 현재 보유 아이템 목록을 슬롯 UI에 반영
    }

    void Update() // 매 프레임마다 호출되어 인벤토리 토글 입력을 처리하는 함수
    {
        if (Input.GetKeyDown(KeyCode.I) && !GameProgress.InputLocked) // 이번 프레임에 I 키가 새로 눌렸고, 입력이 잠겨있지 않다면
        {
            ToggleInventory(); // 인벤토리 창을 열거나 닫음
        }
    }

    public void AddItem(Item newItem) // 아이템을 인벤토리에 추가하는 함수
    {
        items.Add(newItem); // 전달받은 아이템을 보유 아이템 리스트에 추가
        Debug.Log(newItem.itemName + " 획득!"); // 디버그 콘솔에 아이템 획득 로그 출력

        if (GameMessageUI.Instance != null) // 메시지 UI 싱글톤이 존재하면
        {
            GameMessageUI.Instance.ShowMessage(newItem.itemName + "을(를) 획득했다."); // 화면에 아이템 획득 메시지를 표시
        }

        UpdateUI(); // 변경된 아이템 목록을 슬롯 UI에 반영
    }

    public void UpdateUI() // 보유 아이템 리스트를 인벤토리 슬롯 UI에 동기화하는 함수
    {
        for (int i = 0; i < slots.Length; i++) // 모든 슬롯을 순서대로 순회
        {
            if (i < items.Count) // 현재 슬롯 인덱스에 해당하는 아이템이 보유 목록에 존재한다면
            {
                slots[i].AddItem(items[i]); // 해당 슬롯에 아이템 정보를 표시
            }
            else // 해당 인덱스에 아이템이 없다면
            {
                slots[i].ClearSlot(); // 해당 슬롯을 빈 상태로 초기화
            }
        }
    }

    public void RemoveItem(string targetName) // 이름으로 아이템을 찾아 인벤토리에서 제거하는 함수
    {
        Item target = items.Find(x => x.itemName == targetName); // 보유 아이템 중 이름이 일치하는 첫 번째 아이템을 검색

        if (target != null) // 일치하는 아이템을 찾았다면
        {
            items.Remove(target); // 해당 아이템을 보유 목록에서 제거
            UpdateUI(); // 변경된 아이템 목록을 슬롯 UI에 반영
        }
    }

    public bool HasItem(string targetName) // 특정 이름의 아이템을 보유하고 있는지 확인하는 함수
    {
        return items.Exists(x => x.itemName == targetName); // 보유 목록에 이름이 일치하는 아이템이 하나라도 있으면 true 반환
    }

    public void ToggleInventory() // 인벤토리 UI를 열고 닫는 함수
    {
        isInventoryOpen = !isInventoryOpen; // 인벤토리 열림 상태를 반전(토글)
        inventoryUI.SetActive(isInventoryOpen); // 인벤토리 UI 오브젝트의 활성화 상태를 갱신된 값으로 설정

        if (isInventoryOpen) // 인벤토리가 열렸다면
        {
            Cursor.lockState = CursorLockMode.None; // 마우스 커서 잠금을 해제하여 자유롭게 움직일 수 있게 함
            Cursor.visible = true; // 마우스 커서를 화면에 보이게 함
        }
        else // 인벤토리가 닫혔다면
        {
            Cursor.lockState = CursorLockMode.Locked; // 마우스 커서를 다시 화면 중앙에 고정 (FPS 시점 조작 복귀)
            Cursor.visible = false; // 마우스 커서를 다시 숨김
        }
    }
}
