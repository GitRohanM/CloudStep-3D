using UnityEngine;

/// <summary>
/// Notifies the level manager when the player falls below the course.
/// </summary>
[RequireComponent(typeof(Collider))]
public sealed class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
            return;

        AudioManager.Instance?.PlaySound("SFX_Player_Fall");

        GameManager manager = GameManager.Instance;
        if (manager != null)
            manager.PlayerDied(player);
    }
}
