using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TerrainNodeGen : MonoBehaviour
{
    public float seed = 1;
    public int terrainSize;
    public float roughness;
    public float yScale;

    
    private GameObject[,] groundNodes;
    public GameObject[,] GroundNodes { get{ return groundNodes; } } 

    private void Awake()
    {
        seed = Random.Range(1, 1000);
        groundNodes = new GameObject[terrainSize, terrainSize];

        //place nodes
        for (int x = 0; x < terrainSize; x++)
        {
            for (int z = 0; z < terrainSize; z++)
            {
                Debug.Log($"Node: x: {x}, z: {z}");
                groundNodes[x, z] = new GameObject("Node");
                groundNodes[x, z].transform.position = new Vector3( x, (int)PerlinNoise((float) x+seed, (float) z+seed), z);
                groundNodes[x, z].transform.parent = this.gameObject.transform;
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            SceneManager.LoadScene(0);
        }
    }

    public float PerlinNoise(float x, float z)
    {
        float perlin;

        //getting better percision on perlin scale
        x /= terrainSize;
        z /= terrainSize;

        perlin = Mathf.PerlinNoise(x * roughness, z * roughness) * yScale;

        //changing to larger number more relative to world
        perlin *= terrainSize;

        return perlin;
    }
}