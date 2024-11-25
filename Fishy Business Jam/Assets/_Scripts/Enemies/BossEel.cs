using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class BossEel : MonoBehaviour, IDamageable
{
    [Header("References")] [SerializeField]
    private Rigidbody2D Rb;

    [SerializeField] private Collider2D Collider, Collider2;
    [SerializeField] private Animator Animator;
    [SerializeField] private Transform BodyTransform;
    [SerializeField] private GameObject GrenadePrefab;

    [Header("Variables")] [SerializeField] private LayerMask ObstacleLayer;
    [SerializeField] private LayerMask GroundLayer;
    [SerializeField] private LayerMask PlayerLayer;
    [SerializeField] private float _speed;
    [SerializeField] private float _obstacleDetectRange;
    [SerializeField] private float _groundDetectionRayLength;

    private bool _isAttacking = false;
    [SerializeField] private float _initializeAttackTimeMax;
    private float _initializeAttackTimer;
    [SerializeField] private int _damage;
    float _attackDurationTimer;
    [SerializeField] float _attackCooldownTimeMax = 5;
    [SerializeField] private float _lookForAttackRange;
    [SerializeField] private int _attackSpeedMultiplier;
    [SerializeField] private float _hitForce;
    [SerializeField] private GameObject _onHitEffect;
    [SerializeField] private GameObject _onHitEffectStrong;

    [SerializeField] private float _useGrenadeTimeMax = 5;
    private float _useGrenadeTimer;
    private float _attackCooldownTimer = 1;

    private float _hitCooldownMax = 0.5f;
    private float _hitCooldownTimer;

    [Header("Health")] [SerializeField] private float _health = 150;
    [SerializeField] private GameObject _onDeathEffect;
    [SerializeField] private GameObject _onDeathEffect2;
    [SerializeField] private GameObject Master;

    private bool _isFacingRight;
    private bool _isGrounded;

    private float _turnCooldown = 1f;
    private float _turnCooldownTimer;

    private void FixedUpdate()
    {
        Cooldowns();
        AttackCheck();
        MoveCheck();
        CheckIsGrounded();
        CheckTurn();
    }

    private void AttackCheck()
    {
        _attackDurationTimer -= Time.fixedDeltaTime;
        
        if (!_isAttacking) // Se nao esta atacando e nao esta no cooldown
        {
            RaycastHit2D attackHit = Physics2D.Raycast(BodyTransform.position + new Vector3(0f, -1f), _isFacingRight ? Vector2.right : Vector2.left, _lookForAttackRange,
                PlayerLayer);
            if (attackHit.collider && _attackCooldownTimer < 0)
            {
                print("see player");
                StartAttacking();
            }
            else if (attackHit.collider && _useGrenadeTimer < 0)
            {
                _useGrenadeTimer = _useGrenadeTimeMax;
                StartCoroutine(UseGranade());
            }
        }
        
        if (_isAttacking && _attackDurationTimer < 0) // Duracao do ataque
        {
            _isAttacking = false;
            Animator.SetBool("Attack", false);
            _attackCooldownTimer = _attackCooldownTimeMax;
        }
        
        if (_hitCooldownTimer < 0 && _initializeAttackTimer < 0)
        {
            RaycastHit2D boxHit = Physics2D.BoxCast(transform.position + new Vector3(0f, -1f), Collider.bounds.size * 1.1f, 0f, Vector2.down, 0,
                PlayerLayer);
            if (boxHit.collider)
            {
                Player target = boxHit.collider.GetComponentInParent<Player>();
                if (target != null)
                {
                    _hitCooldownTimer = _hitCooldownMax;
                    target.ReceiveDamage(_damage * (_isAttacking ? _attackSpeedMultiplier : 1));
                    Instantiate(_isAttacking ? _onHitEffectStrong : _onHitEffect, boxHit.collider.transform.position, Quaternion.identity);
                    target.PushPlayer(_hitForce);
                }
            }
        }
    }

    private IEnumerator UseGranade()
    {
        Animator.SetBool("Grenade", true);
        Rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(1);
        for (int i = 0; i < 6; i++)
        {
            Instantiate(GrenadePrefab, BodyTransform.position, Quaternion.Euler(0f, 0f, 120f - i * 10f));
            yield return new WaitForSeconds(0.05f);
        }
        Animator.SetBool("Grenade", false);
    }

    private void Cooldowns()
    {
        _useGrenadeTimer -= Time.fixedDeltaTime;
        _attackCooldownTimer -= Time.fixedDeltaTime;
        _turnCooldownTimer -= Time.fixedDeltaTime;
        _hitCooldownTimer -= Time.fixedDeltaTime;
        _initializeAttackTimer -= Time.fixedDeltaTime;
    }

    private void MoveCheck()
    {
        if (_initializeAttackTimer < 0) // Impede de mover ao comecar o ataque
        {
            Rb.AddForce((_isFacingRight ? Vector2.right : Vector2.left) * ((_speed * (_isAttacking ? _attackSpeedMultiplier : 1)) * Time.fixedDeltaTime), ForceMode2D.Impulse);
        }
        else
        {
            Rb.linearVelocity = Vector2.zero;
        }
    }

    private void StartAttacking()
    {
        _isAttacking = true;
        _initializeAttackTimer = _initializeAttackTimeMax;
        _attackDurationTimer = 3;
        Animator.SetBool("Attack", true);
        
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
            Instantiate(_onDeathEffect2, BodyTransform.position, BodyTransform.rotation);
            GameManager.Instance.BossKilled();
            GameManager.Instance.AddScore(100);
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