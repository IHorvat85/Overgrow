using System;
using UnityEngine;
using System.Collections.Generic;

namespace World {
    public static class ZoneMapGenerator {

        public static readonly Utility.Vec2Int ZoneSize = new Utility.Vec2Int(256, 256);
        public static readonly Utility.Vec2Int ZoneTextureSize = new Utility.Vec2Int(512, 512);
        // public static readonly Utility.Vec2Int ZoneSize = new Utility.Vec2Int(350, 350);
        // public static readonly Utility.Vec2Int ZoneTextureSize = new Utility.Vec2Int(700, 700);
        
        public static readonly int MapEdgeSize = 15;

        public enum TileType {
            Ground,
            Hill,
            Water,
        }

        // --------------------------------------

        public struct HillChunk {

            public Vector3[] vertices;
            public int[] triangles;
            public Color[,] texture;
            public Utility.Vec2Int textureSize;
            public Vector2 textureScale;
            public Vector2 textureOffset;

            public HillChunk (Vector3[] vertices, int[] triangles, Color[,] texture, Utility.Vec2Int texSize, Vector2 texScale, Vector2 texOffset) {
                this.vertices = vertices;
                this.triangles = triangles;
                this.texture = texture;
                this.textureSize = texSize;
                this.textureScale = texScale;
                this.textureOffset = texOffset;
            }
        }

        private class DistanceData {

            public int DistLeft;
            public int DistRight;
            public int DistUp;
            public int DistDown;

            public float Closest;

            public Vector3 closestHill;

            public DistanceData () {
                // ?
            }

            public DistanceData Clone () {
                DistanceData d = new DistanceData();
                d.DistLeft = this.DistLeft;
                d.DistRight = this.DistRight;
                d.DistDown = this.DistDown;
                d.DistUp = this.DistUp;
                d.closestHill = this.closestHill;
                return d;
            }

            public void AssignClosest () {
                Closest = DistLeft;
                if (DistRight < Closest) Closest = DistRight;
                if (DistUp < Closest) Closest = DistUp;
                if (DistDown < Closest) Closest = DistDown;
            }
            public void AssignClosest (float d) {
                if (d < Closest) Closest = d;
            }

            public int GetClosestHor () {
                return DistLeft < DistRight ? DistLeft : DistRight;
            }
            public int GetClosestVer () {
                return DistUp < DistDown ? DistUp : DistDown;
            }

            public override string ToString () {
                return DistLeft + " : " + DistRight + " : " + DistUp + " : " + DistDown;
            }
        }

        // --------------------------------------

        public static readonly int HillChunkPixelFactor = 2;

        // --------------------------------------

        public static TileType[,] TileMap;
        public static Color[,] ColorMap;
        public static Color[,] WaterMap;
        public static HillChunk[] HillChunks;
        public static Vector2[] VehicleSpawnPoints;
        public static float[,] HillDistanceMap;
        public static Color[,] EdgeIndicatorMap;
        public static Color[,] Minimap;

        public static Color[,] PathfindMap;

        // --------------------------------------

        private static Color32 dirtColor = new Color32(255, 185, 90, 255);
        private static Color waterColor = new Color32(76, 108, 116, 255);

        private static Color32 hillMinColor = new Color32(32, 16, 0, 255);
        private static Color32 hillMaxColor = new Color32(196, 196, 196, 255);

        // --------------------------------------

        private static float[] ProgressTrackers;

        // --------------------------------------

        public static void GenerateMap (Zone zone) {
            // note: this will be called in a secondary thread
            // meaning this cannot instantiate objects
            // note: zone type and seed are important in generation

            ZoneMapController.GenerationDoneFlag = false;
            ZoneMapController.AwaitingGenerationFlag = true;
            ProgressTrackers = new float[11];

            // generate heightmap
            float[,] heightmap = GenerateHeightMap(zone, 0);
            byte[,] heightmapGrouped = GroupHeightmap(heightmap, 1);

            // generate tileMap (hills, water ..)
            TileMap = GenerateTilemap(heightmapGrouped, 2);
            TileMap = PostProcessTilemap(TileMap, zone.Seed, 3);

            // generate hill chunks and their textures
            HillChunks = GenerateHillChunks(TileMap, 4);

            // generate terrain textures
            WaterMap = GenerateWatermap(TileMap, heightmap, 5);
            ColorMap = GenerateTerrainColor(heightmap, TileMap, zone.Seed, 6);

            // hill distance map for object spawning
            HillDistanceMap = GenerateHillDistanceMap(TileMap, 7);

            // vehicle spawn points
            VehicleSpawnPoints = GenerateVehicleSpawnsPoints(HillDistanceMap, 8);
            // north, south, west, east

            // Map edge indicator (leave zone)
            EdgeIndicatorMap = GenerateMapEdgeIndicator(9);

            // Minimap
            Minimap = GenerateMinimap(10);

            // todo: objects (structures, resources, enemies..)

            // notify zoneController to pull data from here and update everything
            ZoneMapController.GenerationDoneFlag = true;
        }

