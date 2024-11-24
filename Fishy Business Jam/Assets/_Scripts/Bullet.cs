using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _lifeTime = 3;
    [SerializeField] private int _damage = 1;
    [SerializeField] private float speed = 10;
    [SerializeField] private GameObject particles;
    [SerializeField] private string _ignoreTag;
    [SerializeField] private Transform _particlesSpawnPoint;

    void Update()
    {
        transform.Translate(Vector3.right * (speed * Time.deltaTime));
        
        _lifeTime -= Time.deltaTime;
        if (_lifeTime <= 0) Impact();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag(_ignoreTag))
        {
            print("collision with " + other.gameObject.name);
            IDamageable target = other.gameObject.GetComponent<IDamageable>();
            if (target != null)
            {
                target.ReceiveDamage(_damage);
            }
            Impact();
        }
    }

    private void Impact()
    {
        Instantiate(particles, _particlesSpawnPoint.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
