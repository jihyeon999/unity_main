using UnityEngine;

// 서랍 안의 Office_Files에 붙이는 스크립트
// - memo1의 힌트를 읽기 전에는 퀴즈를 풀 수 없음
// - 퀴즈를 맞히면 서류 밑에 숨겨둔 최종 열쇠가 나타남
public class OfficeFilesInteract : MonoBehaviour
{
    [Header("퀴즈 설정")]
    [TextArea(3, 10)]
    public string question = "서류 표지에 자물쇠 장치와 함께 문제가 적혀 있다.\n\nQ. 이 서류 주인의 작업실 이름은?";
    public string answer = "Studio 29"; // memo1 힌트 + 노트북 화면(Studio 29)으로 알 수 있는 답

    [Header("최종 열쇠")]
    public GameObject finalKey; // Office_Files 밑에 숨겨둔 최종 열쇠 (비활성 상태로 배치해 둘 것)

    private bool solved = false;

    public bool IsSolved
    {
        get { return solved; }
    }

    void Start()
    {
        // 열쇠는 퀴즈를 풀기 전까지 숨겨둠
        if (finalKey != null)
        {
            finalKey.SetActive(false);
        }
    }

    // PlayerInteraction.cs에서 E 키로 호출
    public void Interact()
    {
        if (solved)
        {
            if (GameMessageUI.Instance != null)
            {
                GameMessageUI.Instance.ShowMessage("이미 풀어본 서류다.");
            }
            return;
        }

        // memo1의 힌트를 읽기 전에는 풀 수 없음
        if (GameProgress.Instance == null || !GameProgress.Instance.hasReadHint)
        {
            if (GameMessageUI.Instance != null)
            {
                GameMessageUI.Instance.ShowMessage("문제가 적혀 있지만 무슨 뜻인지 모르겠다... 힌트가 필요하다.");
            }
            return;
        }

        if (QuizUI.Instance != null)
        {
            QuizUI.Instance.Open(question, answer, OnSolved);
        }
    }

    void OnSolved()
    {
        solved = true;

        // 서류 밑에 숨겨둔 최종 열쇠 등장
        if (finalKey != null)
        {
            finalKey.SetActive(true);
        }

        if (GameMessageUI.Instance != null)
        {
            GameMessageUI.Instance.ShowMessage("서류 밑에서 열쇠가 나타났다!");
        }
    }
}
