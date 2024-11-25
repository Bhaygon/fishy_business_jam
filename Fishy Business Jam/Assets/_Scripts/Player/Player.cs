using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Player : MonoBehaviour, IDamageable
{
    [Header("References")] public Rigidbody2D Rb;
    [SerializeField] private Transform PlayerTransform;
    [SerializeField] private Collider2D _feetColl, _bodyColl;
    [SerializeField] private Animator _animator;

    [Header("Walk")] [Range(1f, 100f)] public float MaxWalkSpeed;
    [Range(0.25f, 50f)] public float GroundAcceleration;
    [Range(0.25f, 50f)] public float GroundDeceleration;
    [Range(0.25f, 50f)] public float AirAcceleration;
    [Range(0.25f, 50f)] public float AirDeceleration;

    [Header("Run")] [Range(1f, 100f)] public float MaxRunSpeed;

    [Header("Ground / Collision checks")] public LayerMask GroundLayer;
    public float GroundDetectionRayLength;
    public float HeadDetectionRayLength;
    [Range(0f, 1f)] public float HeadWidth = 0.75f;
    public bool DebugShowIsGroundedBox;
    public bool DebugShowIsHeadBumpBox;

    [Header("Variables")] private Vector2 _moveVelocity;
    private bool _isFacingRight;
    private RaycastHit2D _groundHit, _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    [Header("Jump")] public float JumpHeight = 6.5f;
    [Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.054f;
    public float TimeTillJumpApex = 0.35f;
    [Range(0.01f, 5f)] public float GravityOnReleaseMultiplier = 2f;
    public float MaxFallSpeed = 26f;
    [Range(1, 5)] public int NumberOfJumpsAllowed = 2;
    public GameObject JumpParticlesPrefab;

    [Header("Jump cut")] [Range(0.02f, 0.3f)]
    public float TimeForUpwardCancel = 0.027f;

    [Header("Jump apex")] [Range(0.5f, 1f)]
    public float ApexThreshold = 0.97f;

    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f;

    [Header("Jump buffer")] [Range(0f, 1f)]
    public float JumpBufferTime = 0.125f;

    [Header("Jump Coyote time")] [Range(0, 1f)]
    public float JumpCoyoteTime = 0.1f;

    [Header("Jump visualization tool")] public bool ShowWalkJumpArc = false;
    public bool ShowRunJumpArc = false;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    [Range(5, 100)] public int ArcResolution = 20;
    [Range(0, 500)] public int VisualizationSteps = 90;

    [Header("Variables")] public float Gravity { get; private set; }
    public float InitialJumpVelocity { get; private set; }
    public float AdjustedJumpHeight { get; private set; }
    public float VerticalVelocity { get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;
    private float _jumpBufferTimer;
    private bool _jumpReleaseDuringBuffer;
    private float _coyoteTimer;

    [Header("Combat")] [SerializeField] private PlayerAttack _playerAttack;
    private bool _isDead;
    [SerializeField] private int _playerHealthMax;
    private int _currentPlayerHealth;
    [SerializeField] private GameObject _onDeathEffect;
    
    

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _isFacingRight = true;
        _currentPlayerHealth = _playerHealthMax;
        GameManager.Instance?.UpdateHealthUI(_currentPlayerHealth, _playerHealthMax);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) ReceiveDamage(_currentPlayerHealth);
        CountTimers();
        JumpChecks();
        AnimationChecks();
    }

    public void ReceiveDamage(int amount)
    {
        //print("received damage: " + amount);
        if (amount < _currentPlayerHealth)
        {
            _currentPlayerHealth -= amount;
            GameManager.Instance.UpdateHealthUI(_currentPlayerHealth, _playerHealthMax);
        }
        else
        {
            _currentPlayerHealth = 0;
            GameManager.Instance.UpdateHealthUI(_currentPlayerHealth, _playerHealthMax);
            Die();
        }
    }

    private void Die()
    {
        Instantiate(_onDeathEffect, PlayerTransform.position, PlayerTransform.rotation);
        GameManager.Instance.ShowDeathScreen();
        this.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        var rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        CollisionChecks();
        Jump();

        if (IsGrounded())
        {
            Move(GroundAcceleration, GroundDeceleration, rawInput);
        }
        else
        {
            Move(AirAcceleration, AirDeceleration, rawInput);
        }
    }

    public void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);

            var targetVelocity = Vector2.zero;
            var direction = moveInput.normalized;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                targetVelocity = new Vector2(direction.x, 0f) * MaxRunSpeed;
            }
            else
            {
                targetVelocity = new Vector2(direction.x, 0f) * MaxWalkSpeed;
            }

            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }

        Rb.linearVelocity = !_playerAttack.IsShooting
            ? new Vector2(_moveVelocity.x, Rb.linearVelocity.y)
            : new Vector2(_moveVelocity.x / 3, Rb.linearVelocity.y);
    }

    public void TurnCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if (!_isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            _isFacingRight = true;
            PlayerTransform.Rotate(0f, 180f, 0f);
        }
        else
        {
            _isFacingRight = false;
            PlayerTransform.Rotate(0f, -180f, 0f);
        }
    }

    private void CheckIsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_bodyColl.bounds.size.x, GroundDetectionRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, GroundDetectionRayLength,
            GroundLayer);

        if (_groundHit.collider != null)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }

    private void CheckBumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _bodyColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x * HeadWidth, HeadDetectionRayLength);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, HeadDetectionRayLength,
            GroundLayer);

        if (_headHit.collider != null)
        {
            _bumpedHead = true;
        }
        else
        {
            _bumpedHead = false;
        }
    }

    public void CollisionChecks()
    {
        CheckIsGrounded();
        CheckBumpedHead();
    }

    public void AnimationChecks()
    {
        _animator.SetFloat("Velocity", Mathf.Abs(Rb.linearVelocity.x));
        _animator.SetBool("Jumping", _isJumping);
        _animator.SetBool("Falling", _isFalling);
    }

    public void JumpChecks()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !_playerAttack.IsShooting)
        {
            _jumpBufferTimer = JumpBufferTime;
            _jumpReleaseDuringBuffer = false;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (_jumpBufferTimer > 0f)
            {
                _jumpReleaseDuringBuffer = true;
            }

            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = TimeForUpwardCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (_jumpReleaseDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        else if (_jumpBufferTimer > 0f && _isJumping && _numberOfJumpsUsed < NumberOfJumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }
        else if (_jumpBufferTimer > 0f && _isFalling && _numberOfJumpsUsed < NumberOfJumpsAllowed - 1)
        {
            InitiateJump(2);
            _isFastFalling = false;
        }

        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberOfJumpsUsed = 0;

            VerticalVelocity = Physics2D.gravity.y;
        }
    }

    private void InitiateJump(int numberOfJumpsUsed)
    {
        Instantiate(JumpParticlesPrefab, _feetColl.bounds.center, Quaternion.identity);
        if (!_isJumping)
        {
            _isJumping = true;
        }

        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = InitialJumpVelocity;
    }

    public void Jump()
    {
        if (_isJumping)
        {
            if (_bumpedHead)
            {
                _isFastFalling = true;
            }

            if (VerticalVelocity >= 0f)
            {
                _apexPoint = Mathf.InverseLerp(InitialJumpVelocity, 0f, VerticalVelocity);

                if (_apexPoint > ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;

                        if (_timePastApexThreshold < ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }
                else
                {
                    VerticalVelocity += Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }
            }
            else if (!_isFastFalling)
            {
                VerticalVelocity += Gravity * GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (VerticalVelocity < 0f)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }

        if (_isFastFalling)
        {
            if (_fastFallTime > TimeForUpwardCancel)
            {
                VerticalVelocity += Gravity * GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_fastFallTime < TimeForUpwardCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / TimeForUpwardCancel));
            }

            _fastFallTime += Time.fixedDeltaTime;
        }

        if (!_isGrounded && !_isJumping)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }

            VerticalVelocity += Gravity * Time.fixedDeltaTime;
        }

        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MaxFallSpeed, 50f);

        Rb.linearVelocity = !_playerAttack.IsShooting
            ? new Vector2(Rb.linearVelocity.x, VerticalVelocity)
            : new Vector2(Rb.linearVelocity.x, VerticalVelocity / 3);
    }

    private void OnValidate()
    {
        CalculateValues();
    }

    private void OnEnable()
    {
        CalculateValues();
    }

    private void CalculateValues()
    {
        AdjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
        Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
        InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
    }

    public bool IsGrounded()
    {
        return _isGrounded;
    }

    public void CountTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;

        if (!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else
        {
            _coyoteTimer = JumpCoyoteTime;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (DebugShowIsGroundedBox)
        {
            Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
            Vector2 boxCastSize = new Vector2(_bodyColl.bounds.size.x, GroundDetectionRayLength);

            Color rayColor;
            if (_isGrounded)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y),
                Vector2.down * GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y),
                Vector2.down * GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - GroundDetectionRayLength),
                Vector2.right * boxCastSize.x, rayColor);
        }

        if (DebugShowIsHeadBumpBox)
        {
            Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _bodyColl.bounds.max.y);
            Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x * HeadWidth, HeadDetectionRayLength);

            Color rayColor;
            if (_isGrounded)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * HeadWidth, boxCastOrigin.y),
                Vector2.up * HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2 * HeadWidth, boxCastOrigin.y),
                Vector2.up * HeadDetectionRayLength, rayColor);
            Debug.DrawRay(
                new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * HeadWidth, boxCastOrigin.y + HeadDetectionRayLength),
                Vector2.right * boxCastSize.x * HeadWidth, rayColor);
        }

        if (ShowWalkJumpArc)
        {
            DarwJumpArc(MaxWalkSpeed, Color.white);
        }

        if (ShowRunJumpArc)
        {
            DarwJumpArc(MaxRunSpeed, Color.red);
        }
    }

    private void DarwJumpArc(float moveSpeed, Color gizmosColor)
    {
        Vector2 startPosition = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 previousPosition = startPosition;
        float speed = 0;

        if (DrawRight)
        {
            speed = moveSpeed;
        }
        else
        {
            speed = -moveSpeed;
        }

        Vector2 velocity = new Vector2(speed, InitialJumpVelocity);

        Gizmos.color = gizmosColor;

        float timeStep = 2 * TimeTillJumpApex / ArcResolution;

        for (int i = 0; i < VisualizationSteps; i++)
        {
            float simulationTime = i * timeStep;
            Vector2 displacement;
            Vector2 drawPoint;

            if (simulationTime < TimeTillJumpApex)
            {
                displacement = velocity * simulationTime +
                               0.5f * new Vector2(0f, Gravity) * simulationTime * simulationTime;
            }
            else if (simulationTime < TimeTillJumpApex + ApexHangTime)
            {
                float apexTime = simulationTime - TimeTillJumpApex;
                displacement = velocity * TimeTillJumpApex + 0.5f * new Vector2(0f, Gravity) * TimeTillJumpApex * TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * apexTime;
            }
            else
            {
                float descendTime = simulationTime - (TimeTillJumpApex + ApexHangTime);
                displacement = velocity * TimeTillJumpApex + 0.5f * new Vector2(0f, Gravity) * TimeTillJumpApex * TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * ApexHangTime;
                displacement += new Vector2(speed, 0) * descendTime + 0.5f * new Vector2(0f, Gravity) * descendTime * descendTime;
            }

            drawPoint = startPosition + displacement;

            if (StopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPosition, drawPoint - previousPosition, Vector2.Distance(previousPosition, drawPoint),
                    GroundLayer);
                if (hit.collider != null)
                {
                    Gizmos.DrawLine(previousPosition, hit.point);
                    break;
                }
            }

            Gizmos.DrawLine(previousPosition, drawPoint);
            previousPosition = drawPoint;
        }
    }
#endif
}