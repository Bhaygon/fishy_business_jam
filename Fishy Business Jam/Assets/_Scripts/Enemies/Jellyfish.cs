using System;
using System.Collections.Generic;
using UnityEngine;

public class Jellyfish : MonoBehaviour, IDamageable
{
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float cooldown;
    [SerializeField] private int damage;
    [SerializeField] private int speed;
    [SerializeField] private LayerMask playerLayer;
    private float _timer;
    private int _point = 0;
    private int _direction = 1;
    [SerializeField] private List<Transform> points;
    [Header("Health")] [SerializeField] private float _health = 3;
    [SerializeField] private GameObject _onDeathEffect;

    private void Start()
    {
        _timer = 0;
    }

    public void ReceiveDamage(int damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            Instantiate(_onDeathEffect, transform.position, transform.rotation);
            this.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (points.Count > 1)
        {
            if (Vector2.Distance(rb.position, points[_point].position) < 0.1f)
            {
                _point += _direction;

                if (_point >= points.Count || _point < 0)
                {
                    _direction *= -1;
                    _point += _direction * 2;
                }
            }

            rb.position = Vector2.MoveTowards(rb.position, points[_point].position, Time.deltaTime * speed);
        }

        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            RaycastHit2D boxHit = Physics2D.BoxCast(transform.position + new Vector3(0f, -0.32f), new Vector3(1, 1.3f), 0f, Vector2.down, 0,
                playerLayer);
            if (boxHit.collider)
            {
                boxHit.collider.GetComponentInParent<IDamageable>().ReceiveDamage(damage);
                animator.Play("Shock");
                particles.Play();
                _timer = cooldown;
            }
        }
    }
}