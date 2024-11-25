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
    private Vector2 _initialPosition;
    public float distance;

    private void Start()
    {
        _initialPosition = transform.localPosition;
    }

    void Update()
    {
        if (Vector2.Distance(_initialPosition, transform.localPosition) > 10)
        {
            Destroy(gameObject);
            return;
        }
        
        transform.Translate(Vector3.right * (speed * Time.deltaTime));
        
        _lifeTime -= Time.deltaTime;
        if (_lifeTime <= 0) Impact();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag(_ignoreTag))
        {
            //print("collision with " + other.gameObject.name);
            IDamageable target = other.gameObject.GetComponentInParent<IDamageable>();
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
