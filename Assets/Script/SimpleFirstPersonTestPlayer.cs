using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFirstPersonTestPlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject lampObject;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Lamp Input")]
    [SerializeField] private bool lampEquipped = true;
    [SerializeField] private KeyCode lampKey = KeyCode.E;

    private CharacterController controller;
    private float verticalVelocity;
    private float pitch;

    public bool IsUsingLamp { get; private set; }
    public bool HasLamp => lampEquipped;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraTransform = cam.transform;
            }
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateLampVisual();
    }

    private void Update()
    {
        HandleLook();
        HandleMove();
        HandleLampInput();

        // 테스트 중 커서 잠금이 불편하면 Esc로 해제
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 좌클릭 시 다시 잠금
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void HandleLook()
    {
        if (cameraTransform == null) return;
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleMove()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.right * inputX + transform.forward * inputZ).normalized;

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    private void HandleLampInput()
    {
        if (!lampEquipped)
        {
            IsUsingLamp = false;
            UpdateLampVisual();
            return;
        }

        bool useByMouse = Input.GetMouseButton(0);
        bool useByKey = Input.GetKey(lampKey);

        IsUsingLamp = useByMouse || useByKey;
        UpdateLampVisual();
    }

    private void UpdateLampVisual()
    {
        if (lampObject == null) return;

        // 램프를 들고 있지 않으면 항상 끔
        if (!lampEquipped)
        {
            lampObject.SetActive(false);
            return;
        }

        // 지금은 테스트용이므로 "사용 중일 때만" 보이게 설정
        lampObject.SetActive(IsUsingLamp);
    }

    public void EquipLamp()
    {
        lampEquipped = true;
        UpdateLampVisual();
    }

    public void UnequipLamp()
    {
        lampEquipped = false;
        IsUsingLamp = false;
        UpdateLampVisual();
    }
}