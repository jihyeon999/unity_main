using UnityEngine;
using TMPro;

// Laptop_off에 붙이는 스크립트
// - 평소: 화면 텍스트 숨김
// - E 키: 비밀번호 입력 텍스트가 뜨고 숫자 입력 모드 (이동 잠금)
// - 4자리 숫자를 맞히면 화면에 "Studio 29" 출력 (이후 계속 표시됨)
// - E 또는 ESC로 종료
public class LaptopInteract : MonoBehaviour
{
    [Header("비밀번호 설정")]
    public string correctCode = "0429";       // Books_19의 code와 같아야 함
    public string successText = "Studio 29";  // 정답 시 노트북 화면에 출력될 문구

    [Header("화면 텍스트")]
    public TMP_Text screenText; // 노트북 화면 위에 올려둔 TextMeshPro (3D 텍스트)

    [Header("정답 보상")]
    public GameObject rewardObject; // 정답을 맞히면 나타날 오브젝트 (예: 책장의 서랍 열쇠)

    private bool inUse = false;      // 비밀번호 입력 중인지
    private string input = "";       // 현재까지 입력한 숫자
    private int startedFrame = -1;   // 시작한 프레임의 E 입력이 바로 종료로 처리되는 것 방지

    void Start()
    {
        bool unlocked = GameProgress.Instance != null && GameProgress.Instance.laptopUnlocked;

        // 평소에는 화면 텍스트 숨김 (잠금 해제 후에는 항상 표시)
        if (screenText != null)
        {
            screenText.gameObject.SetActive(unlocked);
        }

        // 보상 오브젝트(서랍 열쇠 등)는 정답을 맞히기 전까지 숨김
        if (rewardObject != null)
        {
            rewardObject.SetActive(unlocked);
        }
    }

    // PlayerInteraction.cs에서 E 키로 호출
    public void TryInteract()
    {
        if (inUse)
        {
            StopUsing();
        }
        else
        {
            StartUsing();
        }
    }

    void StartUsing()
    {
        inUse = true;
        startedFrame = Time.frameCount;
        GameProgress.InputLocked = true; // 입력 중에는 이동/시점 잠금

        input = "";
        if (screenText != null)
        {
            screenText.gameObject.SetActive(true);
        }
        UpdateScreen();
    }

    void StopUsing()
    {
        inUse = false;
        GameProgress.InputLocked = false;

        // 잠금 해제 전이면 텍스트 다시 숨김, 해제 후에는 Studio 29 계속 표시
        bool unlocked = GameProgress.Instance != null && GameProgress.Instance.laptopUnlocked;
        if (screenText != null && !unlocked)
        {
            screenText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!inUse) return;
        if (Time.frameCount == startedFrame) return; // 시작한 프레임의 입력은 무시

        // E 또는 ESC로 종료
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
        {
            StopUsing();
            return;
        }

        // 이미 잠금 해제됐으면 더 이상 입력받지 않음
        if (GameProgress.Instance != null && GameProgress.Instance.laptopUnlocked) return;

        // 숫자 키 입력 (키보드 윗줄 + 키패드 둘 다 지원)
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                if (input.Length < 4)
                {
                    input += i.ToString();
                    UpdateScreen();

                    // 4자리를 다 입력하면 자동으로 확인
                    if (input.Length == 4)
                    {
                        CheckCode();
                    }
                }
            }
        }

        // 백스페이스로 한 글자 지우기
        if (Input.GetKeyDown(KeyCode.Backspace) && input.Length > 0)
        {
            input = input.Substring(0, input.Length - 1);
            UpdateScreen();
        }
    }

    void CheckCode()
    {
        if (input == correctCode)
        {
            // 정답: 화면에 Studio 29 출력
            if (GameProgress.Instance != null)
            {
                GameProgress.Instance.laptopUnlocked = true;
            }
            UpdateScreen();

            // 숨겨져 있던 보상 오브젝트 등장
            bool rewardAppeared = false;
            if (rewardObject != null && !rewardObject.activeSelf)
            {
                rewardObject.SetActive(true);
                rewardAppeared = true;
            }

            if (GameMessageUI.Instance != null)
            {
                if (rewardAppeared)
                {
                    GameMessageUI.Instance.ShowMessage("노트북 잠금이 풀렸다. 어디선가 '달칵' 하는 소리가 들렸다...");
                }
                else
                {
                    GameMessageUI.Instance.ShowMessage("노트북 잠금이 풀렸다.");
                }
            }
        }
        else
        {
            // 오답: 잠시 경고 문구를 보여준 뒤 입력 초기화
            input = "";
            if (screenText != null)
            {
                screenText.text = "WRONG PASSWORD";
            }
            CancelInvoke(nameof(UpdateScreen));
            Invoke(nameof(UpdateScreen), 1f);
        }
    }

    void UpdateScreen()
    {
        if (screenText == null) return;

        // 잠금 해제 상태면 항상 Studio 29 표시
        if (GameProgress.Instance != null && GameProgress.Instance.laptopUnlocked)
        {
            screenText.text = successText;
            return;
        }

        // 입력 중인 비밀번호 표시 (예: PASSWORD  1 2 _ _)
        string display = "";
        for (int i = 0; i < 4; i++)
        {
            if (i < input.Length)
            {
                display += input[i] + " ";
            }
            else
            {
                display += "_ ";
            }
        }
        screenText.text = "PASSWORD\n" + display.TrimEnd();
    }
}
