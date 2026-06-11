using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour, Input, Time 등)을 사용하기 위한 네임스페이스
using TMPro; // TextMeshPro UI 컴포넌트(TMP_Text)를 사용하기 위한 네임스페이스

public class LaptopInteract : MonoBehaviour // 노트북 오브젝트와의 비밀번호 입력 상호작용을 담당하는 클래스
{
    [Header("비밀번호 설정")] // 인스펙터에서 아래 변수들을 "비밀번호 설정" 그룹으로 표시
    public string correctCode = "0429"; // 정답 비밀번호 (4자리)
    public string successText = "Studio 29"; // 비밀번호를 맞췄을 때 화면에 표시될 텍스트

    [Header("화면 텍스트")] // 인스펙터에서 아래 변수를 "화면 텍스트" 그룹으로 표시
    public TMP_Text screenText; // 노트북 화면에 입력 상태/결과를 표시할 TextMeshPro 텍스트

    [Header("정답 보상")] // 인스펙터에서 아래 변수를 "정답 보상" 그룹으로 표시
    public GameObject rewardObject; // 비밀번호를 맞췄을 때 활성화될 보상 오브젝트 (예: 떨어진 아이템)

    private bool inUse = false; // 현재 플레이어가 노트북을 사용 중인지 여부
    private string input = ""; // 현재까지 입력된 비밀번호 문자열
    private int startedFrame = -1; // 노트북 사용을 시작한 프레임 번호 (시작 프레임의 입력을 무시하기 위함)

    void Start() // 씬이 시작될 때 호출되어 이전에 잠금이 풀린 상태였다면 화면/보상을 복원하는 함수
    {
        bool unlocked = GameProgress.Instance != null && GameProgress.Instance.laptopUnlocked; // 게임 진행 상태에서 노트북이 이미 잠금 해제되었는지 확인

        if (screenText != null) // 화면 텍스트 UI가 연결되어 있다면
        {
            screenText.gameObject.SetActive(unlocked); // 잠금 해제 상태였다면 화면을 켜두고, 아니라면 꺼둠
        }

        if (rewardObject != null) // 보상 오브젝트가 연결되어 있다면
        {
            rewardObject.SetActive(unlocked); // 잠금 해제 상태였다면 보상 오브젝트를 활성화 상태로 복원
        }
    }

    public void TryInteract() // 플레이어가 노트북과 상호작용(E키 등)했을 때 호출되는 함수
    {
        if (inUse) // 이미 사용 중이라면
        {
            StopUsing(); // 사용을 종료 (노트북에서 빠져나옴)
        }
        else // 사용 중이 아니라면
        {
            StartUsing(); // 노트북 사용을 시작
        }
    }

    void StartUsing() // 노트북 사용을 시작하는 함수
    {
        inUse = true; // 사용 중 상태로 변경
        startedFrame = Time.frameCount; // 사용을 시작한 프레임 번호를 기록 (같은 프레임의 상호작용 입력이 비밀번호 입력으로 중복 처리되는 것 방지)
        GameProgress.InputLocked = true; // 플레이어 이동/시점 입력을 잠금 (노트북 조작 중에는 캐릭터가 움직이지 않도록)

        input = ""; // 입력 문자열을 초기화 (빈 상태로 시작)

        if (screenText != null) // 화면 텍스트 UI가 연결되어 있다면
        {
            screenText.gameObject.SetActive(true); // 노트북 화면을 활성화하여 보이게 함
        }

        UpdateScreen(); // 초기 화면(빈 입력 상태) 표시를 갱신
    }

    void StopUsing() // 노트북 사용을 종료하는 함수
    {
        inUse = false; // 사용 중 상태를 해제
        GameProgress.InputLocked = false; // 플레이어 이동/시점 입력 잠금을 해제

        bool unlocked = GameProgress.Instance != null && GameProgress.Instance.laptopUnlocked; // 노트북이 잠금 해제된 상태인지 확인

        if (screenText != null && !unlocked) // 화면 텍스트가 있고, 아직 잠금 해제되지 않은 상태라면
        {
            screenText.gameObject.SetActive(false); // 노트북에서 빠져나가면 화면을 다시 꺼서 숨김 (잠금 해제 후에는 화면을 계속 켜둠)
        }
    }

