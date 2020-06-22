using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utility {
    public static class Pathfinding {

        private static float[,] PathPassable;
        // private static float[,] PathCostObstacles;

        private static Vec2Int ZoneSize;

        public static void RebuildPathing () {
            
            ZoneSize = World.ZoneMapGenerator.ZoneSize;
            PathPassable = new float[ZoneSize.X, ZoneSize.Y];

            // build path based on terrain (only fixed terrain features)
            float[,] hillDistanceMap = World.ZoneMapGenerator.HillDistanceMap;
            for (int x = 0; x < ZoneSize.X; x++) {
                for (int y = 0; y < ZoneSize.Y; y++) {
                    PathPassable[x, y] = hillDistanceMap[x, y];
                }
            }
        }
        public static void RebuildObstacleMap () {
            // PathCostObstacles = new float[World.ZoneMapGenerator.ZoneSize.X, World.ZoneMapGenerator.ZoneSize.Y];

            // todo: loop thru every dynamic object on the map

            // based on position and size/mass, increment PathCostObstacles on that tile

        }

        // ------------------------------------------

        public static List<Vector2> GetPath (Vector2 from, Vector2 to, float proximityLimit, bool ignoreObstacles) {
            
            // todo: include obstacle cost

            Vec2Int fromPoint = CorrectPos(from);
            Vec2Int toPoint = CorrectPos(to);

            Vec2Int[,] map = new Vec2Int[ZoneSize.X, ZoneSize.Y];
            for (int x = 0; x < ZoneSize.X; x++) {
                for (int y = 0; y < ZoneSize.Y; y++) {
                    map[x, y] = Vec2Int.Invalid;
                }
            }

            List<KeyValuePair<Vec2Int, float>> ptsToCheck = new List<KeyValuePair<Vec2Int, float>>();

            ptsToCheck.Add(new KeyValuePair<Vec2Int, float>(fromPoint, Vec2Int.DistanceManhattan(fromPoint, toPoint)));
            map[fromPoint.X, fromPoint.Y] = fromPoint;

            FindPath(map, ptsToCheck, toPoint, proximityLimit);
            List<Vec2Int> path = ConstructPath(map, toPoint, fromPoint);
            path.Reverse();

            // optimize with fake raycast
            List<Vec2Int> optPath = OptimizePath(path, proximityLimit);

            // convert to vector2
            List<Vector2> pth = new List<Vector2>();
            pth.Add(from);
            Vector2 offset = new Vector2(ZoneSize.X / 2f, ZoneSize.Y / 2f) - new Vector2(0.5f, 0.5f);
            for (int i = 0; i < optPath.Count; i++) {
                pth.Add(new Vector2(optPath[i].X, optPath[i].Y) - offset);
            }

            return pth;
        }

        // ------------------------------------------

        private static Vec2Int CorrectPos (Vector2 pos) {
            Vec2Int zoneSize = World.ZoneMapGenerator.ZoneSize;

            pos -= new Vector2(0.5f, 0.5f);

            int x = Mathf.RoundToInt(pos.x) + zoneSize.X / 2;
            int y = Mathf.RoundToInt(pos.y) + zoneSize.Y / 2;

            if (x < 0) x = 0;
            if (x >= zoneSize.X) x = zoneSize.X;
            if (y < 0) y = 0;
            if (y >= zoneSize.Y) y = zoneSize.Y;

            return new Vec2Int(x, y);
        }

        private static void FindPath (Vec2Int[,] map, List<KeyValuePair<Vec2Int, float>> ptsToCheck, Vec2Int end, float proxLimit) {

            // sort ptsToCheck by distance from end
            ptsToCheck.Sort((x, y) => x.Value.CompareTo(y.Value));

            if (ptsToCheck.Count == 0) {
                // ran out of points - no path found
                Debug.Log("RAN OUT OF POINTS!");
                return;
            }

            Vec2Int closest = ptsToCheck.First().Key;
            ptsToCheck.RemoveAt(0);

            // check if done
            if (closest == end) return;

            // add nearby points and repeat
            AddNeighborPoints(map, closest, ptsToCheck, end, proxLimit);
            FindPath(map, ptsToCheck, end, proxLimit);

        }
        private static void AddNeighborPoints (Vec2Int[,] map, Vec2Int pt,
            List<KeyValuePair<Vec2Int, float>> ptsToCheck, Vec2Int end, float proxLimit) {

            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    Vec2Int curr = pt + new Vec2Int(x, y);

                    if (map[curr.X, curr.Y] != Vec2Int.Invalid) continue;
                    if (!CheckWithinBounds(curr)) continue;

                    float cost = GetCost(curr, proxLimit);

                    float dist = Vec2Int.DistanceManhattan(curr, end);
                    ptsToCheck.Add(new KeyValuePair<Vec2Int, float>(curr, dist * cost));
                    map[curr.X, curr.Y] = pt;
                }
            }
        }

        private static bool CheckWithinBounds (Vec2Int pt) {
            if (pt.X < 0) return false;
            if (pt.Y < 0) return false;
            if (pt.X >= ZoneSize.X) return false;
            if (pt.Y >= ZoneSize.Y) return false;
            return true;
        }
        private static bool CheckProximityLimit (Vec2Int pt, float proxLimit) {
            if (proxLimit > PathPassable[pt.X, pt.Y]) return false;
            return true;
        }
        
        private static float GetCost (Vec2Int pt, float proxLimit) {
            float baseCost = PathPassable[pt.X, pt.Y];
            if (baseCost < proxLimit) return proxLimit - baseCost + 1;
            else return 1;
        }
        private static float GetClosestPointDist (Vec2Int pt, List<Vec2Int> path) {
            if (path == null) return 0;
            if (path.Count == 0) return 0;

            List<KeyValuePair<Vec2Int, float>> distances = new List<KeyValuePair<Vec2Int, float>>();
            for (int i = 0; i < path.Count; i++) {
                float d = Vec2Int.DistanceManhattan(pt, path[i]);
                distances.Add(new KeyValuePair<Vec2Int, float>(path[i], d));
            }

            // sort by distance from pt
            distances.Sort((x, y) => x.Value.CompareTo(y.Value));

            return distances.First().Value;
        }

        private static List<Vec2Int> OptimizePath (List<Vec2Int> path, float proxLimit) {
            if (path == null) return null;
            if (path.Count <= 3) return path;

            List<Vec2Int> optPath = new List<Vec2Int>(path.ToArray());

            for (int i = 0; i < optPath.Count - 2; i++) {
                Vec2Int from = optPath[i];
                for (int j = i + 1; j < optPath.Count - 1; j++) {
                    if (!CheckFakeRaycast(from, optPath[j], proxLimit)) break;
                    optPath.RemoveAt(j);
                    j--;
                }
            }
            return optPath;
        }
        private static bool CheckFakeRaycast (Vec2Int from, Vec2Int to, float proxLimit) {

            Vector2 currPos = new Vector2(from.X, from.Y);
            Vector2 targetPos = new Vector2(to.X, to.Y);
            Vec2Int moveVecInt = to - from;
            Vector2 moveVec = new Vector2(moveVecInt.X, moveVecInt.Y).normalized;
            
            float d = Vec2Int.Distance(from, to);
            while (d > 2) {

                // convert currPos to Vec2Int
                from = new Vec2Int((int)currPos.x, (int)currPos.y);
                if (!CheckWithinBounds(from)) return false;

                // check if currPos is passable (proxlimit)
                if (!CheckProximityLimit(from, proxLimit)) return false;

                // recalculate moveVec
                moveVecInt = to - from;
                moveVec = new Vector2(moveVecInt.X, moveVecInt.Y).normalized;

                // move currPos by moveVec
                currPos += moveVec;

                // recalculate d
                d = Vector2.Distance(currPos, targetPos);
            }

            return true;
        }

        private static List<Vec2Int> ConstructPath (Vec2Int[,] map, Vec2Int end, Vec2Int start) {

            List<Vec2Int> path = new List<Vec2Int>();
            path.Add(end);

            Vec2Int lastPoint = end;
            while (true) {
                Vec2Int nextPoint = map[lastPoint.X, lastPoint.Y];
                if (nextPoint == Vec2Int.Zero) break;
                path.Add(nextPoint);
                lastPoint = nextPoint;
                if (nextPoint == start) break;
            }

            return path;
        }
    }
}
