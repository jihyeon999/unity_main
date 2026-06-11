using UnityEngine;

// 게임 전체 퍼즐 진행 상태를 관리하는 싱글톤
// 빈 GameObject를 만들어 이 스크립트를 붙여두면 됨
public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance;

    [Header("진행 상태 (디버그용으로 인스펙터에 표시)")]
    public bool hasReadRunMemo = false;   // memo 포스트잇에서 'run'을 확인했는지
    public bool hasCode = false;          // Books_19에서 4자리 숫자를 확인했는지
    public bool laptopUnlocked = false;   // 노트북에 Studio 29가 출력됐는지
    public bool hasReadHint = false;      // memo1 포스트잇의 힌트를 확인했는지

    // UI(메모/퀴즈)나 노트북 사용 중에는 플레이어 이동/상호작용을 잠금
    private static bool inputLocked = false;
    private static int lastUnlockFrame = -1;

    public static bool InputLocked
    {
        get { return inputLocked; }
        set
        {
            inputLocked = value;
            if (!value)
            {
                lastUnlockFrame = Time.frameCount;
            }
        }
    }

    // 잠금이 풀린 바로 그 프레임인지 (같은 프레임에 E키가 중복 처리되는 것 방지)
    public static bool JustUnlocked
    {
        get { return Time.frameCount == lastUnlockFrame; }
    }

    void Awake()
    {
        Instance = this;
        inputLocked = false;
        lastUnlockFrame = -1;
    }
}
