using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    private MeshGeneratorV1 meshGenerator;
    private GameObject[,] appliedMeshObject;
    private Material groundMat;
    private int toScaleRenderSize;

    private void Start()
    {
        meshGenerator = GetComponent<MeshGeneratorV1>();
        toScaleRenderSize = GameManager.manager.RenderSize * GameManager.manager.chunkSize;
        appliedMeshObject = new GameObject[toScaleRenderSize, toScaleRenderSize];
        groundMat = GameManager.manager.groundMat;

        for (int x = 0; x < toScaleRenderSize; x++)
        {
            for (int z = 0; z < toScaleRenderSize; z++)
            { 
                GameObject meshGeneration = new GameObject("Active Mesh", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                meshGeneration.transform.parent = this.transform;
                appliedMeshObject[x, z] = meshGenerator.SetUpMesh(meshGeneration, groundMat);
            }
        }
    }

    public void CreateMesh()
    {
        // create mesh according to chunks in manager

        //passing variables for parsing
        ChunkManager chunkManager = GameManager.manager.chunkManager;
        int chunkSize = chunkManager.chunkGenerator.chunkSize;
        Dictionary<Vector3, Chunk> loadedChunks = chunkManager.loadedChunks;
        
        //setting parseable ranges for mesh loading
        int chunkXStart = (int)chunkManager.targetChunk.chunkObject.transform.position.x - (chunkSize * GameManager.manager.renderDistance);
        int chunkXEnd = (int)chunkManager.targetChunk.chunkObject.transform.position.x + (chunkSize * GameManager.manager.renderDistance);
        int chunkZStart = (int)chunkManager.targetChunk.chunkObject.transform.position.z - (chunkSize * GameManager.manager.renderDistance);
        int chunkZEnd = (int)chunkManager.targetChunk.chunkObject.transform.position.z + (chunkSize * GameManager.manager.renderDistance);
        int meshXpositioner = 1; //#MN: multiplies node to get mesh position
        int meshZpositioner = 1;

        //nested loop to find chunks from target position
        for (int chunkX = chunkXStart; chunkX < chunkXEnd; chunkX=+chunkSize)
        {
            for (int chunkZ = chunkZStart; chunkZ < chunkZEnd; chunkZ=+chunkSize)
            {
                Chunk currentChunk = loadedChunks[new Vector3(chunkX, 0, chunkZ)];
                Chunk eastChunk = loadedChunks[new Vector3(chunkX, 0, chunkZ)];
                Chunk southeastChunk = loadedChunks[new Vector3(chunkX, 0, chunkZ)];
                Chunk southChunk = loadedChunks[new Vector3(chunkX, 0, chunkZ)];

                //nested loop to locate nodes within chunk
                for (int nodeX = 0; nodeX < chunkSize; nodeX++)
                {
                    for (int nodeZ = 0; nodeZ < chunkSize; nodeZ++)
                    {
                        int meshX = nodeX * meshXpositioner;
                        int meshZ = nodeZ * meshZpositioner;
                        Vector3 node0;
                        Vector3 node1;
                        Vector3 node2;
                        Vector3 node3;

                        if (nodeX == chunkSize - 1 && nodeZ == chunkSize - 1)
                        {
                            node0 = currentChunk.GroundNodes[nodeX, nodeZ].transform.position;
                            node1 = eastChunk.GroundNodes[0, nodeZ].transform.position;
                            node2 = southeastChunk.GroundNodes[0, 0].transform.position;
                            node3 = southChunk.GroundNodes[nodeX, 0].transform.position;
                        }
                        else if (nodeX == chunkSize - 1)
                        {
                            node0 = currentChunk.GroundNodes[nodeX, nodeZ].transform.position;
                            node1 = eastChunk.GroundNodes[0, nodeZ].transform.position;
                            node2 = eastChunk.GroundNodes[0, nodeZ + 1].transform.position;
                            node3 = currentChunk.GroundNodes[nodeX, nodeZ + 1].transform.position;
                        }
                        else if (nodeZ == chunkSize - 1)
                        {
                            node0 = currentChunk.GroundNodes[nodeX, nodeZ].transform.position;
                            node1 = currentChunk.GroundNodes[nodeX + 1, nodeZ].transform.position;
                            node2 = southChunk.GroundNodes[nodeX + 1, 0].transform.position;
                            node3 = southChunk.GroundNodes[nodeX, 0].transform.position;
                        }
                        else
                        {
                            node0 = currentChunk.GroundNodes[nodeX, nodeZ].transform.position;
                            node1 = currentChunk.GroundNodes[nodeX + 1, nodeZ].transform.position;
                            node2 = currentChunk.GroundNodes[nodeX + 1, nodeZ + 1].transform.position;
                            node3 = currentChunk.GroundNodes[nodeX, nodeZ + 1].transform.position;
                        }

                        appliedMeshObject[meshX, meshZ] = meshGenerator.GenSplitSquareMesh(appliedMeshObject[meshX, meshZ], node0, node1, node2, node3);
                    }
                }

                meshXpositioner++;
                meshZpositioner++;
            }
        }
    }

    public void ReloadMesh()
    {
        // Unload Mesh behind player and load mesh infront of player
    }
}
