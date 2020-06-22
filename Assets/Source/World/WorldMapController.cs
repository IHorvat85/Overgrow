using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utility;

namespace World {
    public class WorldMapController : MonoBehaviour {

        // --------------------------------------------------------

        public Interface.WorldMapUIController UIController;

        public GameObject MapTileParent;
        public GameObject MapTilePrefab;

        public static Vec2Int PlayerPos;

        private Dictionary<Vec2Int, WorldMapTileController> TileControllers;

        // --------------------------------------------------------

        void Start () {
            WorldMapGenerator.Initialize();
            WorldMapGenerator.StartGenThread();
            TileControllers = new Dictionary<Vec2Int, WorldMapTileController>();
            UpdateGridMove();
        }

        void OnDestroy () {
            WorldMapGenerator.StopGenThread();
        }

        void Update () {

            if (WorldMapGenerator.ThreadIdle) {
                WorldMapGenerator.UpdateGridTiles();
                UpdateVision();
            }

            UpdateGrid();
        }

        private void UpdateGrid () {
            Vec2Int[] generatedTiles = WorldMapGenerator.UpdateGeneratedTiles();
            for (int i = 0; i < generatedTiles.Length; i++) {
                RefreshTileController(generatedTiles[i]);
            }
        }

        private void UpdateGridMove () {
            WorldMapGenerator.UpdateGridTiles();
            UIController.UpdateCurrentZone(WorldMapGenerator.GetTile(PlayerPos.X, PlayerPos.Y));
            UpdateVision();
        }

        // --------------------------------------------------------------

        private void UpdateVision () {
            int range = 8;
            for (int x = -range; x <= range; x++) {
                for (int y = -range; y <= range; y++) {
                    Vec2Int currPos = PlayerPos + new Vec2Int(x, y);
                    WorldMapTileController cont;
                    if (TileControllers.TryGetValue(currPos, out cont)) {
                        cont.UpdateVision(PlayerPos);
                    }
                }
            }
        }

        private WorldMapTileController InitializeTileMapCont (int x, int y) {
            GameObject go = Instantiate<GameObject>(MapTilePrefab);

            go.transform.parent = MapTileParent.transform;

            WorldMapTileController cont = go.GetComponent<WorldMapTileController>();
            TileControllers.Add(new Vec2Int(x, y), cont);
            return cont;
        }

        // --------------------------------------------------------------

        public void RefreshTileController (Vec2Int pos) {
            if (TileControllers == null) return;

            WorldMapTileController cont;
            if (!TileControllers.TryGetValue(pos, out cont)) cont = InitializeTileMapCont(pos.X, pos.Y);
            cont.AssignTile(WorldMapGenerator.GetTile(pos.X, pos.Y));
        }

        public void SetPlayerPosition (Vec2Int pos) {
            PlayerPos = pos;
            UpdateGridMove();
        }
    }
}
