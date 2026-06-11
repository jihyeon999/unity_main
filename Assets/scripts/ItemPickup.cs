using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour, Collider, Renderer 등)을 사용하기 위한 네임스페이스

public class ItemPickup : MonoBehaviour // 월드에 배치된 아이템 오브젝트가 플레이어에게 습득되는 동작을 담당하는 클래스
{
    public Item itemData; // ScriptableObject로 만든 아이템 데이터 연결

    void Start() // 씬이 시작될 때 호출되어 상호작용 가능 상태를 보장하는 함수
    {
        EnsureInteractable(); // 레이어와 콜라이더가 올바르게 설정되어 있는지 점검 및 자동 설정
    }

    void EnsureInteractable() // 이 오브젝트가 상호작용 가능하도록 레이어/콜라이더를 자동으로 보정하는 함수
    {
        // Interactable 레이어가 있으면 자동으로 설정
        int layer = LayerMask.NameToLayer("Interactable"); // "Interactable"이라는 이름의 레이어 번호를 가져옴 (없으면 -1)

        if (layer >= 0) // "Interactable" 레이어가 프로젝트에 존재한다면
        {
            gameObject.layer = layer; // 이 오브젝트의 레이어를 "Interactable"로 설정

            foreach (Transform t in GetComponentsInChildren<Transform>(true)) // 자기 자신을 포함한 모든 자식 Transform을 순회 (비활성 오브젝트 포함)
            {
                t.gameObject.layer = layer; // 각 자식 오브젝트의 레이어도 "Interactable"로 설정
            }
        }

        // 콜라이더가 없으면 자동으로 BoxCollider 추가
        if (GetComponentInChildren<Collider>(true) == null) // 자기 자신과 자식들 중 어떤 콜라이더도 없다면 (비활성 포함)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>(); // 이 오브젝트에 BoxCollider 컴포넌트를 새로 추가

            Renderer[] renderers = GetComponentsInChildren<Renderer>(true); // 자기 자신과 자식들의 모든 Renderer 컴포넌트를 가져옴 (비활성 포함)

            if (renderers.Length > 0) // 렌더러가 하나 이상 존재한다면 (시각적으로 보이는 메시가 있다면)
            {
                Bounds bounds = renderers[0].bounds; // 첫 번째 렌더러의 월드 공간 경계(Bounds)를 기준값으로 설정

                foreach (Renderer r in renderers) // 모든 렌더러를 순회하며
                {
                    bounds.Encapsulate(r.bounds); // 각 렌더러의 경계를 포함하도록 전체 경계 범위를 확장
                }

                Vector3 size = bounds.size; // 계산된 전체 경계의 크기(가로/세로/높이)를 가져옴

                size.x = Mathf.Max(size.x, 0.12f); // X축 크기가 너무 작으면 최소 0.12로 보정 (콜라이더가 너무 얇아지는 것 방지)
                size.y = Mathf.Max(size.y, 0.12f); // Y축 크기가 너무 작으면 최소 0.12로 보정
                size.z = Mathf.Max(size.z, 0.12f); // Z축 크기가 너무 작으면 최소 0.12로 보정

                box.center = transform.InverseTransformPoint(bounds.center); // 경계의 월드 중심점을 이 오브젝트의 로컬 좌표로 변환하여 콜라이더 중심으로 설정

                Vector3 scale = transform.lossyScale; // 이 오브젝트의 월드 스케일(부모 스케일까지 누적된 값)을 가져옴

                box.size = new Vector3( // BoxCollider의 크기를 로컬 스케일 기준으로 환산하여 설정
                    size.x / Mathf.Max(Mathf.Abs(scale.x), 0.0001f), // 월드 크기를 X축 스케일로 나누어 로컬 크기로 변환 (0으로 나누는 것 방지)
                    size.y / Mathf.Max(Mathf.Abs(scale.y), 0.0001f), // 월드 크기를 Y축 스케일로 나누어 로컬 크기로 변환
                    size.z / Mathf.Max(Mathf.Abs(scale.z), 0.0001f)  // 월드 크기를 Z축 스케일로 나누어 로컬 크기로 변환
                );
            }

            box.isTrigger = true; // 콜라이더를 트리거로 설정하여 물리적 충돌 없이 상호작용 감지 등에 사용 가능하게 함
        }
    }

    public void Pickup() // 플레이어가 이 아이템을 주울 때 호출되는 함수
    {
        if (itemData == null) // 인스펙터에 아이템 데이터가 연결되어 있지 않다면
        {
            Debug.LogWarning(name + "에 Item Data가 연결되어 있지 않습니다."); // 경고 로그를 출력하여 설정 누락을 알림
            return; // 아이템 데이터가 없으므로 더 이상 진행하지 않고 종료
        }

        InventoryManager.Instance.AddItem(itemData); // 인벤토리 매니저에 이 아이템을 추가

        if (GameMessageUI.Instance != null) // 메시지 UI 싱글톤이 존재하면
        {
            if (!string.IsNullOrEmpty(itemData.pickupLine)) // 아이템에 지정된 획득 대사가 비어있지 않다면
            {
                GameMessageUI.Instance.QueueMessage(itemData.pickupLine); // 아이템에 설정된 전용 획득 대사를 메시지 큐에 추가
            }
            else // 아이템에 별도의 획득 대사가 설정되어 있지 않다면
            {
                GameMessageUI.Instance.QueueMessage(GetDefaultPickupLine(itemData.itemName)); // 아이템 이름을 기준으로 생성한 기본 대사를 메시지 큐에 추가
            }
        }

        Destroy(gameObject); // 아이템을 주웠으므로 월드에서 이 오브젝트를 제거
    }

    string GetDefaultPickupLine(string itemName) // 아이템 이름에 따라 기본 획득 대사를 생성하는 함수
    {
        if (string.IsNullOrEmpty(itemName)) // 아이템 이름이 비어있다면
        {
            return "이거 분명 어딘가에 쓸모가 있을 텐데..."; // 이름이 없을 때의 일반적인 기본 대사 반환
        }

        if (itemName.Contains("문") && itemName.Contains("열쇠")) // 아이템 이름에 "문"과 "열쇠"가 모두 포함되어 있다면 (예: "문 열쇠")
        {
            return "드디어 문 열쇠인가? 이걸로 나갈 수 있겠지."; // 문 열쇠 전용 대사 반환
        }

        if (itemName.Contains("서랍") && itemName.Contains("열쇠")) // 아이템 이름에 "서랍"과 "열쇠"가 모두 포함되어 있다면 (예: "서랍 열쇠")
        {
            return "이 열쇠... 서랍에 맞을 것 같은데?"; // 서랍 열쇠 전용 대사 반환
        }

        if (itemName.Contains("열쇠")) // 아이템 이름에 "열쇠"만 포함되어 있다면 (어떤 열쇠인지 특정되지 않은 경우)
        {
            return "이 열쇠, 분명 어딘가에 맞을 것 같은데."; // 일반 열쇠용 대사 반환
        }

        return "이거 분명 어딘가에 쓸모가 있을 텐데..."; // 위 조건에 모두 해당하지 않는 일반 아이템의 기본 대사 반환
    }
}