    void Update() // 매 프레임마다 호출되어 노트북 사용 중 입력을 처리하는 함수
    {
        if (!inUse) return; // 노트북을 사용 중이 아니면 아무 처리도 하지 않고 종료
        if (Time.frameCount == startedFrame) return; // 노트북 사용을 시작한 바로 그 프레임이라면, 같은 입력이 중복 처리되지 않도록 건너뜀

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)) // E 키 또는 Esc 키가 눌렸다면
        {
            StopUsing(); // 노트북 사용을 종료
            return; // 종료 처리를 했으므로 이후 입력 처리는 하지 않고 함수 종료
        }

        if (GameProgress.Instance != null && GameProgress.Instance.laptopUnlocked) // 이미 비밀번호가 풀려 잠금 해제된 상태라면
        {
            return; // 더 이상 숫자 입력을 받을 필요가 없으므로 함수 종료
        }

        for (int i = 0; i <= 9; i++) // 숫자 0부터 9까지 각 숫자 키에 대해 반복 검사
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i)) // 일반 숫자 키(0~9) 또는 키패드 숫자 키(0~9) 중 i에 해당하는 키가 눌렸다면
            {
                if (input.Length < 4) // 현재 입력된 비밀번호 길이가 4자리 미만이라면
                {
                    input += i.ToString(); // 눌린 숫자를 입력 문자열 끝에 추가
                    UpdateScreen(); // 변경된 입력 상태를 화면에 반영

                    if (input.Length == 4) // 입력이 4자리가 다 채워졌다면
                    {
                        CheckCode(); // 입력된 비밀번호가 정답인지 확인
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace) && input.Length > 0) // 백스페이스 키가 눌렸고, 입력된 문자가 1개 이상 있다면
        {
            input = input.Substring(0, input.Length - 1); // 입력 문자열의 마지막 한 글자를 제거
            UpdateScreen(); // 변경된 입력 상태를 화면에 반영
        }
    }

    void CheckCode() // 입력된 4자리 코드가 정답인지 확인하는 함수
    {
        if (input == correctCode) // 입력값이 정답 코드와 일치한다면
        {
            if (GameProgress.Instance != null) // 게임 진행 매니저가 존재하면
            {
                GameProgress.Instance.laptopUnlocked = true; // 노트북 잠금 해제 상태를 게임 진행 정보에 기록
            }

            UpdateScreen(); // 잠금 해제된 화면(성공 텍스트)으로 갱신

            bool rewardAppeared = false; // 이번에 보상 오브젝트가 새로 나타났는지 여부를 저장할 변수

            if (rewardObject != null && !rewardObject.activeSelf) // 보상 오브젝트가 연결되어 있고, 아직 비활성 상태라면
            {
                rewardObject.SetActive(true); // 보상 오브젝트를 활성화하여 등장시킴
                rewardAppeared = true; // 보상이 새로 등장했음을 표시
            }

            if (GameMessageUI.Instance != null) // 메시지 UI 싱글톤이 존재하면
            {
                if (rewardAppeared) // 보상이 새로 등장한 경우
                {
                    GameMessageUI.Instance.ShowSequence( // 보상 등장에 맞는 메시지 시퀀스를 표시
                        "노트북 잠금이 풀렸다.", // 1. 잠금 해제 안내
                        "Studio 29...?", // 2. 화면에 뜬 텍스트에 대한 의문
                        "이건 무슨 힌트일까?", // 3. 추가 의문
                        "그때 무언가 떨어지는 소리가 들렸다.", // 4. 보상 등장을 알리는 연출 대사
                        "소리는 책장 쪽에서 난 것 같다." // 5. 다음 행동을 유도하는 힌트
                    );
                }
                else // 이미 보상이 등장해 있던 경우 (재진입 등)
                {
                    GameMessageUI.Instance.ShowSequence( // 더 짧은 메시지 시퀀스를 표시
                        "노트북 잠금이 풀렸다.", // 1. 잠금 해제 안내
                        "Studio 29...?", // 2. 화면 텍스트에 대한 의문
                        "책장에서 본 것 같은데..." // 3. 단서를 떠올리는 대사
                    );
                }
            }
        }
        else // 입력값이 정답과 다르다면
        {
            input = ""; // 입력 문자열을 초기화하여 다시 입력할 수 있게 함

            if (screenText != null) // 화면 텍스트가 연결되어 있다면
            {
                screenText.text = "WRONG PASSWORD"; // 화면에 "비밀번호 틀림" 메시지를 표시
            }

            CancelInvoke(nameof(UpdateScreen)); // 이전에 예약된 UpdateScreen 호출이 있다면 취소 (연속으로 틀렸을 때 중복 예약 방지)
            Invoke(nameof(UpdateScreen), 1f); // 1초 뒤에 UpdateScreen을 호출하여 화면을 다시 입력 대기 상태로 되돌림
        }
    }

    void UpdateScreen() // 노트북 화면 텍스트를 현재 상태에 맞게 갱신하는 함수
    {
        if (screenText == null) return; // 화면 텍스트가 연결되어 있지 않으면 아무것도 하지 않고 종료

        if (GameProgress.Instance != null && GameProgress.Instance.laptopUnlocked) // 이미 잠금이 해제된 상태라면
        {
            screenText.text = successText; // 화면에 성공 텍스트(예: "Studio 29")를 표시
            return; // 더 이상 입력 화면을 표시할 필요가 없으므로 종료
        }

        string display = ""; // 화면에 표시할 입력 상태 문자열을 담을 변수

        for (int i = 0; i < 4; i++) // 비밀번호 자릿수(4자리)만큼 반복
        {
            if (i < input.Length) // 현재 자리에 이미 입력된 숫자가 있다면
            {
                display += input[i] + " "; // 해당 숫자와 공백을 표시 문자열에 추가
            }
            else // 아직 입력되지 않은 자리라면
            {
                display += "_ "; // 빈 자리를 나타내는 밑줄과 공백을 표시 문자열에 추가
            }
        }

        screenText.text = "PASSWORD\n" + display.TrimEnd(); // "PASSWORD" 제목과 함께 입력 상태 문자열(끝 공백 제거)을 화면에 표시
    }
}
