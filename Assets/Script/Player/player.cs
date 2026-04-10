using UnityEngine;

// CharacterController-based first-person movement and mouse look.
[RequireComponent(typeof(CharacterController))]
public class player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float deceleration = 14f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float lookSensitivity = 120f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool lockCursorOnStart = true;
    [Header("Walk Feel")]
    [SerializeField] private float bobFrequency = 8.2f;
    [SerializeField] private float bobVerticalAmplitude = 0.02f;
    [SerializeField] private float bobHorizontalAmplitude = 0f;
    [SerializeField] private float bobPositionSmooth = 10f;
    [SerializeField] private float strafeTiltAngle = 0f;
    [SerializeField] private float bobTiltAngle = 0.4f;
    [SerializeField] private float tiltSmooth = 8f;

    private CharacterController controller;
    private float verticalVelocity;
    private float pitch;
    private Vector3 currentPlanarVelocity;
    private Vector3 cameraStartLocalPos;
    private float bobTimer;
    private float currentCameraRoll;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform != null)
        {
            cameraStartLocalPos = cameraTransform.localPosition;
        }
    }

    private void Start()
    {
        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        HandleLook();
        HandleMove();
        HandleHeadBob();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        if (cameraTransform == null)
        {
            return;
        }

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, currentCameraRoll);
    }

    private void HandleMove()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 desiredPlanarVelocity = (transform.right * inputDir.x + transform.forward * inputDir.z) * moveSpeed;

        float changeRate = inputDir.sqrMagnitude > 0.001f ? acceleration : deceleration;
        currentPlanarVelocity = Vector3.MoveTowards(
            currentPlanarVelocity,
            desiredPlanarVelocity,
            changeRate * Time.deltaTime
        );

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        Vector3 finalVelocity = currentPlanarVelocity;
        finalVelocity.y = verticalVelocity;

        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void HandleHeadBob()
    {
        if (cameraTransform == null)
        {
            return;
        }

        Vector3 targetPosition = cameraStartLocalPos;
        Vector3 planarVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        bool isWalking = controller.isGrounded && planarVelocity.magnitude > 0.1f;
        float targetRoll = 0f;

        if (isWalking)
        {
            float speedRatio = Mathf.Clamp01(planarVelocity.magnitude / moveSpeed);
            bobTimer += Time.deltaTime * bobFrequency * Mathf.Lerp(0.85f, 1.4f, speedRatio);

            float bobSin = Mathf.Sin(bobTimer);
            float bobCos = Mathf.Cos(bobTimer * 2f);

            targetPosition.y += bobSin * bobVerticalAmplitude * speedRatio;
            targetPosition.x += bobCos * bobHorizontalAmplitude * speedRatio;
            targetRoll += bobCos * bobTiltAngle * speedRatio;
        }
        else
        {
            bobTimer = 0f;
        }

        float strafeInput = Input.GetAxisRaw("Horizontal");
        targetRoll += -strafeInput * strafeTiltAngle;
        currentCameraRoll = Mathf.Lerp(currentCameraRoll, targetRoll, tiltSmooth * Time.deltaTime);

        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            targetPosition,
            bobPositionSmooth * Time.deltaTime
        );
    }
}
