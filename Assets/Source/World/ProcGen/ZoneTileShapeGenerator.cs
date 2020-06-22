using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using World;

namespace World.ProcGen {
    public static class ZoneTileShapeGenerator {

        public static Mesh GenerateMeshDeductively (WorldMapTile Tile) {

            // start with an oversized quad mesh

            float initSize = 1;

            List<Vector3> verts = new List<Vector3>();
            verts.Add(new Vector3(-initSize, initSize));
            verts.Add(new Vector3(initSize, initSize));
            verts.Add(new Vector3(initSize, -initSize));
            verts.Add(new Vector3(-initSize, -initSize));

            List<int> tris = new List<int>();
            tris.Add(0);
            tris.Add(1);
            tris.Add(2);
            tris.Add(0);
            tris.Add(2);
            tris.Add(3);

            Mesh mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();

            // pull line between center and every point
            Vector2 thisPointLocal = Tile.PointPos;

            List<Vector2> otherCenters = new List<Vector2>();

            for (int i = 0; i < Tile.ConnectedTiles.Count; i++) {
                Vector2 currPointLocal = Tile.ConnectedTiles[i].PointPosWorld - new Vector2(Tile.Position.X, Tile.Position.Y);
                Vector2 currCenter = (thisPointLocal + currPointLocal) / 2f;

                otherCenters.Add(currPointLocal);

                Vector2 perpVector = (currPointLocal - thisPointLocal);
                perpVector = new Vector2(perpVector.y, -perpVector.x);

                Vector2 perpLineP1 = currCenter + (perpVector * 100);
                Vector2 perpLineP2 = currCenter - (perpVector * 100);

                // cut mesh in two along the line
                mesh = CutMesh(mesh, perpLineP1, perpLineP2, thisPointLocal, currPointLocal);

            }

            mesh = CleanupMesh(mesh, thisPointLocal, otherCenters.ToArray());

            mesh = ReduceMesh(mesh, thisPointLocal);

            mesh = GenerateUVs(mesh);

            mesh.RecalculateNormals();

            return mesh;
        }

        private static Mesh CutMesh (Mesh mesh, Vector2 p1, Vector2 p2, Vector2 center, Vector2 centerOther) {

            // p1 and p2 designate a line

            List<Vector3> oldVerts = new List<Vector3>(mesh.vertices);
            List<int> oldTris = new List<int>(mesh.triangles);

            List<Vector3> newVerts = new List<Vector3>();
            List<int> newTris = new List<int>();

            for (int i = 0; i < oldTris.Count; i += 3) {

                Vector2 a = oldVerts[oldTris[i]];
                Vector2 b = oldVerts[oldTris[i + 1]];
                Vector2 c = oldVerts[oldTris[i + 2]];

                bool abInts = false;
                bool bcInts = false;
                bool caInts = false;

                Vector2 abInt = Vector2.zero;
                Vector2 bcInt = Vector2.zero;
                Vector2 caInt = Vector2.zero;

                abInts = CheckIntersect(p1, p2, a, b, ref abInt);
                bcInts = CheckIntersect(p1, p2, b, c, ref bcInt);
                caInts = CheckIntersect(p1, p2, c, a, ref caInt);

                if (abInts && bcInts) {
                    Vector2 t1 = (b + abInt + bcInt) * 0.333f;
                    Vector2 t2 = (c + a + abInt + bcInt) * 0.25f;

                    if (Vector2.Distance(t1, center) < Vector2.Distance(t1, centerOther)) {
                        AddTriangle(newVerts, newTris, b, bcInt, abInt);
                    }
                    if (Vector2.Distance(t2, center) < Vector2.Distance(t2, centerOther)) {
                        AddTriangle(newVerts, newTris, a, abInt, c);
                        AddTriangle(newVerts, newTris, abInt, bcInt, c);
                    }
                }
                else if (bcInts && caInts) {
                    Vector2 t1 = (c + bcInt + caInt) * 0.333f;
                    Vector2 t2 = (b + a + bcInt + caInt) * 0.25f;

                    if (Vector2.Distance(t1, center) < Vector2.Distance(t1, centerOther)) {
                        AddTriangle(newVerts, newTris, c, caInt, bcInt);
                    }
                    if (Vector2.Distance(t2, center) < Vector2.Distance(t2, centerOther)) {
                        AddTriangle(newVerts, newTris, b, bcInt, a);
                        AddTriangle(newVerts, newTris, bcInt, caInt, a);
                    }
                }
                else if (caInts && abInts) {
                    Vector2 t1 = (a + abInt + caInt) * 0.333f;
                    Vector2 t2 = (b + c + caInt + abInt) * 0.25f;

                    if (Vector2.Distance(t1, center) < Vector2.Distance(t1, centerOther)) {
                        AddTriangle(newVerts, newTris, a, abInt, caInt);
                    }
                    if (Vector2.Distance(t2, center) < Vector2.Distance(t2, centerOther)) {
                        AddTriangle(newVerts, newTris, c, caInt, b);
                        AddTriangle(newVerts, newTris, caInt, abInt, b);
                    }
                }
                else {
                    AddTriangle(newVerts, newTris, a, b, c);
                }
            }

            Mesh newMesh = new Mesh();
            newMesh.vertices = newVerts.ToArray();
            newMesh.triangles = newTris.ToArray();

            return newMesh;
        }

