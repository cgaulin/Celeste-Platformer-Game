using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Variables")]
    public float moveSpeed;
    public float jumpForce;
    [SerializeField] private bool isFacingRight;
    [SerializeField] private float maxFallSpeed;
    private Rigidbody2D rb;
    private Vector2 moveVector;
    private Vector2 respawnPoint;

    [Header("Jump Variables")]
    public float jumpTime;
    private float jumpTimeCounter;
    private bool isJumping;
    private bool hasJumpStarted;
    private bool alreadyJumped;

    private float coyoteTimeCounter;
    public float coyoteTime;

    private float jumpBufferCounter;
    public float jumpBufferTime;

    public float fallingGravityScale;

    [Header("Wall Jump Variables")]
    public float wallJumpingTime;
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingCounter;
    public float wallJumpingDuration;
    public Vector2 wallJumpingPower;

    [Header("Wall Check")]
    public float wallSlidingSpeed;
    private bool isWallSliding;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Vector2 wallSpotSize;

    [Header("Ground Check")]
    [SerializeField] private bool isGrounded;
    public Transform groundSpot;
    [SerializeField] private float groundSpotRadius;
    public LayerMask groundLayer;

    [Header("Dash Variables")]
    public bool canDash;
    public int currentDashCount;
    [SerializeField] private bool isDashing;
    [SerializeField] private float dashPower;
    [SerializeField] private float dashTime;
    //[SerializeField] private float dashCooldown;
    [SerializeField] private float dashGravity;
    private float normalGravity;

    [Header("Climb Variables")]
    public bool isClimbing;
    public SpriteRenderer sprRenderer;

    [Header("Scene Handler Variables")]
    public SceneHandler sceneHandler;
    public RoomSpawnPoint spawnPoint;
    public GameObject rooms;

    public bool isInteracting { get; private set; }
    public bool isClosingDialogue { get; private set; }
    private NPC_Controller npc;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprRenderer = GetComponent<SpriteRenderer>();
        normalGravity = rb.gravityScale;
        isFacingRight = true;
        canDash = true;
        currentDashCount = 1;
        SetRespawnPoint(transform.position);
    }

    private void FixedUpdate()
    {
        if (isDashing || isClimbing) { return; }
        if (!InDialogue())
        {
            if (!isWallJumping)
            {
                UtilizeMovement();
            }
        }
    }

    private void Update()
    {
        ZeroOutFriction();  

        StopMovementInDialogue();

        if (!InDialogue())
        {
            if (isDashing) { return; }
            IsGrounded();

            if (!isWallJumping)
            {
                CheckDirection();
            }

            if (IsGrounded())
            {
                alreadyJumped = false;
            }

            AddJumpBoost();

            HandleCoyoteTimeLogic();

            //HandleJumpBufferLogic();

            ExecuteCoyoteTimeAndJumpBufferLogic();

            CheckIfAlreadyDashed();

            HandleDiagonalMovement();

            ClampMaxFallSpeed();

            if (!isClimbing)
            {
                WallSlide();
            }

            WallJump();

            if (isClimbing && !isJumping)
            {
                rb.velocity = new Vector2(0f, moveVector.y * 3f);
            }

            if (!IsWalled())
            {
                isClimbing = false;
                rb.gravityScale = normalGravity;
            }

            if (isClimbing) { return; }
            HandleGravity();
        }

        //Used in PlayerAnimator script
        CurrentPlayerVelocity();
    }

    //Getters
    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundSpot.position, groundSpotRadius, groundLayer);
    }

    public bool IsWalled()
    {
        return Physics2D.OverlapBox(wallCheck.position, wallSpotSize, 0f, wallLayer);
    }

    public Vector2 CurrentPlayerVelocity()
    {
        return rb.velocity;
    }

    public bool IsDashing()
    {
        return isDashing;
    }

    public bool IsFacingRight()
    {
        return isFacingRight;
    }

    public Vector2 GetMoveVector()
    {
        return moveVector;
    }


    //Main Movement Methods
    public void Move(InputAction.CallbackContext ctx)
    {
        moveVector = ctx.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            hasJumpStarted = true;
            rb.gravityScale = normalGravity;
        }

        if (ctx.performed)
        {
            isJumping = true;
            alreadyJumped = true;
            isClimbing = false;
            WallJumpHelper();
        }

        if (ctx.canceled)
        {
            isJumping = false;
            hasJumpStarted = false;
            coyoteTimeCounter = 0f;  
        }
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && canDash && !InDialogue())
        {
            Invoke("StartDash", 0f);
        }
    }

    public void Climb(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsWalled())
        {
            isClimbing = true;
            rb.gravityScale = 0f;
        }

        if (ctx.canceled)
        {
            isClimbing = false;
            rb.gravityScale = normalGravity;
        }
    }

    public void Interact(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            isInteracting = true;
        }

        if (ctx.canceled)
        {
            isInteracting = false;
        }
    }

    public void CloseDialogue(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            isClosingDialogue = true;
        }
        if (ctx.canceled)
        {
            isClosingDialogue = false;
        }
    }


    //Helper functions for the movement
    private void UtilizeMovement()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(moveVector.x * moveSpeed, rb.velocity.y);
        }
    }

    private void AddJumpBoost()
    {
        if (isJumping && !IsGrounded())
        {
            if (jumpTimeCounter > 0f && !alreadyJumped)
            {
                rb.velocity = Vector2.up * jumpForce;
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }
    }

    private void HandleGravity()
    {
        if (CurrentPlayerVelocity().y < 0f && !isClimbing || !isJumping)
        {
            rb.gravityScale = fallingGravityScale;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            //Allows us to turn away from the wall for a brief moment to walljump
            wallJumpingCounter -= Time.deltaTime;
        }
    }

    private void WallJumpHelper()
    {
        if (isJumping && (IsWalled() && !IsGrounded()))
        {
            isWallJumping = true;
            if (!isFacingRight)
            {
                if (IsWalled())
                {
                    rb.velocity = new Vector2(wallJumpingPower.x, wallJumpingPower.y);
                }
                else
                {
                    rb.velocity = new Vector2(-wallJumpingPower.x, wallJumpingPower.y);
                }
            }
            else
            {
                if (IsWalled())
                {
                    rb.velocity = new Vector2(-wallJumpingPower.x, wallJumpingPower.y);
                }
                else
                {
                    rb.velocity = new Vector2(wallJumpingPower.x, wallJumpingPower.y);
                }

            }

            wallJumpingCounter = 0f;

            if (transform.localScale.x != wallJumpingDirection)
            {
                Flip();
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && moveVector.x != 0f && rb.velocity.y <= 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, -wallSlidingSpeed);
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void StartDash()
    {
        TurnOnDashParameters();

        RumbleWhileDashing();

        if (moveVector == Vector2.zero && IsGrounded())
        {
            DashWithNoMoveVector();
        }
        else if (rb.velocity.x == 0f && rb.velocity.y != 0f && moveVector.y == 0)
        {
            DashWithNoMoveVector();
        }
        else if (rb.velocity == Vector2.zero && moveVector == Vector2.zero)
        {
            DashWithNoMoveVector();
        }
        else
        {
            rb.velocity = moveVector.normalized * dashPower;
        }
        Invoke("StopDash", dashTime);
    }

    private void DashWithNoMoveVector()
    {
        if (isFacingRight)
        {
            if (isClimbing && sprRenderer.flipX)
            {
                rb.velocity = new Vector2(-1f * dashPower, 0f);
            }
            else
            {
                rb.velocity = new Vector2(1f * dashPower, 0f);
            }
        }
        if (!isFacingRight)
        {
            if (isClimbing && sprRenderer.flipX)
            {
                rb.velocity = new Vector2(1f * dashPower, 0f);
            }
            else
            {
                rb.velocity = new Vector2(-1f * dashPower, 0f);
            }
        }

    }

    private void TurnOnDashParameters()
    {
        isDashing = true;
        currentDashCount--;
        rb.gravityScale = dashGravity;
    }

    private void StopDash()
    {
        TurnOffDashParameters();

        if (rb.velocity.y >= 10f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }

    private void TurnOffDashParameters()
    {
        isDashing = false;
        //alreadyDashed = true;
        rb.gravityScale = normalGravity;
    }

    private void CheckIfAlreadyDashed()
    {
        if (IsGrounded() && !isDashing)
        {
            canDash = true;
            currentDashCount = 1;
        }
        else if (!IsGrounded() && currentDashCount == 0)
        {
            canDash = false;
        }
    }

    //Misc helper functions
    public void SetRespawnPoint(Vector2 pos)
    {
        respawnPoint = pos;
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(1f);
        transform.position = respawnPoint;
    }

    private void HandleDiagonalMovement()
    {
        if (IsGrounded() && (moveVector.x < 1f && moveVector.x != 0f) && (moveVector.y < 1f && moveVector.y != 0f))
        {
            StopDiagonalSlowSpeed();
        }

        if (!IsGrounded() && moveVector.y > 0f && moveVector.x != 0f)
        {
            SlowDownDiagonalMoveVector();
        }
    }

    private void ZeroOutFriction()
    {
        if (rb.velocity.x >= -0.00009f && rb.velocity.x <= 0f)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        else if (rb.velocity.x <= 0.00009f && rb.velocity.x >= 0f)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }

    private void StopMovementInDialogue()
    {
        if (InDialogue() && rb.velocity != Vector2.zero)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void ClampMaxFallSpeed()
    {
        if (rb.velocity.y <= -maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
        }
    }

    private void SlowDownDiagonalMoveVector()
    {
        if (isFacingRight)
        {
            moveVector = new Vector2(1f, 0.71f);
        }
        if (!isFacingRight)
        {
            moveVector = new Vector2(-1f, 0.71f);
        }
        if (isFacingRight && isClimbing && sprRenderer.flipX)
        {
            moveVector = new Vector2(-1f, 0.71f);
        }
        if (!isFacingRight && isClimbing && sprRenderer.flipX)
        {
            moveVector = new Vector2(1f, 0.71f);
        }
    }

    private void StopDiagonalSlowSpeed()
    {
        if (isFacingRight)
        {
            moveVector = Vector2.right;
        }
        if (!isFacingRight)
        {
            moveVector = Vector2.left;
        }
    }

    private void HandleCoyoteTimeLogic()
    {
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void HandleJumpBufferLogic()
    {
        if (hasJumpStarted)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= jumpBufferTime;
        }
    }

    private void ExecuteCoyoteTimeAndJumpBufferLogic()
    {
        if (hasJumpStarted && coyoteTimeCounter > 0f)
        {
            //isJumping = true;
            jumpTimeCounter = jumpTime;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce * 1.75f);
            hasJumpStarted = false;
            jumpBufferCounter = 0f;
        }
    }

    private void RumbleWhileDashing()
    {
        RumbleManager.instance.RumblePulse(0.25f, 1f, dashTime);
    }

    private void CheckDirection()
    {
        if (isClimbing && moveVector.x < 0f && isFacingRight)
        {
            FlipSpriteOn();
        }
        else if (isClimbing && moveVector.x > 0f && isFacingRight)
        {
            FlipSpriteOff();
        }
        else if (isClimbing && moveVector.x > 0f && !isFacingRight)
        {
            FlipSpriteOn();
        }
        else if (isClimbing && moveVector.x < 0f && !isFacingRight)
        {
            FlipSpriteOff();
        }
        else
        {
            if (moveVector.x > 0f && !isFacingRight)
            {
                Flip();
            }
            else if (moveVector.x < 0f && isFacingRight)
            {
                Flip();
            }
        }

        if (!isClimbing && !isFacingRight && sprRenderer.flipX)
        {
            FlipSpriteOff();
        }
        else if (!isClimbing && isFacingRight && sprRenderer.flipX)
        {
            FlipSpriteOff();
        }
    }

    private void Flip()
    {
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;

        isFacingRight = !isFacingRight;
    }

    private void FlipSpriteOn()
    {
        sprRenderer.flipX = true;
    }

    private void FlipSpriteOff()
    {
        sprRenderer.flipX = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (IsGrounded()) { Gizmos.color = Color.green; }
        else { Gizmos.color = Color.red; }

        Gizmos.DrawWireSphere(groundSpot.position, groundSpotRadius);

        if (IsWalled()) { Gizmos.color = Color.green; }
        else { Gizmos.color = Color.red; }

        Gizmos.DrawWireCube(wallCheck.position, wallSpotSize);
    }

    private bool InDialogue()
    {
        if (npc != null)
        {
            return npc.DialogueActive();
        }
        else
        {
            return false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Hazards")
        {
            StartCoroutine(Respawn());
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            npc = other.gameObject.GetComponent<NPC_Controller>();

            if (isInteracting)
            {
                npc.ActivateDialogue();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        npc = null;
    }
}
