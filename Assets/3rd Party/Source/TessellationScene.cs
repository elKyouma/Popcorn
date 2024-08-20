using iShape.Geometry;
using iShape.Mesh2d;
using iShape.Triangulation.Shape.Delaunay;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Node = Pathfinding.Node<(float, float)>;
using Random = UnityEngine.Random;

namespace Source
{

    public class TesselationScene : MonoBehaviour
    {

        [Range(min: 1f, max: 25)]
        [SerializeField]
        private float maxEdge = 1;

        [Range(min: 0.5f, max: 10)]
        [SerializeField]
        private float maxArea = 1;

        private MeshFilter meshFilter;
        private Mesh mesh;
        private int testIndex = 0;

        private float prevMaxEdge = 1;
        private float prevMaxArea = 1;

        private List<Node> nodes;
        private List<List<Node>> paths = new();
        private bool pathCalculated = false;
        [SerializeField]
        [Range(0, 1000)]
        private int currentTarget = 100;
        private int lastTarget = 100;
        //private int index = 0;
        //private int frame = 0;

        private readonly int enemyCount = 30;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            this.meshFilter = gameObject.GetComponent<MeshFilter>();
            this.mesh = new Mesh();
            this.mesh.MarkDynamic();
            this.meshFilter.mesh = mesh;
            prevMaxEdge = maxEdge;
            prevMaxArea = maxArea;

            setTest(testIndex);
        }

        private void OnDrawGizmos()
        {
            if (pathCalculated)
            {
                for (int i = 0; i < enemyCount; i++)
                {
                    DebugDrawPath(paths[i], Color.red);
                }
                //DebugDrawPath(path, Color.red);

                //Debug.Log(nodes[0].Neighbors.Count);
                //Debug.Log(nodes[6].Neighbors.Count);

                Debug.DrawLine(
                    new Vector3(nodes[0].Data.Item1, nodes[0].Data.Item2, -1),
                    new Vector3(nodes[currentTarget].Data.Item1, nodes[currentTarget].Data.Item2, -1),
                    Color.blue);

                //foreach (var node in nodes[index].Neighbors)
                //    Debug.DrawLine(
                //        new Vector3(nodes[index].Data.Item1, nodes[index].Data.Item2, -1),
                //        new Vector3(node.Data.Item1, node.Data.Item2, -1),
                //        Color.black);



            }
        }

        private void Update()
        {
            if (Math.Abs(prevMaxEdge - maxEdge) > 0.01f || Math.Abs(prevMaxArea - maxArea) > 0.01f)
            {
                prevMaxEdge = maxEdge;
                prevMaxArea = maxArea;
                setTest(testIndex);
            }
            if (lastTarget != currentTarget)
            {
                paths.Clear();
                for (int i = 0; i < enemyCount; i++)
                {
                    paths.Add(ComputePath(nodes[50 + i], nodes[currentTarget], nodes));
                }
                lastTarget = currentTarget;
            }


            //if (frame < 100)
            //{
            //    frame++;
            //}
            //else
            //{

            //    index = index + 1 > nodes.Count - 1 ? 0 : index += 1;
            //    frame = 0;
            //}
        }

        public void Next()
        {
            pathCalculated = false;
            nodes.Clear();
            paths.Clear();
            testIndex = (testIndex + 1) % TriangulationTests.Data.Length;
            setTest(testIndex);
        }

        float Cost(Node node1, Node node2)
        {
            return MathF.Sqrt(MathF.Pow(node1.Data.Item1 - node2.Data.Item1, 2) + MathF.Pow(node1.Data.Item2 - node2.Data.Item2, 2));
        }

        private void setTest(int index)
        {

            var iGeom = IntGeom.DefGeom;

            var pShape = TriangulationTests.Data[index].ToPlainShape(iGeom, Allocator.Temp);

            var extraPoints = new NativeArray<IntVector>(0, Allocator.Temp);
            var delaunay = pShape.Delaunay(iGeom.Int(maxEdge), extraPoints, Allocator.Temp);
            delaunay.Tessellate(iGeom, maxArea);

            extraPoints.Dispose();

            var triangles = delaunay.Indices(Allocator.Temp);
            var vertices = delaunay.Vertices(Allocator.Temp, iGeom, 0);



            if (!pathCalculated)
            {

                nodes = CreateNodes(vertices, triangles);


                for (int i = 0; i < enemyCount; i++)
                {
                    paths.Add(ComputePath(nodes[50 + i], nodes[currentTarget], nodes));
                    //Debug.Log("succes");
                }
                lastTarget = currentTarget;

                pathCalculated = true;

            }

            delaunay.Dispose();
            // set each triangle as a separate mesh

            var subVertices = new NativeArray<float3>(3, Allocator.Temp);
            var subIndices = new NativeArray<int>(new[] { 0, 1, 2 }, Allocator.Temp);

            var colorMesh = new NativeColorMesh(triangles.Length, Allocator.Temp);

            for (int i = 0; i < triangles.Length; i += 3)
            {

                for (int j = 0; j < 3; j += 1)
                {
                    var v = vertices[triangles[i + j]];
                    subVertices[j] = new float3(v.x, v.y, v.z);
                }

                var subMesh = new StaticPrimitiveMesh(subVertices, subIndices, Allocator.Temp);
                var color = new Color(Random.value, Random.value, Random.value);

                colorMesh.AddAndDispose(subMesh, color);
            }

            subIndices.Dispose();
            subVertices.Dispose();

            vertices.Dispose();
            triangles.Dispose();
            colorMesh.FillAndDispose(mesh);


            pShape.Dispose();
        }
        private List<Node> CreateNodes(NativeArray<Vector3> vertices, NativeArray<int> triangles)
        {
            List<Pathfinding.Node<(float, float)>> nodes = new();

            foreach (var vertex in vertices)
            {
                nodes.Add(new Pathfinding.Node<(float, float)>((vertex.x, vertex.y), Cost, Cost));
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

        private List<Node> ComputePath(Node startNode, Node goalNode, List<Node> grid)
        {
            var pathfinder = new Pathfinding.DStarLite<(float, float)>(startNode, goalNode, grid);

            pathfinder.Initialize();
            pathfinder.ComputeShortestPath();

            return pathfinder.GetPath();
        }

        private void DebugDrawPath(List<Node> path, Color color)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(
                    new Vector3(path[i].Data.Item1, path[i].Data.Item2, -1),
                    new Vector3(path[i + 1].Data.Item1, path[i + 1].Data.Item2, -1),
                    color);
            }
        }
    }

}