        private static Color[,] DrawPathfindMap (float[,] HillDistanceMap) {

            Color[,] map = new Color[ZoneSize.X, ZoneSize.Y];
            for (int x = 0; x < ZoneSize.X; x++) {
                for (int y = 0; y < ZoneSize.Y; y++) {
                    float val = HillDistanceMap[x, y];

                    if (val < 2.5f) map[x, y] = new Color(1, 0, 0, 1);
                    else map[x, y] = new Color(0, 1, 0, 1);
                }
            }

            return map;
        }

        private static float[,] GenerateHeightMap (Zone zone, int progressIndex) {
            if (zone == null) return null;

            // initialize noise generator
            LibNoise.RidgedMultifractal generator = new LibNoise.RidgedMultifractal();
            generator.Seed = zone.Seed;

            // set values
            float maxValue = SetGeneratorProperties(generator);

            // generate heightmap
            float[,] height = new float[ZoneSize.X, ZoneSize.Y];
            for (int x = 0; x < ZoneSize.X; x++) {
                for (int y = 0; y < ZoneSize.Y; y++) {
                    height[x, y] = (float)(generator.GetValue(x, y, 0) + 1) / maxValue;
                }

                UpdateLoadProgress(progressIndex, x + 1, ZoneSize.X);
            }

            return height;
        }
        private static float SetGeneratorProperties (LibNoise.RidgedMultifractal gen) {
            // note: returns max value of generator, depending on lacunarity

            // switch/case, different settings for different zone types
            gen.Frequency = 0.03;
            gen.Lacunarity = 2f;
            gen.NoiseQuality = LibNoise.NoiseQuality.Standard;
            gen.OctaveCount = 5;
            return 2.4219f;
        }

        private static byte[,] GroupHeightmap (float[,] heightmap, int progressIndex) {

            byte[,] heightmapGrouped = new byte[ZoneSize.X, ZoneSize.Y];
            float[] groups = GetHeightmapGroups();
            for (int x = 0; x < ZoneSize.X; x++) {
                for (int y = 0; y < ZoneSize.Y; y++) {
                    for (int i = 0; i < groups.Length; i++) {
                        if (heightmap[x, y] < groups[i]) {
                            heightmapGrouped[x, y] = (byte)i;
                            break;
                        }
                    }
                }

                UpdateLoadProgress(progressIndex, x + 1, ZoneSize.X);
            }

            return heightmapGrouped;
        }
        private static float[] GetHeightmapGroups () {
            // if needed, add different values for different zone types
            return new float[] { 0.2f, 0.4f, 0.6f, 0.8f, float.MaxValue };
        }

        private static TileType[,] GenerateTilemap (byte[,] heightmapGrouped, int progressIndex) {
            TileType[,] tileMap = new TileType[ZoneSize.X, ZoneSize.Y];

            for (int x = 0; x < ZoneSize.X; x++) {
                for (int y = 0; y < ZoneSize.Y; y++) {
                    tileMap[x, y] = SelectTileType(heightmapGrouped[x, y]);
                }

                UpdateLoadProgress(progressIndex, x + 1, ZoneSize.X);
            }

            return tileMap;
        }
        private static TileType SelectTileType (byte group) {

            // todo: depending on zoneType, decide what this tile is

            if (group == 0) return TileType.Hill;

            return TileType.Ground;
        }

