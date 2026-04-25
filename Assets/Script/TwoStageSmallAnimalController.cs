using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoStageSmallAnimalController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FlowManager flowManager;

    [Header("Stage 1")]
    [SerializeField] private HotspotTarget stage1Hotspot;
    [SerializeField] private SmallAnimalRunSequence stage1Sequence;

    [Header("Stage 2")]
    [SerializeField] private HotspotTarget stage2Hotspot;
    [SerializeField] private SmallAnimalRunSequence stage2Sequence;
    [SerializeField] private GameObject[] stage2ObjectsToEnable;
    [SerializeField] private float respawnDelay = 1.0f;

    [Header("Hide Unresolved After Wolf Clear")]
    [SerializeField] private bool hideIfUnresolvedAfterWolfClear = true;
    [SerializeField] private GameObject[] unresolvedObjectsToHide;

    [Header("Optional Marker")]
    [SerializeField] private WorldObjectiveMarker stage1Marker;
    [SerializeField] private WorldObjectiveMarker stage2Marker;
    [SerializeField] private string fleeMessage = "µµ¸Á°¬½À´Ï´Ù";
    [SerializeField] private string rescuedMessage = "½£À¸·Î µ¹¾Æ°©´Ï´Ù";

    private bool stage1Triggered = false;
    private bool stage2Triggered = false;
    private bool hiddenAfterWolfClear = false;

    private readonly Dictionary<GameObject, bool> initialObjectStates = new Dictionary<GameObject, bool>();

    private void Awake()
    {
        if (flowManager == null)
        {
            flowManager = FlowManager.Instance;
        }

        CacheInitialStates(stage2ObjectsToEnable, unresolvedObjectsToHide);

        SetStage2Enabled(false);

        if (stage2Hotspot != null)
        {
            stage2Hotspot.SetInteractable(false);
        }
    }

    private void Update()
    {
        if (!hideIfUnresolvedAfterWolfClear) return;
        if (hiddenAfterWolfClear) return;
        if (stage2Triggered) return;

        if (flowManager == null)
        {
            flowManager = FlowManager.Instance;
        }

        if (flowManager != null && (int)flowManager.CurrentStage >= (int)FlowStage.WolfCleared)
        {
            HideUnresolvedNow();
        }
    }

    public void OnStage1Activated()
    {
        if (stage1Triggered) return;
        stage1Triggered = true;

        if (stage1Marker != null)
        {
            stage1Marker.ShowTemporaryMessage(fleeMessage);
        }

        if (stage1Sequence != null)
        {
            stage1Sequence.PlaySequence();
        }

        StartCoroutine(Stage1ToStage2Routine());
    }

    public void OnStage2Activated()
    {
        if (stage2Triggered) return;
        stage2Triggered = true;

        if (stage2Marker != null)
        {
            stage2Marker.ShowTemporaryMessage(rescuedMessage);
        }

        if (stage2Sequence != null)
        {
            stage2Sequence.PlaySequence();
        }
    }

    private IEnumerator Stage1ToStage2Routine()
    {
        float waitTime = 0f;

        if (stage1Sequence != null)
        {
            waitTime = stage1Sequence.TotalDuration;
        }

        yield return new WaitForSeconds(waitTime + respawnDelay);

        if (hiddenAfterWolfClear) yield break;

        if (flowManager == null)
        {
            flowManager = FlowManager.Instance;
        }

        if (flowManager != null && (int)flowManager.CurrentStage >= (int)FlowStage.WolfCleared)
        {
            yield break;
        }

        SetStage2Enabled(true);

        if (stage2Hotspot != null)
        {
            stage2Hotspot.SetInteractable(true);
        }
    }

    private void HideUnresolvedNow()
    {
        hiddenAfterWolfClear = true;

        if (stage1Hotspot != null)
        {
            stage1Hotspot.SetInteractable(false);
        }

        if (stage2Hotspot != null)
        {
            stage2Hotspot.SetInteractable(false);
        }

        SetObjectsActive(unresolvedObjectsToHide, false);
        SetStage2Enabled(false);
    }

    private void SetStage2Enabled(bool value)
    {
        SetObjectsActive(stage2ObjectsToEnable, value);
    }

    private void SetObjectsActive(GameObject[] objects, bool value)
    {
        if (objects == null) return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(value);
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

    public void ResetController()
    {
        StopAllCoroutines();

        stage1Triggered = false;
        stage2Triggered = false;
        hiddenAfterWolfClear = false;

        foreach (var pair in initialObjectStates)
        {
            if (pair.Key != null)
            {
                pair.Key.SetActive(pair.Value);
            }
        }

        SetStage2Enabled(false);

        if (stage2Hotspot != null)
        {
            stage2Hotspot.SetInteractable(false);
        }
    }
}