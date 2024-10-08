using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private CapsuleCollider2D col;

    private bool canBeControlled;
    private float defaultGravityScale;


    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    private float xInput;
    private float yInput;
    private bool canDoubleJump;

    [Header("Stats")]
    [SerializeField] private float HealthPoints;
    [SerializeField] private float AttackPower;
    [SerializeField] private float blockDuration;

    [Header("Collision")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    private bool isWallDetected;
    private bool isGrounded;
    private bool isAirBorne;

    [Header("Wall interactions")]
    [SerializeField] private float wallJumpDuration = .6f;
    [SerializeField] private Vector2 wallJumpForce;
    private bool isWallJumping;

    [Header("KnockBack")]
    [SerializeField] private float knockBackDuration = 0.6f;
    [SerializeField] private Vector2 knockBackForce;
    private bool isKnocked;

    [Header("VFX")]
    [SerializeField] private GameObject DeathVFX;

    private bool facingRight = true;
    private int facingDirection = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        col = GetComponent<CapsuleCollider2D>();
    }

    private void Start()
    {
        defaultGravityScale = rb.gravityScale;
        RespawnFinished(false);
    }

    void Update()
    {
        HandleAirBorneStatus();

        if (!canBeControlled)
            return;

        if (isKnocked)
            return;

        HandleInput();
        HandleWallSlide();
        HandleMovement();
        HandleFlip();
        HandleCollision();
        HandleAnimation();

        RespawnFinished(canBeControlled);
    }

    public void RespawnFinished(bool finished)
    {

        if (finished)
        {
            rb.gravityScale = defaultGravityScale;
            canBeControlled = true;
            col.enabled = true;
        }
        else
        {
            rb.gravityScale = 0;
            canBeControlled = false;
            col.enabled = false;
        }
    }

    private void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpButton();
        }
    }
    private void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround);
    }
    private void HandleMovement()
    {
        if (isWallDetected)
            return;
        if (isWallJumping)
            return;

        rb.velocity = new Vector2(xInput * moveSpeed, rb.velocity.y);
    }
    private void HandleAnimation()
    {
        anim.SetFloat("xVelocity", rb.velocity.x);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWallDetected", isWallDetected);
    }
    public void KnockBack()
    {
        if (isKnocked)
            return;
        StartCoroutine(KnockBackRoutine());
        anim.SetTrigger("knockback");
        rb.velocity = new Vector2(knockBackForce.x * -facingDirection, knockBackForce.y * -facingDirection);
    }
    private IEnumerator KnockBackRoutine()
    {
        isKnocked = true;
        yield return new WaitForSeconds(knockBackDuration);
        isKnocked = false;
    }
    public void Die()
    {
        GameObject newDeathVFX = Instantiate(DeathVFX, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
    private void HandleAirBorneStatus()
    {
        if (isGrounded && isAirBorne)
            HandleLanding();
        else if (!isGrounded && !isAirBorne)
            BecomeAirBorne();
    }
    private void BecomeAirBorne()
    {
        isAirBorne = true;
    }
    private void HandleLanding()
    {
        isAirBorne = false;
        canDoubleJump = true;
    }
    private void jumpButton()
    {
        if (isGrounded)
        {
            Jump();
        }
        else if (isWallDetected)
        {
            WallJump();
        }
        else if (isAirBorne && canDoubleJump)
        {
            DoubleJump();
        }

    }

    private void Jump() => rb.velocity = new Vector2 (rb.velocity.x, jumpForce);
    private void DoubleJump()
    {
        canDoubleJump = false;
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
        anim.SetTrigger("doublejump");
    }
    private void WallJump()
    {
        canDoubleJump = true;
        rb.velocity = new Vector2(wallJumpForce.x * -facingDirection, wallJumpForce.y);

        Flip();

        StopAllCoroutines();
        StartCoroutine(WallJumpRoutine());
    }
    private IEnumerator WallJumpRoutine()
    {
        isWallJumping = true;

        yield return new WaitForSeconds(wallJumpDuration);

        isWallJumping = false;
    }

    private void HandleWallSlide()
    {
        bool canWallSlide = isWallDetected && rb.velocity.y < 0;
        float yModifier = yInput < 0 ? 1 : .05f;

        if (!canWallSlide)
            return;

        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * yModifier);
    }

    private void HandleFlip()
    {
        if (xInput < 0 && facingRight || xInput > 0 && !facingRight)
        {
            Flip();
        }
    }
    private void Flip()
    {
        transform.Rotate(0, 180, 0);
        facingDirection *= -1;
        facingRight = !facingRight;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (wallCheckDistance * facingDirection), transform.position.y));
    }
}
