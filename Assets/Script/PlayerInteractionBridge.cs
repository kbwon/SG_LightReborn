using UnityEngine;

public class PlayerInteractionBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform lampOrigin;
    [SerializeField] private PlayerLamp playerLamp;

    [Header("Input")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private int mouseButton = 0;
    [SerializeField] private bool hasLamp = true;

    public bool HasLamp => hasLamp;
    public bool IsUsingLamp { get; private set; }
    public Camera TargetCamera => targetCamera;
    public Transform LampOrigin => lampOrigin;
    public PlayerLamp Lamp => playerLamp;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (playerLamp == null)
        {
            playerLamp = GetComponentInChildren<PlayerLamp>();
        }
    }

    private void Update()
    {
        bool useByMouse = Input.GetMouseButton(mouseButton);
        bool useByKey = Input.GetKey(interactionKey);

        IsUsingLamp = useByMouse || useByKey;
    }

    public void SetHasLamp(bool value)
    {
        hasLamp = value;
    }
}