using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class WorldCreator : MonoBehaviour
{

    [SerializeField] private UnityEngine.Object[] spaceObjects;
    [SerializeField] private Dictionary<Vector2Int, Chunk> activeChunks = new Dictionary<Vector2Int, Chunk>();
    [SerializeField] private Vector2Int playerChunkKey;
    [SerializeField] private Transform playerTransform;
    private uint baseSeed = 111111;
    private ushort chunkSize = 100;

    private int numberOfDigits = (int)1e6;
    public string folderName = "SpaceObjects";

    void Start()
    {
        spaceObjects = Resources.LoadAll(folderName);
        CheckPlayerChunk();
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
    private void GenerateChunk(int i, int j)
    {
        string seed = GenerateChunkSeed(i, j);
        Chunk newChunk = new Chunk();
        System.Random random = new System.Random();
        foreach (GameObject spaceObject in spaceObjects)
        {
            newChunk.AddObject(CreateObjectInSpace(random.Next(((i - 1) * chunkSize), i * chunkSize), random.Next(((j - 1) * chunkSize), j * chunkSize), spaceObject));
        }
        activeChunks.Add(new Vector2Int(i, j), newChunk);
    }
    
    private string GenerateChunkSeed(int x, int y)
    {
        ulong seed = 0;
        seed += baseSeed;
        seed *= (ulong)numberOfDigits;
        seed += AdjustCoordinate(x);
        seed *= (ulong)numberOfDigits;
        seed += AdjustCoordinate(y);
        return seed.ToString();
    }
    private uint AdjustCoordinate(int coordinate)
    {
        return (uint)((numberOfDigits / 2) + coordinate % (numberOfDigits / 2));
    }
    private GameObject CreateObjectInSpace(int x, int y, GameObject spaceObject)
    {
        return Instantiate(spaceObject, new Vector3(x, y, 0), new Quaternion(0, 0, 0, 0), transform);
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
}