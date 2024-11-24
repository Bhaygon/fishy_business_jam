using System;
using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float lifeTime = 3;
    [SerializeField] private float speed = 10;
    [SerializeField] private GameObject particles;

    void Update()
    {
        transform.Translate(Vector3.right * (speed * Time.deltaTime));
        
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0) Impact();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) Impact();
    }

    private void Impact()
    {
        Instantiate(particles, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
