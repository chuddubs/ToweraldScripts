using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Generates a filled and a hollow polygonal mesh, with optional collider and UVs.
/// The hollow ring  is so we can apply a darker material to it, for an inner outline effect.
/// Must be on a GameObject with a MeshFilter, with a first child with a MeshFilter (for the hollow mesh)
/// </summary>
public class PolygonMeshGenerator : MonoBehaviour
{
    private Mesh filled;
    private Mesh hollow;
    private List<Vector3> outerPts = new List<Vector3>();
    private List<Vector3> innerPts = new List<Vector3>();
    private int[] filledTriangles;
    private int[] hollowTriangles;

    PolygonCollider2D polyCollider;
    
    public void Generate(int sides, float radius, float randomness, bool addCollider = true)
    {
        filled = new Mesh();
        hollow = new Mesh();  // hollow ring
        GetComponent<MeshFilter>().mesh = filled;
        transform.GetChild(0).GetComponent<MeshFilter>().mesh = hollow;
        DrawFilledAndHollow(sides , radius, randomness, addCollider);
    }

    /// <summary>
    /// Main method to create the mesh geometry.
    /// </summary>
    void DrawFilledAndHollow(int sides, float radius, float randomness, bool addCollider)
    {
        // Create irregular points forming the outer and inner edges
        GetIrregularCircumferencePoints(sides, radius, randomness);

        filledTriangles = DrawFilledTriangles(outerPts);            // Outer edge vertices
        hollowTriangles = DrawHollowTriangles(outerPts, innerPts);  // Inner edge vertices (for the hollow mesh)

        // Assign geometry to meshes
        filled.Clear();
        hollow.Clear();
        filled.vertices = outerPts.ToArray();
        hollow.vertices = outerPts.Concat(innerPts).ToArray();
        filled.triangles = filledTriangles;
        hollow.triangles = hollowTriangles;

        // Add UVs and (optionally) a collider
        MakeUVsAndCollider(filled, addCollider);
        MakeUVsAndCollider(hollow, false);
    }

    /// <summary>
    /// Computes outer and inner points arranged in a roughly circular pattern with optional randomness.
    /// Outer points form the outer boundary; inner points are scaled inward (75%) to create a hollow ring.
    /// Randomness is the amount of random variation in radius per point (0 = perfect circle)
    /// </summary>
    void GetIrregularCircumferencePoints(int sides, float _radius, float _randomness)
    {
        float circumferenceProgressPerStep = (float)1f / sides;
        float TAU = 2 * Mathf.PI;
        float radianProgressPerStep = circumferenceProgressPerStep * TAU;

        for (int i = 0; i < sides; i++)
        {
            float currentRadian = radianProgressPerStep * i;
            float randomRadius = _radius * Random.Range(1 - _randomness, 1 + _randomness);

            // Outer vertex
            outerPts.Add(new Vector3(Mathf.Cos(currentRadian) * randomRadius, Mathf.Sin(currentRadian) * randomRadius, 0));

            // Inner vertex (75% of the outer radius)
            float innerRadius = randomRadius * 0.75f;
            innerPts.Add(new Vector3(Mathf.Cos(currentRadian) * innerRadius, Mathf.Sin(currentRadian) * innerRadius, 0));
        }
    }
    
    /// <summary>
    /// Creates triangle indices to form a filled fan shape from the center.
    /// </summary>
    int[] DrawFilledTriangles(List<Vector3> points)
    {   
        int triangleAmount = points.Count - 2;
        List<int> newTriangles = new List<int>();
        for(int i = 0; i<triangleAmount; i++)
        {
            newTriangles.Add(0);
            newTriangles.Add(i+2);
            newTriangles.Add(i+1);
        }
        return newTriangles.ToArray();
    }

    /// <summary>
    /// Creates triangle indices for a ring-shaped mesh by linking outer and inner vertices.
    /// </summary>
    int[] DrawHollowTriangles(List<Vector3> outers, List<Vector3> inners)
    {
        Vector3[] points = outers.Concat(inners).ToArray();
        // points.AddRange(inners);
        int sides = points.Length/2;
        List<int> newTriangles = new List<int>();
        for(int i = 0; i<sides;i++)
        {
            int outerIndex = i;
            int innerIndex = i+sides;
 
            // First triangle: outer -> inner -> next outer
            newTriangles.Add(outerIndex);
            newTriangles.Add(innerIndex);
            newTriangles.Add((i+1)%sides);
            
            // Second triangle: outer -> previous inner -> current inner
            newTriangles.Add(outerIndex);
            newTriangles.Add(sides+((sides+i-1)%sides));
            newTriangles.Add(outerIndex+sides);
        }
        return newTriangles.ToArray();
    }
    
    private void MakeUVsAndCollider(Mesh m, bool addCollider)
    {
        Vector3[] vertices = m.vertices;
        Vector2[] vertices2d = new Vector2[vertices.Length];

        for (var i = 0; i < vertices.Length; i++) {
            vertices2d[i] = new Vector2(vertices[i].x, vertices[i].y);
        }
        if (addCollider)
        {
            polyCollider = gameObject.AddComponent<PolygonCollider2D>();
            polyCollider.points = vertices2d;
            m.normals = m.vertices;
            m.RecalculateNormals();
        }
        m.SetUVs(0, vertices2d);
    }

}
