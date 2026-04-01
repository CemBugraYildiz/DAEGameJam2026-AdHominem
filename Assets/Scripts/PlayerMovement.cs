using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Animator playerAnim;

    // for audio clip
    [SerializeField] private AudioClip[] jumpClips;
    [SerializeField] private float jumpSoundVolume = 0.5f;

    private Rigidbody2D rb;
    private PlayerInput playerInput;

    private bool jumpRequested;
    private bool jumpApplied;
    private bool blockJumpUntilRelease;

    private static readonly int IsJumpHash = Animator.StringToHash("IsJump");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (blockJumpUntilRelease && !IsOwnSouthHeld())
        {
            blockJumpUntilRelease = false;
        }

        bool grounded = IsGrounded();

        if (jumpApplied && grounded && rb.linearVelocity.y <= 0.05f)
        {
            jumpApplied = false;
            jumpRequested = false;

            if (playerAnim != null)
                playerAnim.SetBool(IsJumpHash, false);
        }
    }

    private bool IsOwnSouthHeld()
    {
        if (playerInput == null) return false;

        foreach (var device in playerInput.devices)
        {
            if (device is Gamepad pad && pad.buttonSouth.isPressed)
                return true;
        }

        return false;
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;

        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    public void BlockJumpUntilRelease()
    {
        blockJumpUntilRelease = true;
        jumpRequested = false;

        if (playerAnim != null)
            playerAnim.SetBool(IsJumpHash, false);
    }

    public void OnJump(InputValue value)
    {
        if (blockJumpUntilRelease) return;
        if (!value.isPressed) return;
        if (!IsGrounded()) return;
        if (jumpRequested || jumpApplied) return;

        jumpRequested = true;

        if (playerAnim != null)
            playerAnim.SetBool(IsJumpHash, true);
    }

    public void JumpFromAnimation()
    {
        if (!jumpRequested) return;
        if (!IsGrounded()) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        jumpApplied = true;
        jumpRequested = false;

        // for audio clip
        if (SoundFXManager.Instance != null)
        {
            SoundFXManager.Instance.PlayRandomSoundFXClip(jumpClips, 1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}