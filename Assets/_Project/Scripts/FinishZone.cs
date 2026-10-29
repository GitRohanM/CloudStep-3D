using UnityEngine;

/// <summary>Completes the course when the player passes through the finish gate.</summary>
[RequireComponent(typeof(Collider))]
public sealed class FinishZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null)
            GameManager.Instance?.LevelCompleted();
    }
}
