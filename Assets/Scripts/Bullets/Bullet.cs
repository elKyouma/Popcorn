using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite explosionSprite;
    public BulletSource bulletSource;
    private bool exploded;

    public void FireBullet()
    {
        StartCoroutine(MoveSpirit());
    }

    public IEnumerator MoveSpirit()
    {
        GetComponent<SpriteRenderer>().sprite = defaultSprite;
        float distance = 0;
        while (distance < bulletSource.maxRange && !exploded)
        {
            distance += Time.deltaTime * bulletSource.bulletSpeed;
            transform.Translate(Vector2.right * Time.deltaTime * bulletSource.bulletSpeed);
            yield return null;
        }
    }
    private IEnumerator HandleExplosion()
    {
        GetComponent<SpriteRenderer>().sprite = explosionSprite;
        exploded = true;
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