using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour, GameObject 등)을 사용하기 위한 네임스페이스

// 책장 안의 특정 책에 붙이는 스크립트
public class BookInteract : MonoBehaviour // MonoBehaviour를 상속받아 게임 오브젝트에 컴포넌트로 부착 가능
{
    [Header("책 설정")] // 인스펙터 창에서 아래 변수들을 "책 설정" 그룹으로 표시
    public string code = "0429"; // 책 속에 숨겨진 비밀번호(숫자) 코드, 기본값 "0429"

    [Header("읽은 뒤 플레이어 독백")] // 인스펙터 창에서 아래 변수를 "읽은 뒤 플레이어 독백" 그룹으로 표시
    [TextArea(2, 4)] // 인스펙터에서 여러 줄 입력이 가능한 텍스트 영역으로 표시 (최소 2줄, 최대 4줄)
    public string afterLine = "이 숫자... 어딘가에 입력할 수 있을 것 같은데?"; // 책을 읽은 직후 플레이어가 떠올리는 대사

    public bool CanInteract // 외부에서 이 책과 상호작용 가능한지 여부를 확인하는 프로퍼티
    {
        get // CanInteract 값을 읽을 때 실행되는 게터(getter)
        {
            return GameProgress.Instance != null && GameProgress.Instance.hasReadRunMemo; // 게임 진행 매니저가 존재하고, "달리기 메모"를 읽은 상태여야 true 반환
        }
    }

    public void Interact() // 플레이어가 이 책과 상호작용했을 때 호출되는 메서드
    {
        if (!CanInteract) return; // 상호작용 조건을 만족하지 않으면 아무 동작도 하지 않고 종료

        if (GameProgress.Instance != null) // 게임 진행 매니저 인스턴스가 존재하는지 확인
        {
            GameProgress.Instance.hasCode = true; // 플레이어가 비밀번호 코드를 획득했음을 게임 진행 상태에 기록
        }

        string msg = "책 사이에 숫자가 적힌 쪽지가 끼워져 있다.\n\n\"" + code + "\""; // 책에서 발견한 쪽지 내용을 코드 값과 함께 메시지 문자열로 구성

        if (NoteUI.Instance != null) // 메모/쪽지 전용 UI가 존재하면
        {
            NoteUI.Instance.Open(msg); // 메모 UI를 열어 메시지를 표시
        }
        else if (GameMessageUI.Instance != null) // 메모 UI가 없고 일반 메시지 UI가 존재하면
        {
            GameMessageUI.Instance.ShowMessage(msg); // 일반 메시지 UI에 메시지를 즉시 표시
        }

        if (GameMessageUI.Instance != null) // 일반 메시지 UI가 존재하는 경우 추가 대사를 큐에 등록
        {
            if (!string.IsNullOrEmpty(afterLine)) // 읽은 뒤 독백 대사가 비어있지 않다면
            {
                GameMessageUI.Instance.QueueMessage(afterLine); // 독백 대사를 메시지 큐에 추가하여 순차적으로 표시되게 함
            }

            GameMessageUI.Instance.QueueMessage("분명 노트북에 비밀번호 치던 곳이 있었지..."); // 다음 행동을 유도하는 힌트 대사를 메시지 큐에 추가
        }
    }
}
