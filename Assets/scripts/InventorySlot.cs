using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public string itemName; //현재 슬롯에 담긴 아이템 이름을 저장할 변수 추가

    // 슬롯에 아이템을 표시하는 함수
    public void AddItem(Item newItem)
    {
        itemName = newItem.itemName; //아이템 이름 저장(Item 클래스에 itemName이 있다고 가정)
        icon.sprite = newItem.icon;
        icon.enabled = true; // 아이콘 활성화
    }

    // 슬롯을 비우는 함수
    public void ClearSlot()
    {
        itemName = ""; //이름 초기화
        icon.sprite = null;
        icon.enabled = false; // 아이콘 비활성화
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
