using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SoyCharacterController : MonoBehaviour
{
    #region SERIALIZED FIELDS

    [Header("Sprites")]
    [SerializeField] private GameObject standing;
    [SerializeField] private GameObject walking;
    [SerializeField] private GameObject jumping;
    [SerializeField] private GameObject falling;
    [SerializeField] private GameObject acked;
    [SerializeField] private GameObject squirrel;

    [Header("Movement Settings")]
    [SerializeField] private float baseMoveSpeed = 10f;
    [SerializeField] private float squirrelMoveSpeed = 15f;
    private float moveSpeed = 20f;
    [SerializeField] private float jumpForce = 1400f;
    // [SerializeField] private float smoothMovement = 0.15f;
    [SerializeField] private PhysicsMaterial2D fullFriction;
    [SerializeField] private PhysicsMaterial2D noFriction;

    [Header("Step Settings")]
    [SerializeField] private float stepOffset = 0.6f;
    [SerializeField] private float stepCheckDistance = 0.2f;

    [Header("Ground & Wall Checks")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform slopeCheck;

    [Header("Rock Push")]
    [SerializeField] private float pushRadius = 1f;
    [SerializeField] private float pushForce = 500f;

    [Header("Events")]
    [SerializeField] private UnityEvent OnLandEvent;

    #endregion
    
    [Header("Debug")]   
    [SerializeField] private bool debugMode = false;

    #region PUBLIC FIELDS

    public System.Action<SoyCharacterSprite> onSpriteChanged;
    public Rigidbody2D rb;

    #endregion

    #region PRIVATE FIELDS

    private GameObject currentSprite;
    private CapsuleCollider2D capsule;
    private bool isJumping;
    private bool isMoving;
    private bool isGrounded;
    private bool isOnSlope;
    public bool dead;
    private bool facingRight = true;
    private float xInput = 0;
    private float slopeSideAngle = -90f;
    private float slopeDownAngle = 0f;
    private float slopeDownAngle_Old = 0f;
    private Vector2 slopeNormalPerp;
    private const float k_GroundedRadius = 0.3f;
    private const float k_slopeCheckDist = 1.5f;
    // private float smoothVelocityX;
    // private float smoothVelocityY;
    #endregion

    #region UNITY METHODS

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();
        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();
    }

    private void Start()
    {
        SetSprite(standing);
        moveSpeed = baseMoveSpeed;
    }

    private void FixedUpdate()
    {
        CheckGround();
        CheckSlope();
        rb.sharedMaterial = isGrounded && xInput == 0 ? fullFriction : noFriction;
        CheckWalls();
        UpdateSprite();
    }

    #endregion

    #region MOVEMENT

    public void SetSquirrelSpeed(bool squirrel)
    {
        moveSpeed = squirrel ? squirrelMoveSpeed : baseMoveSpeed;
    }

    public void Move(float moveX)
    {
        if (dead) return;

        if (Mathf.Abs(moveX) <= 0.1f)
        {
            StopMoving();
            return;
        }

        TryStepUp(moveX);
        xInput = moveX;
        float xVelo = xInput * (isGrounded ? moveSpeed : moveSpeed * 0.6f);
        Vector2 targetVelocity;

        // for constant movement speed when climbing slopes
        if (isGrounded && isOnSlope && !isJumping)
        {
            Vector2 slopeDir = -slopeNormalPerp.normalized;
            targetVelocity = slopeDir * xVelo;
        }
        else
        {
            targetVelocity = new Vector2(xVelo, rb.linearVelocity.y);
        }

        rb.linearVelocity = targetVelocity;
        isMoving = true;

        if ((xInput > 0 && !facingRight) || (xInput < 0 && facingRight))
            Flip();
    }

    public void StopMoving()
    {
        isMoving = false;
        xInput = 0;
    }

    public void TryJump()
    {
        if (dead)
            return;
        PushNearbyRocks();
        if (!isGrounded)
            return;
        StartCoroutine(DelayedJump());
    }

    private IEnumerator DelayedJump()
    {
        //wait until physics resolve and nearby rocks are pushed away
        yield return new WaitForFixedUpdate();
        Jump();
    }

    public void Jump()
    {
        isGrounded = false;
        isJumping = true;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(new Vector2(0f, jumpForce));
    }
    #endregion

    #region STEP HANDLING

    // raycasts so we can climb small ledges
    private void TryStepUp(float moveInput)
    {
        if (!isGrounded || isJumping || Mathf.Abs(moveInput) < 0.01f)
            return;

        Vector2 moveDir = moveInput > 0 ? Vector2.right : Vector2.left;
        Vector2 basePos = transform.position;
        Vector2 capsuleOffset = capsule.offset;
        Vector2 rayOrigin = basePos + moveDir * (capsule.size.x / 2f + 0.01f);

        Vector2 lowRayOrigin = rayOrigin + Vector2.down * (capsule.size.y / 2f - 0.05f);
        RaycastHit2D hitLow = Physics2D.Raycast(lowRayOrigin, moveDir, stepCheckDistance, whatIsGround);
        if (debugMode) Debug.DrawRay(lowRayOrigin, moveDir * stepCheckDistance, Color.red);

        Vector2 highRayOrigin = lowRayOrigin + Vector2.up * stepOffset;
        RaycastHit2D hitHigh = Physics2D.Raycast(highRayOrigin, moveDir, stepCheckDistance, whatIsGround);
        if (debugMode) Debug.DrawRay(highRayOrigin, moveDir * stepCheckDistance, Color.green);

        if (hitLow.collider != null && hitHigh.collider == null)
        {
            float surfaceFacingAngle = Vector2.Angle(hitLow.normal, -moveDir);
            if (surfaceFacingAngle > 60f)
                return;

            Vector2 probeOrigin = highRayOrigin + moveDir * stepCheckDistance;
            RaycastHit2D downHit = Physics2D.Raycast(probeOrigin, Vector2.down, stepOffset + 0.1f, whatIsGround);
            if (debugMode) Debug.DrawRay(probeOrigin, Vector2.down * stepOffset, Color.blue);

            if (downHit.collider != null)
            {
                float stepTopY = downHit.point.y;
                float capsuleBottomY = basePos.y + capsuleOffset.y - capsule.size.y / 2f;
                float stepHeight = stepTopY - capsuleBottomY;

                if (stepHeight > 0.01f && stepHeight <= stepOffset + 0.01f)
                {
                    Vector2 targetPos = basePos + Vector2.up * stepHeight;
                    bool blocked = Physics2D.OverlapCapsule(
                        targetPos + capsuleOffset,
                        capsule.size,
                        capsule.direction,
                        0f,
                        whatIsGround);

                    if (!blocked)
                        transform.position += Vector3.up * stepHeight;
                }
            }
        }
    }

    #endregion

    #region GROUND & SLOPE CHECKS

    private void CheckGround()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, k_GroundedRadius, whatIsGround);
        if (rb.linearVelocity.y < 0.0f) isJumping = false;

        foreach (var col in colliders)
        {
            if (col.gameObject != gameObject)
            {
                isGrounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }
    }

    private void CheckSlope()
    {
        CheckSlopeHorizontal();
        CheckSlopeVertical();
    }

    private void CheckSlopeHorizontal()
    {
        RaycastHit2D front = Physics2D.Raycast(slopeCheck.position, transform.right, k_slopeCheckDist, whatIsGround);
        RaycastHit2D back = Physics2D.Raycast(slopeCheck.position, -transform.right, k_slopeCheckDist, whatIsGround);
        isOnSlope = front || back;

        if (front)
            slopeSideAngle = Vector2.Angle(front.normal, Vector2.up);
        else if (back)
            slopeSideAngle = Vector2.Angle(back.normal, Vector2.up);
        else
            slopeSideAngle = 0f;
    }

    private void CheckSlopeVertical()
    {
        RaycastHit2D hit = Physics2D.Raycast(slopeCheck.position, Vector2.down, k_slopeCheckDist, whatIsGround);

        if (hit)
        {
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeDownAngle != slopeDownAngle_Old)
                isOnSlope = true;

            slopeDownAngle_Old = slopeDownAngle;
        }
    }

    #endregion

    #region WALL CHECK

    private void CheckWalls()
    {
        RaycastHit2D front = Physics2D.Raycast(transform.position + new Vector3(0.35f, 0f), transform.right, 0.06f, whatIsWall);
        RaycastHit2D back = Physics2D.Raycast(transform.position - new Vector3(0.35f, 0f), -transform.right, 0.06f, whatIsWall);

        if (front)
            rb.linearVelocity = new Vector2(Mathf.Min(0, rb.linearVelocity.x), rb.linearVelocity.y);
        else if (back)
            rb.linearVelocity = new Vector2(Mathf.Max(0, rb.linearVelocity.x), rb.linearVelocity.y);
    }

    #endregion

    #region ROCK PUSHING

    private void PushNearbyRocks()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pushRadius, whatIsGround);

        foreach (var hit in hits)
        {
            Rigidbody2D rockRb = hit.attachedRigidbody;
            if (rockRb != null && rockRb != rb)
            {
                Vector2 direction = (hit.transform.position - transform.position).normalized;
                rockRb.AddForce(direction * pushForce, ForceMode2D.Impulse);
            }
        }
    }

    #endregion

    #region VISUAL

    private void UpdateSprite()
    {
        if (dead)
        {
            SetSprite(acked);
            return;
        }
        else if (SoyGameController.Instance.HasActiveItem(Item.Nut))
            SetSprite(squirrel);
        else
        {
            if (isGrounded)
                SetSprite(isMoving ? walking : standing);
            else
                SetSprite(rb.linearVelocity.y > 0 ? jumping : falling);
        }
    }

    private void SetSprite(GameObject sprite)
    {
        if (currentSprite != null && currentSprite != sprite)
            currentSprite.SetActive(false);

        currentSprite = sprite;
        currentSprite.SetActive(true);
        onSpriteChanged?.Invoke(currentSprite.GetComponent<SoyCharacterSprite>());
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        UpdateSprite();
    }
    #endregion

    #region ACK
    public void Die()
    {
        if (!dead)
        {
            dead = true;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            SetSprite(acked);
            SoyGameController.Instance.GameOver();
        }
    }
    #endregion
}
