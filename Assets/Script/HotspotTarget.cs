using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum HotspotType
{
    Custom,
    StartDeer,
    StartOrb,
    SmallAnimal,
    Wolf,
    WolfOrb,
    Tree,
    EndingDeer
}

public class HotspotTarget : MonoBehaviour
{
    [Header("Basic Info")]
    [SerializeField] private string targetId = "Target";
    [SerializeField] private HotspotType targetType = HotspotType.Custom;

    [Header("Interaction")]
    [SerializeField] private bool interactable = true;
    [SerializeField] private bool requiresLamp = true;
    [SerializeField] private bool oneShot = true;
    [SerializeField] private FlowStage requiredStage = FlowStage.Intro;
    [SerializeField] private float requiredHoldTime = 1.5f;

    [Header("Lamp Reward")]
    [SerializeField] private float addLampPower = 0f;

    [Header("Show / Hide On Activate")]
    [SerializeField] private GameObject[] showOnActivate;
    [SerializeField] private GameObject[] hideOnActivate;

    [Header("Extra Event")]
    [SerializeField] private UnityEvent onActivated;

    [Header("Debug")]
    [SerializeField] private bool activated = false;
    [SerializeField] private float holdProgress = 0f;

    private bool initialInteractable;
    private readonly Dictionary<GameObject, bool> initialObjectStates = new Dictionary<GameObject, bool>();

    public string TargetId => targetId;
    public HotspotType TargetType => targetType;
    public bool Activated => activated;
    public bool Interactable => interactable;
    public float HoldProgressNormalized
    {
        get
        {
            if (requiredHoldTime <= 0f) return 1f;
            return Mathf.Clamp01(holdProgress / requiredHoldTime);
        }
    }

    private void Awake()
    {
        initialInteractable = interactable;
        CacheInitialStates(showOnActivate, hideOnActivate);
    }

    public bool CanInteract(FlowManager flowManager, PlayerInteractionBridge player)
    {
        if (!interactable) return false;
        if (oneShot && activated) return false;

        if (flowManager != null)
        {
            if ((int)flowManager.CurrentStage < (int)requiredStage)
            {
                return false;
            }
        }

        if (requiresLamp)
        {
            if (player == null) return false;
            if (!player.HasLamp) return false;
        }

        return true;
    }

    public float TickHold(float deltaTime, bool isUsingLamp, FlowManager flowManager, PlayerInteractionBridge player)
    {
        if (!CanInteract(flowManager, player))
        {
            ResetHold();
            return 0f;
        }

        if (requiresLamp && !isUsingLamp)
        {
            ResetHold();
            return 0f;
        }

        if (requiredHoldTime <= 0f)
        {
            Activate(flowManager);
            return 1f;
        }

        holdProgress += deltaTime;

        if (holdProgress >= requiredHoldTime)
        {
            Activate(flowManager);
        }

        return HoldProgressNormalized;
    }

    public void ResetHold()
    {
        holdProgress = 0f;
    }

    public void SetInteractable(bool value)
    {
        interactable = value;
    }

    public void Activate(FlowManager flowManager)
    {
        if (oneShot && activated) return;

        activated = true;
        holdProgress = 0f;

        if (flowManager != null && Mathf.Abs(addLampPower) > 0.0001f)
        {
            flowManager.AddLampPower(addLampPower);
        }

        SetObjectsActive(showOnActivate, true);
        SetObjectsActive(hideOnActivate, false);

        onActivated?.Invoke();

        if (flowManager != null)
        {
            flowManager.NotifyTargetActivated(this);
        }

        Debug.Log($"[HotspotTarget] Activated: {targetId} ({targetType})");
    }

    public void ResetTargetState()
    {
        activated = false;
        holdProgress = 0f;
        interactable = initialInteractable;

        foreach (var pair in initialObjectStates)
        {
            if (pair.Key != null)
            {
                pair.Key.SetActive(pair.Value);
            }
        }
    }

    [ContextMenu("Manual Activate")]
    private void ManualActivate()
    {
        FlowManager manager = FlowManager.Instance;
        Activate(manager);
    }

    private void SetObjectsActive(GameObject[] objects, bool active)
    {
        if (objects == null) return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }

    private void CacheInitialStates(params GameObject[][] objectGroups)
    {
        foreach (GameObject[] group in objectGroups)
        {
            if (group == null) continue;

            foreach (GameObject obj in group)
            {
                if (obj == null) continue;
                if (initialObjectStates.ContainsKey(obj)) continue;

                initialObjectStates.Add(obj, obj.activeSelf);
            }
        }
    }
}