        private static TileType[,] PostProcessTilemap (TileType[,] tilemap, int seed, int progressIndex) {

            // smooth based on neighbors
            for (int x = 0; x < ZoneSize.X; x++) {
                for (int y = 0; y < ZoneSize.Y; y++) {
                    int neighbors = CountNeighboringHills(tilemap, x, y);
                    if (tilemap[x, y] == TileType.Hill && neighbors <= 1) tilemap[x, y] = TileType.Ground;
                    if (tilemap[x, y] == TileType.Ground && neighbors >= 3) tilemap[x, y] = TileType.Hill;
                }
                UpdateLoadProgress(progressIndex, x + 1, ZoneSize.X);
            }

            // swap random hills for lakes etc?
            System.Random rand = new System.Random(seed);
            for (int i = 0; i < 5; i++) {
                int x = rand.Next(0, ZoneSize.X);
                int y = rand.Next(0, ZoneSize.Y);
                if (tilemap[x, y] == TileType.Hill) ConvertContactTilesRecursively(tilemap, x, y, TileType.Hill, TileType.Water);
            }

            return tilemap;
        }
        private static void ConvertContactTilesRecursively (TileType[,] tilemap, int x, int y, TileType from, TileType to) {
            tilemap[x, y] = to;
            for (int xOffset = -1; xOffset < 2; xOffset++) {
                for (int yOffset = -1; yOffset < 2; yOffset++) {
                    int newX = x + xOffset;
                    int newY = y + yOffset;
                    if (CheckTileWithinBounds(newX, newY)) {
                        if (tilemap[newX, newY] == from) ConvertContactTilesRecursively(tilemap, newX, newY, from, to);
                    }
                }
            }
        }
        private static bool CheckTileWithinBounds (int x, int y) {
            if (x < 0) return false;
            if (y < 0) return false;
            if (x >= ZoneSize.X) return false;
            if (y >= ZoneSize.Y) return false;
            return true;
        }
        private static int CountNeighboringHills (TileType[,] tilemap, int x, int y) {
            int cnt = 0;
            if (CheckHillExists(tilemap, x - 1, y, true)) cnt++;
            if (CheckHillExists(tilemap, x + 1, y, true)) cnt++;
            if (CheckHillExists(tilemap, x, y - 1, true)) cnt++;
            if (CheckHillExists(tilemap, x, y + 1, true)) cnt++;
            return cnt;
        }

