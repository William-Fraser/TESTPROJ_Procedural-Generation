using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunkManager : MonoBehaviour
{
    public GameObject player; // offload this into game manager, which should also handle terrain generator class

    [Header("Terrain Generation")]
    public int seed = 0;
    [Tooltip("chunks loaded in a radius format from target chunk")]
    public int renderDistance = 4;

    [Header("Chunk Generation")]
    public int chunkSize = 10;
    public float yScale = 1;
    public float roughness = 1;
    public Material groundMat;

    [Header("private debug")] 
    //chunk loading
    private Chunk targetChunk;
    private Chunk chunkGeneration; // only used to create instances of chunks
    private Dictionary<Vector3, Chunk> loadedChunks;
    [SerializeField]
    private List<Vector3> loadedChunkPositions = new List<Vector3>();

    void Start()
    {
        loadedChunks = new Dictionary<Vector3, Chunk>();
        
        //intitalizes chunk subclass
        GenerateWorld();

        Debug.Log($"Inital Target Chunk: {targetChunk.chunkObject.transform.position}");

        // spawning debug player 
        float spawnPoint = targetChunk.chunkObject.transform.position.x + ((float)chunkSize / 2); // #MN: spawns player in the middle of terrain generations
        player = Instantiate(player, new Vector3(spawnPoint, spawnPoint + yScale + 5, spawnPoint), Quaternion.identity); // #MN: ensures spawning above the terrain
    }

    void Update()
    {
        // handles chunk loading
        CheckAndChangeTarget(player);
    }
    
    private void GenerateWorld()
    {
        int renderSize = (renderDistance * 2);
        int targetChunkMarker = (renderSize - renderDistance);
        Debug.Log($"rendersize: {renderSize}");

        for (int x = seed; x <= (seed + renderSize); x++)
        {
            for (int z = seed; z <= (seed + renderSize); z++)
            {
                //instance and position Chunk object pool
                Vector3 positioning = new Vector3(x*chunkSize, 0, z*chunkSize);
                chunkGeneration = new Chunk((int)positioning.x, (int)positioning.z, groundMat, chunkSize, roughness, yScale);

                //populate list and dictionary to call and save chunks
                loadedChunkPositions.Add(chunkGeneration.chunkObject.transform.position);
                loadedChunks.Add(chunkGeneration.chunkObject.transform.position, chunkGeneration);

                //setting target chunk
                if (x == targetChunkMarker && z == targetChunkMarker)
                {
                    targetChunk = chunkGeneration;
                }
            }
        }
    }
    
    private void CheckAndChangeTarget(GameObject targeter) // targeter is the object being checked for the Target Chunk, which is the chunk the targeter is currently in
    {
        bool changeTarget = false;
        Vector3 changeDistance = Vector3.zero;

        //checking to load chunks in x axis
        if (targeter.transform.position.x < targetChunk.chunkObject.transform.position.x)
        {
            changeTarget = true;
            changeDistance = new Vector3(-chunkSize, 0, 0);
        }
        else if (targeter.transform.position.x > (targetChunk.chunkObject.transform.position.x + chunkSize))
        {
            changeTarget = true;
            changeDistance = new Vector3(chunkSize, 0, 0);
        }

        //checking to load chucks in y axis
        if (targeter.transform.position.z < targetChunk.chunkObject.transform.position.z)
        {
            changeTarget = true;
            changeDistance = new Vector3(0, 0, -chunkSize);
        }
        else if (targeter.transform.position.z > (targetChunk.chunkObject.transform.position.z + chunkSize))
        {
            changeTarget = true;
            changeDistance = new Vector3(0, 0, chunkSize);
        }

        if (changeTarget)
        {
            FindTarget(targetChunk, changeDistance);
            RenderFromTarget();
        }
    }


    #region Chunk Loading Methods
    // only happens on target change
    private void FindTarget(Chunk previousTarget, Vector3 changeDistance)
    {
        targetChunk = loadedChunks[previousTarget.chunkObject.transform.position + changeDistance];
    }
    private void RenderFromTarget()
    {
        int toScaleRendDis = renderDistance * chunkSize;
        Vector3 renderEdgeX = new Vector3(toScaleRendDis, 0, 0);
        Vector3 renderEdgeZ = new Vector3(0, 0, toScaleRendDis);

        UnloadFromTarget(toScaleRendDis);

        LoadFromTarget(toScaleRendDis, renderEdgeX, renderEdgeZ); // checks render edge to load along the perpendicular axis
    }

    //unload chunks to pool
    private void UnloadFromTarget(int toScaleRendDis)
    {
        //unloading chunk outside of actual render distance
        foreach (Vector3 position in loadedChunkPositions)
        {
            if (position.x > targetChunk.chunkObject.transform.position.x + toScaleRendDis)
            {
                Debug.LogWarning($"unloading chunks with x pos: { position.x }");
                UnloadChunk(position);
            }
            else if (position.x < targetChunk.chunkObject.transform.position.x - toScaleRendDis)
            {
                Debug.LogWarning($"unloading chunks with x pos: { position.x }");
                UnloadChunk(position);
            }
            else if (position.z > targetChunk.chunkObject.transform.position.z + toScaleRendDis)
            {
                Debug.LogWarning($"unloading chunks with z pos: { position.z }");
                UnloadChunk(position);
            }
            else if (position.z < targetChunk.chunkObject.transform.position.z - toScaleRendDis)
            {
                Debug.LogWarning($"unloading chunks with z pos: { position.z }");
                UnloadChunk(position);
            }
        }
    }

    private void UnloadChunk(Vector3 position) // removes/deletes chunk subclass instance in position
    {
        loadedChunks[position].chunkObject.SetActive(false);
    }

    //load chunks from pool
    private void LoadFromTarget(int toScaleRendDis, Vector3 renderEdgeX, Vector3 renderEdgeZ)
    {
        //checking each direction to add chunks where needed in render distance
        if (!loadedChunkPositions.Contains(targetChunk.chunkObject.transform.position + renderEdgeX))
        {
            //Debug.Log($"load chunks along Z axis from X check: { targetChunk.chunkObject.transform.position + renderEdgeX}");
            Vector3 startingPos = targetChunk.chunkObject.transform.position + renderEdgeX - new Vector3(0, 0, toScaleRendDis);
            LoadChunksAlongAxis(startingPos, renderEdgeZ);
        }
        else if (!loadedChunkPositions.Contains(targetChunk.chunkObject.transform.position - renderEdgeX))
        {
            Vector3 startingPos = targetChunk.chunkObject.transform.position - renderEdgeX - new Vector3(0, 0, toScaleRendDis);
            LoadChunksAlongAxis(startingPos, renderEdgeZ);
        }
        else if (!loadedChunkPositions.Contains(targetChunk.chunkObject.transform.position + renderEdgeZ))
        {
            Vector3 startingPos = targetChunk.chunkObject.transform.position + renderEdgeZ - new Vector3(toScaleRendDis, 0, 0);
            LoadChunksAlongAxis(startingPos, renderEdgeX);
        }
        else if (!loadedChunkPositions.Contains(targetChunk.chunkObject.transform.position - renderEdgeZ))
        {
            Vector3 startingPos = targetChunk.chunkObject.transform.position - renderEdgeZ - new Vector3(toScaleRendDis, 0, 0);
            LoadChunksAlongAxis(startingPos, renderEdgeX);
        }
    }

    private void LoadChunksAlongAxis(Vector3 loadStart, Vector3 Axis) // loads chunks along an axis, this axis should be the edge of the rendering
    { 
        // chunks load along the perpendicular axis to the one the target is following!
        int renderSize = renderDistance * 2;

        if (Axis.x != 0) // axis is on X
        {
            //Debug.LogWarning("Chunks loading on xAxis");

            for (int i = 0; i <= renderSize; i++)
            {
                Vector3 chunkPlacement = new Vector3(i*chunkSize, 0, 0);
                Vector3 newPos = loadStart + chunkPlacement;
                LoadChunk(newPos);
            }
        }
        else if (Axis.z != 0) // axis is on Z
        {
            for (int i = 0; i <= renderSize; i++)
            {
                Vector3 chunkPlacement = new Vector3(0, 0, i*chunkSize);
                Vector3 newPos = loadStart + chunkPlacement;
                LoadChunk(newPos);
            }
        }
        else { Debug.LogError("Chunk loader did not load on Axis"); }
    }
    
    private void LoadChunk(Vector3 newPos) // adds/instances chunk from subclass in position
    {
        //Debug.LogWarning($"Chunk Loaded on :{position}");
        int iterator = 0; // used to locade loaded position
        foreach (Vector3 pos in loadedChunkPositions)
        {
            if (!loadedChunks[pos].chunkObject.activeInHierarchy)
            {
                Debug.LogWarning($"loading chunk from {pos} to {newPos}");

                //calculating neighboring chunks needed for loading the mesh
                Chunk eastChunk = loadedChunks[newPos + new Vector3(chunkSize,0,0)];
                Chunk southeastChunk = loadedChunks[newPos + new Vector3(chunkSize, 0, chunkSize)];
                Chunk southChunk = loadedChunks[newPos + new Vector3(0, 0, chunkSize)];

                loadedChunks[pos].ReloadChunk(newPos, eastChunk, southeastChunk, southChunk);
                
                //readding chunk to dictionary and saving it's new position
                Chunk chunkLoader = loadedChunks[pos];
                loadedChunks.Remove(pos);

                loadedChunks.Add(newPos, chunkLoader);
                loadedChunkPositions[iterator] = newPos;
                
                break;
            }
            iterator++;
        }
    }
    #endregion
}

