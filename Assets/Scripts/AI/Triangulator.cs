//using iShape.Geometry;
//using iShape.Triangulation.Shape.Delaunay;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Node = Pathfinding.Node<(float, float)>;

public class Triangulator : MonoBehaviour
{
    [Range(min: 1f, max: 25)]
    [SerializeField]
    private float maxEdge = 1;

    [Range(min: 0.5f, max: 10)]
    [SerializeField]
    private float maxArea = 1;

    private float prevMaxEdge = 1;
    private float prevMaxArea = 1;

    private static List<Node> grid;
    private int frame = 0;


    // Start is called before the first frame update
    void Awake()
    {
        prevMaxEdge = maxEdge;
        prevMaxArea = maxArea;

        GenerateGrid();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Math.Abs(prevMaxEdge - maxEdge) > 0.01f || Math.Abs(prevMaxArea - maxArea) > 0.01f)
        {
            prevMaxEdge = maxEdge;
            prevMaxArea = maxArea;
            GenerateGrid();
        }

        if (frame < 20)
        {
            frame++;
        }
        else
        {
            frame = 0;
            prevMaxEdge = maxEdge;
            prevMaxArea = maxArea;
            GenerateGrid();
        }
    }

    static public List<Node> GetGrid() => grid;

    private void GenerateGrid()
    {
        /*
        var iGeom = IntGeom.DefGeom;

        //var pShape = // TODO get plain shape from world manager

        var extraPoints = new NativeArray<IntVector>(0, Allocator.Temp);
        var delaunay = pShape.Delaunay(iGeom.Int(maxEdge), extraPoints, Allocator.Temp);
        delaunay.Tessellate(iGeom, maxArea);

        extraPoints.Dispose();

        var triangles = delaunay.Indices(Allocator.Temp);
        var vertices = delaunay.Vertices(Allocator.Temp, iGeom, 0);


        grid = CreateNodes(vertices, triangles);

        delaunay.Dispose();

        vertices.Dispose();
        triangles.Dispose();

        pShape.Dispose();
        */
    }



    private List<Node> CreateNodes(NativeArray<Vector3> vertices, NativeArray<int> triangles)
    {
        float Cost(Node node1, Node node2)
        {
            return MathF.Sqrt(MathF.Pow(node1.Data.Item1 - node2.Data.Item1, 2) + MathF.Pow(node1.Data.Item2 - node2.Data.Item2, 2));
        }

        List<Node> nodes = new();

        foreach (var vertex in vertices)
        {
            nodes.Add(new Node((vertex.x, vertex.y), Cost, Cost));
        }

        for (int i = 0; i < triangles.Length / 3; i++)
        {
            nodes[triangles[3 * i]].Neighbors.Add(nodes[triangles[3 * i + 1]]);
            nodes[triangles[3 * i + 1]].Neighbors.Add(nodes[triangles[3 * i]]);

            nodes[triangles[3 * i + 1]].Neighbors.Add(nodes[triangles[3 * i + 2]]);
            nodes[triangles[3 * i + 2]].Neighbors.Add(nodes[triangles[3 * i + 1]]);

            nodes[triangles[3 * i + 2]].Neighbors.Add(nodes[triangles[3 * i]]);
            nodes[triangles[3 * i]].Neighbors.Add(nodes[triangles[3 * i + 2]]);
        }

        return nodes;
    }

}