        private static HillChunk[] GenerateHillChunks (TileType[,] tileMap, int progressIndex) {

            bool[,] usedMap = new bool[ZoneSize.X, ZoneSize.Y];

            List<HillChunk> hcList = new List<HillChunk>();

            for (int x = 0; x < ZoneSize.X; x++) {
                for (int y = 0; y < ZoneSize.Y; y++) {
                    if (tileMap[x, y] == TileType.Hill && !usedMap[x, y]) {
                        HillChunk hc = GenerateHillChunk(tileMap, usedMap, x, y);
                        if (hc.vertices == null) continue;
                        hcList.Add(hc);
                    }
                }

                UpdateLoadProgress(progressIndex, x + 1, ZoneSize.X);
            }

            return hcList.ToArray();
        }
        private static HillChunk GenerateHillChunk (TileType[,] tileMap, bool[,] usedMap, int startX, int startY) {

            List<Utility.Vec2Int> chunkPoints = new List<Utility.Vec2Int>();

            // find points that belong to this chunk (recursive dijkstra)
            PopulateHillChunkRecursively(chunkPoints, tileMap, usedMap, startX, startY);

            // ignore if too small
            if (chunkPoints.Count < 10) {
                for (int i = 0; i < chunkPoints.Count; i++) {
                    tileMap[chunkPoints[i].X, chunkPoints[i].Y] = TileType.Ground;
                }
                return new HillChunk(null, null, null, new Utility.Vec2Int(0, 0), Vector2.zero, Vector2.zero);
            }

            // generate mesh
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            for (int i = 0; i < chunkPoints.Count; i++) {
                verts.Add(new Vector3(chunkPoints[i].X - 0.5f, chunkPoints[i].Y - 0.5f, 0));
                verts.Add(new Vector3(chunkPoints[i].X + 0.5f, chunkPoints[i].Y + 0.5f, 0));
                verts.Add(new Vector3(chunkPoints[i].X + 0.5f, chunkPoints[i].Y - 0.5f, 0));

                verts.Add(new Vector3(chunkPoints[i].X - 0.5f, chunkPoints[i].Y - 0.5f, 0));
                verts.Add(new Vector3(chunkPoints[i].X - 0.5f, chunkPoints[i].Y + 0.5f, 0));
                verts.Add(new Vector3(chunkPoints[i].X + 0.5f, chunkPoints[i].Y + 0.5f, 0));
            }
            for (int i = 0; i < verts.Count; i += 3) {
                tris.Add(i);
                tris.Add(i + 1);
                tris.Add(i + 2);
            }

            // get data about border vertices
            Vector3[] borderVertPositions = GetBorderVerts(chunkPoints, tileMap);
            KeyValuePair<Vector3, int[]>[] borderVerts = new KeyValuePair<Vector3, int[]>[borderVertPositions.Length];
            float vertexMatchDistance = Utility.Meshes.MatchDistance;
            for (int i = 0; i < borderVerts.Length; i++) {
                Vector3 currPos = borderVertPositions[i];
                List<int> currPosIndexes = new List<int>();
                for (int j = 0; j < verts.Count; j++) {
                    if (Vector3.Distance(borderVertPositions[i], verts[j]) < vertexMatchDistance)
                        currPosIndexes.Add(j);
                }
                borderVerts[i] = new KeyValuePair<Vector3, int[]>(currPos, currPosIndexes.ToArray());
            }

            // expand border verts outwards to adjust for smoothing shrink
            Vector3[] expandedVerts = Utility.Meshes.ExpandHillMesh(verts.ToArray(), borderVerts);

            // smooth border vertices
            Vector3[] smoothedVerts = Utility.Meshes.SmoothHillMesh(expandedVerts, borderVerts);

            // generate texture

            Utility.Vec2Int texSize;
            Vector2 texScale;
            Vector2 texOffset;

            Color[,] texture = GenerateHillChunkColor(borderVerts, out texSize, out texScale, out texOffset);

            return new HillChunk(smoothedVerts, tris.ToArray(), texture, texSize, texScale, texOffset);
        }
        private static void PopulateHillChunkRecursively (List<Utility.Vec2Int> chunkPoints, TileType[,] tileMap, bool[,] usedMap, int currX, int currY) {
            if (CheckHillExists(tileMap, currX, currY, false) && !usedMap[currX, currY]) {
                chunkPoints.Add(new Utility.Vec2Int(currX, currY));
                usedMap[currX, currY] = true;

                PopulateHillChunkRecursively(chunkPoints, tileMap, usedMap, currX + 1, currY);
                PopulateHillChunkRecursively(chunkPoints, tileMap, usedMap, currX - 1, currY);
                PopulateHillChunkRecursively(chunkPoints, tileMap, usedMap, currX, currY + 1);
                PopulateHillChunkRecursively(chunkPoints, tileMap, usedMap, currX, currY - 1);
            }
        }
        private static Vector3[] GetBorderVerts (List<Utility.Vec2Int> chunkPoints, TileType[,] tileMap) {
            List<Vector3> borderVerts = new List<Vector3>();
            for (int i = 0; i < chunkPoints.Count; i++) {

                int x = chunkPoints[i].X;
                int y = chunkPoints[i].Y;

                // down
                if (!CheckHillExists(tileMap, x, y - 1)) {
                    AddBorderVert(borderVerts, x - 0.5f, y - 0.5f);
                    AddBorderVert(borderVerts, x + 0.5f, y - 0.5f);
                }
                // up
                if (!CheckHillExists(tileMap, x, y + 1)) {
                    AddBorderVert(borderVerts, x - 0.5f, y + 0.5f);
                    AddBorderVert(borderVerts, x + 0.5f, y + 0.5f);
                }
                // right
                if (!CheckHillExists(tileMap, x + 1, y)) {
                    AddBorderVert(borderVerts, x + 0.5f, y - 0.5f);
                    AddBorderVert(borderVerts, x + 0.5f, y + 0.5f);
                }
                // left
                if (!CheckHillExists(tileMap, x - 1, y)) {
                    AddBorderVert(borderVerts, x - 0.5f, y - 0.5f);
                    AddBorderVert(borderVerts, x - 0.5f, y + 0.5f);
                }

            }
            return borderVerts.ToArray();
        }
        private static bool CheckHillExists (TileType[,] tileMap, int x, int y, bool defaultReturn = true) {
            if (x < 0) return defaultReturn;
            if (y < 0) return defaultReturn;
            if (x >= ZoneSize.X) return defaultReturn;
            if (y >= ZoneSize.Y) return defaultReturn;
            return tileMap[x, y] == TileType.Hill;
        }
        private static void AddBorderVert (List<Vector3> list, float xPos, float yPos) {
            // check if point already exists on the list, and add if not
            Vector3 currPoint = new Vector3(xPos, yPos, 0);
            float matchDistance = Utility.Meshes.MatchDistance;
            for (int i = 0; i < list.Count; i++) {
                if (Vector3.Distance(list[i], currPoint) < matchDistance) return;
            }
            list.Add(currPoint);
        }

