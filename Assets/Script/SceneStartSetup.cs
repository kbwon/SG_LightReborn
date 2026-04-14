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
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDarkRuinBGM();
        }
    }
}