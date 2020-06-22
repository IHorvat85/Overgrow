using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Utility;

namespace Interface {
    public class WorldMapUIController : MonoBehaviour {

        private static WorldMapUIController InstanceRef;

        public Controls.WorldMouse WorldMouseController;

        public World.WorldMapController WorldMapController;

        public GameObject WorldMapParent;

        private Vector3 PlayerIconTargetPos;
        private Vector3 PlayerIconTargetRot;
        public GameObject PlayerIcon;

        // -----------------------------------------
        // UI Element References

        public Text TextFuelLeft;
        public Text TextFuelUse;

        public TooltipController Tooltip;

        public Button ExploreButton;

        public WorldMapVehiclePanel[] VehiclePanels;

        public CanvasGroup FadeScreen;

        // -----------------------------------------

        private float cgWorldMapAlpha = 1;
        private float cgWorldMapAlphaTarget = 1;

        private bool aoSceneLoadStart = false;
        private AsyncOperation aoSceneLoad = null;

        private World.WorldMapTile currentTile;

        private World.WorldMapTileController lastMouseoverTile;

        // -----------------------------------------

        void Start () {
            InstanceRef = this;
            UpdateFuelLevels();
            StartCoroutine(UpdatePlayerIcon(World.WorldMapController.PlayerPos + new Vector3(0.5f, 0.5f, 0), 0));
        }

        void Update () {
            
            UpdateScreenFade();

            if (!CheckSceneLoad()) {
                CheckPlayerClick();
                UpdateMouseoverInfo();
            }
        }

        // -----------------------------------------

        private void CheckPlayerClick () {
            if (UnityEngine.EventSystems.EventSystem.current == null) return;

            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
                // mouse not on UI

                if (Input.GetMouseButtonDown(0)) {
                    // move player to mouseover zone

                    World.WorldMapTileController cont = WorldMouseController.CheckMouseTile();

                    // see if cont is neighbor of current tile (player pos)
                    Vec2Int playerPos = World.WorldMapController.PlayerPos;
                    bool isNeighbor = false;
                    World.WorldMapTileController playerCont = World.WorldMapGenerator.GetTile(playerPos.X, playerPos.Y).controller;
                    for (int i = 0; i < playerCont.TouchingNeighbors.Count; i++) {
                        if (playerCont.TouchingNeighbors[i] == cont) {
                            isNeighbor = true;
                            break;
                        }
                    }
                    // if no, return
                    if (!isNeighbor) return;

                    // if yes, try move to tile (check fuel etc)
                    int fuelNeeded = Resources.Convoy.CalculateFuelUse();
                    int fuelAmount = Resources.Convoy.GetResourceAmount("res_fuel");

                    if (fuelAmount < fuelNeeded) {
                        // todo: throw message that there isn't enough fuel
                        return;
                    }
                    if (Resources.Convoy.SubtractResource("res_fuel", fuelNeeded) != fuelNeeded) {
                        // todo: throw message that there isn't enough fuel
                        return;
                    }

                    TimeOfDay.TimeManager.IncrementTime(3600);
                    WorldMapController.SetPlayerPosition(cont.Tile.Position);
                    UpdateCurrentZone(cont.Tile);

                    // move player icon
                    Vector3 iconTargetPos = cont.Tile.Position;
                    Vector3 posDiff = iconTargetPos - playerCont.Tile.Position;
                    float iconTargetRot = Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), posDiff);
                    if (posDiff.x > 0.0f) iconTargetRot = 360.0f - iconTargetRot;

                    World.ZoneMapController.LastMoveVector = posDiff;
                    StartCoroutine(UpdatePlayerIcon(iconTargetPos + new Vector3(0.5f, 0.5f), iconTargetRot));

