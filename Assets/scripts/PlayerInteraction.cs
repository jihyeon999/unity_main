using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour, Physics, Input 등)을 사용하기 위한 네임스페이스
using TMPro; // TextMeshPro UI 컴포넌트(TextMeshProUGUI)를 사용하기 위한 네임스페이스

public class PlayerInteraction : MonoBehaviour // 플레이어가 바라보는 오브젝트와의 상호작용을 총괄하는 클래스
{
    [Header("설정")] // 인스펙터에서 아래 변수들을 "설정" 그룹으로 표시
    public float interactDistance = 2.5f; // 상호작용 가능한 최대 거리
    public LayerMask interactLayer; // 상호작용 검사 대상이 되는 레이어 마스크
    public TextMeshProUGUI promptText; // 화면에 상호작용 안내 문구를 표시할 텍스트 UI

    void Start() // 씬이 시작될 때 호출되는 초기화 함수
    {
        if (promptText != null) // 안내 텍스트 UI가 연결되어 있다면
        {
            promptText.gameObject.SetActive(false); // 시작 시 안내 텍스트를 비활성화하여 화면에 보이지 않게 함
        }
    }

    void Update() // 매 프레임마다 호출되어 상호작용 검사를 수행하는 함수
    {
        if (GameProgress.InputLocked || GameProgress.JustUnlocked) // 입력이 잠겨 있거나, 방금 막 잠금이 해제된 프레임이라면
        {
            if (promptText != null) // 안내 텍스트 UI가 연결되어 있다면
            {
                promptText.gameObject.SetActive(false); // 안내 텍스트를 숨김 (UI 사용 중에는 상호작용 안내를 표시하지 않음)
            }

            return; // 입력이 잠긴 상태이므로 상호작용 검사를 하지 않고 함수 종료
        }

        CheckForInteractable(); // 플레이어가 바라보는 방향에 상호작용 가능한 오브젝트가 있는지 검사
    }

