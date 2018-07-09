using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    Vector3[] vertices;
    int GridSize = 16;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        if (vertices != null)
        {
            Gizmos.color = Color.black;
            for (int i = 0; i < vertices.Length; i++)
            {
                Gizmos.DrawSphere(vertices[i], 0.1f);
            }
        }
    }

    public void CreateGrid(float[,] heightmap, float zOffset)
    {
        vertices = new Vector3[(GridSize + 1) * (GridSize + 1)];

        for (int i = 0, y = 0; y <= GridSize; y++)
        {
            for (int x = 0; x <= GridSize; x++, i++)
            {
                vertices[i] = new Vector3(x - GridSize >> 1, heightmap[x,y]-zOffset, y - GridSize >> 1);
            }
        }
    }

    public void LoadTerrain(int zoom, int fromX, int fromY, int toX, int toY)
    {
        for (int x = fromX; x <= toX; x++)
        {
            for (int y = fromY; y <= toY; y++)
            {
                Debug.Log("??");
                /*MeshFilter mesh = */
                float[,] heighData = LoadHelper.Instance.GetHeightData(zoom, x, y, GridSize + 1);
                CreateGrid(heighData, 0);
            }
        }
    }
}
