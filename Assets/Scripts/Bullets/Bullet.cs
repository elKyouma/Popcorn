using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite explosionSprite;
    public BulletSource bulletSource;
    [SerializeField] private float distance;

    public void FireBullet()
    {
        MoveSpirit();
    }

    public void MoveSpirit()
    {
        GetComponent<SpriteRenderer>().sprite = defaultSprite;
        GetComponent<Rigidbody2D>().velocity = transform.right * bulletSource.bulletSpeed;
    }
    private IEnumerator HandleExplosion()
    {
        GetComponent<SpriteRenderer>().sprite = explosionSprite;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        yield return new WaitForSeconds(0.1f);
        gameObject.SetActive(false);
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject == bulletSource.gameObject)
            return;
        StartCoroutine(HandleExplosion());
    }
}