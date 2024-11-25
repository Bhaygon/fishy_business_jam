using System;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float TimePerShoot;
    private float _shootTimer;
    [SerializeField] private GameObject BulletPrefab;
    [SerializeField] private Transform ShootPoint;
    public bool IsShooting { get; private set; }

    private Player _player;
    private Animator _animator;

    private void Start()
    {
        _player = GetComponentInParent<Player>();
        _animator = GetComponent<Animator>();
        _shootTimer = 0;
    }

    private void Update()
    {
        IsShooting = Input.GetKey(KeyCode.F);
        _animator.SetBool("Shooting", IsShooting);
        
        _shootTimer -= Time.deltaTime;
        if (IsShooting && _shootTimer <= 0)
        {
            ShootBullet();
            _shootTimer = TimePerShoot;
        }
    }

    public void ShootBullet()
    {
        Instantiate(BulletPrefab, ShootPoint.position, transform.rotation);
        _player.Recoil();
    }
}
