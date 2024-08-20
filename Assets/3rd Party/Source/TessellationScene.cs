using iShape.Geometry;
using iShape.Mesh2d;
using iShape.Triangulation.Shape.Delaunay;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
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

        private List<Pathfinding.Node<(float, float)>> nodes = new List<Pathfinding.Node<(float, float)>>();
        private List<Pathfinding.Node<(float, float)>> path;
        private bool pathCalculated = false;
        //private int index = 0;
        //private int frame = 0;

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
            //Debug.Log(path.Count);
            if (pathCalculated)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    //Debug.Log("node");
                    //Debug.Log(path[i].Data);
                    Debug.DrawLine(
                        new Vector3(path[i].Data.Item1, path[i].Data.Item2, -1),
                        new Vector3(path[i + 1].Data.Item1, path[i + 1].Data.Item2, -1),
                        Color.red);
                }

                //Debug.Log(nodes[0].Neighbors.Count);
                //Debug.Log(nodes[6].Neighbors.Count);

                Debug.DrawLine(
                    new Vector3(nodes[0].Data.Item1, nodes[0].Data.Item2, -1),
                    new Vector3(nodes[40].Data.Item1, nodes[40].Data.Item2, -1),
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
            path.Clear();
            testIndex = (testIndex + 1) % TriangulationTests.Data.Length;
            setTest(testIndex);
        }

        float Cost(Pathfinding.Node<(float, float)> node1, Pathfinding.Node<(float, float)> node2)
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

                var pathfinder = new Pathfinding.DStarLite<(float, float)>(nodes[0], nodes[40], nodes);

                pathfinder.Initialize();
                pathfinder.ComputeShortestPath();

                path = pathfinder.GetPath();
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
    }

}