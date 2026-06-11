using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour 등)을 사용하기 위한 네임스페이스

// 포스트잇에 붙이는 스크립트
public class MemoInteract : MonoBehaviour // 메모/포스트잇 오브젝트와의 상호작용을 담당하는 클래스
{
    public enum MemoType // 메모의 종류를 구분하기 위한 열거형
    {
        None, // 특별한 종류가 지정되지 않은 일반 메모
        RunMemo, // "RUN" 단서가 적힌 메모
        HintMemo // 퀴즈 힌트가 적힌 메모
    }

    [Header("메모 설정")] // 인스펙터에서 아래 변수들을 "메모 설정" 그룹으로 표시
    public MemoType memoType = MemoType.None; // 이 메모의 종류 (게임 진행 상태 갱신 및 대사 분기에 사용)

    [TextArea(3, 10)] // 인스펙터에서 여러 줄 입력이 가능한 텍스트 영역으로 표시 (최소 3줄, 최대 10줄)
    public string content = "RUN"; // 메모에 적혀 있는 실제 내용 (UI에 표시될 텍스트)

    [Header("읽은 뒤 플레이어 독백")] // 인스펙터에서 아래 변수를 "읽은 뒤 플레이어 독백" 그룹으로 표시
    [TextArea(2, 4)] // 인스펙터에서 여러 줄 입력이 가능한 텍스트 영역으로 표시 (최소 2줄, 최대 4줄)
    public string customAfterLine = ""; // 메모를 읽은 직후 표시할 커스텀 독백 (비어있으면 memoType에 따른 기본 대사 사용)

    public void Read() // 플레이어가 이 메모를 읽었을 때 호출되는 함수
    {
        if (GameProgress.Instance != null) // 게임 진행 매니저가 존재한다면
        {
            if (memoType == MemoType.RunMemo) // 이 메모가 "RUN" 메모라면
            {
                GameProgress.Instance.hasReadRunMemo = true; // RUN 메모를 읽었음을 게임 진행 상태에 기록
            }
            else if (memoType == MemoType.HintMemo) // 이 메모가 힌트 메모라면
            {
                GameProgress.Instance.hasReadHint = true; // 힌트 메모를 읽었음을 게임 진행 상태에 기록
            }
        }

        if (NoteUI.Instance != null) // 메모/쪽지 전용 UI가 존재하면
        {
            NoteUI.Instance.Open(content); // 메모 UI를 열어 메모 내용을 표시
        }
        else if (GameMessageUI.Instance != null) // 메모 UI가 없고 일반 메시지 UI가 존재하면
        {
            GameMessageUI.Instance.ShowMessage(content); // 일반 메시지 UI에 메모 내용을 즉시 표시
        }

        if (GameMessageUI.Instance != null) // 일반 메시지 UI가 존재하는 경우 추가 대사를 큐에 등록
        {
            if (!string.IsNullOrEmpty(customAfterLine)) // 커스텀 독백이 비어있지 않다면
            {
                GameMessageUI.Instance.QueueMessage(customAfterLine); // 설정된 커스텀 독백을 메시지 큐에 추가
            }
            else if (memoType == MemoType.RunMemo) // 커스텀 독백이 없고, 이 메모가 RUN 메모라면
            {
                GameMessageUI.Instance.QueueMessage("RUN...? 이게 무슨 뜻이지?"); // 1번째 독백: 단어에 대한 의문
                GameMessageUI.Instance.QueueMessage("어... 이 단어, 책장에서 본 것 같은데?"); // 2번째 독백: 단서를 떠올림
                GameMessageUI.Instance.QueueMessage("책장 쪽을 한번 확인해보자."); // 3번째 독백: 다음 행동 유도
            }
            else if (memoType == MemoType.HintMemo) // 커스텀 독백이 없고, 이 메모가 힌트 메모라면
            {
                GameMessageUI.Instance.QueueMessage("오리? 비둘기? 이게 무슨 뜻이지?"); // 1번째 독백: 힌트 내용에 대한 의문
                GameMessageUI.Instance.QueueMessage("힌트를 다시 한 번 잘 생각해보자."); // 2번째 독백: 다시 생각해보도록 유도
            }
            else // 커스텀 독백도 없고 특정 종류도 아닌 일반 메모라면
            {
                GameMessageUI.Instance.QueueMessage("다시 한 번 살펴보는 게 어떨까?"); // 일반적인 기본 독백 표시
            }
        }
    }
}
