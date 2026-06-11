using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;      // 아이템 이름 (열쇠A, 열쇠B 등)
    public Sprite icon;          // 인벤토리 UI에 보일 아이콘
    public bool isKey = true;    // 열쇠인지 여부
}
