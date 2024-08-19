using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldCreator : MonoBehaviour
{

    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject background;
    [SerializeField] private List<Planet> spaceObjects;
    [SerializeField] private int minNumberOfPlanets = 1;
    [SerializeField] private int maxNumberOfPlanets = 4;
    private int limit = 0;

    private int chunkSize = 100;
    private Vector2Int playerChunkKey;
    private Dictionary<Vector2Int, Chunk> activeChunks = new Dictionary<Vector2Int, Chunk>();
    public List<Vector2> edgePoints = new List<Vector2>();

    private int numberOfDigits = (int)1e3;
    private int baseSeed = 111; //3 digits

    void Start()
    {
        spaceObjects.Sort();
        limit = spaceObjects.Sum(planet => planet.GetChance());
        CheckPlayerChunk();
        baseSeed = new System.Random().Next(numberOfDigits);
    }
    private void Update()
    {
        CheckPlayerChunk();
    }
    private void OnDrawGizmos()
    {
        if (!edgePoints.Any())
            return;
        for (int i = 0; i < edgePoints.Count() - 1; i++)
        {
            Debug.DrawLine(edgePoints[i], edgePoints[i + 1]);
        }
        Debug.DrawLine(edgePoints.Last(), edgePoints.First());
    }
    public void PrepareMapOutline()
    {
        edgePoints.Clear();
        int offset = 5;
        int xMin = (playerChunkKey.x - 2) * chunkSize - offset;
        int xMax = (playerChunkKey.x + 3) * chunkSize + offset;
        int yMin = (playerChunkKey.y - 2) * chunkSize - offset;
        int yMax = (playerChunkKey.y + 3) * chunkSize + offset;
        int distanceBetweenPoints = 4;

        for (int x = xMin; x <= xMax; x += distanceBetweenPoints)
        {
            CheckPoint(x, yMin);
        }
        for (int y = yMin; y <= yMax; y += distanceBetweenPoints)
        {
            CheckPoint(xMax, y);
        }
        for (int x = xMax; x >= xMin; x -= distanceBetweenPoints)
        {
            CheckPoint(x, yMax);
        }
        for (int y = yMax; y >= yMin; y -= distanceBetweenPoints)
        {
            CheckPoint(xMin, y);
        }

        void CheckPoint(int x, int y)
        {
            Vector2 potentialPoint = new Vector2(x, y);
            if (!edgePoints.Contains(potentialPoint))
            {
                edgePoints.Add(potentialPoint);
            }
        }
    }
    public void PreparePlanetsOutline()
    {

    }
    private void CheckPlayerChunk()
    {
        float x = playerTransform.position.x / chunkSize;
        float y = playerTransform.position.y / chunkSize;
        x = (int)Math.Floor(x);
        y = (int)Math.Floor(y);
        Vector2Int currentPlayerChunk = new Vector2Int((int)x, (int)y);
        if (currentPlayerChunk != playerChunkKey)
        {
            StartCoroutine(LoadChunks(currentPlayerChunk));
            playerChunkKey = currentPlayerChunk;
        }
    }
    private void GenerateChunk(int x, int y)
    {
        int seed = GenerateChunkSeed(x, y);
        System.Random random = new System.Random(seed);
        Chunk newChunk = new Chunk();

        int curentMaxPlanets = random.Next(minNumberOfPlanets, maxNumberOfPlanets + 1);
        for (int i = 0; i < curentMaxPlanets; i++)
        {
            int chance = random.Next(limit);
            int cumulativeChance = 0;
            foreach (Planet planet in spaceObjects)
            {
                cumulativeChance += planet.GetChance();
                if (chance < cumulativeChance)
                {
                    Vector3 position = CalculatePosition(x, y, planet.GetObject().GetComponentInChildren<CircleCollider2D>().radius, random);
                    GameObject obj = CreateObjectInSpace(planet.GetObject(), position);
                    newChunk.AddObject(obj);
                    break;
                }
            }
        };
        newChunk.AddObject(CreateObjectInSpace(background, new Vector3((x + 0.5f) * chunkSize, (y + 0.5f) * chunkSize, 0f), new Quaternion(-0.707106829f, 0, 0, 0.707106829f))); //Quanternion to change for euler
        activeChunks.Add(new Vector2Int(x, y), newChunk);
    }
    private Vector3 CalculatePosition(int x, int y, float radius, System.Random random)
    {
        while (true)
        {
            Vector2 potentialPosition = Vector2.zero;
            potentialPosition.x = random.Next(x * chunkSize, (x + 1) * chunkSize);
            potentialPosition.y = random.Next(y * chunkSize, (y + 1) * chunkSize);

            if (!Physics2D.OverlapCircle(potentialPosition, radius * 2))
            {
                return new Vector3(potentialPosition.x, potentialPosition.y, 0);
            }
        }
    }

    private GameObject CreateObjectInSpace(GameObject spaceObject, Vector3 position, Quaternion rotation = new Quaternion())
    {
        return Instantiate(spaceObject, position, rotation, transform);
    }

    private int GenerateChunkSeed(int x, int y)
    {
        int seed = 0;
        seed += baseSeed;
        seed *= numberOfDigits;
        seed += (numberOfDigits / 2) + x % (numberOfDigits / 2);
        seed *= numberOfDigits;
        seed += (numberOfDigits / 2) + y % (numberOfDigits / 2);
        return seed;
    }
    IEnumerator LoadChunks(Vector2Int playerChunk)
    {
        HashSet<Vector2Int> keysToKeep = new HashSet<Vector2Int>();
        for (int i = playerChunk.x - 2; i <= playerChunk.x + 2; i++)
        {
            for (int j = playerChunk.y - 2; j <= playerChunk.y + 2; j++)
            {
                Vector2Int chunkKey = new Vector2Int(i, j);
                keysToKeep.Add(chunkKey);
                if (activeChunks.ContainsKey(chunkKey))
                {
                    continue;
                }
                GenerateChunk(i, j);
                yield return null;
            }
        }
        PrepareMapOutline();
        StartCoroutine(ClearChunks(keysToKeep));
    }
    IEnumerator ClearChunks(HashSet<Vector2Int> keysToKeep)
    {
        foreach (var key in new List<Vector2Int>(activeChunks.Keys))
        {
            if (!keysToKeep.Contains(key))
            {
                activeChunks[key].DestroyObjects(); 
                activeChunks.Remove(key);
            }
            yield return null;
        }
    }
    private class Chunk
    {
        private List<GameObject> objects = new List<GameObject>();

        public void AddObject(GameObject spaceObject)
        {
            objects.Add(spaceObject);
        }
        public void DestroyObjects()
        {
            foreach (GameObject obj in objects)
            {
                Destroy(obj);
            }
            objects.Clear();
        }

    }

    [System.Serializable]
    private struct Planet : IComparable
    { 
        [SerializeField] private GameObject spaceObject;
        [SerializeField] private int chanceOfSpawn;
        public GameObject GetObject()
        {
            return spaceObject;
        }
        public int GetChance()
        {
            return chanceOfSpawn;
        }

        public int CompareTo(object obj)
        {
            Planet tmp = (Planet)obj;
            if (this.chanceOfSpawn > tmp.chanceOfSpawn)
                return 1;
            if(this.chanceOfSpawn < tmp.chanceOfSpawn)
                return -1;
            return 0;
        }
    }
}