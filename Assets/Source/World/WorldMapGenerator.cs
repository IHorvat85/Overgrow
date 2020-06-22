using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utility;

namespace World {
    public static class WorldMapGenerator {

        public const int GridSize = 21; // Must be ODD number!!
        public const int HalfGrid = GridSize / 2;

        public static int WorldSeed = 0;

        private static Dictionary<Vec2Int, WorldMapTile> Tiles;

        private const int MaxGenLevel = 3;

        private const int PointCount = 3;
        private const float PointProximityMin = 0.7f;

        private const int BackgroundGraphicSize = 32;
        private const float PointActiveWaterLevel = 0.4f;
        private const float PointActiveHillLevel = 0.7f;

        public static bool ThreadIdle = false;

        // ----------------------------------------

        private class PriorityTileComparer : IEqualityComparer<KeyValuePair<int, Vec2Int>>, IComparer<KeyValuePair<int, Vec2Int>> {
            public bool Equals (KeyValuePair<int, Vec2Int> x, KeyValuePair<int, Vec2Int> y) {
                return x.Value == y.Value;
            }
            public int GetHashCode (KeyValuePair<int, Vec2Int> obj) {
                return obj.Value.GetHashCode();
            }
            public int Compare (KeyValuePair<int, Vec2Int> x, KeyValuePair<int, Vec2Int> y) {
                return x.Key.CompareTo(y.Key);
            }
        }

        // ----------------------------------------

        private static TSList<KeyValuePair<int, Vec2Int>> TilesToGenerate;
        private static TSQueue<Vec2Int> TilesToUpdate;

        private static Thread GenThread = null;

        public static void StartGenThread () {
            StopGenThread();
            GenThread = new Thread(new ThreadStart(GenLoop));
            GenThread.Start();
        }
        public static void StopGenThread () {
            if (GenThread == null) return;
            try {
                GenThread.Abort();
            }
            catch (Exception) { }
        }

        private static void GenLoop () {
            while (true) {
                try {
                    while (TilesToGenerate.Count > 0) {
                        Vec2Int tileToGen;
                        lock (TilesToGenerate) {
                            TilesToGenerate.Sort(new PriorityTileComparer());
                            tileToGen = TilesToGenerate[0].Value;
                            TilesToGenerate.RemoveAt(0);
                        }

                        if (CheckGenerationLevel(tileToGen.X, tileToGen.Y)) {
                            lock (TilesToUpdate) {
                                if (!TilesToUpdate.Contains(tileToGen)) {
                                    TilesToUpdate.Enqueue(tileToGen);
                                }
                            }
                        }
                    }
                }
                catch (ThreadAbortException) {
                    // this is normal, end the thread
                    return;
                }
                catch (Exception e) {
                    // report error and move on
                    Utility.ErrorLog.ReportError("Error in WorldMapGenerator thread!", e);
                }

                ThreadIdle = true;
                Thread.Sleep(10);
                ThreadIdle = false;
            }
        }

        // ----------------------------------------

        public static void Initialize () {
            int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            Initialize(seed);
        }
        public static void Initialize (int seed) {
            WorldSeed = seed;
            Tiles = new Dictionary<Vec2Int, WorldMapTile>();

            TilesToUpdate = new TSQueue<Vec2Int>();
            TilesToGenerate = new TSList<KeyValuePair<int, Vec2Int>>();
        }

        public static void UpdateGridTiles () {
            Vec2Int playerPos = WorldMapController.PlayerPos;

            // initialize tiles that are null
            for (int x = -HalfGrid; x <= HalfGrid; x++) {
                for (int y = -HalfGrid; y <= HalfGrid; y++) {
                    Vec2Int currPos = playerPos + new Vec2Int(x, y);
                    if (GetTile(currPos.X, currPos.Y) == null) {
                        // generate new grid
                        WorldMapTile tile = new WorldMapTile(currPos, WorldSeed);
                        Tiles.Add(currPos, tile);
                    }
                }
            }

            // update generation level
            for (int x = -HalfGrid; x <= HalfGrid; x++) {
                for (int y = -HalfGrid; y <= HalfGrid; y++) {
                    Vec2Int currPos = playerPos + new Vec2Int(x, y);

                    lock (TilesToUpdate) {
                        if (TilesToUpdate.Contains(currPos)) continue;
                    }

                    int genLevel = GetMinGenerationLevel(currPos.X, currPos.Y, playerPos);

                    WorldMapTile currTile = GetTile(currPos.X, currPos.Y);
                    currTile.GeneratedLevelTarget = genLevel;
                    if (currTile.GeneratedLevelCurr >= currTile.GeneratedLevelTarget) continue;

                    lock (TilesToGenerate) {
                        if (TilesToGenerate.Contains(new KeyValuePair<int, Vec2Int>(0, currPos), new PriorityTileComparer())) continue;
                        TilesToGenerate.Add(new KeyValuePair<int, Vec2Int>(currTile.GeneratedLevelCurr, currPos));
                        ThreadIdle = false;
                    }
                }
            }
        }

