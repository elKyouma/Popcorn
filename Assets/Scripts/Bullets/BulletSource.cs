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
    [SerializeField] private float aimDelay;
    [SerializeField] private int poolSize;
    [SerializeField] private LayerMask layer;
    [Tooltip("How many raycasts are sent in front of the enemy")]
    [SerializeField] private int visionAccuracy;
    [Tooltip("How many degrees are between each raycast")]
    [SerializeField] private float visionAngle;
    private Ray2D ray;
    public RaycastHit2D hit;
    private Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();

    private bool isGunActive;
    private bool targetInSight;
    private bool isReloading = false;

    public bool IsGunActive() => isGunActive;
    public void SetIsGunActive(bool active) => isGunActive = active;

    private void Start()
    {
        CreatePool(bulletPrefab, poolSize);
        SetIsGunActive(true);
    }

    private void Update()
    {
        if(isGunActive && targetInSight && !isReloading)
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
                newObject.transform.SetParent(transform);
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
        objectToReuse.transform.position = position;
        objectToReuse.transform.rotation = rotation;

        return objectToReuse.GetComponent<Bullet>();
    }
    private void FixedUpdate()
    {
        if (!isGunActive) return;
        // ray = new Ray2D(transform.position, transform.right);
        for (int i = 0; i < visionAccuracy; i++)
        {
            ray = new Ray2D(transform.position, Quaternion.Euler(0, 0, -visionAngle / 2 + i * visionAngle / (visionAccuracy - 1)) * transform.right);
            Debug.DrawRay(ray.origin, ray.direction * maxRange, Color.red);
            hit = Physics2D.CircleCast(ray.origin, 0.25f, ray.direction, maxRange, layer);
            targetInSight = hit.collider != null;
            if (targetInSight) break;
        }
    }
    IEnumerator Shoot()
    {
        isReloading = true;
        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
        //Debug.Log("Shooting to " + hit.collider.gameObject.name);
        Bullet bullet = ReuseObject(bulletPrefab, transform.position, transform.rotation);
        bullet.FireBullet(transform.right);
        yield return new WaitForSeconds(minShootDelay);
        isReloading = false;
    }
}
