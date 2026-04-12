using UnityEngine;

public class SceneStartSetup : MonoBehaviour
{
    [SerializeField] private SimpleFirstPersonTestPlayer player;
    [SerializeField] private FlowManager flowManager;
    [SerializeField] private bool equipLampOnStart = true;
    [SerializeField] private bool notifyLampTakenOnStart = true;

    private void Start()
    {
        if (player != null && equipLampOnStart)
        {
            player.EquipLamp();
        }

        if (flowManager != null && notifyLampTakenOnStart)
        {
            flowManager.NotifyLampTaken();
        }
    }
}