        public static Vec2Int[] UpdateGeneratedTiles () {
            // note: called by main thread every frame

            List<Vec2Int> generatedTiles = new List<Vec2Int>();
            lock (TilesToUpdate) {
                while (TilesToUpdate.Count() > 0) {
                    Vec2Int tileToUpdate = TilesToUpdate.Dequeue();
                    generatedTiles.Add(tileToUpdate);
                }
            }
            return generatedTiles.ToArray();
        }

        // ----------------------------------------

        private static int GetMinGenerationLevel (int posX, int posY, Vec2Int playerPos) {
            int xDist = HalfGrid - Math.Abs(posX - playerPos.X);
            int yDist = HalfGrid - Math.Abs(posY - playerPos.Y);
            int minVal = xDist < yDist ? xDist : yDist;
            if (minVal > MaxGenLevel) minVal = MaxGenLevel;
            return minVal;
        }

        // ----------------------------------------

        public static WorldMapTile GetTile (int x, int y) {
            if (Tiles == null) return null;
            WorldMapTile tile = null;
            Tiles.TryGetValue(new Vec2Int(x, y), out tile);
            return tile;
        }

        // ----------------------------------------

        private static bool CheckGenerationLevel (int x, int y) {
            // note: returns whether changes were made

            WorldMapTile currTile = GetTile(x, y);
            if (currTile == null) return false;
            if (currTile.GeneratedLevelCurr >= currTile.GeneratedLevelTarget) return false;

            // generate to currLevel+1
            // note: do not generate to target immediately

            switch (currTile.GeneratedLevelCurr) {
                case -1:
                    Generate0(x, y);
                    break;
                case 0:
                    Generate1(x, y);
                    break;
                case 1:
                    Generate2(x, y);
                    break;
                case 2:
                    Generate3(x, y);
                    break;
                default:
                    return false;
            }

            return true;
        }

        private static void Generate0 (int x, int y) {
            WorldMapTile currTile = GetTile(x, y);
            // add N random points

            currTile.PointPos = new Vector2(currTile.GetRand() - 0.5f, currTile.GetRand() - 0.5f) * 0.5f;
            currTile.PointPosWorld = currTile.PointPos + new Vector2(x, y);
            currTile.PointStrength = currTile.GetRand();
            currTile.PointActive = true;

            currTile.GeneratedLevelCurr = 0;
        }

        private static void Generate1 (int posX, int posY) {
            WorldMapTile currTile = GetTile(posX, posY);
            // compare points to nearby ones, disable if too close

            float[] noiseMap = GenerateZoneTileBackgroundNoise(posX, posY);

            float minHeight = float.MaxValue;
            float avgHeight = 0;
            for (int i = 0; i < noiseMap.Length; i++) {
                if (noiseMap[i] < minHeight) minHeight = noiseMap[i];
                avgHeight += noiseMap[i];
            }
            avgHeight /= noiseMap.Length;

            currTile.AssignBackgroundPixels(
                GenerateZoneTileBackground(noiseMap, posX, posY),
                GenerateZoneTileMask(noiseMap, posX, posY));

            float currValue = ((minHeight + avgHeight * 0.1f) * 0.9f * 0.7f) + 0.3f;
            currTile.PointActive = true; // currTile.PointActive ? currValue > PointActiveWaterLevel : false;
            currTile.GeneratedLevelCurr = 1;
        }
        private static float[] GenerateZoneTileBackgroundNoise (int posX, int posY) {

            int graphicSize = BackgroundGraphicSize;
            int pixelCount = graphicSize * graphicSize;
            
            // initialize noise generator
            LibNoise.RidgedMultifractal gen = new LibNoise.RidgedMultifractal();
            gen.Seed = WorldSeed;
            gen.Frequency = 0.5f; // 0.2f;
            gen.Lacunarity = 2f;
            gen.NoiseQuality = LibNoise.NoiseQuality.Standard;
            gen.OctaveCount = 5;

            // generate noise map
            float[] map = new float[pixelCount];
            for (int i = 0; i < pixelCount; i++) {
                float x = (float)(i % graphicSize) / graphicSize;
                float y = (float)(i / graphicSize) / graphicSize;
                map[i] = ((float)gen.GetValue(x + posX, y + posY, 1) + 1) / 2f;
                map[i] = Utility.TransformFunctions.SmoothStart(map[i], 2);
            }

            return map;
        }
        private static Color[] GenerateZoneTileBackground (float[] noiseMap, int posX, int posY) {

            int graphicSize = 32;
            int pixelCount = graphicSize * graphicSize;

            // convert to colors
            Color32 dirtColor = new Color32(255, 185, 90, 255);
            Color32 waterColor = new Color32(230, 35, 35, 255);
            Color32 hillColor = new Color32(220, 200, 150, 255);

            Color[] colorArray = new Color[pixelCount];
            for (int i = 0; i < noiseMap.Length; i++) {

                float currValue = noiseMap[i];
                currValue = (currValue * 0.7f) + 0.3f;

                float r, g, b;
                if (currValue < PointActiveWaterLevel) {
                    r = currValue * 0.7f;
                    g = GetColorSq(currValue, 1.35f) * 0.7f;
                    b = GetColorSq(currValue, 1.35f) * 0.7f;
                    float lerpT = currValue / PointActiveWaterLevel;
                    Color currColor = Color.Lerp(waterColor, dirtColor, lerpT);
                    colorArray[i] = new Color(r, g, b, 1) * currColor;
                }
                else if (currValue < PointActiveHillLevel) {
                    r = currValue * 0.7f;
                    g = GetColorSq(currValue, 1.35f) * 0.7f;
                    b = GetColorSq(currValue, 1.35f) * 0.7f;
                    colorArray[i] = new Color(r, g, b, 1) * dirtColor;
                }
                else {
                    r = currValue * 0.7f;
                    g = GetColorSq(currValue, 1.35f) * 0.7f;
                    b = GetColorSq(currValue, 1.35f) * 0.7f;
                    float lerpT = currValue - PointActiveHillLevel;
                    Color currColor = Color.Lerp(dirtColor, hillColor, lerpT);
                    colorArray[i] = new Color(r, g, b, 1) * currColor;
                }
            }

            return colorArray;
        }
        private static Color[] GenerateZoneTileMask (float[] noiseMap, int posX, int posY) {

            int graphicSize = 32;
            int pixelCount = graphicSize * graphicSize;

            // convert to colors
            Color colorValid = new Color(1, 1, 1, 1);
            Color colorInvalid = new Color(1, 1, 1, 0);

            Color[] colorArray = new Color[pixelCount];
            for (int i = 0; i < noiseMap.Length; i++) {

                float currValue = noiseMap[i];
                currValue = (currValue * 0.7f) + 0.3f;

                if (currValue < PointActiveWaterLevel) {
                    colorArray[i] = colorInvalid;
                }
                else {
                    colorArray[i] = colorValid;
                }
            }

            return colorArray;
        }
        private static float GetColorSq (float val, float exp) {
            return Mathf.Pow(val, exp);
        }

