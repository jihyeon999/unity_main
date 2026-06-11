using UnityEngine; // Unity 엔진의 기본 기능(MonoBehaviour, CharacterController, Input 등)을 사용하기 위한 네임스페이스

public class PlayerMovement : MonoBehaviour // 플레이어 캐릭터의 이동, 시점 회전, 중력 처리를 담당하는 클래스
{
    [Header("이동 설정")] // 인스펙터에서 아래 변수들을 "이동 설정" 그룹으로 표시
    public float moveSpeed = 5f; // 캐릭터의 이동 속도
    public float mouseSensitivity = 2f; // 마우스 이동에 대한 시점 회전 민감도

    [Header("중력 설정")] // 인스펙터에서 아래 변수들을 "중력 설정" 그룹으로 표시
    public float gravity = -9.81f; // 캐릭터에 적용되는 중력 가속도 (음수 = 아래 방향)
    public float groundedGravity = -2f; // 바닥에 있을 때 적용할 약한 하강 속도 (캐릭터를 바닥에 밀착시키기 위함)

    [Header("카메라")] // 인스펙터에서 아래 변수를 "카메라" 그룹으로 표시
    public Camera playerCamera; // 플레이어 시점을 담당하는 카메라 (상하 회전에 사용)

    private CharacterController controller; // 실제 이동 처리를 담당하는 Unity의 CharacterController 컴포넌트 참조
    private float xRotation = 0f; // 카메라의 상하(피치) 회전 누적 각도
    private Vector3 velocity; // 현재 프레임의 수직 이동 속도(중력에 의한 낙하 속도 등)를 저장하는 벡터

    void Start() // 씬이 시작될 때 호출되는 초기화 함수
    {
        controller = GetComponent<CharacterController>(); // 같은 오브젝트에 부착된 CharacterController 컴포넌트를 가져와 캐싱

        Cursor.lockState = CursorLockMode.Locked; // 마우스 커서를 화면 중앙에 고정시켜 시점 조작이 가능하게 함
        Cursor.visible = false; // 마우스 커서를 화면에서 보이지 않게 숨김
    }

    void Update() // 매 프레임마다 호출되어 입력에 따른 이동/회전을 처리하는 함수
    {
        if (GameProgress.InputLocked) return; // 게임 진행 상태에서 입력이 잠겨 있다면(UI 사용 중 등) 이동/회전 처리를 하지 않고 종료

        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼이 눌렸다면
        {
            Cursor.lockState = CursorLockMode.Locked; // 마우스 커서를 다시 화면 중앙에 고정 (UI 클릭 등으로 풀렸던 커서를 게임 화면 클릭 시 복원)
            Cursor.visible = false; // 마우스 커서를 다시 숨김
        }

        LookAround(); // 마우스 입력에 따른 시점(카메라/캐릭터) 회전 처리
        Move(); // 키보드 입력에 따른 이동 및 중력 처리
    }

    void Move() // 키보드 입력에 따라 캐릭터를 이동시키고 중력을 적용하는 함수
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // 좌우 이동 입력값(A/D 또는 화살표 좌/우)을 보정 없이 즉시 가져옴 (-1, 0, 1)
        float vertical = Input.GetAxisRaw("Vertical"); // 전후 이동 입력값(W/S 또는 화살표 위/아래)을 보정 없이 즉시 가져옴 (-1, 0, 1)

        Vector3 moveDir = transform.right * horizontal + transform.forward * vertical; // 캐릭터의 오른쪽/앞쪽 방향 벡터에 입력값을 곱하여 합산한 이동 방향 벡터 계산

        if (moveDir.magnitude > 1f) // 대각선 이동 시 이동 방향 벡터의 길이가 1보다 커질 수 있으므로 (예: 1.41)
        {
            moveDir.Normalize(); // 벡터를 정규화하여 길이를 1로 맞춤 (대각선 이동이 더 빨라지지 않도록)
        }

        controller.Move(moveDir * moveSpeed * Time.deltaTime); // 이동 방향에 속도와 프레임 시간을 곱하여 실제 캐릭터를 수평 이동시킴

        if (controller.isGrounded && velocity.y < 0) // 캐릭터가 바닥에 닿아 있고, 현재 수직 속도가 아래 방향(음수)이라면
        {
            velocity.y = groundedGravity; // 수직 속도를 약한 하강값으로 고정하여 캐릭터를 바닥에 밀착시킴 (중력 누적으로 인한 통과 방지)
        }

        velocity.y += gravity * Time.deltaTime; // 중력 가속도를 프레임 시간만큼 누적하여 수직 속도를 점점 감소(하강)시킴

        controller.Move(velocity * Time.deltaTime); // 계산된 수직 속도에 프레임 시간을 곱하여 캐릭터를 수직으로 이동시킴 (낙하/착지 처리)
    }

    void LookAround() // 마우스 입력에 따라 카메라(상하)와 캐릭터(좌우) 시점을 회전시키는 함수
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity; // 마우스의 좌우 이동량에 감도를 곱하여 좌우 회전량 계산
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity; // 마우스의 상하 이동량에 감도를 곱하여 상하 회전량 계산

        transform.Rotate(Vector3.up * mouseX); // 캐릭터 본체를 Y축(위쪽 방향) 기준으로 좌우 회전시킴

        xRotation -= mouseY; // 카메라의 상하 회전 누적 각도를 갱신 (마우스를 위로 올리면 위를 보도록 부호 반전)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 상하 회전 각도가 -90도에서 90도 범위를 벗어나지 않도록 제한 (과도하게 꺾이는 것 방지)

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // 계산된 상하 회전 각도를 카메라의 로컬 회전값으로 적용 (카메라만 위아래로 움직임)
    }
}
