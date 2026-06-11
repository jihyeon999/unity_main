using UnityEngine;
using TMPro; // TextMeshPro 관련 기능을 쓰기 위해

public class PlayerInteraction : MonoBehaviour
{
    [Header("설정")]
    public float interactDistance = 2.5f;   // 상호작용 가능한 최대 거리
    public LayerMask interactLayer;         // 감지할 레이어 (Interactable)
    public TextMeshProUGUI promptText;      // 화면에 띄울 [E] 안내 텍스트 UI

    void Start()
    {
        // 게임이 시작할 때는 텍스트 숨김
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // 메모/노트북/퀴즈 사용 중에는 상호작용 감지 중지
        // (JustUnlocked: 닫은 프레임의 E 입력이 다시 열기로 처리되는 것 방지)
        if (GameProgress.InputLocked || GameProgress.JustUnlocked)
        {
            if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
            return;
        }

        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        // 카메라의 정중앙 방향으로 Ray 생성
        Ray ray = new Ray(transform.position, transform.forward);

        // 서랍처럼 큰 콜라이더가 안의 물건(서류, 열쇠)을 가로채는 문제를 막기 위해
        // 맞은 콜라이더를 전부 가져와서 가까운 순서로 정렬한다
        RaycastHit[] hits = Physics.RaycastAll(ray, interactDistance, interactLayer);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // ---------- 1순위: 서랍이 아닌 구체적인 대상 ----------
        // (서랍 안에 있는 물건은 그 서랍이 열려 있을 때만 상호작용 가능)
        foreach (RaycastHit hit in hits)
        {
            // 포스트잇(memo, memo_1)인지 확인
            MemoInteract memo = hit.collider.GetComponentInParent<MemoInteract>();

            if (memo != null && IsContainerOpen(memo.transform))
            {
                ShowPrompt("[E] 읽기");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    memo.Read();
                }

                return;
            }

            // 책(Books_19)인지 확인 - memo('run')를 읽은 뒤에만 클릭 가능
            BookInteract book = hit.collider.GetComponentInParent<BookInteract>();

            if (book != null && book.CanInteract && IsContainerOpen(book.transform))
            {
                ShowPrompt("[E] 책 살펴보기");

                // E 키 또는 마우스 왼쪽 클릭으로 상호작용
                if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
                {
                    book.Interact();
                }

                return;
            }

            // 노트북인지 확인
            LaptopInteract laptop = hit.collider.GetComponentInParent<LaptopInteract>();

            if (laptop != null)
            {
                ShowPrompt("[E] 노트북 사용");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    laptop.TryInteract();
                }

                return;
            }

            // 서류(Office_Files)인지 확인 - 들어있는 서랍이 열려 있을 때만
            OfficeFilesInteract files = hit.collider.GetComponentInParent<OfficeFilesInteract>();

            if (files != null && IsContainerOpen(files.transform))
            {
                ShowPrompt("[E] 서류 살펴보기");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    files.Interact();
                }

                return;
            }

            // 아이템인지 확인 - 들어있는 서랍이 열려 있을 때만
            ItemPickup pickup = hit.collider.GetComponentInParent<ItemPickup>();

            if (pickup != null && IsContainerOpen(pickup.transform))
            {
                // Item Data를 깜빡하고 연결 안 해도 오류가 나지 않도록
                if (pickup.itemData != null)
                {
                    ShowPrompt("[E] 줍기: " + pickup.itemData.itemName);
                }
                else
                {
                    ShowPrompt("[E] 줍기");
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    pickup.Pickup();
                }

                return;
            }
        }

        // ---------- 2순위: 서랍 ----------
        // 위에서 아무것도 못 찾았을 때만 서랍 열기/닫기 처리
        foreach (RaycastHit hit in hits)
        {
            DrawerInteract drawer = hit.collider.GetComponentInParent<DrawerInteract>();

            if (drawer != null)
            {
                // 서랍이 열려 있으면 닫기, 닫혀 있으면 열기 표시
                if (drawer.IsOpen)
                {
                    ShowPrompt("[E] 닫기");
                }
                else
                {
                    ShowPrompt("[E] 열기");
                }

                // E 키를 누르면 서랍 상호작용 실행
                if (Input.GetKeyDown(KeyCode.E))
                {
                    drawer.TryInteract();
                }

                return;
            }
        }

        // 아무것도 감지하지 못했으면 텍스트 숨김
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    // 대상이 서랍 안에 들어있는 물건이라면, 그 서랍이 열려 있는지 확인
    // (서랍 안이 아니면 항상 true)
    bool IsContainerOpen(Transform target)
    {
        DrawerInteract container = target.GetComponentInParent<DrawerInteract>();
        return container == null || container.IsOpen;
    }

    void ShowPrompt(string message)
    {
        if (promptText == null) return;
        promptText.gameObject.SetActive(true);
        promptText.text = message;
    }
}
