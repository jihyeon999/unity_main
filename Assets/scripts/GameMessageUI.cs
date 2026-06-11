using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour 등)을 사용하기 위한 네임스페이스
using TMPro; // TextMeshPro UI 컴포넌트(TextMeshProUGUI)를 사용하기 위한 네임스페이스
using System.Collections; // 코루틴(IEnumerator)을 사용하기 위한 네임스페이스
using System.Collections.Generic; // Queue<T> 등 제네릭 컬렉션을 사용하기 위한 네임스페이스

// 화면 하단 안내 메시지 UI
// - ShowMessage: 즉시 표시 (대기 중이던 메시지는 취소하고 새로 표시)
// - QueueMessage: 지금 표시 중인 메시지가 끝난 뒤에 이어서 표시 (독백 연출용)
// - ShowSequence: 여러 줄을 순서대로 표시 (게임 시작 독백 등)
public class GameMessageUI : MonoBehaviour // 화면 하단 메시지 UI를 제어하는 클래스
{
    public static GameMessageUI Instance; // 어디서든 GameMessageUI.Instance로 접근할 수 있도록 하는 싱글톤 인스턴스

    public TextMeshProUGUI messageText; // 메시지를 표시할 TextMeshPro UI 텍스트 컴포넌트
    public float showTime = 2f; // 한 줄당 표시 시간

    private Coroutine routine; // 현재 실행 중인 메시지 출력 코루틴 참조 (중복 실행 방지 및 중지를 위해 보관)
    private Queue<string> queue = new Queue<string>(); // 순서대로 출력할 메시지들을 저장하는 큐(선입선출)

    void Awake() // 오브젝트가 생성될 때 가장 먼저 호출되는 초기화 함수
    {
        Instance = this; // 자기 자신을 싱글톤 인스턴스로 등록하여 다른 스크립트에서 접근 가능하게 함
    }

    void Start() // 씬이 시작될 때 호출되는 초기화 함수
    {
        if (messageText != null) // 메시지 텍스트 UI가 연결되어 있다면
        {
            messageText.gameObject.SetActive(false); // 시작 시 메시지 UI를 비활성화하여 화면에 보이지 않게 함
        }
    }

    public void ShowMessage(string message) // 메시지를 즉시(다른 메시지를 취소하고) 표시하는 함수
    {
        queue.Clear(); // 기존에 대기 중이던 메시지들을 모두 제거
        queue.Enqueue(message); // 새 메시지를 큐에 추가
        Restart(); // 메시지 출력 코루틴을 새로 시작
    }

    public void ShowSequence(params string[] messages) // 여러 개의 메시지를 순서대로 표시하는 함수 (가변 인자)
    {
        queue.Clear(); // 기존에 대기 중이던 메시지들을 모두 제거
        foreach (string m in messages) // 전달받은 메시지 배열을 순회하며
        {
            queue.Enqueue(m); // 각 메시지를 큐에 순서대로 추가
        }
        Restart(); // 메시지 출력 코루틴을 새로 시작
    }

    public void QueueMessage(string message) // 현재 표시 중인 메시지가 끝난 뒤 이어서 표시할 메시지를 추가하는 함수
    {
        queue.Enqueue(message); // 메시지를 큐의 맨 뒤에 추가
        if (routine == null) // 현재 실행 중인 출력 코루틴이 없다면
        {
            Restart(); // 새로 코루틴을 시작하여 큐에 쌓인 메시지를 출력
        }
    }

    void Restart() // 메시지 출력 코루틴을 (다시) 시작하는 내부 함수
    {
        if (messageText == null) return; // 표시할 텍스트 UI가 없으면 아무것도 하지 않고 종료

        if (routine != null) // 이미 실행 중인 코루틴이 있다면
        {
            StopCoroutine(routine); // 기존 코루틴을 중지하여 중복 실행을 방지
        }
        routine = StartCoroutine(RunQueue()); // 큐에 쌓인 메시지를 순서대로 출력하는 코루틴을 새로 시작하고 참조를 저장
    }

    IEnumerator RunQueue() // 큐에 있는 메시지를 하나씩 꺼내 일정 시간 동안 표시하는 코루틴
    {
        while (queue.Count > 0) // 큐에 출력할 메시지가 남아있는 동안 반복
        {
            messageText.text = queue.Dequeue(); // 큐에서 메시지 하나를 꺼내 텍스트 UI에 설정
            messageText.gameObject.SetActive(true); // 메시지 UI를 활성화하여 화면에 표시
            yield return new WaitForSeconds(showTime); // showTime(초)만큼 대기한 후 다음 메시지로 진행
        }

        messageText.gameObject.SetActive(false); // 모든 메시지 출력이 끝나면 UI를 비활성화하여 숨김
        routine = null; // 코루틴 참조를 초기화하여 다음 호출 시 새로 시작될 수 있도록 함
    }
}
