using System.Collections; // 코루틴(IEnumerator) 등 비제네릭 컬렉션 관련 기능을 사용하기 위한 네임스페이스 (현재 코드에서는 직접 사용되지 않음)
using System.Collections.Generic; // List<T> 등 제네릭 컬렉션을 사용하기 위한 네임스페이스 (현재 코드에서는 직접 사용되지 않음)
using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour, CharacterController, Input 등)을 사용하기 위한 네임스페이스


namespace CharacterScript // 캐릭터(플레이어) 관련 스크립트를 담는 네임스페이스
{
    [RequireComponent(typeof(CharacterController))] // 이 스크립트가 부착된 오브젝트에 CharacterController 컴포넌트가 반드시 있어야 함을 명시 (없으면 자동 추가)

    public class FPSController : MonoBehaviour // 1인칭 시점 캐릭터의 이동과 시점 회전을 담당하는 클래스
    {
        public float walkingSpeed = 7.5f; // 일반적으로 걷는 속도
        public float runningSpeed = 11.5f; // 달릴 때(Shift) 속도
        public float jumpSpeed = 8.0f; // 점프 시 위로 가해지는 초기 속도
        public float gravity = 20.0f; // 캐릭터에 적용되는 중력 가속도 값
        public Camera playerCamera; // 플레이어 시점을 담당하는 카메라 (상하 회전에 사용)
        public float lookSpeed = 2.0f; // 마우스 이동에 따른 시점 회전 속도
        public float lookXLimit = 45.0f; // 카메라가 위/아래로 회전할 수 있는 최대 각도 제한

        CharacterController characterController; // 실제 이동 처리를 담당하는 Unity의 CharacterController 컴포넌트 참조

        Vector3 moveDirection = Vector3.zero; // 현재 프레임의 이동 방향 및 속도를 담는 벡터 (수평 이동 + 수직 중력/점프 포함)
        float rotationX = 0; // 카메라의 상하(피치) 회전 누적 각도

        [HideInInspector] // 인스펙터 창에 노출하지 않음 (코드에서만 제어)
        public bool canMove = true; // 플레이어가 움직이고 시점을 회전할 수 있는지 여부 (UI 등에서 입력 차단 시 false로 설정)

        void Start() // 씬이 시작될 때 한 번 호출되는 초기화 함수
        {
            characterController = GetComponent<CharacterController>(); // 같은 오브젝트에 부착된 CharacterController 컴포넌트를 가져와 캐싱

            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked; // 마우스 커서를 화면 중앙에 고정시켜 FPS 시점 조작이 가능하게 함
            Cursor.visible = false; // 마우스 커서를 화면에서 보이지 않게 숨김
        }

        void Update() // 매 프레임마다 호출되어 이동 및 시점 회전을 처리하는 함수
        {
            // We are grounded, so recalculate move direction based on axes
            Vector3 forward = transform.TransformDirection(Vector3.forward); // 캐릭터의 로컬 "앞" 방향을 월드 좌표계 방향 벡터로 변환
            Vector3 right = transform.TransformDirection(Vector3.right); // 캐릭터의 로컬 "오른쪽" 방향을 월드 좌표계 방향 벡터로 변환
            // Press Left Shift to run
            bool isRunning = Input.GetKey(KeyCode.LeftShift); // 왼쪽 Shift 키를 누르고 있는지 여부 (달리기 여부 판단)
            float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0; // 전후 이동 속도 계산: 이동 가능 상태이고, 달리기 여부에 따라 속도를 정하고 수직 입력값(W/S 또는 화살표 위/아래)을 곱함
            float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0; // 좌우 이동 속도 계산: 이동 가능 상태이고, 달리기 여부에 따라 속도를 정하고 수평 입력값(A/D 또는 화살표 좌/우)을 곱함
            float movementDirectionY = moveDirection.y; // 이번 프레임 계산 전, 기존의 수직(Y축) 이동량(중력/점프 속도)을 임시 저장
            moveDirection = (forward * curSpeedX) + (right * curSpeedY); // 전후/좌우 이동 입력을 합산하여 수평 이동 방향 벡터를 계산 (Y값은 아직 0)

            if (Input.GetButton("Jump") && canMove && characterController.isGrounded) // 점프 버튼(스페이스바)이 눌려 있고, 이동 가능하며, 캐릭터가 바닥에 닿아 있다면
            {
                moveDirection.y = jumpSpeed; // Y축 이동 속도를 점프 속도로 설정하여 위로 솟구치게 함
            }
            else // 점프 조건이 아니라면
            {
                moveDirection.y = movementDirectionY; // 이전 프레임에서 계산된 Y축 이동량(중력에 의한 낙하 속도 등)을 그대로 유지
            }

            // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
            // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
            // as an acceleration (ms^-2)
            if (!characterController.isGrounded) // 캐릭터가 공중에 떠 있는 상태라면 (바닥에 닿아있지 않다면)
            {
                moveDirection.y -= gravity * Time.deltaTime; // 중력 가속도를 적용하여 Y축 속도를 점점 감소시킴(아래로 떨어지게 함)
            }

            // Move the controller
            characterController.Move(moveDirection * Time.deltaTime); // 계산된 이동 방향과 속도에 프레임 시간을 곱하여 실제 캐릭터를 이동시킴

            // Player and Camera rotation
            if (canMove) // 플레이어가 시점을 회전할 수 있는 상태라면
            {
                rotationX += -Input.GetAxis("Mouse Y") * lookSpeed; // 마우스의 상하 이동값을 누적하여 카메라의 상하 회전 각도를 갱신 (마우스를 위로 올리면 위를 보도록 부호 반전)
                rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit); // 상하 회전 각도가 설정된 최소/최대 각도 범위를 벗어나지 않도록 제한
                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0); // 계산된 상하 회전 각도를 카메라의 로컬 회전값으로 적용 (카메라만 위아래로 움직임)
                transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0); // 마우스의 좌우 이동값만큼 캐릭터 본체를 Y축 기준으로 좌우 회전시킴
            }
        }
    }
}
