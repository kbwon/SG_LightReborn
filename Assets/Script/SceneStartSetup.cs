using UnityEngine;

public class SceneStartSetup : MonoBehaviour
{
    [SerializeField] private FlowManager flowManager;

    private void Start()
    {
        if (flowManager != null)
        {
            flowManager.NotifyLampTaken();
        }
    }
}