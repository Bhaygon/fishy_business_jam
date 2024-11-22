using System;
using UnityEngine;

public class SimplePlayerController : MonoBehaviour
{
    
    private Rigidbody2D _rb;
    
    [SerializeField] private float _speed;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        var movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        var inputNormalized = movementInput.normalized;
        
        _rb.linearVelocity = inputNormalized * _speed;
    }
}
