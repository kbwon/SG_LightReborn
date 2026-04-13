using System.Collections.Generic;
using UnityEngine;

public enum FlowStage
{
    Intro = 0,
    LampTaken = 1,
    DeerActivated = 2,
    FreeRestore = 3,
    WolfUnlocked = 4,
    WolfCleared = 5,
    TreeCleared = 6,
    Finished = 7
}

public class FlowManager : MonoBehaviour
{
    public static FlowManager Instance { get; private set; }

    [Header("Current Flow")]
    [SerializeField] private FlowStage currentStage = FlowStage.Intro;

    [Header("Lamp Data")]
    [SerializeField] private float lampPower = 0f;

    [Header("Restore Progress")]
    [SerializeField] private int restoredSmallAnimals = 0;
    [SerializeField] private int requiredSmallAnimalsToUnlockWolf = 2;

    [Header("Flags")]
    [SerializeField] private bool lampTaken = false;
    [SerializeField] private bool deerActivated = false;
    [SerializeField] private bool orbCollected = false;
    [SerializeField] private bool wolfUnlocked = false;
    [SerializeField] private bool wolfCleared = false;
    [SerializeField] private bool treeCleared = false;
    [SerializeField] private bool endingCompleted = false;

    [Header("Targets To Unlock By Stage")]
    [SerializeField] private HotspotTarget[] unlockOnLampTaken;
    [SerializeField] private HotspotTarget[] unlockOnFreeRestore;
    [SerializeField] private HotspotTarget[] unlockOnWolfUnlocked;
    [SerializeField] private HotspotTarget[] unlockOnWolfCleared;
    [SerializeField] private HotspotTarget[] unlockOnTreeCleared;

    [Header("Objects To Show / Hide By Stage")]
    [SerializeField] private GameObject[] showOnLampTaken;
    [SerializeField] private GameObject[] hideOnLampTaken;

    [SerializeField] private GameObject[] showOnFreeRestore;
    [SerializeField] private GameObject[] hideOnFreeRestore;

    [SerializeField] private GameObject[] showOnWolfUnlocked;
    [SerializeField] private GameObject[] hideOnWolfUnlocked;

    [SerializeField] private GameObject[] showOnWolfCleared;
    [SerializeField] private GameObject[] hideOnWolfCleared;

    [SerializeField] private GameObject[] showOnTreeCleared;
    [SerializeField] private GameObject[] hideOnTreeCleared;

    [SerializeField] private GameObject[] showOnFinished;
    [SerializeField] private GameObject[] hideOnFinished;

    private readonly Dictionary<GameObject, bool> initialObjectStates = new Dictionary<GameObject, bool>();

    [SerializeField] private PlayerLamp playerLampVisual;

    public FlowStage CurrentStage => currentStage;
    public float LampPower => lampPower;
    public int RestoredSmallAnimals => restoredSmallAnimals;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CacheInitialStates(showOnLampTaken, hideOnLampTaken);
        CacheInitialStates(showOnFreeRestore, hideOnFreeRestore);
        CacheInitialStates(showOnWolfUnlocked, hideOnWolfUnlocked);
        CacheInitialStates(showOnWolfCleared, hideOnWolfCleared);
        CacheInitialStates(showOnTreeCleared, hideOnTreeCleared);
        CacheInitialStates(showOnFinished, hideOnFinished);

