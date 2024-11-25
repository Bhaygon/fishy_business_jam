using UnityEngine;
using UnityEngine.Serialization;

public class BossEel : MonoBehaviour, IDamageable
{
    [Header("References")] [SerializeField]
    private Rigidbody2D Rb;

    [SerializeField] private Collider2D Collider, Collider2;
    [SerializeField] private Animator Animator;
    [SerializeField] private Transform BodyTransform;

    [Header("Variables")] [SerializeField] private LayerMask ObstacleLayer;
    [SerializeField] private LayerMask GroundLayer;
    [SerializeField] private LayerMask ImpactLayer;
    [SerializeField] private float _speed;
    [SerializeField] private float _obstacleDetectRange;
    [SerializeField] private float _groundDetectionRayLength;

    private bool _isAttacking = false;
    [SerializeField] private int _damage;
    [SerializeField] float _attackStart;
    [SerializeField] float _attackCooldown;
    [SerializeField] private int _attackSpeedMultiplier;
    [SerializeField] private float _hitForce;
    private float _attackCooldownTimer;

    private float _hitCooldownMax = 0.5f;
    private float _hitCooldownTimer;

    [Header("Health")] [SerializeField] private float _health = 150;
    [SerializeField] private GameObject _onDeathEffect;
    [SerializeField] private GameObject Master;

    private bool _isFacingRight;
    private bool _isGrounded;

    private float _turnCooldown = 1f;
    private float _turnCooldownTimer;

    private void FixedUpdate()
    {
        Cooldowns();
        AttackCheck();
        if (!_isAttacking)
        {
            MoveCheck();
            CheckIsGrounded();
            CheckTurn();
        }
    }

    private void AttackCheck()
    {
        if (_hitCooldownTimer < 0)
        {
            RaycastHit2D boxHit = Physics2D.BoxCast(transform.position + new Vector3(0f, -1f), Collider.bounds.size, 0f, Vector2.down, 0,
                ImpactLayer);
            if (boxHit.collider)
            {
                Player target = boxHit.collider.GetComponentInParent<Player>();
                if (target != null)
                {
                    _hitCooldownTimer = _hitCooldownMax;
                    target.ReceiveDamage(_damage * (_isAttacking ? _attackSpeedMultiplier : 1));
                    target.PushPlayer(_hitForce);
                }
            }
        }
    }

    private void Cooldowns()
    {
        _turnCooldownTimer -= Time.fixedDeltaTime;
        _hitCooldownTimer -= Time.fixedDeltaTime;
    }

    private void MoveCheck()
    {
        //Vector2 moveHorizontal = (_isFacingRight ? Vector2.right : Vector2.left) * (_speed * Time.deltaTime);
        //transform.Translate(moveHorizontal);
        Rb.AddForce((_isFacingRight ? Vector2.right : Vector2.left) * (_speed * Time.fixedDeltaTime), ForceMode2D.Impulse);
    }

    private void CheckTurn()
    {
        if (_isGrounded && _turnCooldownTimer <= 0)
        {
            _turnCooldownTimer = _turnCooldown;
            RaycastHit2D sideHit = Physics2D.BoxCast(BodyTransform.position, new Vector2(2, 3.9f), 0f, _isFacingRight ? Vector2.right : Vector2.left,
                3,
                ObstacleLayer);
            if (sideHit.collider)
            {
                Turn(!_isFacingRight);
            }

            RaycastHit2D sideDownwardsHit = Physics2D.Raycast(
                BodyTransform.position + ((_isFacingRight ? Vector3.right : Vector3.left) * _obstacleDetectRange),
                Vector2.down, _obstacleDetectRange,
                GroundLayer);
            if (!sideDownwardsHit.collider)
            {
                Turn(!_isFacingRight);
            }
        }
    }

    private void CheckIsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(Collider.bounds.center.x, Collider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(Collider.bounds.size.x, _groundDetectionRayLength);

        RaycastHit2D groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, _groundDetectionRayLength,
            GroundLayer);
        if (groundHit.collider)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            _isFacingRight = true;
            BodyTransform.Rotate(0f, -180f, 0f);
        }
        else
        {
            _isFacingRight = false;
            BodyTransform.Rotate(0f, 180f, 0f);
        }

        _isAttacking = false;
    }

    public void ReceiveDamage(int damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            Instantiate(_onDeathEffect, BodyTransform.position, BodyTransform.rotation);
            Master.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(BodyTransform.position, BodyTransform.position + (Vector3.left * _obstacleDetectRange));
        Gizmos.DrawLine(BodyTransform.position, BodyTransform.position + (Vector3.right * _obstacleDetectRange));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(BodyTransform.position + (Vector3.left * _obstacleDetectRange),
            BodyTransform.position + (Vector3.left * _obstacleDetectRange) + Vector3.down * _obstacleDetectRange);
        Gizmos.DrawLine(BodyTransform.position + (Vector3.right * _obstacleDetectRange),
            BodyTransform.position + (Vector3.right * _obstacleDetectRange) + Vector3.down * _obstacleDetectRange);

        Vector2 boxCastOrigin = new Vector2(Collider.bounds.center.x, Collider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(Collider.bounds.size.x, _groundDetectionRayLength);

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
            Vector2.down * _groundDetectionRayLength, rayColor);
        Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y),
            Vector2.down * _groundDetectionRayLength, rayColor);
        Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - _groundDetectionRayLength),
            Vector2.right * boxCastSize.x, rayColor);
    }
}