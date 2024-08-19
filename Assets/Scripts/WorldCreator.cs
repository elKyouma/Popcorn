using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldCreator : MonoBehaviour
{

    [SerializeField] private Transform playerTransform;
    [SerializeField] private List<Planet> spaceObjects;
    [SerializeField] private int minNumberOfPlanets = 1;
    [SerializeField] private int maxNumberOfPlanets = 4;
    private int limit;

    private int chunkSize = 100;
    private Vector2Int playerChunkKey;
    private Dictionary<Vector2Int, Chunk> activeChunks = new Dictionary<Vector2Int, Chunk>();

    private int numberOfDigits = (int)1e3;
    private int baseSeed = 111; //3 digits

    void Start()
    {
        limit = 0;
        spaceObjects.Sort();
        limit = spaceObjects.Sum(planet => planet.GetChance());
        CheckPlayerChunk();
        //baseSeed = new System.Random().Next(numberOfDigits);
    }
    private void Update()
    {
        CheckPlayerChunk();
    }
    private void CheckPlayerChunk()
    {
        float x = playerTransform.position.x / chunkSize;
        float y = playerTransform.position.y / chunkSize;
        x = (int)Math.Ceiling(x);
        y = (int)Math.Ceiling(y);
        Vector2Int currentPlayerChunk = new Vector2Int((int)x, (int)y);
        if (currentPlayerChunk != playerChunkKey)
        {
            LoadChunks(currentPlayerChunk);
            playerChunkKey = currentPlayerChunk;
        }
    }
    private void LoadChunks(Vector2Int playerChunk)
    {
        HashSet<Vector2Int> keysToKeep = new HashSet<Vector2Int>();
        for(int i = playerChunk.x - 1; i <= playerChunk.x + 1 ; i++)
        {
            for (int j = playerChunk.y - 1; j <= playerChunk.y + 1 ; j++)
            {
                Vector2Int chunkKey = new Vector2Int(i, j);
                keysToKeep.Add(chunkKey);
                if (activeChunks.ContainsKey(chunkKey)){
                    continue;
                }
                GenerateChunk(i, j);
            }
        }
        StartCoroutine(ClearChunks(keysToKeep));
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
                    Vector3 position = CalculatePosition(random, x, y);
                    GameObject obj = CreateObjectInSpace(planet.GetObject(), position);
                    newChunk.AddObject(obj);
                    break;
                }
            }
        }

        activeChunks.Add(new Vector2Int(x, y), newChunk);
    }
    private Vector3 CalculatePosition(System.Random random, int x, int y)
    {
        return new Vector3(random.Next(((x - 1) * chunkSize), x * chunkSize), random.Next(((y - 1) * chunkSize), y * chunkSize), 0);
    }

    private int GenerateChunkSeed(int x, int y)
    {
        int seed = 0;
        seed += baseSeed;
        seed *= numberOfDigits;
        seed += AdjustCoordinate(x);
        seed *= numberOfDigits;
        seed += AdjustCoordinate(y);
        return seed;
    }
    private int AdjustCoordinate(int coordinate)
    {
        return (numberOfDigits / 2) + coordinate % (numberOfDigits / 2);
    }
    private GameObject CreateObjectInSpace(GameObject spaceObject, Vector3 position)
    {
        return Instantiate(spaceObject, position, new Quaternion(0, 0, 0, 0), transform);
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