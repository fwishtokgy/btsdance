using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CustomMeshObject : MonoBehaviour
{
    protected Mesh mesh;
    protected MeshFilter meshFilter;
    public void SetMeshPoints(Vector3[] points)
    {
        meshFilter = this.GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.name = "CustomMesh";
        mesh.vertices = points;

        int[] tris = new int[6]
        {
            // lower left triangle
            3, 1, 0,
            // upper right triangle
            3, 2, 1
        };
        mesh.triangles = tris;

        Vector3[] normals = new Vector3[4]
        {
            Vector3.down,
            Vector3.down,
            Vector3.down,
            Vector3.down
        };
        mesh.normals = normals;

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };
        mesh.uv = uv; 

        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }
    public float BogusDoubledRadius()
    {
        var radius = Mathf.Sqrt(Mathf.Pow(mesh.bounds.size.x, 2) + Mathf.Pow(mesh.bounds.size.z, 2));
        return radius;
    }
    public Vector3 Center()
    {
        return mesh.bounds.center;
    }
}
