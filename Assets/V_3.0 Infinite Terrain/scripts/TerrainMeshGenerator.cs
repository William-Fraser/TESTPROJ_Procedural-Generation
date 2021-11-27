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
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        this.material = material;
    }

    public void GenMesh(Vector3 node0, Vector3 node1, Vector3 node2, Vector3 node3)
    {
        vertices = new Vector3[]
        {
            new Vector3(0, node0.y, 0),
            new Vector3(1, node1.y, 0),
            new Vector3(1, node2.y, 1),
            new Vector3(0, node3.y, 1),
        };

        //check which way to spit quad here
        Debug.LogError($"{node1.y }%{ node3.y }>{ node0.y }%{ node2.y}: {node1.y % node3.y > node0.y % node2.y} if true produce odd split");
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
