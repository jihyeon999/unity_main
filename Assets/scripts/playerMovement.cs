using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("�̵� ����")]
    public float moveSpeed = 5f;        // �̵� �ӵ�
    public float mouseSensitivity = 2f; // ���콺 ����

    [Header("�߷� ����")]
    public float gravity = -9.81f;      // �߷�
    public float groundedGravity = -2f; // ���� �پ��ְ� �ϴ� ���� �߷°�

    [Header("ī�޶�")]
    public Camera playerCamera;         // �÷��̾� ī�޶�

    private CharacterController controller; 
    private float xRotation = 0f;       // ī�޶� ���� ȸ����
    private Vector3 velocity;           // �߷� �ӵ� �����

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // ���콺 Ŀ�� ���
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // UI (memo / quiz) or laptop in use -> lock movement and look
        if (GameProgress.InputLocked) return;

        // ���콺 Ŭ���ϸ� Ŀ�� �ٽ� ���
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        LookAround();
        Move();
    }

    void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A / D
        float vertical = Input.GetAxisRaw("Vertical");     // W / S

        // �÷��̾ �ٶ󺸴� ���� �������� �̵�
        Vector3 moveDir = transform.right * horizontal + transform.forward * vertical;

        // �밢�� �̵� �ӵ� ����
        if (moveDir.magnitude > 1f)
        {
            moveDir.Normalize();
        }

        // ���� �̵�
        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        // �ٴڿ� ��� ������ �Ʒ��� ��¦ ������
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = groundedGravity;
        }

        // �߷� ����
        velocity.y += gravity * Time.deltaTime;

        // ���� �̵�
        controller.Move(velocity * Time.deltaTime);
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // �÷��̾� ���� �¿� ȸ��
        transform.Rotate(Vector3.up * mouseX);

        // ī�޶� ���� ȸ��
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); //�ʹ� ���� ȸ������ �ʵ��� �þ� ������ ����

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}