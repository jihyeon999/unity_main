using UnityEngine;

// 포스트잇(memo, memo1)에 붙이는 스크립트
// memo      -> memoType = RunMemo,  content에 "run" 적기
// memo1     -> memoType = HintMemo, content에 퀴즈 힌트 적기
public class MemoInteract : MonoBehaviour
{
    public enum MemoType
    {
        None,     // 진행 상태와 상관없는 일반 메모
        RunMemo,  // 'run'이 적힌 memo (읽으면 Books_19를 클릭할 수 있게 됨)
        HintMemo  // 퀴즈 힌트가 적힌 memo1
    }

    [Header("메모 설정")]
    public MemoType memoType = MemoType.None;
    [TextArea(3, 10)]
    public string content = "run";

    // PlayerInteraction.cs에서 E 키로 호출
    public void Read()
    {
        // 진행 상태 갱신
        if (GameProgress.Instance != null)
        {
            if (memoType == MemoType.RunMemo)
            {
                GameProgress.Instance.hasReadRunMemo = true;
            }
            else if (memoType == MemoType.HintMemo)
            {
                GameProgress.Instance.hasReadHint = true;
            }
        }

        // 메모 내용 표시
        if (NoteUI.Instance != null)
        {
            NoteUI.Instance.Open(content);
        }
        else if (GameMessageUI.Instance != null)
        {
            GameMessageUI.Instance.ShowMessage(content);
        }
    }
}
