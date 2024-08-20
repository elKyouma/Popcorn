using iShape.Geometry;
using iShape.Mesh2d;
using iShape.Triangulation.Shape.Delaunay;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Source {

    public class DelaunayScene : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private Mesh mesh;
        private int testIndex = 0;
        private TestShape gameMap = new TestShape();

        private void Awake() {
            Application.targetFrameRate = 60;
            this.meshFilter = gameObject.GetComponent<MeshFilter>();
            this.mesh = new Mesh();
            this.mesh.MarkDynamic();
            this.meshFilter.mesh = mesh;
            setTest();
        }
        public void Next() {
            gameMap.hull = WorldManager.mapOutlinePoints.ToArray();
            gameMap.holes = new Vector2[WorldManager.planetsOutlines.Count][];
            int i = 0;
            foreach (List<Vector2> list in WorldManager.planetsOutlines)
            {
                gameMap.holes[i] = list.ToArray();
                i++;
            }
            setTest();
        }

        private void setTest() {
            var iGeom = IntGeom.DefGeom;

            var pShape = gameMap.ToPlainShape(iGeom, Allocator.Temp);
            //Here Error \/
            //var triangles = pShape.DelaunayTriangulate(Allocator.Temp);
            //var points = iGeom.Float(pShape.points, Allocator.Temp);
            //var vertices = new NativeArray<float3>(points.Length, Allocator.Temp);
            //for (int i = 0; i < points.Length; ++i) {
            //    var p = points[i];
            //    vertices[i] = new float3(p.x, p.y, 0);
            //}
            //points.Dispose();
            
            //var bodyMesh = new StaticPrimitiveMesh(vertices, triangles);
            
            //vertices.Dispose();
            //triangles.Dispose();
            
            //var colorMesh = new NativeColorMesh(vertices.Length, Allocator.Temp);
            
            //colorMesh.AddAndDispose(bodyMesh, Color.green);
            //colorMesh.FillAndDispose(mesh);

            //pShape.Dispose();
        }
    }

}
