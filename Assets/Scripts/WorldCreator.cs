using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
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
    public List<Vector2> mapOutlinePoints = new List<Vector2>();
    public List<List<Vector2>> planetsOutlines = new List<List<Vector2>>();

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
        PreparePlanetsOutline();
    }
    private void OnDrawGizmos()
    {
        if (!planetsOutlines.Any())
        {
            return;
        }
        foreach (List<Vector2> planetOutline in planetsOutlines)
        {
            if (!planetOutline.Any())
            {
                continue;
            }
            for (int i = 0; i < planetOutline.Count() - 1; i++)
            {
                Debug.DrawLine(planetOutline[i], planetOutline[i + 1]);
            }
            Debug.DrawLine(planetOutline.Last(), planetOutline.First());
        }

        if (!mapOutlinePoints.Any())
        {
            return;
        }
        for (int i = 0; i < mapOutlinePoints.Count() - 1; i++)
        {
            Debug.DrawLine(mapOutlinePoints[i], mapOutlinePoints[i + 1]);
        }
        Debug.DrawLine(mapOutlinePoints.Last(), mapOutlinePoints.First());
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
                    try
                    {
                        Vector3 position = CalculatePosition(x, y, planet.GetObject().GetComponentInChildren<CircleCollider2D>().radius, random);
                        GameObject obj = CreateObjectInSpace(planet.GetObject(), position);
                        newChunk.AddObject(obj);
                    }
                    catch (Exception warning)
                    {
                        Debug.Log(warning.Message);
                    }
                    break;
                }
            }
        };
        newChunk.AddBackground(CreateObjectInSpace(background, new Vector3((x + 0.5f) * chunkSize, (y + 0.5f) * chunkSize, 0f), new Quaternion(-0.707106829f, 0, 0, 0.707106829f))); //Quanternion to change for euler
        activeChunks.Add(new Vector2Int(x, y), newChunk);
    }
    private Vector3 CalculatePosition(int x, int y, float radius, System.Random random)
    {
        ushort counter = 0;
        while (true)
        {
            if (counter > 50)
            {
                throw new Exception("To many tries");
            }
            Vector2 potentialPosition = Vector2.zero;
            potentialPosition.x = random.Next(x * chunkSize, (x + 1) * chunkSize);
            potentialPosition.y = random.Next(y * chunkSize, (y + 1) * chunkSize);

            if (!Physics2D.OverlapCircle(potentialPosition, radius * 2))
            {
                return new Vector3(potentialPosition.x, potentialPosition.y, 0);
            }
            counter++;
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
    public void PrepareMapOutline()
    {
        mapOutlinePoints.Clear();
        int offset = 5;
        int xMin = (playerChunkKey.x - 2) * chunkSize - offset;
        int xMax = (playerChunkKey.x + 3) * chunkSize + offset;
        int yMin = (playerChunkKey.y - 2) * chunkSize - offset;
        int yMax = (playerChunkKey.y + 3) * chunkSize + offset;
        int distanceBetweenPoints = 10;

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
            if (!mapOutlinePoints.Contains(potentialPoint))
            {
                mapOutlinePoints.Add(potentialPoint);
            }
        }
    }
    public void PreparePlanetsOutline()
    {
        planetsOutlines.Clear();
        foreach (KeyValuePair<Vector2Int, Chunk> element in activeChunks)
        {
            foreach (GameObject planet in element.Value.objects)
            {
                CreatePlanetOutline(planet);
            }
        }
    }
    private void CreatePlanetOutline(GameObject obj)
    {
        int offset = 5;
        float radius = obj.GetComponent<CircleCollider2D>().radius * obj.transform.lossyScale.x + offset;
        Vector3 centerPosition = obj.transform.position;

        List<Vector2> planetPoints = new List<Vector2>();
        for (int angle = 0; angle < 360; angle += 24)
        {
            float radians = angle * Mathf.Deg2Rad;

            float x = Mathf.Cos(radians) * radius + centerPosition.x;
            float y = Mathf.Sin(radians) * radius + centerPosition.y;

            planetPoints.Add(new Vector3(x, y));
        }
        planetsOutlines.Add(planetPoints);

        List<GameObject> children = GetChildrenWithRigidbody(obj);
        if (children.Any())
        {
            foreach (GameObject child in children)
                CreatePlanetOutline(child);
        }
    }
    public List<GameObject> GetChildrenWithRigidbody(GameObject parent)
    {
        List<GameObject> childrenWithRigidbody = new List<GameObject>();

        foreach (Transform child in parent.transform)
        {
            if (child.gameObject.GetComponent<Rigidbody2D>() != null)
            {
                childrenWithRigidbody.Add(child.gameObject);
            }
        }
        return childrenWithRigidbody;
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
        public List<GameObject> objects = new List<GameObject>();
        private GameObject background;

        public void AddObject(GameObject spaceObject)
        {
            objects.Add(spaceObject);
        }
        public void AddBackground(GameObject newBackground)
        {
            background = newBackground;
        }
        public void DestroyObjects()
        {
            foreach (GameObject obj in objects)
            {
                Destroy(obj);
            }
            Destroy(background);
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