        // 처음에는 해금 대상들을 잠가 둡니다.
        SetTargetsInteractable(unlockOnLampTaken, false);
        SetTargetsInteractable(unlockOnFreeRestore, false);
        SetTargetsInteractable(unlockOnWolfUnlocked, false);
        SetTargetsInteractable(unlockOnWolfCleared, false);
        SetTargetsInteractable(unlockOnTreeCleared, false);
    }

    private void Start()
    {
        
    }

    public void NotifyLampTaken()
    {
        if (lampTaken) return;

        lampTaken = true;
        currentStage = FlowStage.LampTaken;

        SetTargetsInteractable(unlockOnLampTaken, true);
        ApplyObjectGroup(showOnLampTaken, hideOnLampTaken);

        Debug.Log("[FlowManager] Lamp taken -> Stage: LampTaken");
    }

    public void NotifyTargetActivated(HotspotTarget target)
    {
        if (target == null) return;

        switch (target.TargetType)
        {
            case HotspotType.StartDeer:
                deerActivated = true;
                currentStage = FlowStage.DeerActivated;
                Debug.Log("[FlowManager] Start deer activated -> Stage: DeerActivated");
                break;

            case HotspotType.StartOrb:
                orbCollected = true;
                currentStage = FlowStage.FreeRestore;

                SetTargetsInteractable(unlockOnFreeRestore, true);
                ApplyObjectGroup(showOnFreeRestore, hideOnFreeRestore);

                Debug.Log("[FlowManager] Start orb collected -> Stage: FreeRestore");
                break;

            case HotspotType.SmallAnimal:
                restoredSmallAnimals++;
                Debug.Log($"[FlowManager] Small animal restored: {restoredSmallAnimals}");

                if (!wolfUnlocked && restoredSmallAnimals >= requiredSmallAnimalsToUnlockWolf)
                {
                    wolfUnlocked = true;
                    currentStage = FlowStage.WolfUnlocked;

                    SetTargetsInteractable(unlockOnWolfUnlocked, true);
                    ApplyObjectGroup(showOnWolfUnlocked, hideOnWolfUnlocked);

                    Debug.Log("[FlowManager] Wolf unlocked -> Stage: WolfUnlocked");
                }
                break;

            case HotspotType.Wolf:
                Debug.Log("[FlowManager] Wolf hotspot activated");
                break;

            case HotspotType.WolfOrb:
                wolfCleared = true;
                currentStage = FlowStage.WolfCleared;

                SetTargetsInteractable(unlockOnWolfCleared, true);
                ApplyObjectGroup(showOnWolfCleared, hideOnWolfCleared);

                Debug.Log("[FlowManager] Wolf cleared -> Stage: WolfCleared");
                break;

            case HotspotType.Tree:
                treeCleared = true;
                currentStage = FlowStage.TreeCleared;

                SetTargetsInteractable(unlockOnTreeCleared, true);
                ApplyObjectGroup(showOnTreeCleared, hideOnTreeCleared);

                Debug.Log("[FlowManager] Tree cleared -> Stage: TreeCleared");
                break;

            case HotspotType.EndingDeer:
                endingCompleted = true;
                currentStage = FlowStage.Finished;

                ApplyObjectGroup(showOnFinished, hideOnFinished);

                Debug.Log("[FlowManager] Ending completed -> Stage: Finished");
                break;

            case HotspotType.Custom:
            default:
                Debug.Log("[FlowManager] Custom hotspot activated");
                break;
        }
    }

    public void AddLampPower(float amount)
    {
        lampPower += amount;
        Debug.Log($"[FlowManager] LampPower changed: {lampPower}");

        if (playerLampVisual != null)
        {
            playerLampVisual.IncreaseLight(amount);
        }
    }

    public void SetLampPower(float value)
    {
        lampPower = value;
        Debug.Log($"[FlowManager] LampPower set: {lampPower}");
    }

    public void ResetAll()
    {
        currentStage = FlowStage.Intro;

        lampPower = 0f;
        restoredSmallAnimals = 0;

        lampTaken = false;
        deerActivated = false;
        orbCollected = false;
        wolfUnlocked = false;
        wolfCleared = false;
        treeCleared = false;
        endingCompleted = false;

        // 추적 중인 오브젝트를 처음 상태로 복구
        foreach (var pair in initialObjectStates)
        {
            if (pair.Key != null)
            {
                pair.Key.SetActive(pair.Value);
            }
        }

        // 핫스팟 자체도 초기 상태로 복구
        HotspotTarget[] allTargets = FindObjectsOfType<HotspotTarget>(true);
        foreach (HotspotTarget target in allTargets)
        {
            target.ResetTargetState();
        }

        // 시작 시 잠가야 하는 것들 다시 잠금
        SetTargetsInteractable(unlockOnLampTaken, false);
        SetTargetsInteractable(unlockOnFreeRestore, false);
        SetTargetsInteractable(unlockOnWolfUnlocked, false);
        SetTargetsInteractable(unlockOnWolfCleared, false);
        SetTargetsInteractable(unlockOnTreeCleared, false);

        ProjectionActorSequence[] sequences = FindObjectsOfType<ProjectionActorSequence>(true);
        foreach (ProjectionActorSequence sequence in sequences)
        {
            sequence.ResetSequence();
        }

        SmallAnimalRunSequence[] sequences_s = FindObjectsOfType<SmallAnimalRunSequence>(true);
        foreach (SmallAnimalRunSequence sequence in sequences_s)
        {
            sequence.ResetSequence();
        }
        SpriteCrossFadeGroup[] fadeGroups = FindObjectsOfType<SpriteCrossFadeGroup>(true);
        foreach (SpriteCrossFadeGroup group in fadeGroups)
        {
            group.ResetFade();
        }

        DeerEndController[] deerEndControllers = FindObjectsOfType<DeerEndController>(true);
        foreach (DeerEndController controller in deerEndControllers)
        {
            controller.ResetSequence();
        }

        Debug.Log("[FlowManager] ResetAll completed");
    }

    private void ApplyObjectGroup(GameObject[] showGroup, GameObject[] hideGroup)
    {
        SetObjectsActive(showGroup, true);
        SetObjectsActive(hideGroup, false);
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

    private void SetTargetsInteractable(HotspotTarget[] targets, bool value)
    {
        if (targets == null) return;

        foreach (HotspotTarget target in targets)
        {
            if (target != null)
            {
                target.SetInteractable(value);
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

    public void CompleteEnding()
    {
        endingCompleted = true;
        currentStage = FlowStage.Finished;

        ApplyObjectGroup(showOnFinished, hideOnFinished);

        Debug.Log("[FlowManager] Ending completed -> Stage: Finished");
    }


}