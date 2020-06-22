using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Controls.Zone;


namespace World {
    public class ZoneMapController : MonoBehaviour {

        public Interface.ZoneMapUIController UIController;

        public GameObject VehiclePrefab;
        public GameObject VehicleParent;

        [HideInInspector]
        public Vehicles.VehicleController PlayerVehicle;

        public MeshRenderer GroundRenderer;
        public MeshRenderer GroundSplatRenderer;

        public MeshRenderer GrowthRenderer;
        public MeshRenderer WaterRenderer;

        public UnityEngine.UI.Image EdgeIndicatorRenderer;
        private Color edgeIndicatorColor;
        private Color edgeIndicatorTargetColor;
        private float edgeIndicatorLerpT = 3f;

        public MeshRenderer DebugRenderer;

        public GameObject HillColliderPrefab;
        public GameObject HillChunkPrefab;

        public GameObject HillColliderParent;
        public GameObject HillChunksParent;

        [Header("Border Colliders")]
        public BoxCollider2D BorderColliderTop;
        public BoxCollider2D BorderColliderBottom;
        public BoxCollider2D BorderColliderLeft;
        public BoxCollider2D BorderColliderRight;

        public static Zone CurrentZone;

        public static bool AwaitingGenerationFlag = true;
        public static bool GenerationDoneFlag = false;

        public static Vector2 LastMoveVector = new Vector2(0, 1);

        // Use this for initialization
        void Start () {
            new Thread(new ThreadStart(() => {
                ZoneMapGenerator.GenerateMap(CurrentZone);
            })).Start();

            // set initial edge indicator color
            Color edgeColor = Utility.ColorData.MainBlue;
            edgeIndicatorTargetColor = edgeColor;
            edgeIndicatorColor = edgeColor;
        }

        // Update is called once per frame
        void Update () {
            if (AwaitingGenerationFlag && GenerationDoneFlag) {
                AwaitingGenerationFlag = false;
                StartCoroutine(FinalizeMapGeneration());
            }

            UpdateEdgeIndicator();
        }

        // -----------------------------------

        private void UpdateEdgeIndicator () {

            // see if player vehicle in the leave zone
            bool playerInLeaveZone = IsVehicleInLeaveZone(PlayerVehicle);

            // paint the leave zone indicator appropriately
            Color edgeColor;
            if (!playerInLeaveZone) {
                edgeColor = Utility.ColorData.MainRed;
                UIController.leaveButtonTargetColor = edgeColor;
                UIController.leaveButtonTargetColor.a = 64f / 255f;
                UIController.leaveButtonVisible = false;
                UIController.BtnLeaveZone.interactable = false;
            }
            else {
                edgeColor = Utility.ColorData.MainBlue;
                UIController.leaveButtonTargetColor = new Color32(128, 128, 128, 64);
                UIController.leaveButtonVisible = true;
                UIController.BtnLeaveZone.interactable = true;
            }

            edgeIndicatorTargetColor = edgeColor;
            edgeIndicatorColor = Color.Lerp(edgeIndicatorColor, edgeIndicatorTargetColor, edgeIndicatorLerpT * Time.deltaTime);
            EdgeIndicatorRenderer.color = edgeIndicatorColor;
        }

        public bool CheckVehicleLeaveZone () {
            if (PlayerVehicle == null) return false;
            return IsVehicleInLeaveZone(PlayerVehicle);
        }

        private bool IsVehicleInLeaveZone (Vehicles.VehicleController veh) {
            if (veh == null) return false;
            Utility.Vec2Int zoneSize = ZoneMapGenerator.ZoneSize;
            Vector3 vehPosCorrected = veh.transform.position + new Vector3(zoneSize.X / 2f, zoneSize.Y / 2f, 0);
            int leaveZoneSize = ZoneMapGenerator.MapEdgeSize;
            Rect outsideLeaveZone = new Rect(leaveZoneSize, leaveZoneSize, zoneSize.X - (leaveZoneSize * 2), zoneSize.Y - (leaveZoneSize * 2));
            return !outsideLeaveZone.Contains(vehPosCorrected);
        }

        public IEnumerator LeaveZone () {

            // fade to black (on zone screen)
            float fadeScreenAlpha = 0;
            while (fadeScreenAlpha < 1) {
                fadeScreenAlpha += Time.unscaledDeltaTime * 2;
                if (fadeScreenAlpha > 1) fadeScreenAlpha = 1;
                UIController.SetFadeScreen(fadeScreenAlpha);
                yield return 0;
            }

            yield return 0;

            // kill secondary threads (vehicle ai etc)
            KillAllZoneThreads();

            // set time scale to 1
            int zoneUIContHash = UIController.GetHashCode();
            Utility.TimeController.SetFactor(zoneUIContHash, 1);

            // enable world parent
            Interface.WorldMapUIController.EnableWorldMap();

            // destroy zone parent (this)
            Destroy(this.gameObject);
        }
        private void KillAllZoneThreads () {
            // todo: kill all threads for zone map
            // (monster ai, pathfinding update...)

        }