        private static Color[,] GenerateHillChunkColor (KeyValuePair<Vector3, int[]>[] borderVerts, out Utility.Vec2Int texSize, out Vector2 texScale, out Vector2 minPos) {

            // determine size (max-min delta)
            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMin = float.MaxValue;
            float yMax = float.MinValue;
            for (int i = 0; i < borderVerts.Length; i++) {
                Vector3 currPos = borderVerts[i].Key;
                if (currPos.x < xMin) xMin = currPos.x;
                if (currPos.x > xMax) xMax = currPos.x;
                if (currPos.y < yMin) yMin = currPos.y;
                if (currPos.y > yMax) yMax = currPos.y;
            }

            minPos = new Vector2(xMin, yMin);

            float xDelta = xMax - xMin;
            float yDelta = yMax - yMin;

            int xSize = (int)((xMax - xMin) * HillChunkPixelFactor) + 1;
            int ySize = (int)((yMax - yMin) * HillChunkPixelFactor) + 1;

            texSize = new Utility.Vec2Int(xSize, ySize);
            texScale = new Vector2(xDelta, yDelta);

            // fill color map
            Color[,] tex = new Color[xSize, ySize];
            for (int x = 0; x < xSize; x++) {
                for (int y = 0; y < ySize; y++) {

                    // calculate distance from edge
                    float xPos = xMin + ((float)x / xSize) * xDelta;
                    float yPos = yMin + ((float)y / ySize) * yDelta;
                    float dist = float.MaxValue;
                    for (int i = 0; i < borderVerts.Length; i++) {
                        float d = Vector3.Distance(new Vector3(xPos, yPos), borderVerts[i].Key);
                        if (d < dist) dist = d;
                    }

                    // convert to color
                    dist /= 7.5f;
                    dist = 0.2f + dist;

                    if (dist < 0) dist = 0;
                    if (dist > 1) dist = 1;

                    tex[x, y] = hillMinColor + ((Color)hillMaxColor * dist);
                }
            }

            return tex;
        }
        private static Color[,] GenerateWatermap (TileType[,] tilemap, float[,] heightmap, int progressIndex) {
            Color[,] waterMap = new Color[ZoneSize.X, ZoneSize.Y];

            for (int x = 0; x < ZoneSize.X; x++) {
                for (int y = 0; y < ZoneSize.Y; y++) {

                    Color32 currCol;

                    if (tilemap[x, y] == TileType.Water) {
                        currCol = ((heightmap[x, y] * 2) + 0.6f) * waterColor;
                        currCol.a = 255;
                    }
                    else currCol = dirtColor * new Color(1, 1, 1, 0) * 0.3f;

                    waterMap[x, y] = currCol;
                }

                UpdateLoadProgress(progressIndex, x + 1, ZoneSize.X);
            }

            return waterMap;
        }
        private static Color[,] GenerateTerrainColor (float[,] heightmap, TileType[,] tilemap, int seed, int progressIndex) {

            Color[,] colorMap = new Color[ZoneTextureSize.X, ZoneTextureSize.Y];
            
            for (int x = 0; x < ZoneTextureSize.X; x++) {
                for (int y = 0; y < ZoneTextureSize.Y; y++) {

                    int xPos = (int)(((float)x / ZoneTextureSize.X) * ZoneSize.X);
                    int yPos = (int)(((float)y / ZoneTextureSize.Y) * ZoneSize.Y);


                    if (tilemap[xPos, yPos] == TileType.Water) {
                        colorMap[x, y] = dirtColor * new Color(1, 1, 1, 0) * 0.3f;
                    }
                    else {
                        float currValue = (heightmap[xPos, yPos] * 0.7f) + 0.3f;

                        float r, g, b;

                        r = currValue * 0.7f;
                        g = GetColorSq(currValue, 1.35f) * 0.7f;
                        b = GetColorSq(currValue, 1.35f) * 0.7f;

                        colorMap[x, y] = new Color(r, g, b, 1) * dirtColor;
                    }
                }

                UpdateLoadProgress(progressIndex, x + 1, ZoneTextureSize.X);
            }

            return colorMap;
        }

