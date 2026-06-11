using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour, Time 등)을 사용하기 위한 네임스페이스

// 게임 전체 퍼즐 진행 상태를 관리하는 싱글톤
public class GameProgress : MonoBehaviour // 게임의 진행 플래그와 입력 잠금 상태를 전역으로 관리하는 클래스
{
    public static GameProgress Instance; // 어디서든 GameProgress.Instance로 접근할 수 있도록 하는 싱글톤 인스턴스

    [Header("진행 상태")] // 인스펙터에서 아래 변수들을 "진행 상태" 그룹으로 표시
    public bool hasReadRunMemo = false;   // RUN 메모를 읽었는지
    public bool hasCode = false;          // 책에서 4자리 숫자를 확인했는지
    public bool laptopUnlocked = false;   // 노트북 잠금이 풀렸는지
    public bool hasReadHint = false;      // 퀴즈 힌트 메모를 읽었는지

    [Header("게임 시작 독백")] // 인스펙터에서 아래 변수를 "게임 시작 독백" 그룹으로 표시
    public bool playOpeningMonologue = true; // 게임 시작 시 오프닝 독백을 재생할지 여부

    // 메모, 노트북, 퀴즈 UI 사용 중에는 플레이어 입력을 막기 위한 변수
    private static bool inputLocked = false; // 현재 플레이어 입력(이동/시점 등)이 잠겨 있는지 여부 (정적 변수로 모든 인스턴스가 공유)
    private static int lastUnlockFrame = -1; // 입력 잠금이 마지막으로 해제된 프레임 번호 (해제 직후 한 프레임 동안만 특정 처리를 하기 위함)

    public static bool InputLocked // 입력 잠금 상태를 외부에서 읽고 설정할 수 있는 정적 프로퍼티
    {
        get { return inputLocked; } // 현재 입력 잠금 여부를 반환
        set // 입력 잠금 상태를 설정할 때 실행되는 세터(setter)
        {
            inputLocked = value; // 전달받은 값으로 입력 잠금 상태를 갱신

            if (!value) // 잠금이 해제(false로 설정)되었다면
            {
                lastUnlockFrame = Time.frameCount; // 현재 프레임 번호를 "마지막 잠금 해제 프레임"으로 기록
            }
        }
    }

    public static bool JustUnlocked // 입력 잠금이 "바로 이번 프레임"에 해제되었는지 확인하는 프로퍼티
    {
        get { return Time.frameCount == lastUnlockFrame; } // 현재 프레임 번호가 마지막 잠금 해제 프레임과 같으면 true 반환
    }

    void Awake() // 오브젝트가 생성될 때 가장 먼저 호출되는 초기화 함수
    {
        Instance = this; // 자기 자신을 싱글톤 인스턴스로 등록
        inputLocked = false; // 입력 잠금 상태를 초기화 (게임 시작 시 입력 가능 상태로)
        lastUnlockFrame = -1; // 마지막 잠금 해제 프레임 값을 초기화
    }

    void Start() // 씬이 시작될 때 호출되어 오프닝 연출을 재생하는 함수
    {
        if (playOpeningMonologue && GameMessageUI.Instance != null) // 오프닝 독백 재생 옵션이 켜져 있고, 메시지 UI 싱글톤이 존재한다면
        {
            GameMessageUI.Instance.ShowSequence( // 여러 줄의 메시지를 순서대로 표시하는 함수 호출
                "아... 벌써 12시네.", // 1번째 독백 대사
                "과제 조금만 더 하려다가 시간이 이렇게 됐네.", // 2번째 독백 대사
                "이제 집에 가야겠다..." // 3번째 독백 대사
            );
        }
    }
}
