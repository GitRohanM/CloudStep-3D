using UnityEngine;

/// <summary>
/// Sends movement and jump state to the character Animator.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimationDriver : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int GroundedHash = Animator.StringToHash("Grounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");

    [Min(0f)] public float speedDamping = 0.12f;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false;
    }

    public void SetMovement(float normalizedSpeed, bool grounded)
    {
        if (_animator == null) return;
        _animator.SetFloat(SpeedHash, normalizedSpeed, speedDamping, Time.deltaTime);
        _animator.SetBool(GroundedHash, grounded);
    }

    public void PlayJump()
    {
        if (_animator == null) return;
        _animator.ResetTrigger(JumpHash);
        _animator.SetTrigger(JumpHash);
    }
}