    void CheckForInteractable() // 시야 방향으로 광선을 쏘아 상호작용 가능한 오브젝트를 찾아 처리하는 함수
    {
        Ray ray = new Ray(transform.position, transform.forward); // 플레이어(카메라)의 현재 위치에서 정면 방향으로 향하는 광선을 생성

        RaycastHit[] hits = Physics.RaycastAll(ray, interactDistance, interactLayer); // 광선이 interactDistance 이내에서 interactLayer에 속한 모든 콜라이더와 충돌한 결과를 가져옴
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance)); // 충돌 결과들을 거리(distance)가 가까운 순서로 정렬

        foreach (RaycastHit hit in hits) // 거리순으로 정렬된 충돌 결과를 가까운 것부터 순회
        {
            MemoInteract memo = hit.collider.GetComponentInParent<MemoInteract>(); // 충돌한 오브젝트(또는 부모)에서 MemoInteract 컴포넌트를 찾음

            if (memo != null && IsContainerOpen(memo.transform)) // 메모 컴포넌트가 있고, 메모가 들어있는 서랍 등이 닫혀있지 않다면
            {
                ShowPrompt("[E] 읽기"); // 화면에 "[E] 읽기" 안내 표시

                if (Input.GetKeyDown(KeyCode.E)) // E 키가 눌렸다면
                {
                    memo.Read(); // 메모를 읽는 동작 실행
                }

                return; // 가장 가까운 상호작용 대상 처리를 완료했으므로 이후 검사는 하지 않고 종료
            }

            BookInteract book = hit.collider.GetComponentInParent<BookInteract>(); // 충돌한 오브젝트(또는 부모)에서 BookInteract 컴포넌트를 찾음

            if (book != null && book.CanInteract && IsContainerOpen(book.transform)) // 책 컴포넌트가 있고, 상호작용 가능 조건을 만족하며, 책이 들어있는 컨테이너가 닫혀있지 않다면
            {
                ShowPrompt("[E] 책 살펴보기"); // 화면에 "[E] 책 살펴보기" 안내 표시

                if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) // E 키 또는 마우스 왼쪽 버튼이 눌렸다면
                {
                    book.Interact(); // 책과 상호작용 실행
                }

                return; // 가장 가까운 상호작용 대상 처리를 완료했으므로 이후 검사는 하지 않고 종료
            }

            LaptopInteract laptop = hit.collider.GetComponentInParent<LaptopInteract>(); // 충돌한 오브젝트(또는 부모)에서 LaptopInteract 컴포넌트를 찾음

            if (laptop != null) // 노트북 컴포넌트를 찾았다면
            {
                ShowPrompt("[E] 노트북 사용"); // 화면에 "[E] 노트북 사용" 안내 표시

                if (Input.GetKeyDown(KeyCode.E)) // E 키가 눌렸다면
                {
                    laptop.TryInteract(); // 노트북 사용을 시작/종료
                }

                return; // 가장 가까운 상호작용 대상 처리를 완료했으므로 이후 검사는 하지 않고 종료
            }

            OfficeFilesInteract files = hit.collider.GetComponentInParent<OfficeFilesInteract>(); // 충돌한 오브젝트(또는 부모)에서 OfficeFilesInteract 컴포넌트를 찾음

            if (files != null && IsContainerOpen(files.transform)) // 서류 컴포넌트가 있고, 서류가 들어있는 컨테이너가 닫혀있지 않다면
            {
                ShowPrompt("[E] 서류 살펴보기"); // 화면에 "[E] 서류 살펴보기" 안내 표시

                if (Input.GetKeyDown(KeyCode.E)) // E 키가 눌렸다면
                {
                    files.Interact(); // 서류와 상호작용(퀴즈 열기 등) 실행
                }

                return; // 가장 가까운 상호작용 대상 처리를 완료했으므로 이후 검사는 하지 않고 종료
            }

            ItemPickup pickup = hit.collider.GetComponentInParent<ItemPickup>(); // 충돌한 오브젝트(또는 부모)에서 ItemPickup 컴포넌트를 찾음

            if (pickup != null && IsContainerOpen(pickup.transform)) // 아이템 습득 컴포넌트가 있고, 아이템이 들어있는 컨테이너가 닫혀있지 않다면
            {
                if (pickup.itemData != null) // 아이템 데이터가 연결되어 있다면
                {
                    ShowPrompt("[E] 줍기: " + pickup.itemData.itemName); // 아이템 이름을 포함한 안내 문구 표시 (예: "[E] 줍기: 서랍 열쇠")
                }
                else // 아이템 데이터가 연결되어 있지 않다면
                {
                    ShowPrompt("[E] 줍기"); // 이름 없이 일반적인 "[E] 줍기" 안내 표시
                }

                if (Input.GetKeyDown(KeyCode.E)) // E 키가 눌렸다면
                {
                    pickup.Pickup(); // 아이템을 줍는 동작 실행
                }

                return; // 가장 가까운 상호작용 대상 처리를 완료했으므로 이후 검사는 하지 않고 종료
            }
        }

        foreach (RaycastHit hit in hits) // 위 1차 순회에서 처리되지 않았다면, 다시 거리순으로 순회 (서랍은 우선순위를 낮춰 별도 처리)
        {
            DrawerInteract drawer = hit.collider.GetComponentInParent<DrawerInteract>(); // 충돌한 오브젝트(또는 부모)에서 DrawerInteract 컴포넌트를 찾음

            if (drawer != null) // 서랍 컴포넌트를 찾았다면
            {
                if (drawer.IsOpen) // 서랍이 이미 열려 있다면
                {
                    ShowPrompt("[E] 닫기"); // 화면에 "[E] 닫기" 안내 표시
                }
                else // 서랍이 닫혀 있다면
                {
                    ShowPrompt("[E] 열기"); // 화면에 "[E] 열기" 안내 표시
                }

                if (Input.GetKeyDown(KeyCode.E)) // E 키가 눌렸다면
                {
                    drawer.TryInteract(); // 서랍을 열거나 닫는 동작 실행
                }

                return; // 서랍 처리를 완료했으므로 이후 검사는 하지 않고 종료
            }
        }

        if (promptText != null) // 안내 텍스트 UI가 연결되어 있다면
        {
            promptText.gameObject.SetActive(false); // 상호작용 가능한 대상이 없으므로 안내 텍스트를 비활성화
        }
    }

    bool IsContainerOpen(Transform target) // 대상이 서랍 등 컨테이너 안에 있을 때, 그 컨테이너가 열려 있는지 확인하는 함수
    {
        DrawerInteract container = target.GetComponentInParent<DrawerInteract>(); // 대상의 부모 계층에서 DrawerInteract 컴포넌트를 찾음 (서랍 안에 들어있는 경우)
        return container == null || container.IsOpen; // 서랍이 아예 없으면(컨테이너 밖) true, 서랍이 있다면 그 서랍이 열려있을 때만 true 반환
    }

    void ShowPrompt(string message) // 화면에 상호작용 안내 문구를 표시하는 함수
    {
        if (promptText == null) return; // 안내 텍스트 UI가 없으면 아무것도 하지 않고 종료

        promptText.gameObject.SetActive(true); // 안내 텍스트 UI를 활성화하여 화면에 표시
        promptText.text = message; // 안내 텍스트 내용을 전달받은 메시지로 설정
    }
}
