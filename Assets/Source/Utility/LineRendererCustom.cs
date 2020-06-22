using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Utility {
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class LineRendererCustom : MonoBehaviour {

        public bool UseWorldSpace;
        public float Width = 1;

        private float animateT = 0;
        public float AnimateSpeed = 2;

        public Color Color;

        private Vector3[] CurrentPath;

        private Vector3[] Vertices;
        private int[] Triangles;
        private Vector2[] UVs;

        private Vec2Int[] PointVertexMap;

        private Mesh Mesh;
        private bool MeshChangedFlag = false;

        private Material matRef;

        // Use this for initialization
        void Start () {
            matRef = GetComponent<MeshRenderer>().material;
            matRef.color = this.Color;
        }

        // Update is called once per frame
        void Update () {
            if (UseWorldSpace) {
                transform.position = new Vector3(0, 0, 0);
                transform.rotation = Quaternion.Euler(Vector3.zero);
            }
            if(MeshChangedFlag) GenerateMesh();

            Animate();
        }

        private void Animate () {
            animateT += Time.deltaTime * AnimateSpeed;
            animateT %= 1;
            matRef.mainTextureOffset = new Vector2(0, animateT);
        }

        private void GenerateMesh () {
            MeshChangedFlag = false;

            Mesh mesh = new Mesh();
            lock(this) {
                mesh.vertices = this.Vertices;
                mesh.triangles = this.Triangles;
                mesh.uv = this.UVs;
            }
            GetComponent<MeshFilter>().mesh = mesh;
        }

        // --------------------------------------------

        public void SetAll (Vector2[] points, float zPos) {
            if (points == null) return;
            Vector3[] positions = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++) {
                positions[i] = new Vector3(points[i].x, points[i].y, zPos);
            }
            SetAll(positions);
        }
        public void SetAll (Vector3[] points) {

            // check if points array is valid
            if (points == null || points.Length < 2) {
                lock (this) {
                    Vertices = null;
                    Triangles = null;
                    UVs = null;
                }
                MeshChangedFlag = true;
                return;
            }

            // set path
            lock (this) {
                CurrentPath = points;
            }

            lock (this) {
                // set vertices
                PointVertexMap = new Vec2Int[points.Length];
                Vector3[] verts = new Vector3[points.Length * 2];
                UVs = new Vector2[verts.Length];

                for (int i = 0; i < points.Length; i++) {
                    UpdatePointVertices(i, verts, false, true);
                }
                for (int i = points.Length - 1; i >= 0; i--) {
                    UpdatePointVertices(i, verts, true, false);
                }

                // rebuild tris (clockwise)
                List<int> tris = new List<int>();
                for (int i = 2; i < verts.Length; i += 2) {
                    // tri 1
                    tris.Add(i - 2);
                    tris.Add(i);
                    tris.Add(i - 1);
                    // tri 2
                    tris.Add(i);
                    tris.Add(i + 1);
                    tris.Add(i - 1);
                }

                // assign verts and tris
                this.Vertices = verts;
                this.Triangles = tris.ToArray();
            }

            MeshChangedFlag = true;
        }

        public void Set (int index, Vector2 point, float zPos) {
            Set(index, new Vector3(point.x, point.y, zPos));
        }
        public bool Set (int index, Vector3 point) {
            // returns success state

            if (PointVertexMap == null) return false;
            if (PointVertexMap.Length <= index) return false;

            lock (this) {
                CurrentPath[index] = point;

                // update mesh (only for this and neighbor verts)
                UpdatePointVertices(index, this.Vertices, true, true);
                UpdatePointVertices(index - 1, this.Vertices, false, true);
                UpdatePointVertices(index + 1, this.Vertices, false, true);
            }

            MeshChangedFlag = true;
            return true;
        }

        public void SetTexture (Texture2D tex) {
            matRef.mainTexture = tex;
        }

        // --------------------------------------------

        private void UpdatePointVertices (int index, Vector3[] vertices, bool updateUVs, bool updateVerts) {

            if (index < 0) return;
            if (vertices == null) return;

            int vertIndex = index * 2;
            if (vertIndex + 1 >= vertices.Length) return;

            // find this and neighbor points
            Vector3 currPoint = CurrentPath[index];
            Vector3 lastPoint;
            Vector3 nextPoint;
            if (index == 0) {
                nextPoint = CurrentPath[index + 1];
                lastPoint = currPoint - (nextPoint - currPoint);
            }
            else if (index + 1 == CurrentPath.Length) {
                lastPoint = CurrentPath[index - 1];
                nextPoint = currPoint + (currPoint - lastPoint);
            }
            else {
                lastPoint = CurrentPath[index - 1];
                nextPoint = CurrentPath[index + 1];
            }

            // Set UV's
            if (updateUVs) {
                float yDist;
                if (vertIndex + 2 >= vertices.Length) yDist = 0;
                else yDist = Vector2.Distance(currPoint, nextPoint) + UVs[vertIndex + 2].y;
                UVs[vertIndex] = new Vector2(0, yDist);
                UVs[vertIndex + 1] = new Vector2(1, yDist);
            }

            // Set Vertices
            if (updateVerts) {
                // get vertex positions based on last and next point
                Vector3[] currVerts = FindPointVertexPositions(currPoint, lastPoint, nextPoint);
                vertices[vertIndex] = currVerts[0];
                vertices[vertIndex + 1] = currVerts[1];
                PointVertexMap[index] = new Vec2Int(vertIndex, vertIndex + 1);
            }
        }

        private Vector3[] FindPointVertexPositions (Vector3 currPoint, Vector3 lastPoint, Vector3 nextPoint) {
            // return positions of 2 vertices for this point
            // take into account rotation (previous and next point)

            Vector3 rotVector1 = (currPoint - lastPoint).normalized;
            Vector3 rotVector2 = (nextPoint - currPoint).normalized;
            Vector3 rotOffsetVector = (rotVector1 + rotVector2).normalized;

            float thicknessMult = ((Vector3.Angle(rotVector1, rotVector2) / 180) * 0.8f) + 1;
            thicknessMult = Mathf.Pow(thicknessMult, 2);

            Vector3 p1 = currPoint + (new Vector3(-rotOffsetVector.y, rotOffsetVector.x, 0) * this.Width * 0.5f * thicknessMult);
            Vector3 p2 = currPoint + (new Vector3(rotOffsetVector.y, -rotOffsetVector.x, 0) * this.Width * 0.5f * thicknessMult);

            return new Vector3[] { p1, p2 };
        }

    }
}