        private static float[,] GenerateHillDistanceMap (TileType[,] tileMap, int progressIndex) {

            DistanceData[,] distanceDataMap = new DistanceData[ZoneSize.X, ZoneSize.Y];
            for (int x = 0; x < ZoneSize.X; x++) {
                for (int y = 0; y < ZoneSize.Y; y++) {
                    distanceDataMap[x, y] = new DistanceData();
                }
            }

            #region Fill data for all directions
            // left to right
            for (int y = 0; y < ZoneSize.Y; y++) {
                int lastHillX = -1;
                for (int x = 0; x < ZoneSize.X; x++) {
                    if (tileMap[x, y] == TileType.Hill) lastHillX = x;
                    distanceDataMap[x, y].DistLeft = x - lastHillX;
                }
            }
            // right to left
            for (int y = 0; y < ZoneSize.Y; y++) {
                int lastHillX = ZoneSize.X;
                for (int x = ZoneSize.X - 1; x >= 0; x--) {
                    if (tileMap[x, y] == TileType.Hill) lastHillX = x;
                    distanceDataMap[x, y].DistRight = lastHillX - x;
                }
            }
            // top-down
            for (int x = 0; x < ZoneSize.X; x++) {
                int lastHillY = -1;
                for (int y = 0; y < ZoneSize.Y; y++) {
                    if (tileMap[x, y] == TileType.Hill) lastHillY = y;
                    distanceDataMap[x, y].DistUp = y - lastHillY;
                }
            }
            // bottom-up
            for (int x = 0; x < ZoneSize.X; x++) {
                int lastHillY = ZoneSize.Y;
                for (int y = ZoneSize.Y - 1; y >= 0; y--) {
                    if (tileMap[x, y] == TileType.Hill) lastHillY = y;
                    distanceDataMap[x, y].DistDown = lastHillY - y;
                }
            }
            #endregion

            UpdateLoadProgress(progressIndex, 1, 3);

            // assign closest value
            for (int x = 0; x < ZoneSize.X; x++) {
                for (int y = 0; y < ZoneSize.Y; y++) {
                    distanceDataMap[x, y].AssignClosest();
                }
            }

            #region Fix corner artifacts
            // left-right
            for (int y = 0; y < ZoneSize.Y; y++) {
                Vector2 currClosestPoint = new Vector2(0, y);
                for (int x = 0; x < ZoneSize.X; x++) {
                    if (tileMap[x, y] == TileType.Hill) {
                        distanceDataMap[x, y].AssignClosest(0);
                        currClosestPoint = new Vector2(x, y);
                    }
                    else {
                        float currClosest = distanceDataMap[x, y].Closest;
                        float currDist = Vector2.Distance(currClosestPoint, new Vector2(x, y));
                        float upDist = distanceDataMap[x, y].DistUp;
                        float DownDist = distanceDataMap[x, y].DistDown;
                        if (currDist < currClosest) distanceDataMap[x, y].AssignClosest(currDist);
                        if (upDist < currDist) currClosestPoint = new Vector2(x, y - upDist);
                        if (DownDist < currDist && DownDist < upDist) currClosestPoint = new Vector2(x, y + DownDist);
                    }
                }
            }
            // right-left
            for (int y = 0; y < ZoneSize.Y; y++) {
                Vector2 currClosestPoint = new Vector2(ZoneSize.X - 1, y);
                for (int x = ZoneSize.X - 1; x >= 0; x--) {
                    if (tileMap[x, y] == TileType.Hill) {
                        distanceDataMap[x, y].AssignClosest(0);
                        currClosestPoint = new Vector2(x, y);
                    }
                    else {
                        float currClosest = distanceDataMap[x, y].Closest;
                        float currDist = Vector2.Distance(currClosestPoint, new Vector2(x, y));
                        float upDist = distanceDataMap[x, y].DistUp;
                        float DownDist = distanceDataMap[x, y].DistDown;
                        if (currDist < currClosest) distanceDataMap[x, y].AssignClosest(currDist);
                        if (upDist < currDist) currClosestPoint = new Vector2(x, y - upDist);
                        if (DownDist < currDist && DownDist < upDist) currClosestPoint = new Vector2(x, y + DownDist);
                    }
                }
            }
            // top-down
            for (int x = 0; x < ZoneSize.X; x++) {
                Vector2 currClosestPoint = new Vector2(x, 0);
                for (int y = 0; y < ZoneSize.Y; y++) {
                    if (tileMap[x, y] == TileType.Hill) {
                        distanceDataMap[x, y].AssignClosest(0);
                        currClosestPoint = new Vector2(x, y);
                    }
                    else {
                        float currClosest = distanceDataMap[x, y].Closest;
                        float currDist = Vector2.Distance(currClosestPoint, new Vector2(x, y));
                        float leftDist = distanceDataMap[x, y].DistLeft;
                        float rightDist = distanceDataMap[x, y].DistRight;
                        if (currDist < currClosest) distanceDataMap[x, y].AssignClosest(currDist);
                        if (leftDist < currDist) currClosestPoint = new Vector2(x - leftDist, y);
                        if (rightDist < currDist && rightDist < leftDist) currClosestPoint = new Vector2(x + rightDist, y);
                    }
                }
            }
            // bottom-up
            for (int x = 0; x < ZoneSize.X; x++) {
                Vector2 currClosestPoint = new Vector2(x, ZoneSize.Y - 1);
                for (int y = ZoneSize.Y - 1; y >= 0; y--) {
                    if (tileMap[x, y] == TileType.Hill) {
                        distanceDataMap[x, y].AssignClosest(0);
                        currClosestPoint = new Vector2(x, y);
                    }
                    else {
                        float currClosest = distanceDataMap[x, y].Closest;
                        float currDist = Vector2.Distance(currClosestPoint, new Vector2(x, y));
                        float leftDist = distanceDataMap[x, y].DistLeft;
                        float rightDist = distanceDataMap[x, y].DistRight;
                        if (currDist < currClosest) distanceDataMap[x, y].AssignClosest(currDist);
                        if (leftDist < currDist) currClosestPoint = new Vector2(x - leftDist, y);
                        if (rightDist < currDist && rightDist < leftDist) currClosestPoint = new Vector2(x + rightDist, y);
                    }
                }
            }

            #endregion

            UpdateLoadProgress(progressIndex, 2, 3);

            float[,] dMap = new float[ZoneSize.X, ZoneSize.Y];
            for (int x = 0; x < ZoneSize.X; x++) {
                for (int y = 0; y < ZoneSize.Y; y++) {
                    dMap[x, y] = distanceDataMap[x, y].Closest;
                }
            }

            UpdateLoadProgress(progressIndex, 3, 3);

            return dMap;
        }
        
