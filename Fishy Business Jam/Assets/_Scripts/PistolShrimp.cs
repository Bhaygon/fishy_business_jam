using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class PistolShrimp : MonoBehaviour, IDamageable
{
    [Header("References")] [SerializeField]
    private Rigidbody2D Rb;

    [SerializeField] private Collider2D Collider;
    [SerializeField] private Animator Animator;
    [SerializeField] private Transform BodyTransform;

    [Header("Variables")] [SerializeField] private LayerMask ObstacleLayer;
    [SerializeField] private LayerMask GroundLayer;
    [SerializeField] private LayerMask PlayerLayer;
    [SerializeField] private float _speed;
    [SerializeField] private float _obstacleDetectRange;
    [SerializeField] private float _groundDetectionRayLength;
    [SerializeField] private float _attackRange;
    private bool _isFacingRight;
    private bool _isGrounded;
    private bool _isAttacking = false;
    [SerializeField] float _attackStart;
    [SerializeField] float _attackCooldown;
    private float _attackCooldownTimer;
    [SerializeField] private GameObject _projectile;
    [Header("Health")] [SerializeField] private float _health = 3;
    [SerializeField] private GameObject _onDeathEffect;

    private void Start()
    {
        Turn(true);
    }

    public void ReceiveDamage(int damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            Instantiate(_onDeathEffect, BodyTransform.position, BodyTransform.rotation);
            this.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        AttackCheck();
        if (!_isAttacking)
        {
            MoveCheck();
            CheckIsGrounded();
            CheckTurn();
        }
    }

    private void MoveCheck()
    {
        Vector2 moveHorizontal = (_isFacingRight ? Vector2.right : Vector2.left) * (_speed * Time.deltaTime);
        transform.Translate(moveHorizontal);
    }

    private void AttackCheck()
    {
        _attackCooldownTimer -= Time.deltaTime;
        if (!_isAttacking && _attackCooldownTimer <= 0)
        {
            RaycastHit2D attackHit = Physics2D.Raycast(BodyTransform.position, _isFacingRight ? Vector2.right : Vector2.left, _attackRange,
                PlayerLayer);
            if (attackHit.collider)
            {
                _isAttacking = true;
                Animator.SetBool("Attack", true);
                StartCoroutine(Attack());
            }
        }
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(_attackStart);
        Instantiate(_projectile, new Vector2(BodyTransform.position.x, BodyTransform.position.y - 0.2f), BodyTransform.rotation);
        yield return new WaitForSeconds(1.2f - _attackStart);
        Animator.SetBool("Attack", false);
        Turn(!_isFacingRight);
        _attackCooldownTimer = _attackCooldown;
        _isAttacking = false;
    }

    private void CheckTurn()
    {
        if (_isGrounded)
        {
            RaycastHit2D sideHit = Physics2D.Raycast(BodyTransform.position, _isFacingRight ? Vector2.right : Vector2.left, _obstacleDetectRange,
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
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(BodyTransform.position, BodyTransform.position + (Vector3.left * _obstacleDetectRange));
        Gizmos.DrawLine(BodyTransform.position, BodyTransform.position + (Vector3.right * _obstacleDetectRange));

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine((BodyTransform.position) + Vector3.down * 0.1f,
            (BodyTransform.position + (Vector3.left * _attackRange)) + Vector3.down * 0.1f);
        Gizmos.DrawLine((BodyTransform.position) + Vector3.down * 0.1f,
            (BodyTransform.position + (Vector3.right * _attackRange)) + Vector3.down * 0.1f);

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