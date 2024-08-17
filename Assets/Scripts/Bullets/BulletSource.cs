using System.Collections;
using UnityEngine;

/*
    Basically when we detect any collider in range it shots the bullet to that collider :)
    Uses object pooling. In order to work put the BulletPool component on the same object
*/

public class BulletSource : MonoBehaviour
{
    [SerializeField] private GameObject spiritPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float minShootDelay;
    [SerializeField] private float maxRange;
    private Ray2D ray;
    private BulletPool bulletPool;

    private void Start()
    {
        bulletPool = GetComponent<BulletPool>();
        bulletPool.CreatePool(spiritPrefab, 20);
        StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        while (true)
        {
            yield return new WaitForSeconds(minShootDelay);
            FireBullet();
        }
    }

    private void FireBullet()
    {
        ray = new Ray2D(transform.position, transform.right);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, maxRange);

        GameObject spirit = bulletPool.ReuseObject(spiritPrefab, ray.origin, Quaternion.identity);
        // Shooting only when some target can be hit
        if (hit.collider != null)
        {
            Debug.Log(hit.collider.name);
            StartCoroutine(MoveSpirit(spirit, ray.origin, hit.point));
        }
        // else
        // {
        //     StartCoroutine(MoveSpirit(spirit, ray.origin, ray.origin + ray.direction * maxRange));
        // }
    }

    IEnumerator MoveSpirit(GameObject spirit, Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        float time = distance / bulletSpeed;
        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            spirit.transform.position = Vector2.Lerp(start, end, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spirit.transform.position = end;
        spirit.GetComponent<SpriteRenderer>().sprite = bulletPool.explosionSprite;
        yield return new WaitForSeconds(0.1f);
        spirit.SetActive(false);
    }
}
