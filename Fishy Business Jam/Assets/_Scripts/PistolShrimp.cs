using System;
using UnityEngine;

public class PistolShrimp : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private Rigidbody2D Rb;

    [SerializeField] private Collider2D Collider;
    [SerializeField] private Animator Animator;
    [SerializeField] private Transform BodyTransform;

    [Header("Variables")] [SerializeField] private LayerMask ObstacleLayer;
    [SerializeField] private float _speed;
    [SerializeField] private float _obstacleDetectRange;
    private bool _isFacingRight;

    private void Start()
    {
        Turn(true);
    }

    private void FixedUpdate()
    {
        transform.Translate((_isFacingRight ? Vector2.right : Vector2.left) * (_speed * Time.deltaTime));

        RaycastHit2D hit = Physics2D.Raycast(BodyTransform.position, _isFacingRight ? Vector2.right : Vector2.left, _obstacleDetectRange, ObstacleLayer);
        if (hit.collider != null)
        {
            Turn(!_isFacingRight);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            _isFacingRight = true;
            BodyTransform.Rotate(0f, 180f, 0f);
        }
        else
        {
            _isFacingRight = false;
            BodyTransform.Rotate(0f, -180f, 0f);
        }
    }

    private void OnDrawGizmos()
    {
        RaycastHit2D hit = Physics2D.Raycast(BodyTransform.position, _isFacingRight ? Vector2.right : Vector2.left, _obstacleDetectRange, ObstacleLayer);
        if (hit.collider != null)
        {
            Gizmos.DrawLine(BodyTransform.position, hit.point);
        }
    }
}