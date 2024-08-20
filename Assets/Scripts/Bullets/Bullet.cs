using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite explosionSprite;
    [SerializeField] private float distance;
    [SerializeField] private AudioClip bulletSound;
    [SerializeField] private float damage;
    [SerializeField] private float speed;
    [SerializeField] private LayerMask damageableLayers;
    private Rigidbody2D rb;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    public void FireBullet(Vector2 dir)
    {
        SoundManager.Instance.PlaySound(bulletSound, transform.position);
        MoveSpirit(dir);
    }

    private void FixedUpdate()
    {
        Vector2 dir = rb.velocity.normalized;
        
        float alpha = Mathf.Atan2(dir.x, dir.y);
        transform.rotation = Quaternion.Euler(0f, 0f, alpha - 90);
    }

    public void MoveSpirit(Vector2 dir)
    {
        GetComponent<SpriteRenderer>().sprite = defaultSprite;
        rb.velocity = dir * speed;
    }
    private IEnumerator HandleExplosion()
    {
        GetComponent<SpriteRenderer>().sprite = explosionSprite;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.1f);
        gameObject.SetActive(false);
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.gameObject.tag == "BulletSource")
            return;

<<<<<<< HEAD
<<<<<<< HEAD
        other.collider.gameObject.GetComponent<IDamagable>()?.TakeDamage(damage);
=======
>>>>>>> e9bd029 (Add damageableLayers to bullet prefab and bullet 2 prefab)
        StartCoroutine(HandleExplosion());
        
        
=======
        StartCoroutine(HandleExplosion());
        
>>>>>>> 87fce7f (Add damageableLayers to bullet prefab and bullet 2 prefab)
        if (damageableLayers == (damageableLayers | (1 << other.collider.gameObject.layer)))
            other.collider.gameObject.GetComponent<IDamagable>()?.TakeDamage(damage);

    }
}