        // -----------------------------------
        
        #region Player Vehicle Spawning

        private const float VehicleSpawnFormationDistance = 5;

        public void SpawnPlayerVehicles () {
            if (PlayerVehicle != null) {
                // delete existing vehicles
                Destroy(PlayerVehicle);
            }

            Vehicles.Vehicle vehicle = Resources.Convoy.GetVehicle();

            GameObject tempGO = Instantiate<GameObject>(VehiclePrefab);

            tempGO.transform.parent = VehicleParent.transform;
            tempGO.transform.position = Vector3.zero;

            // assign and rebuild the vehicle
            Vehicles.VehicleController cont = tempGO.GetComponent<Vehicles.VehicleController>();
            cont.AssignVehicle(vehicle);

            PlayerVehicle = cont;

            PositionPlayerVehicle();
        }

        private void PositionPlayerVehicle () {

            // calculate global offset
            Vector2 globalOffset = GetGlobalVehicleOffset();

            Vector2 formVector = LastMoveVector.normalized;

            float rot = Vector3.Angle(Vector3.up, LastMoveVector);
            if (LastMoveVector.x > 0) rot = 360 - rot;

            PlayerVehicle.transform.position = globalOffset;
            PlayerVehicle.transform.rotation = Quaternion.Euler(0, 0, rot);

            PlayerVehicle.transform.position += new Vector3(0, 0, -1);

            // set the camera to vehicle1 position
            UIController.CameraController.MoveToInstant(PlayerVehicle.transform.position);

        }
        private Vector2 GetGlobalVehicleOffset () {

            Vector2 vector = LastMoveVector;
            
            float sizeX = ZoneMapGenerator.ZoneSize.X / 2f;
            float sizeY = ZoneMapGenerator.ZoneSize.Y / 2f;

            vector = new Vector2(vector.x * sizeX * -1, vector.y * sizeY * -1) + new Vector2(sizeX, sizeY);

            Vector2[] spawnPoints = ZoneMapGenerator.VehicleSpawnPoints;

            float shortestDist = float.MaxValue;
            int shortestDistIndex = 0;
            for (int i = 0; i < spawnPoints.Length; i++) {
                float d = Vector2.Distance(spawnPoints[i], vector);
                if (d < shortestDist) {
                    shortestDist = d;
                    shortestDistIndex = i;
                }
            }

            return spawnPoints[shortestDistIndex] - new Vector2(sizeX, sizeY);

        }

        #endregion

        // -----------------------------------

        #region Map Generation

