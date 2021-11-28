using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMeshGenerator : MonoBehaviour
{
    private Material material;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    public void SetUpMesh(Material material)
    {
        this.material = material;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void GenMesh(Vector3 node0, Vector3 node1, Vector3 node2, Vector3 node3)
    {
        vertices = new Vector3[]
        {
            node0,
            node1,
            node2,
            node3
        };

        //check which way to spit quad here
        //Debug.LogError($"{node1.y }%{ node3.y }>{ node0.y }%{ node2.y}: {node1.y % node3.y > node0.y % node2.y} if true produce odd split");
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

        UpdateMesh();
    }

    private void UpdateMesh()
    {
        mesh.Clear();
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        GetComponent<MeshRenderer>().material = material;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}