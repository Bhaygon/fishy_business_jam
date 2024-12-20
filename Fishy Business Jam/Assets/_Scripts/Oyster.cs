using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Oyster : MonoBehaviour
{
    public LayerMask PlayerLayer;
    private Animator _animator;
    private bool _collected = false;
    public AudioClip pearlSFX;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_collected) return;
        RaycastHit2D boxHit = Physics2D.BoxCast(transform.position, new Vector2(2,2), 0f, Vector2.down, 0,
            PlayerLayer);
        if (boxHit.collider)
        {
            _collected = true;
            GameManager.Instance.AddPearl();
            _animator.SetBool("Collected", true);
            GameManager.Instance.PlaySFX(pearlSFX);
            Player target = boxHit.collider.GetComponentInParent<Player>();
            if (target)
            {
                target.ReceiveDamage(-3);
            }
        }
    }
}
