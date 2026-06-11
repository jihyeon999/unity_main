using UnityEngine;
using TMPro;
using System;

// Office_Files 퀴즈의 문제/답 입력 UI
// Canvas 밑에 패널 + 문제 텍스트(TextMeshProUGUI) + 답 입력칸(TMP_InputField)을 만들어 연결
// (제출 버튼을 만들고 싶으면 OnClick에 Submit, 닫기 버튼에는 Close를 연결하면 됨)
public class QuizUI : MonoBehaviour
{
    public static QuizUI Instance;

    [Header("UI 연결")]
    public GameObject panel;              // 퀴즈 패널
    public TextMeshProUGUI questionText;  // 문제 텍스트
    public TMP_InputField answerInput;    // 답 입력칸

    private string correctAnswer;
    private Action onSuccess;
    private bool isOpen = false;

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

    public void Open(string question, string answer, Action successCallback)
    {
        if (panel == null || questionText == null || answerInput == null) return;

        correctAnswer = Normalize(answer);
        onSuccess = successCallback;

        questionText.text = question;
        answerInput.text = "";

        panel.SetActive(true);
        isOpen = true;
        GameProgress.InputLocked = true;

        // 답을 입력할 수 있도록 마우스 커서 표시
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        answerInput.ActivateInputField();
    }

    void Update()
    {
        if (!isOpen) return;

        // ESC로 퀴즈 닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
            return;
        }

        // 엔터로 답 제출
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Submit();
        }
    }

    // 제출 버튼 OnClick에도 연결 가능
    public void Submit()
    {
        if (!isOpen) return;

        if (Normalize(answerInput.text) == correctAnswer)
        {
            Action callback = onSuccess;
            Close();
            if (callback != null)
            {
                callback();
            }
        }
        else
        {
            answerInput.text = "";
            answerInput.ActivateInputField();
            if (GameMessageUI.Instance != null)
            {
                GameMessageUI.Instance.ShowMessage("틀린 답이다...");
            }
        }
    }

    public void Close()
    {
        panel.SetActive(false);
        isOpen = false;
        onSuccess = null;
        GameProgress.InputLocked = false;

        // 다시 1인칭 모드로 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 대소문자/공백 차이는 무시하고 비교 (예: "studio29"도 "Studio 29"로 인정)
    string Normalize(string s)
    {
        if (s == null) return "";
        return s.Replace(" ", "").ToLowerInvariant();
    }
}
