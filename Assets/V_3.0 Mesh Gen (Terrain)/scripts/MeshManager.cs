using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    private Material groundMat;
    private GameObject[,] appliedMesh;

    private void Start()
    {
        int toScaleRenderSize = GameManager.manager.RenderSize * GameManager.manager.chunkSize;

        appliedMesh = new GameObject[toScaleRenderSize, toScaleRenderSize];

        foreach (GameObject MeshObject in appliedMesh)
        { 
            MeshObject = new GameObject("Active Mesh", typeof(MeshGeneratorV1), typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        }

        groundMat = GameManager.manager.groundMat;
        meshGenerator.SetUpMesh(groundMat);
    }

    public void CreateMesh()
    {
        // create mesh according to generated chunks
    }

    public void ReloadMesh()
    {
        // Unload Mesh behind player and load mesh infront of player
    }
}