public class Chunk
{
    public GameObject chunkObject = new GameObject("Chunk");

    private GameObject[,] groundPoints;
    private TerrainMeshGenerator meshGen;
    private int chunkSize;
    private float roughness;
    private float yScale;

    public Chunk(int chunkX, int chunkZ, Material groundMat, int chunkSize, float roughness, float yScale)
    {
        this.chunkSize = chunkSize;
        this.roughness = roughness;
        this.yScale = yScale;
        groundPoints = new GameObject[chunkSize, chunkSize];

        chunkObject.transform.position = new Vector3(chunkX, 0, chunkZ);

        //place nodes
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                groundPoints[x, z] = new GameObject("Node", typeof(TerrainMeshGenerator), typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));

                //position node
                groundPoints[x, z].transform.position = new Vector3(chunkX + x - 1, (int)PerlinNoise((float)chunkX + x, (float)chunkZ + z), chunkZ + z - 1);
                groundPoints[x, z].transform.parent = chunkObject.transform;
            }
        }

        //create mesh
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                //set up the mesh
                groundPoints[x, z].GetComponent<TerrainMeshGenerator>().SetUpMesh(groundMat);

                //generate mesh // generated in a square shape starting in topleft going clockwise for mesh gen rules 
                if (x+1 == chunkSize || z+1 == chunkSize)
                { groundPoints[x, z].GetComponent<TerrainMeshGenerator>().GenMesh(groundPoints[x, z].transform.position, groundPoints[x, z].transform.position, groundPoints[x, z].transform.position, groundPoints[x, z].transform.position); }
                else { groundPoints[x, z].GetComponent<TerrainMeshGenerator>().GenMesh(groundPoints[x, z].transform.position, groundPoints[x + 1, z].transform.position, groundPoints[x + 1, z + 1].transform.position, groundPoints[x, z + 1].transform.position); }

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
        ReloadMesh(eastChunk, southeastChunk, southChunk);
    }

    public void ReloadNodes(Vector3 newPos)
    {
        int xPos = (int)newPos.x;
        int zPos = (int)newPos.z;


        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                groundPoints[x, z].transform.position = new Vector3(xPos + x, (int)PerlinNoise((float)xPos + x, (float)zPos + z), zPos + z);
            }
        }
    }

    public void ReloadMesh(Chunk eChunk, Chunk seChunk, Chunk sChunk) // south and east chunks are needed to create mesh
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                //generate mesh // generated in a square shape starting in topleft going clockwise for mesh gen rules 
                if (x + 1 == chunkSize)
                { // if chunks needs nodes outside of chunk access the passed chunk // #MN: 0 access' the neighbor (since it's the first) 
                    groundPoints[x, z].GetComponent
                        <TerrainMeshGenerator>().GenMesh(
                    groundPoints[x, z].transform.position,
                    eChunk.groundPoints[0, z].transform.position,
                    eChunk.groundPoints[0, z+1].transform.position,
                    groundPoints[x, z+1].transform.position); 
                }
                else if (z + 1 == chunkSize)
                {
                    groundPoints[x, z].GetComponent
                        <TerrainMeshGenerator>().GenMesh(
                    groundPoints[x, z].transform.position,
                    groundPoints[x+1, z].transform.position,
                    sChunk.groundPoints[x+1, 0].transform.position,
                    sChunk.groundPoints[x, 0].transform.position);
                }
                else if (x + 1 == chunkSize && z + 1 == chunkSize)
                {
                    groundPoints[x, z].GetComponent
                        <TerrainMeshGenerator>().GenMesh(
                    groundPoints[x, z].transform.position,
                    eChunk.groundPoints[0, z].transform.position,
                    seChunk.groundPoints[0, 0].transform.position,
                    sChunk.groundPoints[x, 0].transform.position);
                }
                else { groundPoints[x, z].GetComponent<TerrainMeshGenerator>().GenMesh(groundPoints[x, z].transform.position, groundPoints[x + 1, z].transform.position, groundPoints[x + 1, z + 1].transform.position, groundPoints[x, z + 1].transform.position); }
            }
        }
    }
}