        private static void Generate2 (int posX, int posY) {
            WorldMapTile currTile = GetTile(posX, posY);

            if (!currTile.PointActive) {
                currTile.GeneratedLevelCurr = 2;
                return;
            }

            List<WorldMapTile> tempList = new List<WorldMapTile>();

            if (!AddConnectedTile(currTile, -1, 1, tempList)) return;
            if(!AddConnectedTile(currTile, 0, 1, tempList)) return;
            if(!AddConnectedTile(currTile, 1, 1, tempList)) return;
            if(!AddConnectedTile(currTile, 1, 0, tempList)) return;
            if(!AddConnectedTile(currTile, 1, -1, tempList)) return;
            if(!AddConnectedTile(currTile, 0, -1, tempList)) return;
            if(!AddConnectedTile(currTile, -1, -1, tempList)) return;
            if (!AddConnectedTile(currTile, -1, 0, tempList)) return;

            for (int i = 0; i < tempList.Count; i++) {
                currTile.ConnectedTiles.Add(tempList[i]);
            }

            currTile.GeneratedLevelCurr = 2;
        }
        private static bool AddConnectedTile (WorldMapTile currTile, int offX, int offY, List<WorldMapTile> list) {
            if (offX == 0 && offY == 0) return true;

            offX += currTile.Position.X;
            offY += currTile.Position.Y;

            WorldMapTile offTile = GetTile(offX, offY);
            if (offTile == null) return false;

            list.Add(offTile);
            return true;
        }

        private static void Generate3 (int posX, int posY) {
            WorldMapTile currTile = GetTile(posX, posY);

            if (currTile.controller == null) return;
            if (!currTile.controller.meshGenerated) return;

            if (!currTile.PointActive) {
                currTile.GeneratedLevelCurr = 3;
                return;
            }

            List<WorldMapTileController> contList = new List<WorldMapTileController>();

            for (int i = 0; i < currTile.ConnectedTiles.Count; i++) {
                WorldMapTile conTile = currTile.ConnectedTiles[i];

                if (!conTile.PointActive) continue;
                if (conTile.controller == null) return;
                if (!conTile.controller.meshGenerated) return;
                if (conTile.controller.ShapeMeshVerts == null) return;

                if (TestNeighboringTiles(currTile.controller, conTile.controller)) contList.Add(conTile.controller);
            }

            currTile.controller.TouchingNeighbors = contList;
            currTile.GeneratedLevelCurr = 3;
        }
        private static bool TestNeighboringTiles (WorldMapTileController cont1, WorldMapTileController cont2) {

            Vector3[] verts1 = cont1.ShapeMeshVerts;
            Vector3[] verts2 = cont2.ShapeMeshVerts;

            Vector3 offset = (Vector3)(Vector2)(cont2.Tile.Position - cont1.Tile.Position);

            float distThreshold = 0.25f;
            for (int i = 0; i < verts1.Length; i++) {
                for (int j = 0; j < verts2.Length; j++) {
                    float d = Vector2.Distance(verts1[i], verts2[j] + offset);
                    if (d < distThreshold) return true;
                }
            }

            return false;
        }
    }
}
