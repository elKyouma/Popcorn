using iShape.Geometry;
using iShape.Geometry.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using iShape.Triangulation.Shape.Delaunay;
using iShape.Mesh2d;
using Unity.Mathematics;
using Unity.VisualScripting;
using System.Xml.Linq;
using iShape.Geometry.Extension;
using System.Drawing;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject originalBackground;
    [SerializeField] private List<ProbableObject> spaceObjects;
    [SerializeField] private List<ProbableObject> enemyObjects;
    [SerializeField] private int minNumberOfPlanets = 1;
    [SerializeField] private int maxNumberOfPlanets = 4;
    [SerializeField] private Transform PlanetParent;
    [SerializeField] private Transform EnemyParent;

    private static bool debug = true;
    private int limitPlanet = 0;
    private int limitEnemy = 0;

    private int numberOfExtraChunks = 2;
    private int chunkSize = 200;
    [SerializeField]  private Vector2Int playerChunkKey = new Vector2Int(999, 999);
    private Dictionary<Vector2Int, Chunk> activeChunks = new Dictionary<Vector2Int, Chunk>();
    private static List<Vector2> mapOutlinePoints = new List<Vector2>();
    private static List<List<Vector2>> planetsOutlines = new List<List<Vector2>>();

    private int numberOfDigits = (int)1e3;
    private int baseSeed = 111; //3 digits

    private MeshFilter meshFilter;
    private static Mesh mesh;
    private static PointShape gameMap = new PointShape();

    private int xMin = 0;
    private int xMax = 0;
    private int yMin = 0;
    private int yMax = 0;
    private int enemyRandomness = 2;
    private int maxEnemyWaves = 3 + 1;
    private int maxEnemyiesInWave = 5;
    private int enemySpread = 30;

    void Awake()
    {
        if (!debug)
        {
            GetComponent<MeshRenderer>().enabled = false;
        }
        spaceObjects.Sort();
        limitPlanet = spaceObjects.Sum(planet => planet.GetChance());

        baseSeed = new System.Random().Next(numberOfDigits);
        CheckPlayerChunk();

        meshFilter = gameObject.GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.MarkDynamic();
        meshFilter.mesh = mesh;

        GenerateEnemies();
    }
    private void Update()
    {
        CheckPlayerChunk();
        PreparePlanetsOutline();
    }
    private void OnDrawGizmos()
    {
        if (!debug) return;
        for (int i = 0; i < mapOutlinePoints.Count() - 1; i++)
        {
            Debug.DrawLine(mapOutlinePoints[i], mapOutlinePoints[i + 1]);
        }
        if (mapOutlinePoints.Any())
        {
            Debug.DrawLine(mapOutlinePoints.Last(), mapOutlinePoints.First());
        }

        foreach (List<Vector2> planetOutline in planetsOutlines)
        {
            if (!mapOutlinePoints.Any())
            {
                continue;
            }
            for (int i = 0; i < planetOutline.Count() - 1; i++)
            {
                Debug.DrawLine(planetOutline[i], planetOutline[i + 1]);
            }
            Debug.DrawLine(planetOutline.Last(), planetOutline.First());
        }
    }
    static public PlainShape GetPlainMesh()
    {
        gameMap.hull = mapOutlinePoints.ToArray();
        gameMap.holes = new Vector2[planetsOutlines.Count][];
        int i = 0;
        foreach (List<Vector2> list in planetsOutlines)
        {
            gameMap.holes[i] = list.ToArray();
            i++;
        }
        var iGeom = IntGeom.DefGeom;

        var pShape = gameMap.ToPlainShape(iGeom, Allocator.Temp);
        //var triangles = pShape.DelaunayTriangulate(Allocator.Temp);
        //var points = iGeom.Float(pShape.points, Allocator.Temp);
        //var vertices = new NativeArray<float3>(points.Length, Allocator.Temp);
        //for (int j = 0; j < points.Length; ++j)
        //{
        //    var p = points[j];
        //    vertices[j] = new float3(p.x, p.y, 0);
        //}
        //points.Dispose();

        //var bodyMesh = new StaticPrimitiveMesh(vertices, triangles);
        //bodyMesh.Fill(mesh);
        //vertices.Dispose();
        //triangles.Dispose();

        //if (debug)
        //{
        //    var colorMesh = new NativeColorMesh(vertices.Length, Allocator.Temp);

        //    colorMesh.AddAndDispose(bodyMesh, Color.green);
        //    colorMesh.FillAndDispose(mesh);
        //}

        //pShape.Dispose();
        return pShape;
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
            playerChunkKey = currentPlayerChunk;
            AdjustMinMax();
            LoadChunks();
        }
    }

    private void GenerateEnemies()
    {
        System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
        limitEnemy = enemyObjects.Sum(planet => planet.GetChance());
        for (int i = 0; i < maxEnemyWaves; i++)
        {
            int index = random.Next() % mapOutlinePoints.Count();
            Vector2 point = mapOutlinePoints[index];

            int enemyNumbers = maxEnemyiesInWave + (random.Next(enemyRandomness) - enemyRandomness % 2);
            GenerateFlockOfEnemies(point, enemyNumbers, random);
        }
    }
    private void GenerateFlockOfEnemies(Vector2 point, int enemyNumbers, System.Random random)
    {
        AdjustVector2(point);
        for (int i = 0; i < enemyNumbers; i++)
        {
            int chance = random.Next(limitEnemy);
            int cumulativeChance = 0;
            foreach (ProbableObject enemy in enemyObjects)
            {
                cumulativeChance += enemy.GetChance();
                if (chance < cumulativeChance)
                {
                    try
                    {
                        Vector2 hitbox = enemy.GetObject().GetComponent<BoxCollider2D>().size;
                        float radius = Mathf.Sqrt(Mathf.Pow(hitbox.x, 2) + Mathf.Pow(hitbox.y, 2));
                        Vector3 position = CalculatePositionEnemy(point, radius, random);
                        GameObject obj = CreateObjectInSpace(enemy.GetObject(), position, EnemyParent);
                        obj.GetComponent<Enemy>().SetTarget(playerTransform);
                    }
                    catch (Exception warning)
                    {
                        Debug.Log(warning.Message);
                    }
                    break;
                }
            }
        }
    }
    private Vector3 CalculatePositionEnemy(Vector2 point, float radius, System.Random random)
    {
        ushort counter = 0;
        while (true)
        {
            if (counter > 50)
                throw new Exception("To many tries");

            int angle = random.Next(360);
            int Spreadradius = random.Next(enemySpread);

            float radians = angle * Mathf.Deg2Rad;

            float x = Mathf.Cos(radians) * Spreadradius + point.x;
            float y = Mathf.Sin(radians) * Spreadradius + point.y;
            Vector2 pos = new Vector2(x, y);

            if (!Physics2D.OverlapCircle(pos, radius * 2.5f))
            {
                return new Vector3(pos.x, pos.y, 0);
            }
            counter++;
        }
    }

    private void GenerateChunk(int x, int y)
    {
        int seed = GenerateChunkSeed(x, y);
        System.Random random = new System.Random(seed);
        Chunk newChunk = new Chunk();
        limitEnemy = enemyObjects.Sum(planet => planet.GetChance());

        int curentMaxPlanets = random.Next(minNumberOfPlanets, maxNumberOfPlanets + 1);
        for (int i = 0; i < curentMaxPlanets; i++)
        {
            int chance = random.Next(limitPlanet);
            int cumulativeChance = 0;
            foreach (ProbableObject planet in spaceObjects)
            {
                cumulativeChance += planet.GetChance();
                if (chance < cumulativeChance)
                {
                    try
                    {
                        Vector3 position = CalculatePosition(x, y, planet.GetObject().GetComponentInChildren<CircleCollider2D>().radius, random);
                        GameObject obj = CreateObjectInSpace(planet.GetObject(), position, PlanetParent);
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
        GameObject background = CreateObjectInSpace(originalBackground, new Vector3((x + 0.5f) * chunkSize, (y + 0.5f) * chunkSize, 0f), PlanetParent, new Quaternion(-0.707106829f, 0, 0, 0.707106829f)); //Quanternion to change for euler
        background.transform.localScale = background.transform.lossyScale * (chunkSize / 100f);
        newChunk.AddBackground(background);
        activeChunks.Add(new Vector2Int(x, y), newChunk);
    }
    private Vector3 CalculatePosition(int x, int y, float radius, System.Random random)
    {
        ushort counter = 0;
        while (true)
        {
            if (counter > 100)
            {
                throw new Exception("To many tries");
            }
            int freeSpace = (int)(0.05f * chunkSize);
            Vector2 potentialPosition = Vector2.zero;
            potentialPosition.x = random.Next(x * chunkSize + freeSpace, (x + 1) * chunkSize - freeSpace);
            potentialPosition.y = random.Next(y * chunkSize + freeSpace, (y + 1) * chunkSize - freeSpace);

            if (!Physics2D.OverlapCircle(potentialPosition, radius * 2.5f))
            {
                return new Vector3(potentialPosition.x, potentialPosition.y, 0);
            }
            counter++;
        }
    }

    private GameObject CreateObjectInSpace(GameObject spaceObject, Vector3 position, Transform parent = null, Quaternion rotation = new Quaternion())
    {
        if (!parent)
            return Instantiate(spaceObject, position, rotation, transform);
        return Instantiate(spaceObject, position, rotation, parent);
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
        int distanceBetweenPoints = 20;
        int offset = (int)(chunkSize * 0.1f);
        for (int y = yMin - offset; y <= yMax + offset; y += distanceBetweenPoints)
        {
            CheckPoint(xMin - offset, y);
        }
        for (int x = xMin - offset; x <= xMax + offset; x += distanceBetweenPoints)
        {
            CheckPoint(x, yMax + offset);
        }
        for (int y = yMax + offset; y >= yMin - offset; y -= distanceBetweenPoints)
        {
            CheckPoint(xMax + offset, y);
        }
        for (int x = xMax + offset; x >= xMin - offset; x -= distanceBetweenPoints)
        {
            CheckPoint(x, yMin - offset);
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
    private void CreatePlanetOutline(GameObject obj, int iterator = -1)
    {
        obj.GetComponent<CircleCollider2D>().enabled = false;
        iterator++;
        if (iterator == 2) return;
        float radius = 0;
        float limit = obj.GetComponent<CircleCollider2D>().radius * obj.transform.localScale.x;
        var children = obj.transform.Cast<Transform>().Select(t => t.gameObject).ToList();
        GameObject unwantedChild = null;
        foreach (var child in children)
        {
            if (!child.transform.Cast<Transform>().ToList().Any())
            {
                radius = child.GetComponent<CircleCollider2D>().radius;
                radius *= obj.transform.localScale.x;
                unwantedChild = child;
                continue;
            }
        }
        children.Remove(unwantedChild);
        while (Physics2D.OverlapCircleAll(obj.transform.position, radius * 2f).Where(collider => !collider.isTrigger).Any() && radius > limit)
        {
            radius *= 0.8f;
        }
        if (radius > limit)
        {
            Vector3 centerPosition = obj.transform.position;
            obj.GetComponent<CircleCollider2D>().enabled = true;

            List<Vector2> planetPoints = new List<Vector2>();
            for (int angle = 0; angle < 360; angle += 24)
            {
                float radians = angle * Mathf.Deg2Rad;

                float x = Mathf.Cos(radians) * radius + centerPosition.x;
                float y = Mathf.Sin(radians) * radius + centerPosition.y;
                Vector2 pos = new Vector2(x, y);
                AdjustVector2(pos);
                planetPoints.Add(pos);
            }
            planetsOutlines.Add(planetPoints);
        }

        if (!children.Any())
            return;

        foreach (GameObject child in children)
            CreatePlanetOutline(child, iterator);
    }
    void AdjustMinMax()
    {
        xMin = (playerChunkKey.x - numberOfExtraChunks) * chunkSize;
        xMax = (playerChunkKey.x + numberOfExtraChunks + 1) * chunkSize;
        yMin = (playerChunkKey.y - numberOfExtraChunks) * chunkSize;
        yMax = (playerChunkKey.y + numberOfExtraChunks + 1) * chunkSize;
    }
    void AdjustVector2(Vector2 pos)
    {
        if (pos.x < xMin) pos.x = xMin;
        if (pos.x > xMax) pos.x = xMax;
        if (pos.y < yMin) pos.y = yMin;
        if (pos.y > yMax) pos.y = yMax;
    }
    void LoadChunks()
    {
        HashSet<Vector2Int> keysToKeep = new HashSet<Vector2Int>();
        for (int i = playerChunkKey.x - numberOfExtraChunks; i <= playerChunkKey.x + numberOfExtraChunks; i++)
        {
            for (int j = playerChunkKey.y - numberOfExtraChunks; j <= playerChunkKey.y + numberOfExtraChunks; j++)
            {
                Vector2Int chunkKey = new Vector2Int(i, j);
                keysToKeep.Add(chunkKey);
                if (activeChunks.ContainsKey(chunkKey))
                {
                    continue;
                }
                GenerateChunk(i, j);
            }
        }
        PrepareMapOutline();
        ClearChunks(keysToKeep);
    }
    void ClearChunks(HashSet<Vector2Int> keysToKeep)
    {
        foreach (var key in new List<Vector2Int>(activeChunks.Keys))
        {
            if (!keysToKeep.Contains(key))
            {
                activeChunks[key].DestroyObjects();
                activeChunks.Remove(key);
            }
        }
    }
    private class Chunk
    {
        public List<GameObject> objects = new List<GameObject>();
        private GameObject background;

        public List<GameObject> GetList()
        {
            return objects;
        }
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
    private struct ProbableObject : IComparable
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
            ProbableObject tmp = (ProbableObject)obj;
            if (this.chanceOfSpawn > tmp.chanceOfSpawn)
                return 1;
            if (this.chanceOfSpawn < tmp.chanceOfSpawn)
                return -1;
            return 0;
        }
    }

    public struct PointShape
    {
        public Vector2[] hull;
        public Vector2[][] holes;

        public PlainShape ToPlainShape(IntGeom iGeom, Allocator allocator)
        {
            var iHull = iGeom.Int(hull);

            IntShape iShape;
            if (holes != null && holes.Length > 0)
            {
                var iHoles = iGeom.Int(holes);
                iShape = new IntShape(iHull, iHoles);
            }
            else
            {
                iShape = new IntShape(iHull, Array.Empty<IntVector[]>());
            }

            var pShape = new PlainShape(iShape, allocator);

            return pShape;
        }
    }
}
