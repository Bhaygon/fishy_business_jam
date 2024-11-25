using System;
using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    public AudioClip clip;
    public LayerMask PlayerLayer;
    public GameObject ObjectToActivate;

    private void Update()
    {
        RaycastHit2D boxHit = Physics2D.BoxCast(transform.position, transform.localScale, 0f, Vector2.down, 0,
            PlayerLayer);
        if (boxHit.collider)
        {
            GameManager.Instance.PlayMusic(clip);
            if (ObjectToActivate) ObjectToActivate.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }
}
