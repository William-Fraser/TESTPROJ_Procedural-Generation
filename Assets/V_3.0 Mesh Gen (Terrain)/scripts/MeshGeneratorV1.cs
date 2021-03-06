using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGeneratorV1 : MonoBehaviour
{
    private Material material;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    public GameObject SetUpMesh(GameObject GO, Material material)
    {
        this.material = material;
        mesh = new Mesh();
        GO.GetComponent<MeshFilter>().mesh = mesh;

        return GO;
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