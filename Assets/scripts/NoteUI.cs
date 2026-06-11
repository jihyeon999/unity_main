using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour, Input, Time 등)을 사용하기 위한 네임스페이스
using TMPro; // TextMeshPro UI 컴포넌트(TextMeshProUGUI)를 사용하기 위한 네임스페이스

// 메모나 쪽지 내용을 화면 가운데 띄워주는 UI
public class NoteUI : MonoBehaviour // 메모/쪽지 내용을 보여주는 팝업 UI를 제어하는 클래스
{
    public static NoteUI Instance; // 어디서든 NoteUI.Instance로 접근할 수 있도록 하는 싱글톤 인스턴스

    [Header("UI 연결")] // 인스펙터에서 아래 변수들을 "UI 연결" 그룹으로 표시
    public GameObject panel; // 메모 내용을 표시할 패널(배경 + 텍스트를 포함하는 UI 오브젝트)
    public TextMeshProUGUI noteText; // 메모 내용을 표시할 텍스트 컴포넌트

    private bool isOpen = false; // 현재 메모 패널이 열려 있는지 여부
    private int openedFrame = -1; // 패널이 열린 프레임 번호 (연 직후 같은 프레임의 입력으로 바로 닫히는 것을 방지)

    public bool IsOpen // 외부에서 메모 패널의 열림 여부를 읽기 전용으로 확인할 수 있는 프로퍼티
    {
        get { return isOpen; } // isOpen 값을 그대로 반환
    }

    void Awake() // 오브젝트가 생성될 때 가장 먼저 호출되는 초기화 함수
    {
        Instance = this; // 자기 자신을 싱글톤 인스턴스로 등록하여 다른 스크립트에서 접근 가능하게 함
    }

    void Start() // 씬이 시작될 때 호출되는 초기화 함수
    {
        if (panel != null) // 메모 패널이 연결되어 있다면
        {
            panel.SetActive(false); // 시작 시 패널을 비활성화하여 화면에 보이지 않게 함
        }
    }

    public void Open(string content) // 전달받은 내용으로 메모 패널을 여는 함수
    {
        if (panel == null || noteText == null) return; // 패널이나 텍스트 UI가 연결되어 있지 않으면 아무것도 하지 않고 종료

        noteText.text = content; // 메모 텍스트를 전달받은 내용으로 설정
        panel.SetActive(true); // 메모 패널을 활성화하여 화면에 표시
        isOpen = true; // 패널이 열린 상태로 표시
        openedFrame = Time.frameCount; // 패널을 연 프레임 번호를 기록 (같은 프레임에 닫힘 입력이 처리되는 것을 방지)

        GameProgress.InputLocked = true; // 메모를 보는 동안 플레이어 이동/시점 입력을 잠금
    }

    void Update() // 매 프레임마다 호출되어 메모 패널 닫기 입력을 처리하는 함수
    {
        if (!isOpen) return; // 패널이 열려 있지 않으면 아무 처리도 하지 않고 종료
        if (Time.frameCount == openedFrame) return; // 패널을 연 바로 그 프레임이라면, 같은 입력이 중복 처리되지 않도록 건너뜀

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)) // E 키 또는 Esc 키가 눌렸다면
        {
            Close(); // 메모 패널을 닫음
        }
    }

    public void Close() // 메모 패널을 닫는 함수
    {
        panel.SetActive(false); // 패널을 비활성화하여 화면에서 숨김
        isOpen = false; // 패널이 닫힌 상태로 표시

        GameProgress.InputLocked = false; // 플레이어 이동/시점 입력 잠금을 해제
    }
}
