using System;
using UnityEngine;

public class Jellyfish : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private float cooldown;
    [SerializeField] private int damage;
    [SerializeField] private LayerMask playerLayer;
    private float _timer;

    private void Start()
    {
        _timer = 0;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            RaycastHit2D boxHit = Physics2D.BoxCast(transform.position + new Vector3(0f, -0.32f),  new Vector3(1, 1.3f), 0f, Vector2.down, 0,
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