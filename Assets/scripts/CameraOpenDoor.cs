using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour, Physics, Input 등)을 사용하기 위한 네임스페이스
using TMPro; // TextMeshPro UI 컴포넌트(TextMeshProUGUI)를 사용하기 위한 네임스페이스

namespace CameraDoorScript // 카메라 기반 문 상호작용 스크립트를 담는 네임스페이스
{
    public class CameraOpenDoor : MonoBehaviour // 카메라(또는 플레이어 시점)에 부착하여 문을 바라보면 상호작용 안내를 띄우는 클래스
    {
        public float DistanceOpen = 3f;        // 문과 상호작용 가능한 최대 거리
        public TextMeshProUGUI promptText;     // [E] 문 열기 / [E] 문 닫기 텍스트

        void Start() // 씬이 시작될 때 한 번 호출되는 초기화 함수
        {
            if (promptText != null) // 안내 텍스트 UI가 인스펙터에 연결되어 있는지 확인
            {
                promptText.gameObject.SetActive(false); // 시작 시에는 안내 텍스트를 비활성화하여 화면에 보이지 않게 함
            }
        }

        void Update() // 매 프레임마다 호출되는 함수 (입력 및 레이캐스트 처리)
        {
            RaycastHit hit; // Raycast(광선 충돌 검사) 결과 정보를 저장할 변수 선언

            // 카메라 위치에서 카메라가 바라보는 방향으로 Ray를 쏜다.
            if (Physics.Raycast(transform.position, transform.forward, out hit, DistanceOpen)) // 현재 오브젝트 위치에서 정면 방향으로 DistanceOpen 거리만큼 광선을 쏘아 충돌 여부 확인
            {
                // Ray에 맞은 오브젝트 또는 부모 오브젝트에서 Door 스크립트를 찾는다.
                DoorScript.Door door = hit.transform.GetComponentInParent<DoorScript.Door>(); // 광선에 맞은 오브젝트(혹은 그 부모)에서 Door 컴포넌트를 가져옴

                // 문 또는 손잡이를 보고 있는 경우
                if (door != null) // Door 컴포넌트를 찾았다면 (즉, 문을 바라보고 있다면)
                {
                    promptText.gameObject.SetActive(true); // 상호작용 안내 텍스트를 화면에 표시

                    // 문이 열려 있으면 닫기, 닫혀 있으면 열기 표시
                    if (door.open) // 문의 현재 상태가 "열림"이라면
                    {
                        promptText.text = "[E] 문 닫기"; // 안내 텍스트를 "[E] 문 닫기"로 설정
                    }
                    else // 문의 현재 상태가 "닫힘"이라면
                    {
                        promptText.text = "[E] 문 열기"; // 안내 텍스트를 "[E] 문 열기"로 설정
                    }

                    // E 키를 누르면 문 열기/닫기 실행
                    if (Input.GetKeyDown(KeyCode.E)) // 이번 프레임에 E 키가 새로 눌렸는지 확인
                    {
                        door.OpenDoor(); // Door 스크립트의 OpenDoor 함수를 호출하여 문을 열거나 닫음
                    }

                    return; // 문을 바라보고 있는 동안에는 아래의 "텍스트 숨김" 로직을 실행하지 않고 함수 종료
                }
            }

            // 문을 보고 있지 않으면 텍스트 숨김
            if (promptText != null) // 안내 텍스트 UI가 연결되어 있는지 확인
            {
                promptText.gameObject.SetActive(false); // 문을 바라보고 있지 않으므로 안내 텍스트를 비활성화
            }
        }
    }
}
