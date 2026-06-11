using UnityEngine; // Unity 엔진의 기본 기능(ScriptableObject 등)을 사용하기 위한 네임스페이스

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")] // 프로젝트 창에서 우클릭 메뉴(Inventory/Item)를 통해 이 타입의 에셋(.asset)을 생성할 수 있게 함
public class Item : ScriptableObject // 인벤토리에서 사용하는 아이템 데이터를 정의하는 ScriptableObject 클래스
{
    public string itemName;      // 아이템 이름
    public Sprite icon;          // 인벤토리 UI에 표시될 아이콘
    public bool isKey = true;    // 열쇠류인지 여부

    [Header("획득 시 대사")] // 인스펙터에서 아래 변수를 "획득 시 대사" 그룹으로 표시
    [TextArea(2, 4)] // 인스펙터에서 여러 줄 입력이 가능한 텍스트 영역으로 표시 (최소 2줄, 최대 4줄)
    public string pickupLine = ""; // 아이템을 주웠을 때 나올 독백
}
