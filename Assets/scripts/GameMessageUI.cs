using UnityEngine;
using TMPro;
using System.Collections;

public class GameMessageUI : MonoBehaviour // 안내 메시지 출력
{
    public static GameMessageUI Instance;

    public TextMeshProUGUI messageText;
    public float showTime = 2f; //텍스트를 보여줄 시간

    private Coroutine messageCoroutine; //코루틴: 일정 시간 동안 기다렸다가 다음 동작을 실행하는 구조

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false); //게임 시작할 때는 끔
        }
    }

    public void ShowMessage(string message)
    {
        if (messageText == null) return;

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(ShowMessageRoutine(message));
    }

    IEnumerator ShowMessageRoutine(string message)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(showTime);

        messageText.gameObject.SetActive(false);
    }
}