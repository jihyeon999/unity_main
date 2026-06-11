using UnityEngine;

// bookshelf 안의 Books_19에 붙이는 스크립트
// memo에서 'run'을 읽은 뒤에만 클릭(상호작용)할 수 있고,
// 클릭하면 4자리 숫자가 적힌 쪽지를 보여줌
public class BookInteract : MonoBehaviour
{
    [Header("책 설정")]
    public string code = "0429"; // 책에 숨겨진 4자리 숫자 (LaptopInteract의 correctCode와 같아야 함)

    // memo('run')를 읽기 전에는 상호작용 자체가 불가능
    public bool CanInteract
    {
        get
        {
            return GameProgress.Instance != null && GameProgress.Instance.hasReadRunMemo;
        }
    }

    // PlayerInteraction.cs에서 E 키 또는 마우스 클릭으로 호출
    public void Interact()
    {
        if (!CanInteract) return;

        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.hasCode = true;
        }

        string msg = "책 사이에 숫자가 적힌 쪽지가 끼워져 있다.\n\n\"" + code + "\"";

        if (NoteUI.Instance != null)
        {
            NoteUI.Instance.Open(msg);
        }
        else if (GameMessageUI.Instance != null)
        {
            GameMessageUI.Instance.ShowMessage(msg);
        }
    }
}
