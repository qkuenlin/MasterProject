using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshUtils : MonoBehaviour
{

    private int[] Neighboors;

    // Use this for initialization
    void Start()
    {
        //StartCoroutine("CalculateNormals");
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator GetNeighboorVertices(Mesh mesh, int v)
    {
        List<int> n = new List<int>();
        for (int f = 0; f < mesh.triangles.Length; f += 3)
        {
            if (mesh.triangles[f] == v || mesh.triangles[f + 1] == v || mesh.triangles[f + 2] == v)
            {
                yield return new WaitForEndOfFrame();
                Debug.Log(f + "/" + mesh.triangles.Length);

                if (!n.Contains(mesh.triangles[f])) n.Add(mesh.triangles[f]);
                if (!n.Contains(mesh.triangles[f + 1])) n.Add(mesh.triangles[f + 1]);
                if (!n.Contains(mesh.triangles[f + 2])) n.Add(mesh.triangles[f + 2]);
            }

        }

        Neighboors = n.ToArray();

    }

    IEnumerator CalculateNormals()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        if (mesh == null) yield return null;
        else
        {
            yield return new WaitForEndOfFrame();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            //for (int v=0; v<mesh.vertexCount; v++)
            {
                yield return GetNeighboorVertices(mesh, 0);
                Debug.Log(Neighboors.Length);
            }
        }
    }
}
