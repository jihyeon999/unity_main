using UnityEngine;

public class ItemPickup : MonoBehaviour //월드에 놓인 아이템에 붙이는 스크립트
{
    public Item itemData; // 아까 만든 ScriptableObject 연결

    void Start()
    {
        // 레이어/콜라이더 설정을 깜빡해도 주울 수 있도록 자동 보정
        // (오브젝트가 비활성 상태였다가 나중에 켜지는 경우에도 그 시점에 실행됨)
        EnsureInteractable();
    }

    void EnsureInteractable()
    {
        // 1. 레이어를 Interactable로 (자식 포함)
        int layer = LayerMask.NameToLayer("Interactable");
        if (layer >= 0)
        {
            gameObject.layer = layer;
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = layer;
            }
        }

        // 2. 콜라이더가 하나도 없으면 모델 크기에 맞는 BoxCollider 자동 추가
        if (GetComponentInChildren<Collider>(true) == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            Renderer[] rs = GetComponentsInChildren<Renderer>(true);
            if (rs.Length > 0)
            {
                Bounds b = rs[0].bounds;
                foreach (Renderer r in rs)
                {
                    b.Encapsulate(r.bounds);
                }

                // 열쇠처럼 작은 물건도 조준하기 쉽도록 최소 크기 보장 (월드 기준 약 12cm)
                Vector3 size = b.size;
                size.x = Mathf.Max(size.x, 0.12f);
                size.y = Mathf.Max(size.y, 0.12f);
                size.z = Mathf.Max(size.z, 0.12f);

                box.center = transform.InverseTransformPoint(b.center);
                Vector3 ls = transform.lossyScale;
                box.size = new Vector3(
                    size.x / Mathf.Max(Mathf.Abs(ls.x), 0.0001f),
                    size.y / Mathf.Max(Mathf.Abs(ls.y), 0.0001f),
                    size.z / Mathf.Max(Mathf.Abs(ls.z), 0.0001f));
            }

            // 물리 충돌 없이 시선 감지만 되도록
            box.isTrigger = true;
        }
    }

    public void Pickup()
    {
        InventoryManager.Instance.AddItem(itemData);
        Destroy(gameObject); // 월드에서 제거
    }
}