        private static Vector2[] GenerateVehicleSpawnsPoints (float[,] dMap, int progressIndex) {

            int borderOffset = 12;
            Vector2[] spawns = new Vector2[4];
            // north, south, west, east

            float centerX = ZoneSize.X / 2f;
            float centerY = ZoneSize.Y / 2f;

            float best1 = 0;
            float best2 = 0;
            for (int x = 0; x < ZoneSize.X; x++) {
                
                float centerDist = Math.Abs(centerX - x);
                if (centerDist <= 0) centerDist = 1;
                centerDist = 1f / centerDist;

                if (dMap[x, borderOffset] + centerDist > best1) {
                    spawns[0] = new Vector2(x, borderOffset);
                    best1 = dMap[x, borderOffset] + centerDist;
                }
                if (dMap[x, ZoneSize.Y - borderOffset] + centerDist > best2) {
                    spawns[1] = new Vector2(x, ZoneSize.Y - borderOffset);
                    best2 = dMap[x, ZoneSize.Y - borderOffset] + centerDist;
                }
            }

            UpdateLoadProgress(progressIndex, 1, 2);

            best1 = 0;
            best2 = 0;
            for (int y = 0; y < ZoneSize.Y; y++) {

                float centerDist = Math.Abs(centerY - y);
                if (centerDist <= 0) centerDist = 1;
                centerDist = 1f / centerDist;

                if (dMap[borderOffset, y] + centerDist > best1) {
                    spawns[2] = new Vector2(borderOffset, y);
                    best1 = dMap[borderOffset, y] + centerDist;
                }
                if (dMap[ZoneSize.X - borderOffset, y] + centerDist > best2) {
                    spawns[3] = new Vector2(ZoneSize.X - borderOffset, y);
                    best2 = dMap[ZoneSize.X - borderOffset, y] + centerDist;
                }
            }

            UpdateLoadProgress(progressIndex, 2, 2);

            return spawns;
        }

