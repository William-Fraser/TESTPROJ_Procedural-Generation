using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneratorTest : MonoBehaviour
{
    public GameObject player; // offload this into game manager, which should also handle terrain generator class

    [Header("Terrain Generation")]
    public int seed = 0;
    [Tooltip("chunks loaded in a radius format from target chunk")]
    public int renderDistance = 4;

    [Header("Chunk Generation")]
    public int chunkSize = 10;
    public float yScale = 1;
    public float randomness = 1;
    public Material groundMat;

    [Header("private debug")] // [SHIFT+ALT] all fields to easily make private
    //chunk loadings
    public TestChunk targetChunk;
    public List<Vector3> loadedChunkPositions = new List<Vector3>();
    public Dictionary<Vector3, TestChunk> loadedChunks;

    private TestChunk chunkGeneration;

    void Start()
    {
        loadedChunks = new Dictionary<Vector3, TestChunk>();

        GenerateWorld();

        Debug.Log($"Inital Target Chunk: {targetChunk.chunkObject.transform.position}");

        // debug controller
        float spawnPoint = targetChunk.chunkObject.transform.position.x + ((float)chunkSize / 2);
        player = Instantiate(player, new Vector3(spawnPoint, spawnPoint + yScale * 2, spawnPoint), Quaternion.identity);
    }
    void Update()
    {
        CheckAndChangeTargetChunk(player);
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
                chunkGeneration = new TestChunk((int)positioning.x, (int)positioning.z, groundMat, chunkSize, randomness, yScale);
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
    private void CheckAndChangeTargetChunk(GameObject targeter) // targeter is the object being checked for the Target Chunk, which is the chunk the targeter is currently in
    {
        bool changeTarget = false;
        Vector3 changeDistance = Vector3.zero;

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
            RenderFromTargetChunk();
        }
    }

    //check these only when target chunk changes
    private void FindTarget(TestChunk previousTarget, Vector3 changeDistance)
    {
        targetChunk = loadedChunks[previousTarget.chunkObject.transform.position + changeDistance];
    }
    private void RenderFromTargetChunk()
    {
        int toScaleRendDis = renderDistance * chunkSize;
        Vector3 renderEdgeX = new Vector3(toScaleRendDis, 0, 0);
        Vector3 renderEdgeZ = new Vector3(0, 0, toScaleRendDis);

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
    { // chunks load along the perpendicular axis to the one the target is following!
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
                loadedChunks[pos].ReloadChunk(newPos);
                
                //readding chunk to dictionary and saving it's new position
                TestChunk chunkLoader = loadedChunks[pos];
                loadedChunks.Remove(pos);

                loadedChunks.Add(newPos, chunkLoader);
                loadedChunkPositions[iterator] = newPos;
                
                break;
            }
            iterator++;
        }
    }
    private void UnloadChunk(Vector3 position) // removes/deletes chunk subclass instance in position
    {
        loadedChunks[position].chunkObject.SetActive(false);
    }
    

}

public class TestChunk
{
    private int chunkSize;
    private float randomness;
    private float yScale;
    private GameObject[,] groundPoints;// points will be represented by cubes until mesh creation which links points, which the game object will then hold the drawn shape immediately to the right and back in the world space

    public GameObject chunkObject = new GameObject("Chunk");

    public TestChunk(int xPos, int zPos, Material groundMat, int chunkSize, float randomness, float yScale)
    {
        this.chunkSize = chunkSize;
        this.randomness = randomness;
        this.yScale = yScale;
        groundPoints = new GameObject[chunkSize, chunkSize];

        chunkObject.transform.position = new Vector3(xPos, 0, zPos);

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                //Debug.Log($"x: {x}, z: {z}");
                groundPoints[x, z] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                groundPoints[x, z].gameObject.GetComponent<MeshRenderer>().material = groundMat;
                groundPoints[x, z].transform.position = new Vector3(xPos+x, (int)PerlinNoise((float)xPos+x, (float)zPos+z), zPos+z);
                groundPoints[x, z].transform.parent = chunkObject.transform;
            }
        }
        
    }
    public float PerlinNoise(float x, float y)
    {
        return (Mathf.PerlinNoise((x / 10) * randomness, (y / 10) * randomness) * yScale) * 10;
    }
    public void ReloadChunk(Vector3 newPos)
    {
        int xPos = (int)newPos.x;
        int zPos = (int)newPos.z;

        chunkObject.SetActive(true);
        chunkObject.transform.position = newPos;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                groundPoints[x, z].transform.position = new Vector3(xPos + x, (int)PerlinNoise((float)xPos + x, (float)zPos + z), zPos + z);
            }
        }
    }
}
