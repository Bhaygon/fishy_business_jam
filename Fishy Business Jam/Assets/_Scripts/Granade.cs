using UnityEngine;

public class Granade : MonoBehaviour
{
    [SerializeField] private float _lifeTime = 3;
    [SerializeField] private int _damage = 2;
    [SerializeField] private int _area = 2;
    [SerializeField] private float speed = 10;
    [SerializeField] private GameObject particles;
    [SerializeField] private LayerMask DamageLayer;
    public AudioClip GrenadeExplosion;

    private void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(transform.right * speed, ForceMode2D.Impulse);
    }

    void Update()
    {
        _lifeTime -= Time.deltaTime;
        if (_lifeTime <= 0) Explode();
    }

    private void Explode()
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, _area, Vector2.right, DamageLayer);
        if (hit.collider)
        {
            Player target = hit.collider.GetComponentInParent<Player>();
            if (target)
            {
                target.ReceiveDamage(_damage);
            }
        }
        GameManager.Instance.PlaySFX(GrenadeExplosion);
        Instantiate(particles, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}