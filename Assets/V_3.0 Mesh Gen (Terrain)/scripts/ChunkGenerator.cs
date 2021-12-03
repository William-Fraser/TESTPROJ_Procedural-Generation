using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    public int chunkSize;
    public float yScale;
    public float roughness;

    private Chunk startingTarget;
    public Chunk StartingTarget { get { return startingTarget; } }

    public void Awake()
    {
        chunkSize = GameManager.manager.chunkSize;
        yScale = GameManager.manager.yScale;
        roughness = GameManager.manager.roughness;
    }
  
    //seperate into different methods
    public Dictionary<Vector3, Chunk> GenerateNodes()
    {
        Dictionary<Vector3, Chunk> loadingChunks = new Dictionary<Vector3, Chunk>();
        int loadingSize = GameManager.manager.RenderSize + 2; // #MN: +2 add's edge buffer for MeshGenerator to rend to
        int targetChunkMarker = (loadingSize - GameManager.manager.renderDistance);
        int seed = GameManager.manager.seed;
        
        Debug.Log($"loadingSize: {loadingSize}");

        for (int x = seed; x <= (seed + loadingSize); x++)
        {
            for (int z = seed; z <= (seed + loadingSize); z++)
            {
                //instance and position Chunk object pool
                Vector3 positioning = new Vector3(x * chunkSize, 0, z * chunkSize);
                Chunk chunkGeneration = new Chunk((int)positioning.x, (int)positioning.z, chunkSize, roughness, yScale);

                //populate dictionary to call and save chunks
                loadingChunks.Add(chunkGeneration.chunkObject.transform.position, chunkGeneration);

                //setting target chunk
                if (x == targetChunkMarker && z == targetChunkMarker)
                {
                    startingTarget = chunkGeneration;
                }
            }
        }

        Debug.LogWarning("all chunks loaded");
        return loadingChunks;
    }
}

public class Chunk
{
    public GameObject chunkObject = new GameObject("Chunk");

    private GameObject[,] groundNodes; // nodes should be a subclass(the subclass could have a game object, it will hold the position vector needed for mesh generation)
    private int chunkSize;
    private float roughness;
    private float yScale;

    public GameObject[,] GroundNodes { get { return groundNodes; } }

    public Chunk(int chunkX, int chunkZ, int chunkSize, float roughness, float yScale)
    {
        this.chunkSize = chunkSize;
        this.roughness = roughness;
        this.yScale = yScale;
        groundNodes = new GameObject[chunkSize, chunkSize];

        chunkObject.transform.position = new Vector3(chunkX, 0, chunkZ);

        //place nodes
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                groundNodes[x, z] = new GameObject("Node");
                groundNodes[x, z].transform.position = new Vector3(chunkX + x, (int)PerlinNoise((float)chunkX + x, (float)chunkZ + z), chunkZ + z);
                groundNodes[x, z].transform.parent = chunkObject.transform;
            }
        }


    }

    //public methods
    public float PerlinNoise(float x, float z)
    {
        float perlin;

        //getting better percision on perlin scale
        x /= chunkSize;
        z /= chunkSize;

        perlin = Mathf.PerlinNoise(x * roughness, z * roughness) * yScale;

        //changing to larger number more relative to world
        perlin *= chunkSize;

        return perlin;
    }

    public void ReloadChunk(Vector3 newPos, Chunk eastChunk, Chunk southeastChunk, Chunk southChunk)
    {
        chunkObject.SetActive(true);
        chunkObject.transform.position = newPos;

        ReloadNodes(newPos);
    }

    private void ReloadNodes(Vector3 newPos)
    {
        int xPos = (int)newPos.x;
        int zPos = (int)newPos.z;


        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                groundNodes[x, z].transform.position = new Vector3(xPos + x, (int)PerlinNoise((float)xPos + x, (float)zPos + z), zPos + z);
            }
        }
    }
}