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
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void FireBullet(Vector2 dir)
    {
        SoundManager.Instance.PlaySound(bulletSound, transform.position);
        MoveSpirit(dir);
    }

    private void FixedUpdate()
    {
        Vector2 v = rb.velocity;
        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void MoveSpirit(Vector2 dir)
    {
        spriteRenderer.sprite = defaultSprite;
        rb.velocity = dir * speed;
    }
    private IEnumerator HandleExplosion()
    {
        spriteRenderer.sprite = explosionSprite;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.1f);
        gameObject.SetActive(false);
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.gameObject.tag == "BulletSource")
            return;

        other.collider.gameObject.GetComponent<IDamagable>()?.TakeDamage(damage);
        StartCoroutine(HandleExplosion());
    }
}