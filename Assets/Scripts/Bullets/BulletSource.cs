using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Basically when we detect any collider in range it shots the bullet to that collider :)
    Uses object pooling. In order to work put the BulletPool component on the same object
*/

public class BulletSource : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    public float bulletSpeed;
    public float maxRange;
    [SerializeField] private float minShootDelay;
    [SerializeField] private int poolSize;
    [SerializeField] private LayerMask layer;
    [Tooltip("How many raycasts are sent in front of the enemy")]
    [SerializeField] private int visionAccuracy;
    [Tooltip("How many degrees are between each raycast")]
    [SerializeField] private float visionAngle;
    private Ray2D ray;
    private RaycastHit2D hit;
    private Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();

    private void Start()
    {
        CreatePool(bulletPrefab, poolSize);
        StartCoroutine(Shoot());
    }
    public void CreatePool(GameObject prefab, int poolSize)
    {
        int poolKey = prefab.GetInstanceID();

        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<GameObject>());

            for (int i = 0; i < poolSize; i++)
            {
                GameObject newObject = Instantiate(prefab);
                newObject.GetComponent<Bullet>().bulletSource = this;
                newObject.SetActive(false);
                poolDictionary[poolKey].Enqueue(newObject);
            }
        }
    }

    public Bullet ReuseObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int poolKey = prefab.GetInstanceID();

        if (poolDictionary.ContainsKey(poolKey) && poolDictionary[poolKey].Count <= 0)
            return null;

        GameObject objectToReuse = poolDictionary[poolKey].Dequeue();
        poolDictionary[poolKey].Enqueue(objectToReuse);

        objectToReuse.SetActive(true);
        objectToReuse.transform.SetParent(gameObject.transform);
        objectToReuse.transform.position = position;
        objectToReuse.transform.rotation = rotation;

        return objectToReuse.GetComponent<Bullet>();
    }
    private void FixedUpdate()
    {
        // ray = new Ray2D(transform.position, transform.right);
        for (int i = 0; i < visionAccuracy; i++)
        {
            ray = new Ray2D(transform.position, Quaternion.Euler(0, 0, -visionAngle / 2 + i * visionAngle / (visionAccuracy - 1)) * transform.right);
            Debug.DrawRay(ray.origin, ray.direction * maxRange, Color.red, 0.5f);
            hit = Physics2D.CircleCast(ray.origin, 0.25f, ray.direction, maxRange, layer);
        }
    }
    IEnumerator Shoot()
    {
        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
        while (true)
        {
            Bullet bullet = ReuseObject(bulletPrefab, transform.position, transform.rotation);
            bullet.FireBullet();

            yield return new WaitForSeconds(minShootDelay);
        }
    }
}
