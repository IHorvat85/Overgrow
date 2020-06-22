using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utility {
    public static class Meshes {

        private const float SmoothDistance = 1.5f;
        public const float MatchDistance = 0.1f;

        public const float ExpandFactor = 0.25f;

        public static Vector3[] ExpandHillMesh (Vector3[] verts, KeyValuePair<Vector3, int[]>[] borderVerts) {

            // copy the verts array
            Vector3[] expandedVerts = new Vector3[verts.Length];
            for (int i = 0; i < verts.Length; i++) {
                expandedVerts[i] = verts[i];
            }

            // find center of borderVerts
            Vector3 center = Vector3.zero;
            for (int i = 0; i < borderVerts.Length; i++) {
                center += borderVerts[i].Key;
            }
            center /= borderVerts.Length;

            // move all borderVerts away from center
            for (int i = 0; i < borderVerts.Length; i++) {
                Vector3 newPos = borderVerts[i].Key + ((borderVerts[i].Key - center).normalized * ExpandFactor);
                for (int j = 0; j < borderVerts[i].Value.Length; j++) {
                    expandedVerts[borderVerts[i].Value[j]] = newPos;
                }
                borderVerts[i] = new KeyValuePair<Vector3, int[]>(newPos, borderVerts[i].Value);
            }

            return expandedVerts;
        }

        public static Vector3[] SmoothHillMesh (Vector3[] verts, KeyValuePair<Vector3, int[]>[] borderVerts) {
            // note: borderVerts contains indexes of verts that are on the border

            // copy the verts array
            Vector3[] smoothedVerts = new Vector3[verts.Length];
            for (int i = 0; i < verts.Length; i++) {
                smoothedVerts[i] = verts[i];
            }

            // smooth border verts by averaging other nearby border verts
            for (int i = 0; i < borderVerts.Length; i++) {
                Vector3 mainPos = borderVerts[i].Key;

                Vector3 avgPos = borderVerts[i].Key;
                int avgCount = 1;

                for (int j = 0; j < borderVerts.Length; j++) {
                    if (i == j) continue;

                    Vector3 currPos = borderVerts[j].Key;

                    if (Vector3.Distance(mainPos, currPos) < SmoothDistance) {
                        avgPos += currPos;
                        avgCount++;
                    }
                }

                avgPos /= avgCount;

                // move all matching verts to avg pos
                for (int j = 0; j < borderVerts[i].Value.Length; j++) {
                    smoothedVerts[borderVerts[i].Value[j]] = avgPos;
                }

            }
            
            return smoothedVerts;
        }

    }
}
