using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
  private Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();
  public Sprite defaultSprite;
  public Sprite explosionSprite;

  public void CreatePool(GameObject prefab, int poolSize)
  {
    int poolKey = prefab.GetInstanceID();

    if (!poolDictionary.ContainsKey(poolKey))
    {
      poolDictionary.Add(poolKey, new Queue<GameObject>());

      for (int i = 0; i < poolSize; i++)
      {
        GameObject newObject = Instantiate(prefab);
        newObject.SetActive(false);
        poolDictionary[poolKey].Enqueue(newObject);
      }
    }
  }

  public GameObject ReuseObject(GameObject prefab, Vector3 position, Quaternion rotation)
  {
    int poolKey = prefab.GetInstanceID();

    if (poolDictionary.ContainsKey(poolKey) && poolDictionary[poolKey].Count <= 0)
      return null;

    GameObject objectToReuse = poolDictionary[poolKey].Dequeue();
    poolDictionary[poolKey].Enqueue(objectToReuse);

    objectToReuse.SetActive(true);
    objectToReuse.transform.position = position;
    objectToReuse.transform.rotation = rotation;
    objectToReuse.GetComponent<SpriteRenderer>().sprite = defaultSprite;

    return objectToReuse;
  }
}
