using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Animator playerAnim;

    private Rigidbody2D rb;

    private bool jumpRequested;
    private bool jumpApplied;

    private static readonly int IsJumpHash = Animator.StringToHash("IsJump");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        bool grounded = IsGrounded();

        if (jumpApplied && grounded && rb.linearVelocity.y <= 0.05f)
        {
            jumpApplied = false;
            jumpRequested = false;

            if (playerAnim != null)
                playerAnim.SetBool(IsJumpHash, false);
        }
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

    public void OnJump(InputValue value)
    {
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
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}