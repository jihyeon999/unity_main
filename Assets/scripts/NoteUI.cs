using UnityEngine;
using TMPro;

// 메모(포스트잇)나 쪽지 내용을 화면 가운데 띄워주는 UI
// Canvas 밑에 패널 하나 + TextMeshProUGUI 하나를 만들어 연결하면 됨
public class NoteUI : MonoBehaviour
{
    public static NoteUI Instance;

    [Header("UI 연결")]
    public GameObject panel;          // 메모 내용을 보여줄 패널
    public TextMeshProUGUI noteText;  // 메모 내용 텍스트

    private bool isOpen = false;
    private int openedFrame = -1;

    public bool IsOpen
    {
        get { return isOpen; }
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void Open(string content)
    {
        if (panel == null || noteText == null) return;

        noteText.text = content;
        panel.SetActive(true);
        isOpen = true;
        openedFrame = Time.frameCount;
        GameProgress.InputLocked = true; // 메모를 보는 동안 이동/상호작용 잠금
    }

    void Update()
    {
        if (!isOpen) return;
        if (Time.frameCount == openedFrame) return; // 연 프레임의 E 입력은 무시

        // E 또는 ESC로 메모 닫기
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Close()
    {
        panel.SetActive(false);
        isOpen = false;
        GameProgress.InputLocked = false;
    }
}