        private static Color[,] GenerateMapEdgeIndicator (int progressIndex) {
            Color[,] pixels = new Color[ZoneSize.X, ZoneSize.Y];

            Color colorBase = new Color32(255, 255, 255, 32);
            Color colorEdge = new Color32(255, 255, 255, 128);

            Rect innerRect = new Rect(MapEdgeSize, MapEdgeSize, ZoneSize.X - (MapEdgeSize * 2), ZoneSize.Y - (MapEdgeSize * 2));
            Rect outerRect = new Rect(MapEdgeSize - 1, MapEdgeSize - 1, ZoneSize.X - (MapEdgeSize * 2) + 2, ZoneSize.Y - (MapEdgeSize * 2) + 2);

            for (int x = 0; x < ZoneSize.X; x++) {
                for (int y = 0; y < ZoneSize.Y; y++) {
                    Vector2 currPoint = new Vector2(x, y);
                    if (innerRect.Contains(currPoint)) {
                        continue;
                    }
                    else if (outerRect.Contains(currPoint)) {
                        pixels[x, y] = colorEdge;
                    }
                    else {
                        pixels[x, y] = colorBase;
                    }
                }
                UpdateLoadProgress(progressIndex, x + 1, ZoneSize.X);
            }

            return pixels;
        }

        private static Color[,] GenerateMinimap (int progressIndex) {
            Color[,] pixels = new Color[ZoneSize.X, ZoneSize.Y];
            for (int x = 0; x < ZoneSize.X; x++) {
                for (int y = 0; y < ZoneSize.Y; y++) {
                    switch (TileMap[x,y]) {
                        case TileType.Hill:
                            pixels[x, y] = hillMinColor;
                            break;
                        case TileType.Water:
                            pixels[x, y] = WaterMap[x, y] * 0.75f;
                            break;
                        default:
                            pixels[x, y] = ColorMap[x * 2, y * 2];
                            break;
                    }
                    pixels[x, y].a = 1;
                }
                UpdateLoadProgress(progressIndex, x + 1, ZoneSize.X);
            }
            return pixels;
        }

        // -------------------------------------------------------

        private static Color GetPixelBlackedBound (Color[,] map, int x, int y, int sizeX, int sizeY) {
            if (x < 0) return Color.white;
            if (y < 0) return Color.white;
            if (x >= sizeX) return Color.white;
            if (y >= sizeY) return Color.white;
            return map[x, y];
        }

        private static Color GetPixel (Color[,] map, int x, int y, int sizeX, int sizeY) {
            if (x < 0) x = sizeX - x;
            if (y < 0) y = sizeY - y;
            x = x % sizeX;
            y = y % sizeY;
            return map[x, y];
        }
        private static TileType GetTileType (TileType[,] map, int x, int y, int sizeX, int sizeY) {
            if (x < 0) x = sizeX - x;
            if (y < 0) y = sizeY - y;
            x = x % sizeX;
            y = y % sizeY;
            return map[x, y];
        }

        private static float GetColorGrouped (float val, int groupCount) {
            return (int)(val * groupCount) / (float)(groupCount - 1);
            // assuming val is 0 to 1
        }

        private static float GetColorSq (float val, float exp) {
            return Mathf.Pow(val, exp);
        }

        // -------------------------------------------------------

        private static void UpdateLoadProgress (int index, int current, int max) {
            float progress = (float)(current) / max;
            ProgressTrackers[index] = progress;
            UpdateLoadProgress();
        }
        private static void UpdateLoadProgress () {
            // calculate total progress and update

            float totalProgress = 0;
            for (int i = 0; i < ProgressTrackers.Length; i++) {
                totalProgress += ProgressTrackers[i];
            }
            totalProgress /= ProgressTrackers.Length;

            Utility.LoadScreenManager.UpdateProgress(totalProgress);
        }

    }
}
