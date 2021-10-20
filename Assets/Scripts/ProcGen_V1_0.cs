using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ProcGen_V1_0 : MonoBehaviour
{
    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    [Range(0,100)]
    public int randomFillPercent;

    private int[,] map;

    private void Start()
    {
        GenerateMap();
    }
    private void OnDrawGizmos()
    {
        if (map != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }

    private void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();
    }
    private void RandomFillMap() // based on seed
    {
        if (useRandomSeed) {
            seed = Time.time.ToString();
        }

        System.Random rdmNum = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = (rdmNum.Next(0, 100) < randomFillPercent)? 1: 0;
            }
        }
    }
}
