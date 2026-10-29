using UnityEngine;

/// <summary>
/// Spins a coin and moves it gently around its placed position.
/// </summary>
public sealed class CoinMotion : MonoBehaviour
{
    [SerializeField, Min(0f)] private float rotationSpeed = 90f;
    [SerializeField, Min(0f)] private float bobHeight = 0.18f;
    [SerializeField, Min(0f)] private float bobFrequency = 1.4f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        float phase = Time.time * bobFrequency * Mathf.PI * 2f;
        transform.position = startPosition + Vector3.up * (Mathf.Sin(phase) * bobHeight);
    }
}
