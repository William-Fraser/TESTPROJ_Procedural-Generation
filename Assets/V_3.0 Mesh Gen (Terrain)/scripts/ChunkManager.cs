using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public Chunk targetChunk;
    public Dictionary<Vector3, Chunk> loadedChunks = new Dictionary<Vector3, Chunk>();
    [SerializeField] // serialized for viewing
    public List<Vector3> loadedChunkPositions = new List<Vector3>();
    
    public ChunkGenerator chunkGenerator;

    void Update()
    {
        // handles chunk loading
        if (chunkGenerator != null)
        { 
            CheckAndChangeTarget(GameManager.manager.levelManager.player);
            Debug.Log($"Player Pos: {GameManager.manager.levelManager.player.transform.position}");
        }
    }

    public void CreateManager()
    {
        chunkGenerator = this.gameObject.AddComponent<ChunkGenerator>();

        //intitalizes chunk subclass
        loadedChunks = chunkGenerator.GenerateNodes(); // this references the current script that handles the generated chunks

        //setting target chunk
        targetChunk = chunkGenerator.StartingTarget;

        Debug.Log($"Inital Target Chunk: {targetChunk.chunkObject.transform.position}");

        // spawning debug player on target chunk
        float spawnPositioning = targetChunk.chunkObject.transform.position.x + ((float)chunkGenerator.chunkSize / 2); // #MN: spawns player in the middle of terrain generations
        Vector3 spawnPoint = new Vector3(spawnPositioning, GameManager.manager.yScale + 5, spawnPositioning);
        GameManager.manager.levelManager.SpawnDebugPlayer(spawnPoint, chunkGenerator);
        GameManager.manager.levelManager.player.transform.position = spawnPoint;
        Debug.LogError($"spawn position: {spawnPoint}, player spawned at: {GameManager.manager.levelManager.player.transform.position}");
        Debug.LogWarning($"spawn point set");
    }

    private void CheckAndChangeTarget(GameObject targeter) // targeter is the object being checked for the Target Chunk, which is the chunk the targeter is currently in
    {
        bool changeTarget = false;
        Vector3 changeDistance = Vector3.zero;
        int chunkSize = chunkGenerator.chunkSize; // passing info for easy parsing

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
        Vector3 newTargetPos = previousTarget.chunkObject.transform.position + changeDistance;
        Debug.LogWarning($"newTarget: {newTargetPos}");
        targetChunk = loadedChunks[newTargetPos];
    }
    private void RenderFromTarget()
    {
        int toScaleRendDis = GameManager.manager.renderDistance+1 * GameManager.manager.chunkSize; //#MN: +1 adds the node buffer edge for mesh generation
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
        // passing info for easy parsing
        int renderSize = GameManager.manager.renderDistance * 2 + 1;
        int chunkSize = chunkGenerator.chunkSize;

        // chunks load along the perpendicular axis to the one the target is following!
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
        int iterator = 0; // used to locate loaded position
        foreach (Vector3 pos in loadedChunkPositions)
        {
            if (!loadedChunks[pos].chunkObject.activeInHierarchy)
            {
                Debug.LogWarning($"loading chunk from {pos} to {newPos}");

                //readding chunk to dictionary to save it's new position
                Chunk chunkLoader = loadedChunks[pos];
                ConnectAndLoadChunk(chunkLoader, newPos);

                loadedChunks.Remove(pos);
                loadedChunks.Add(newPos, chunkLoader);
                loadedChunkPositions[iterator] = newPos;
                
                break;
            }
            iterator++;
        }
    }

    private void ConnectAndLoadChunk(Chunk loadingChunk, Vector3 newPos)
    {
        int chunkSize = chunkGenerator.chunkSize; // passing info for easy parsing

        //calculating neighboring chunks needed for loading the mesh
        Vector3 east = newPos + new Vector3(chunkSize, 0, 0);
        Vector3 southeast = newPos + new Vector3(chunkSize, 0, chunkSize);
        Vector3 south = newPos + new Vector3(0, 0, chunkSize);
        Chunk eastChunk;
        Chunk southeastChunk;
        Chunk southChunk;

        //if chunks aren't on the list don't load to them // null value is to be handled inside ReloadChunk
        if (loadedChunks.ContainsKey(east))
        { eastChunk = loadedChunks[newPos + new Vector3(chunkSize, 0, 0)]; }
        else
        { eastChunk = null; }

        if (loadedChunks.ContainsKey(southeast))
        { southeastChunk = loadedChunks[newPos + new Vector3(chunkSize, 0, chunkSize)]; }
        else
        { southeastChunk = null; }

        if (loadedChunks.ContainsKey(south))
        { southChunk = loadedChunks[newPos + new Vector3(0, 0, chunkSize)]; }
        else
        { southChunk = null; }
        
        loadingChunk.ReloadChunk(newPos, eastChunk, southeastChunk, southChunk);
    }
    #endregion
}