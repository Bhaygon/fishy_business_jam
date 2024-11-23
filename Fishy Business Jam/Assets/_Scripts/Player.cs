using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    public State Air, Ground, IdleGround, Interact, Run, Swim;

    [Header("References")] [SerializeField]
    private Rigidbody2D _rb;
    [SerializeField] private Transform PlayerTransform;
    [SerializeField] private Collider2D _feetColl, _bodyColl; // On foot


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

    [Header("Variables")] private Vector2 _moveVelocity;
    private bool _isFacingRight;

    //Collision check
    private RaycastHit2D _groundHit, _headHit;
    public bool IsGrounded;
    public bool BumpedHead;

    private void Awake()
    {
        _isFacingRight = true;
    }

    public void MoveOnGround(float acceleration, float deceleration, Vector2 moveInput)
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

        _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
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
            IsGrounded = true;
        }
        else
        {
            IsGrounded = false;
        }
    }

    public void CollisionChecks()
    {
        CheckIsGrounded();
    }

    private void OnDrawGizmos()
    {
        if (DebugShowIsGroundedBox)
        {
            Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
            Vector2 boxCastSize = new Vector2(_bodyColl.bounds.size.x, GroundDetectionRayLength);
            
            Color rayColor;
            if (IsGrounded){rayColor = Color.green;}
            else {rayColor = Color.red;}
            
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - GroundDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);
        }
    }
}