using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Step Timing")]
    [SerializeField] private float minMoveSpeedToStep = 1.2f;
    [SerializeField] private float stepDistance = 2f;
    [SerializeField] private float groundCheckDistance = 1.8f;

    [Header("Audio Variation")]
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;
    [SerializeField] private float minVolume = 0.9f;
    [SerializeField] private float maxVolume = 1f;

    [Header("Surface Clips")]
    [SerializeField] private AudioClip[] woodClips;
    [SerializeField] private AudioClip[] stoneClips;
    [SerializeField] private AudioClip[] dirtClips;
    [SerializeField] private AudioClip[] defaultClips;

    private CharacterController controller;
    private float walkedDistance;

    private enum SurfaceType
    {
        Default,
        Wood,
        Stone,
        Dirt
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (footstepSource == null)
        {
            footstepSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        Vector3 planarVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        float speed = planarVelocity.magnitude;

        if (!controller.isGrounded || speed < minMoveSpeedToStep)
        {
            walkedDistance = 0f;
            return;
        }

        walkedDistance += speed * Time.deltaTime;
        if (walkedDistance < stepDistance)
        {
            return;
        }

        walkedDistance = 0f;
        PlayFootstep();
    }

    private void PlayFootstep()
    {
        if (footstepSource == null)
        {
            return;
        }

        AudioClip[] clips = GetClipsForSurface(DetectSurface());
        if (clips == null || clips.Length == 0)
        {
            clips = defaultClips;
        }

        if (clips == null || clips.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, clips.Length);
        footstepSource.pitch = Random.Range(minPitch, maxPitch);
        footstepSource.volume = Random.Range(minVolume, maxVolume);
        footstepSource.PlayOneShot(clips[randomIndex]);
    }

    private SurfaceType DetectSurface()
    {
        Vector3 origin = transform.position + Vector3.up * 0.2f;
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            return SurfaceType.Default;
        }

        string tag = hit.collider.tag.ToLowerInvariant();
        if (tag.Contains("wood"))
        {
            return SurfaceType.Wood;
        }

        if (tag.Contains("stone") || tag.Contains("rock"))
        {
            return SurfaceType.Stone;
        }

        if (tag.Contains("dirt") || tag.Contains("ground") || tag.Contains("soil"))
        {
            return SurfaceType.Dirt;
        }

        PhysicMaterial physicMaterial = hit.collider.sharedMaterial;
        if (physicMaterial == null)
        {
            return SurfaceType.Default;
        }

        string materialName = physicMaterial.name.ToLowerInvariant();
        if (materialName.Contains("wood"))
        {
            return SurfaceType.Wood;
        }

        if (materialName.Contains("stone") || materialName.Contains("rock") || materialName.Contains("concrete"))
        {
            return SurfaceType.Stone;
        }

        if (materialName.Contains("dirt") || materialName.Contains("ground") || materialName.Contains("soil") || materialName.Contains("sand"))
        {
            return SurfaceType.Dirt;
        }

        return SurfaceType.Default;
    }

    private AudioClip[] GetClipsForSurface(SurfaceType type)
    {
        switch (type)
        {
            case SurfaceType.Wood:
                return woodClips;
            case SurfaceType.Stone:
                return stoneClips;
            case SurfaceType.Dirt:
                return dirtClips;
            default:
                return defaultClips;
        }
    }
}
