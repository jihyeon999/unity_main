using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour, Input, Cursor 등)을 사용하기 위한 네임스페이스
using TMPro; // TextMeshPro UI 컴포넌트(TextMeshProUGUI, TMP_InputField)를 사용하기 위한 네임스페이스
using System; // Action 델리게이트를 사용하기 위한 네임스페이스

// Office_Files 퀴즈 UI
public class QuizUI : MonoBehaviour // 서류 퀴즈의 문제/정답 입력 UI를 제어하는 클래스
{
    public static QuizUI Instance; // 어디서든 QuizUI.Instance로 접근할 수 있도록 하는 싱글톤 인스턴스

    [Header("UI 연결")] // 인스펙터에서 아래 변수들을 "UI 연결" 그룹으로 표시
    public GameObject panel; // 퀴즈 UI 전체를 감싸는 패널 오브젝트
    public TextMeshProUGUI questionText; // 문제 내용을 표시할 텍스트 컴포넌트
    public TMP_InputField answerInput; // 플레이어가 답을 입력할 입력 필드

    private string correctAnswer; // 정규화(공백 제거, 소문자 변환)된 정답 문자열을 저장
    private Action onSuccess; // 정답을 맞췄을 때 실행할 콜백 함수
    private bool isOpen = false; // 현재 퀴즈 패널이 열려 있는지 여부

    void Awake() // 오브젝트가 생성될 때 가장 먼저 호출되는 초기화 함수
    {
        Instance = this; // 자기 자신을 싱글톤 인스턴스로 등록하여 다른 스크립트에서 접근 가능하게 함
    }

    void Start() // 씬이 시작될 때 호출되는 초기화 함수
    {
        if (panel != null) // 퀴즈 패널이 연결되어 있다면
        {
            panel.SetActive(false); // 시작 시 패널을 비활성화하여 화면에 보이지 않게 함
        }
    }

    public void Open(string question, string answer, Action successCallback) // 퀴즈 UI를 열고 문제/정답/성공 콜백을 설정하는 함수
    {
        if (panel == null || questionText == null || answerInput == null) return; // 필수 UI 요소 중 하나라도 연결되어 있지 않으면 아무것도 하지 않고 종료

        correctAnswer = Normalize(answer); // 전달받은 정답을 정규화(공백 제거, 소문자 변환)하여 저장
        onSuccess = successCallback; // 정답을 맞췄을 때 호출할 콜백 함수를 저장

        questionText.text = question; // 문제 텍스트 UI에 전달받은 문제 내용을 설정
        answerInput.text = ""; // 입력 필드를 빈 문자열로 초기화

        panel.SetActive(true); // 퀴즈 패널을 활성화하여 화면에 표시
        isOpen = true; // 패널이 열린 상태로 표시

        GameProgress.InputLocked = true; // 퀴즈를 푸는 동안 플레이어 이동/시점 입력을 잠금

        Cursor.lockState = CursorLockMode.None; // 마우스 커서 잠금을 해제하여 자유롭게 움직일 수 있게 함
        Cursor.visible = true; // 마우스 커서를 화면에 보이게 함

        answerInput.ActivateInputField(); // 입력 필드에 자동으로 포커스를 주어 바로 타이핑할 수 있게 함
    }

    void Update() // 매 프레임마다 호출되어 퀴즈 UI 관련 입력을 처리하는 함수
    {
        if (!isOpen) return; // 퀴즈 패널이 열려 있지 않으면 아무 처리도 하지 않고 종료

        if (Input.GetKeyDown(KeyCode.Escape)) // Esc 키가 눌렸다면
        {
            Close(); // 퀴즈 패널을 닫음
            return; // 닫기 처리를 했으므로 이후 입력 처리는 하지 않고 함수 종료
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) // 일반 엔터 키 또는 키패드 엔터 키가 눌렸다면
        {
            Submit(); // 입력된 답을 제출하여 정답 여부 확인
        }
    }

    public void Submit() // 입력된 답을 제출하고 정답 여부를 확인하는 함수
    {
        if (!isOpen) return; // 퀴즈 패널이 열려 있지 않으면 아무것도 하지 않고 종료

        if (Normalize(answerInput.text) == correctAnswer) // 입력된 답을 정규화한 값이 저장된 정답과 일치한다면
        {
            Action callback = onSuccess; // 닫기 전에 콜백 함수를 임시 변수에 저장 (Close()에서 onSuccess가 null로 초기화되므로)

            Close(); // 퀴즈 패널을 닫음 (입력 잠금 해제, 커서 상태 복원 등 포함)

            if (callback != null) // 저장해둔 콜백 함수가 존재한다면
            {
                callback(); // 정답 처리 콜백 함수를 실행 (예: 보상 등장 처리)
            }
        }
        else // 입력된 답이 정답과 일치하지 않는다면
        {
            answerInput.text = ""; // 입력 필드를 비워 다시 입력할 수 있게 함
            answerInput.ActivateInputField(); // 입력 필드에 다시 포커스를 주어 바로 타이핑할 수 있게 함

            if (GameMessageUI.Instance != null) // 메시지 UI 싱글톤이 존재하면
            {
                GameMessageUI.Instance.ShowMessage("틀린 답이다."); // 오답임을 알리는 메시지 표시
            }
        }
    }

    public void Close() // 퀴즈 UI를 닫는 함수
    {
        panel.SetActive(false); // 패널을 비활성화하여 화면에서 숨김
        isOpen = false; // 패널이 닫힌 상태로 표시
        onSuccess = null; // 저장해둔 콜백 함수 참조를 해제 (메모리 누수 및 잘못된 재호출 방지)

        GameProgress.InputLocked = false; // 플레이어 이동/시점 입력 잠금을 해제

        Cursor.lockState = CursorLockMode.Locked; // 마우스 커서를 다시 화면 중앙에 고정 (FPS 시점 조작 복귀)
        Cursor.visible = false; // 마우스 커서를 다시 숨김
    }

    string Normalize(string s) // 문자열을 비교하기 쉽게 정규화하는 함수 (공백 제거, 소문자 변환)
    {
        if (s == null) return ""; // 입력이 null이면 빈 문자열을 반환하여 예외 방지

        return s.Replace(" ", "").ToLowerInvariant(); // 모든 공백을 제거하고 소문자로 변환하여 반환 (대소문자/공백 차이 무시하고 비교하기 위함)
    }
}
