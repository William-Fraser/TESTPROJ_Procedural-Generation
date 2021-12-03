using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMeshGen : MonoBehaviour
{
    public Material material;
    
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    private void Start()
    {
        TerrainNodeGen nodeGen = GetComponent<TerrainNodeGen>();

        //place mesh
        for (int x = 0; x < nodeGen.terrainSize; x++)
        {
            for (int z = 0; z < nodeGen.terrainSize; z++)
            {
                Debug.Log($"mesh: x: {x}, z: {z}");

                if (x < nodeGen.terrainSize-1 && z < nodeGen.terrainSize-1)
                {
                    Vector3 node0 = nodeGen.GroundNodes[x, z].transform.position;
                    Vector3 node1 = nodeGen.GroundNodes[x+1, z].transform.position;
                    Vector3 node2 = nodeGen.GroundNodes[x+1, z+1].transform.position;
                    Vector3 node3 = nodeGen.GroundNodes[x, z+1].transform.position;

                    GameObject meshGeneration = new GameObject("Active Mesh", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                    meshGeneration = GenSplitSquareMesh(meshGeneration, node0, node1, node2, node3);
                }

            }
        }
    }

    public GameObject GenSplitSquareMesh(GameObject GO, Vector3 node0, Vector3 node1, Vector3 node2, Vector3 node3)
    {
        vertices = new Vector3[]
        {
            node0,
            node1,
            node2,
            node3
        };

        //check which way to spit quad here
        if (node1.y % node3.y > node0.y % node2.y)
        {
            triangles = new int[]
            {
                1, 0, 3,
                3, 2, 1,
            };
        }
        else
        {
            triangles = new int[]
            {
                0, 3, 2,
                2, 1, 0
            };
        }

        return UpdateMesh(GO);
    }

    private GameObject UpdateMesh(GameObject GO)
    {
        mesh = GO.GetComponent<MeshFilter>().mesh;

        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        GO.GetComponent<MeshFilter>().mesh = mesh;
        GO.GetComponent<MeshRenderer>().material = material;
        GO.GetComponent<MeshCollider>().sharedMesh = mesh;

        return GO;
    }
}
