using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class ProceduralGen : MonoBehaviour
{
    public int length = 10;
    public int height = 10;
    private GameObject[,] cubes;
    MethodInfo getBuiltinExtraResourcesMethod;

    // Start is called before the first frame update
    void Start()
    {
        cubes = new GameObject[height, length];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < length; j++)
            { 
                cubes[i, j] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //cubes[i].transform.Rotate(new Vector3(0, i*10,0));
                cubes[i, j].transform.position = new Vector3(j, i, 0);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
/*
    private Material GetDefaultMaterial()
    {
        if (getBuiltinExtraResourcesMethod == null) {
            BindingFlags bfs = BindingFlags.NonPublic | BindingFlags.Static;
            getBuiltinExtraResourcesMethod = typeof(EditorGUIUtility)
        }
    }*/
}
