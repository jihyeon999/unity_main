using UnityEngine;
using TMPro;

namespace CameraDoorScript
{
    public class CameraOpenDoor : MonoBehaviour
    {
        public float DistanceOpen = 3f;        // 문과 상호작용 가능한 최대 거리
        public TextMeshProUGUI promptText;     // [E] 문 열기 / [E] 문 닫기 텍스트

        void Start()
        {
            if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            RaycastHit hit;

            // 카메라 위치에서 카메라가 바라보는 방향으로 Ray를 쏜다.
            if (Physics.Raycast(transform.position, transform.forward, out hit, DistanceOpen))
            {
                // Ray에 맞은 오브젝트 또는 부모 오브젝트에서 Door 스크립트를 찾는다.
                DoorScript.Door door = hit.transform.GetComponentInParent<DoorScript.Door>();

                // 문 또는 손잡이를 보고 있는 경우
                if (door != null)
                {
                    promptText.gameObject.SetActive(true);

                    // 문이 열려 있으면 닫기, 닫혀 있으면 열기 표시
                    if (door.open)
                    {
                        promptText.text = "[E] 문 닫기";
                    }
                    else
                    {
                        promptText.text = "[E] 문 열기";
                    }

                    // E 키를 누르면 문 열기/닫기 실행
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        door.OpenDoor();
                    }

                    return;
                }
            }

            // 문을 보고 있지 않으면 텍스트 숨김
            if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
        }
    }
}