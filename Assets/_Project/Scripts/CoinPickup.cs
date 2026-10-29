using UnityEngine;

/// <summary>
/// Awards score once when the player enters this coin's trigger.
/// </summary>
public sealed class CoinPickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int value = 1;
    [SerializeField] private ParticleSystem pickupEffect;

    private bool collected;

    private void OnTriggerEnter(Collider other)
    {
        if (collected || other.GetComponentInParent<PlayerController>() == null)
            return;

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
            return;

        collected = true;
        gameManager.AddCoins(value);
        AudioManager.Instance?.PlaySound("SFX_Coin");

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