                    UpdateFuelLevels();
                }
            }
        }

        private void UpdateMouseoverInfo () {
            
            World.WorldMapTileController cont = WorldMouseController.CheckMouseTile();

            if (lastMouseoverTile != null) {
                if (lastMouseoverTile == cont) return;
                // un-highlight(?) lastMouseoverTile
                lastMouseoverTile.SetHighlight(false);
            }
            if (cont != null) {
                // highlight new zone
                cont.SetHighlight(true);

                // update tooltip
                ITooltipData tooltipData = cont.Tile.zone;
                Tooltip.SetTooltip(tooltipData);
            }
            else {
                // clear tooltip
                Tooltip.SetTooltip(null);
            }

            lastMouseoverTile = cont;
        }

        private void UpdateScreenFade () {
            CanvasGroup cgWorldMap = GetComponent<CanvasGroup>();
            cgWorldMapAlpha = Mathf.Lerp(cgWorldMapAlpha, cgWorldMapAlphaTarget, 0.15f);
            cgWorldMap.alpha = cgWorldMapAlpha;
            FadeScreen.alpha = 1 - cgWorldMapAlpha;
        }

        private bool CheckSceneLoad () {
            if (aoSceneLoadStart) {
                if (cgWorldMapAlpha < 0.3f) {
                    aoSceneLoad = Application.LoadLevelAdditiveAsync("ZoneMap");
                    aoSceneLoadStart = false;
                }
                return true;
            }
            else {
                if (aoSceneLoad == null) return false;

                // disable world map elements
                if (aoSceneLoad.isDone && cgWorldMapAlpha < 0.01f) 
                    WorldMapParent.SetActive(false);
                
                return true;
            }
        }

        private IEnumerator UpdatePlayerIcon (Vector3 targetPos, float targetRot) {

            targetPos.z = -2;

            Vector3 startPos = PlayerIcon.transform.position;
            float startRot = PlayerIcon.transform.rotation.eulerAngles.z;

            float t = 0;
            while (t < 1) {
                t += Time.deltaTime * 2;
                if (t > 1) t = 1;

                float v1 = Utility.TransformFunctions.SmoothStep(t, 4);
                float v2 = Utility.TransformFunctions.SmoothStop(t, 3);

                // position
                Vector3 currPos = Vector3.Lerp(startPos, targetPos, v1);
                PlayerIcon.transform.position = currPos;

                // rotation
                float currRot = Mathf.LerpAngle(startRot, targetRot, v2);
                PlayerIcon.transform.rotation = Quaternion.Euler(0, 0, currRot);

                yield return null;
            }

            yield return 0;
        }

        // -----------------------------------------

        public void UpdateCurrentZone (World.WorldMapTile tile) {
            currentTile = tile;
            // todo: update UI to show data for current tile..
        }

        public void ExploreCurrentZone () {

            // run a check on whether this zone is explorable (just to be sure)
            World.Zone currZone = currentTile.zone;
            if (currZone == null) {
                Utility.ErrorLog.ReportError("Curr zone is null!");
                return;
            }

            // disable world map ui elements
            CanvasGroup cgWorldMapUI = GetComponent<CanvasGroup>();
            cgWorldMapUI.interactable = false;
            cgWorldMapUI.blocksRaycasts = false;
            cgWorldMapAlphaTarget = 0;

            // set the zone reference to controller
            World.ZoneMapController.CurrentZone = currZone;

            // set load start flag
            aoSceneLoadStart = true;

            // note: rest will be handled on Start() in scripts loaded with the new scene
        }

        public void UpdateFuelLevels () {
            int fuelLeft = Resources.Convoy.GetResourceAmount("res_fuel");
            TextFuelLeft.text = fuelLeft.ToString();

            int fuelUse = Resources.Convoy.CalculateFuelUse();
            TextFuelUse.text = fuelUse + Language.Table.Get("gui_perHour");
        }

        public static void UpdateVehiclePanels () {
            if (InstanceRef == null) return;
            if (InstanceRef.VehiclePanels == null) return;
            for (int i = 0; i < InstanceRef.VehiclePanels.Length; i++) {
                InstanceRef.VehiclePanels[i].RefreshAssignedVehicle();
            }
        }

        public static void SetExploreButtonState (bool state) {
            InstanceRef.ExploreButton.interactable = state;
        }

        public static void EnableWorldMap () {
            // note: called when exiting zone map

            if (InstanceRef == null) {
                Utility.ErrorLog.ReportError("Instance reference for WorldMapUIController is NULL!");
                return;
            }

            InstanceRef.aoSceneLoad = null;
            InstanceRef.aoSceneLoadStart = false;

            // enable world map ui elements (see ExploreCurrentZone for how it's disabled above)
            InstanceRef.transform.parent.gameObject.SetActive(true);
            InstanceRef.cgWorldMapAlphaTarget = 1;
            CanvasGroup cgWorldMapUI = InstanceRef.GetComponent<CanvasGroup>();
            cgWorldMapUI.interactable = true;
            cgWorldMapUI.blocksRaycasts = true;

            // restore mouse cursor
            ZoneCursorController.RestoreMouseCursor();

            // reset load screen
            Utility.LoadScreenManager.Reset();

            // update world map vehicle panel
            UpdateVehiclePanels();
        }
    }
}
