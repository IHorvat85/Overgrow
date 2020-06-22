using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace World {
    public class WorldMapTileController : MonoBehaviour {

        [HideInInspector]
        public WorldMapTile Tile;
        [HideInInspector]
        public List<WorldMapTileController> TouchingNeighbors;
        [HideInInspector]
        public Vector3[] ShapeMeshVerts;

        public GameObject PointRenderer;
        public MeshRenderer ShapeRenderer;
        public GameObject BackgroundRenderer;
        public GameObject VisionBlocker;

        private enum VisionState {
            Hidden,
            Visible,
            RevealedPast,
        }
        private VisionState Vision = VisionState.Hidden;

        private const float PosZ = 0;

        [HideInInspector]
        public bool backgroundApplied = false;
        [HideInInspector]
        public bool meshGenerated = false;

        private static Color ColorHighlight = new Color(0, 0, 0, 0.3f);
        private static Color ColorDefault = new Color(0, 0, 0, 0.15f);
        private static Color ColorNeighbor = new Color(0, 0.3f, 0, 0.25f);
        private static Color ColorDisabled = new Color(0, 0, 0, 0);

        private float visionAlphaStart = 1;
        private float visionAlphaTarget = 1;
        private float visionAlphaCurrent = 1;

        private Color colorTarget = ColorDisabled;
        private Color colorCurr = ColorDisabled;

        void Start () {
            
        }

        void Update () {
            colorCurr = Color.Lerp(colorCurr, colorTarget, 0.2f);
            ShapeRenderer.material.color = colorCurr;
        }

        public void AssignTile (WorldMapTile tile) {
            this.Tile = tile;
            tile.controller = this;
            Reposition();
        }

        public void SetHighlight (bool state) {
            if (Tile == null) return;
            if (Tile.GeneratedLevelCurr < 2) return;

            if (!Tile.PointActive) colorTarget = ColorDisabled;
            else colorTarget = state ? ColorHighlight : ColorDefault;
            
            // set neighbor highlights
            if (Tile.GeneratedLevelCurr < 3) return;
            for (int i = 0; i < TouchingNeighbors.Count; i++) {
                if (!TouchingNeighbors[i].Tile.PointActive) continue;
                TouchingNeighbors[i].colorTarget = state ? ColorNeighbor : ColorDefault;
            }
        }

        private void Reposition () {
            if (Tile == null) return;

            transform.name = "Tile " + Tile.GeneratedLevelTarget;

            Vector3 pos = new Vector3(Tile.Position.X + 0.5f, Tile.Position.Y + 0.5f, PosZ);
            transform.position = pos;

            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.localScale = new Vector3(1, 1, 1);

            PointRenderer.GetComponent<SpriteRenderer>().color = Tile.PointActive ? Color.white : Color.red;
            PointRenderer.transform.localPosition = Tile.PointPos;

            if (Tile.GeneratedLevelCurr >= 1 && !backgroundApplied) {
                MeshRenderer backgroundMeshRend = BackgroundRenderer.GetComponent<MeshRenderer>();

                Texture2D backgroundTex = Utility.Textures.GenerateTexture(Tile.TileBackgroundPixels, 32, 32);
                backgroundTex.wrapMode = TextureWrapMode.Clamp;

                Texture2D maskTex = Utility.Textures.GenerateTexture(Tile.TileBackgroundMask, 32, 32);
                maskTex.wrapMode = TextureWrapMode.Clamp;

                backgroundMeshRend.material.SetTextureOffset("_DetailAlbedoMap", new Vector2(Tile.Position.X * 0.2f, Tile.Position.Y * 0.2f));

                backgroundMeshRend.material.SetTexture("_DetailMask", maskTex);
                backgroundMeshRend.material.mainTexture = backgroundTex;
                backgroundMeshRend.material.color = Color.white;
                backgroundApplied = true;
            }

            if (Tile.GeneratedLevelCurr >= 2 && !meshGenerated) {
                
                ShapeRenderer.enabled = Tile.PointActive;
                ShapeRenderer.GetComponent<MeshCollider>().enabled = Tile.PointActive;

                if (Tile.PointActive) {
                    Mesh newMesh = ProcGen.ZoneTileShapeGenerator.GenerateMeshDeductively(Tile);
                    ShapeRenderer.GetComponent<MeshFilter>().mesh = newMesh;

                    this.ShapeMeshVerts = newMesh.vertices;

                    colorTarget = ColorDefault;
                    ShapeRenderer.GetComponent<MeshCollider>().sharedMesh = newMesh;

                    Vector2 newPointPos = ProcGen.ZoneTileShapeGenerator.RepositionCenterPoint(Tile, newMesh);
                    Tile.PointPos = newPointPos;
                    PointRenderer.transform.localPosition = Tile.PointPos;
                }
                meshGenerated = true;
            }
        }

        public void UpdateVision (Utility.Vec2Int playerPos) {
            float d = Utility.Vec2Int.Distance(Tile.Position, playerPos);
            bool shouldBeVisible = d < 7;

            if (shouldBeVisible) {
                if (Vision == VisionState.Visible) return;
                // set to visible
                Vision = VisionState.Visible;
                VisionBlocker.GetComponent<BoxCollider>().enabled = false;
                visionAlphaTarget = 0;
                StartCoroutine("UpdateVisionAlpha");
            }
            else if (Vision == VisionState.Visible) {
                // set to revealedPast
                Vision = VisionState.RevealedPast;
                visionAlphaTarget = 0.5f;
                StartCoroutine("UpdateVisionAlpha");
            }
        }

        private IEnumerator UpdateVisionAlpha () {
            float d = 1;
            while (d > 0.01f) {
                d = Mathf.Abs(visionAlphaTarget - visionAlphaCurrent);
                visionAlphaCurrent = Mathf.Lerp(visionAlphaCurrent, visionAlphaTarget, Time.deltaTime * 5);
                VisionBlocker.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, visionAlphaCurrent);
                yield return null;
            }

            visionAlphaCurrent = visionAlphaTarget;
            VisionBlocker.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, visionAlphaCurrent);

            yield return 0;
        }
    }
}
