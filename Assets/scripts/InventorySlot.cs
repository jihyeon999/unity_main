using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour 등)을 사용하기 위한 네임스페이스
using UnityEngine.UI; // Image 등 UGUI 컴포넌트를 사용하기 위한 네임스페이스

public class InventorySlot : MonoBehaviour // 인벤토리 UI의 개별 슬롯 하나를 나타내는 클래스
{
    public Image icon; // 아이템 아이콘을 표시할 이미지 UI 컴포넌트
    public string itemName; // 현재 슬롯에 담긴 아이템 이름

    public void AddItem(Item newItem) // 슬롯에 새 아이템 정보를 채워 넣는 함수
    {
        itemName = newItem.itemName; // 슬롯에 표시할 아이템 이름을 새 아이템의 이름으로 설정
        icon.sprite = newItem.icon; // 슬롯 아이콘 이미지를 새 아이템의 아이콘 스프라이트로 설정
        icon.enabled = true; // 아이콘 이미지를 화면에 보이도록 활성화
    }

    public void ClearSlot() // 슬롯을 비어있는 상태로 초기화하는 함수
    {
        itemName = ""; // 슬롯에 저장된 아이템 이름을 빈 문자열로 초기화
        icon.sprite = null; // 슬롯 아이콘 이미지의 스프라이트를 제거
        icon.enabled = false; // 아이콘 이미지를 비활성화하여 빈 슬롯처럼 보이게 함
    }
}