        private IEnumerator FinalizeMapGeneration () {
            // pull data from generator and apply / instantiate

            Utility.Vec2Int zoneSize = ZoneMapGenerator.ZoneSize;
            Utility.Vec2Int zoneTextureSize = ZoneMapGenerator.ZoneTextureSize;

            Utility.LoadScreenManager.SetText(Language.Table.Get("gui_finalizing"));

            // flatten colormaps to arrays
            Color[] colorArray = Utility.Textures.ConvertMapToArray(ZoneMapGenerator.ColorMap, zoneTextureSize.X, zoneTextureSize.Y);
            Color[] waterArray = Utility.Textures.ConvertMapToArray(ZoneMapGenerator.WaterMap, zoneSize.X, zoneSize.Y);
            Color[] edgeIndicatorArray = Utility.Textures.ConvertMapToArray(ZoneMapGenerator.EdgeIndicatorMap, zoneSize.X, zoneSize.Y);
            ZoneMapGenerator.TileType[,] tileMap = ZoneMapGenerator.TileMap;
            ZoneMapGenerator.HillChunk[] hillChunks = ZoneMapGenerator.HillChunks;

            yield return null;
            
            // resize renderers
            Vector3 rendSize = new Vector3(zoneSize.X, zoneSize.Y, 1);
            EdgeIndicatorRenderer.transform.parent.GetComponent<RectTransform>().sizeDelta = rendSize;
            GroundRenderer.transform.localScale = rendSize;
            WaterRenderer.transform.localScale = rendSize;
            GrowthRenderer.transform.localScale = rendSize;

            // resize border colliders
            float halfZoneSizeX = zoneSize.X / 2;
            float halfZoneSizeY = zoneSize.Y / 2;
            BorderColliderTop.transform.position = new Vector3(0, halfZoneSizeY + 10, 0);
            BorderColliderBottom.transform.position = new Vector3(0, -halfZoneSizeY - 10, 0);
            BorderColliderLeft.transform.position = new Vector3(-halfZoneSizeY - 10, 0, 0);
            BorderColliderRight.transform.position = new Vector3(halfZoneSizeY + 10, 0, 0);
            BorderColliderTop.size = new Vector3(1000, 20);
            BorderColliderBottom.size = new Vector3(1000, 20);
            BorderColliderRight.size = new Vector3(20, 1000);
            BorderColliderLeft.size = new Vector3(20, 1000);

            // generate colliders from tilemap
            for (int x = 0; x < zoneSize.X; x++) {
                for (int y = 0; y < zoneSize.Y; y++) {
                    if (tileMap[x, y] == ZoneMapGenerator.TileType.Hill) {
                        GameObject tempCollider = Instantiate<GameObject>(HillColliderPrefab);
                        tempCollider.transform.position = new Vector3(x - halfZoneSizeX + 0.5f, y - halfZoneSizeY + 0.5f, 0);
                        tempCollider.transform.parent = HillColliderParent.transform;
                    }
                    if (x == 0 && y % 4 == 0) yield return null;
                }
            }

            // apply to renderer
            GroundRenderer.sharedMaterial.mainTexture = Utility.Textures.GenerateTexture(colorArray, zoneTextureSize.X, zoneTextureSize.Y);
            WaterRenderer.sharedMaterial.mainTexture = Utility.Textures.GenerateTexture(waterArray, zoneSize.X, zoneSize.Y);

            Texture2D edgeIndicatorTex = Utility.Textures.GenerateTexture(edgeIndicatorArray, zoneSize.X, zoneSize.Y);
            EdgeIndicatorRenderer.sprite = Sprite.Create(edgeIndicatorTex, new Rect(0, 0, zoneSize.X, zoneSize.Y), new Vector2(0.5f, 0.5f));

            #region generate and render all hills
            for (int i = 0; i < hillChunks.Length; i++) {
                if (hillChunks[i].vertices == null) continue;

                // setup GO
                GameObject tempChunk = Instantiate<GameObject>(HillChunkPrefab);
                tempChunk.transform.position = new Vector3(0.5f, 0.5f, -2) - new Vector3(zoneSize.X / 2, zoneSize.Y / 2, 0);
                tempChunk.transform.parent = HillChunksParent.transform;

                // apply mesh
                Mesh mesh = new Mesh();
                mesh.vertices = hillChunks[i].vertices;
                mesh.triangles = hillChunks[i].triangles;
                tempChunk.GetComponent<MeshFilter>().mesh = mesh;

                // apply texture
                Utility.Vec2Int texSize = hillChunks[i].textureSize;
                Vector2 texScale = hillChunks[i].textureScale;
                Vector2 texOffset = hillChunks[i].textureOffset;

                texOffset = new Vector2(-texOffset.x / texScale.x, -texOffset.y / texScale.y);
                texScale = new Vector2(1f / texScale.x, 1f / texScale.y);

                Color[] chunkPixels = Utility.Textures.ConvertMapToArray(hillChunks[i].texture, texSize.X, texSize.Y);
                Texture2D chunkTex = Utility.Textures.GenerateTexture(chunkPixels, texSize.X, texSize.Y);
                chunkTex.wrapMode = TextureWrapMode.Clamp;

                tempChunk.GetComponent<MeshRenderer>().material.mainTexture = chunkTex;
                tempChunk.GetComponent<MeshRenderer>().material.mainTextureScale = texScale;
                tempChunk.GetComponent<MeshRenderer>().material.mainTextureOffset = texOffset;

            }
            #endregion

            // update minimap
            UIController.minimapBaseImage = Sprite.Create(Utility.Textures.GenerateTexture(ZoneMapGenerator.Minimap, zoneSize.X, zoneSize.Y),
                new Rect(0, 0, zoneSize.X, zoneSize.Y), new Vector2(0.5f, 0.5f));

            // rebuild pathfinding
            Utility.Pathfinding.RebuildPathing();

            // inform load screen manager that loading is done
            Utility.LoadScreenManager.UpdateProgress(1);
            Utility.LoadScreenManager.IsDone = true;

            // todo: debug, remove

            Material[] splatMats = GroundSplatRenderer.sharedMaterials;






            yield return null;
        }

        #endregion

        // -----------------------------------

    }
}