        private static void AddTriangle (List<Vector3> verts, List<int> tris, Vector2 a, Vector2 b, Vector2 c) {
            verts.Add(a);
            verts.Add(b);
            verts.Add(c);
            int vertCount = verts.Count;
            tris.Add(vertCount - 3);
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 1);
        }

        private static bool CheckIntersect (Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection) {

            float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num/*,offset*/;
            float x1lo, x1hi, y1lo, y1hi;

            Ax = p2.x - p1.x;
            Bx = p3.x - p4.x;

            // X bound box test/
            if (Ax < 0) {
                x1lo = p2.x;
                x1hi = p1.x;
            }
            else {
                x1hi = p2.x;
                x1lo = p1.x;
            }

            if (Bx > 0) {
                if (x1hi < p4.x || p3.x < x1lo) return false;
            }
            else {
                if (x1hi < p3.x || p4.x < x1lo) return false;
            }

            Ay = p2.y - p1.y;
            By = p3.y - p4.y;

            // Y bound box test//
            if (Ay < 0) {
                y1lo = p2.y;
                y1hi = p1.y;
            }
            else {
                y1hi = p2.y;
                y1lo = p1.y;
            }

            if (By > 0) {
                if (y1hi < p4.y || p3.y < y1lo) return false;
            }
            else {
                if (y1hi < p3.y || p4.y < y1lo) return false;
            }

            Cx = p1.x - p3.x;
            Cy = p1.y - p3.y;
            d = By * Cx - Bx * Cy;  // alpha numerator//
            f = Ay * Bx - Ax * By;  // both denominator//

            // alpha tests//
            if (f > 0) {
                if (d < 0 || d > f) return false;
            }
            else {
                if (d > 0 || d < f) return false;
            }

            e = Ax * Cy - Ay * Cx;  // beta numerator//

            // beta tests //
            if (f > 0) {
                if (e < 0 || e > f) return false;
            }
            else {
                if (e > 0 || e < f) return false;
            }

            // check if they are parallel
            if (f == 0) return false;

            // compute intersection coordinates //
            num = d * Ax; // numerator //

            //    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;   // round direction //

            //    intersection.x = p1.x + (num+offset) / f;
            intersection.x = p1.x + num / f;

            num = d * Ay;
            //    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;

            //    intersection.y = p1.y + (num+offset) / f;
            intersection.y = p1.y + num / f;

            return true;
        }

        private static Mesh CleanupMesh (Mesh mesh, Vector2 center, Vector2[] centerOthers) {

            List<Vector3> newVerts = new List<Vector3>();
            List<int> newTris = new List<int>();

            List<Vector3> oldVerts = new List<Vector3>(mesh.vertices);
            List<int> oldTris = new List<int>(mesh.triangles);

            for (int i = 0; i < oldTris.Count; i += 3) {

                Vector2 a = oldVerts[oldTris[i]];
                Vector2 b = oldVerts[oldTris[i + 1]];
                Vector2 c = oldVerts[oldTris[i + 2]];

                Vector2 avg = (a + b + c) * 0.333f;

                float dCent = Vector2.Distance(avg, center);

                bool valid = true;
                for (int j = 0; j < centerOthers.Length; j++) {
                    float dCurr = Vector2.Distance(avg, centerOthers[j]);
                    if (dCurr < dCent) {
                        valid = false;
                        break;
                    }
                }

                if (valid) AddTriangle(newVerts, newTris, a, b, c);
            }

            Mesh newMesh = new Mesh();
            newMesh.vertices = newVerts.ToArray();
            newMesh.triangles = newTris.ToArray();

            return newMesh;
        }

        private static Mesh ReduceMesh (Mesh mesh, Vector3 center) {

            List<Vector3> oldVerts = new List<Vector3>(mesh.vertices);
            Vector3[] newVerts = new Vector3[oldVerts.Count];

            for (int i = 0; i < oldVerts.Count; i++) {
                float d = 1 / Vector2.Distance(oldVerts[i], center);
                if (d < 0.7f) d = 0.7f;
                if (d > 0.9f) d = 0.9f;
                newVerts[i] = oldVerts[i] * d;
            }

            Mesh newMesh = new Mesh();
            newMesh.vertices = newVerts;
            newMesh.triangles = mesh.triangles;

            return newMesh;
        }

        private static Mesh GenerateUVs (Mesh mesh) {
            Vector3[] verts = mesh.vertices;
            Vector2[] uvs = new Vector2[verts.Length];

            Vector3 avg = Vector3.zero;
            for (int i = 0; i < verts.Length; i++) {
                avg += verts[i];
            }
            avg /= verts.Length;

            for (int i = 0; i < verts.Length; i++) {
                uvs[i] = ((verts[i] - avg).normalized + new Vector3(1, 1)) * 0.5f;
            }
            mesh.uv = uvs;
            return mesh;
        }

        public static Vector2 RepositionCenterPoint (WorldMapTile Tile, Mesh mesh) {
            Vector3[] verts = mesh.vertices;
            Vector2 sum = Vector3.zero;
            for (int i = 0; i < verts.Length; i++) {
                sum += (Vector2)verts[i];
            }
            sum /= verts.Length;
            return (Tile.PointPos + sum) / 2f;
        }

    }
}
