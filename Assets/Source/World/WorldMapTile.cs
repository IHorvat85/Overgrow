using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utility;

namespace World {
    public class WorldMapTile {

        private const int xConst = 62039;
        private const int yConst = 49369;

        public WorldMapTileController controller;
        public Zone zone;

        public int GeneratedLevelCurr = -1;
        public int GeneratedLevelTarget = 0;

        public Vec2Int Position;

        public float Value;
        public int Seed;

        private System.Random rand;

        public Vector2 PointPos;
        public Vector2 PointPosWorld;
        public float PointStrength;
        public bool PointActive;

        public List<WorldMapTile> ConnectedTiles;

        public Color[] TileBackgroundPixels;
        public Color[] TileBackgroundMask;

        public WorldMapTile (Vec2Int position, int worldSeed) {
            
            this.Position = position;
            this.Seed = worldSeed + position.GetHashCode() + (position.X * xConst) + (position.Y * yConst);
            this.GeneratedLevelCurr = -1;
            this.GeneratedLevelTarget = 0;

            ConnectedTiles = new List<WorldMapTile>();

            Value = Seed;
            rand = new System.Random(Seed);

            zone = new Zone(Seed);
        }

        public float GetRand () {
            return (float)rand.NextDouble();
        }

        public void AssignBackgroundPixels (Color[] pixels, Color[] mask) {
            TileBackgroundPixels = pixels;
            TileBackgroundMask = mask;
        }

    }
}
