using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour 등)을 사용하기 위한 네임스페이스

// 서류 오브젝트에 붙이는 스크립트
public class OfficeFilesInteract : MonoBehaviour // 서류(퀴즈) 오브젝트와의 상호작용 및 정답 처리를 담당하는 클래스
{
    [Header("퀴즈 설정")] // 인스펙터에서 아래 변수들을 "퀴즈 설정" 그룹으로 표시
    [TextArea(3, 10)] // 인스펙터에서 여러 줄 입력이 가능한 텍스트 영역으로 표시 (최소 3줄, 최대 10줄)
    public string question = "서류 표지에 자물쇠 장치와 함께 문제가 적혀 있다.\n\nQ. 이 서류 주인의 작업실 이름은?"; // 퀴즈 UI에 표시될 문제 내용

    public string answer = "Studio 29"; // 이 퀴즈의 정답 문자열

    [Header("최종 열쇠")] // 인스펙터에서 아래 변수를 "최종 열쇠" 그룹으로 표시
    public GameObject finalKey; // 퀴즈 정답을 맞췄을 때 등장할 최종 열쇠 오브젝트

    private bool solved = false; // 이 퀴즈를 이미 풀었는지 여부

    public bool IsSolved // 외부에서 퀴즈가 풀렸는지 여부를 읽기 전용으로 확인할 수 있는 프로퍼티
    {
        get { return solved; } // solved 값을 그대로 반환
    }

    void Start() // 씬이 시작될 때 호출되어 최종 열쇠를 숨겨두는 함수
    {
        if (finalKey != null) // 최종 열쇠 오브젝트가 연결되어 있다면
        {
            finalKey.SetActive(false); // 시작 시 최종 열쇠를 비활성화하여 보이지 않게 함
        }
    }

    public void Interact() // 플레이어가 서류와 상호작용했을 때 호출되는 함수
    {
        if (solved) // 이미 퀴즈를 푼 상태라면
        {
            if (GameMessageUI.Instance != null) // 메시지 UI 싱글톤이 존재하면
            {
                GameMessageUI.Instance.ShowMessage("이미 풀어본 서류다."); // 이미 푼 서류임을 알리는 메시지 표시
            }

            return; // 더 이상 진행할 필요가 없으므로 함수 종료
        }

        if (GameProgress.Instance == null || !GameProgress.Instance.hasReadHint) // 게임 진행 매니저가 없거나, 아직 힌트 메모를 읽지 않았다면
        {
            if (GameMessageUI.Instance != null) // 메시지 UI 싱글톤이 존재하면
            {
                GameMessageUI.Instance.ShowSequence( // 힌트가 없어 풀 수 없다는 메시지 시퀀스를 표시
                    "문제가 적혀 있지만 무슨 뜻인지 모르겠다...", // 1. 문제를 이해하지 못함을 표현
                    "어딘가에 이 문제에 대한 힌트가 있을 것 같은데." // 2. 힌트를 찾아야 한다는 단서 제공
                );
            }

            return; // 힌트를 읽지 않았으므로 퀴즈 UI를 열지 않고 함수 종료
        }

        if (QuizUI.Instance != null) // 퀴즈 UI 싱글톤이 존재하면
        {
            QuizUI.Instance.Open(question, answer, OnSolved); // 퀴즈 UI를 열고, 문제/정답/정답 시 콜백(OnSolved)을 전달
        }
    }

    void OnSolved() // 퀴즈 정답을 맞췄을 때 QuizUI로부터 호출되는 콜백 함수
    {
        solved = true; // 이 퀴즈를 풀었음을 기록

        if (finalKey != null) // 최종 열쇠 오브젝트가 연결되어 있다면
        {
            finalKey.SetActive(true); // 최종 열쇠 오브젝트를 활성화하여 등장시킴
        }

        if (GameMessageUI.Instance != null) // 메시지 UI 싱글톤이 존재하면
        {
            GameMessageUI.Instance.ShowSequence( // 정답을 맞췄을 때의 메시지 시퀀스를 표시
                "서류 밑에서 열쇠가 나타났다!", // 1. 열쇠 등장을 알림
                "드디어 문 열쇠인가?", // 2. 기대감을 표현하는 대사
                "이제 나갈 수 있겠다." // 3. 탈출 가능성을 암시하는 대사
            );
        }
    }
}
