using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Utility;

namespace Interface {
    public class ZoneMapUIController : MonoBehaviour {

        public static ZoneMapUIController InstanceRef = null;

        public World.ZoneMapController MapController;
        public Controls.Zone.ZoneCamera CameraController;

        public CanvasGroup FadeScreen;
        public static float FadeScreenTarget = 1;
        private float FadeScreenAlpha = 1;

        [HideInInspector]
        public Color leaveButtonTargetColor = new Color32(128, 128, 128, 64);
        private Color leaveButtonCurrentColor = new Color32(128, 128, 128, 64);
        private Vector2 leaveButtonTargetPos1 = new Vector2(0.1f, 0.155f);
        private Vector2 leaveButtonTargetPos2 = new Vector2(0.175f, 0.23f);
        private float leaveButtonPositionT = 0;
        [HideInInspector]
        public bool leaveButtonVisible = false;

        private float leavePopupTimeFactor = 1;
        private float leavePopupAlpha = 0;
        private bool leavePopupVisible = false;
        public CanvasGroup LeavePopupCG;
        
        [Header("UI Elements")]

        public Button BtnLeaveZone;
        public Image BtnLeaveZoneStripes;

        public Button BtnLeaveLeave;
        public Button BtnLeaveCancel;

        // Minimap

        public Image MinimapRenderer;
        [HideInInspector]
        public Sprite minimapBaseImage = null;
        private bool moveMinimapMouse = false;

        public Image minimapCamera;

        public GameObject minimapContentParent;
        public GameObject minimapVehiclePointPrefab;

        // -----------------

        [HideInInspector]
        public bool InputEnabled = false;

        // -----------------------------------------

        public SelectionBoxController SelectionBox;

        // -----------------------------------------

        public static void Activate () {

            // enable main camera
            InstanceRef.CameraController.gameObject.SetActive(true);

            // spawn player vehicles
            InstanceRef.MapController.SpawnPlayerVehicles();

            // enable player input
            InstanceRef.InputEnabled = true;

            // set fade target
            FadeScreenTarget = 0;

            // show UI
            InstanceRef.GetComponent<CanvasGroup>().alpha = 1;

            // todo: other stuff

        }

        // -----------------------------------------

        void Start () {
            InstanceRef = this;
            FadeScreen.alpha = FadeScreenAlpha;
            MinimapRenderer.color = Color.white;
        }

        void Update () {

            CheckPlayerInput();

            UpdateFadeScreen();

            UpdateMinimap();

            UpdateLeaveButtonMechanics();

        }

        // -----------------------------------------

        private void CheckPlayerInput () {
            if (InputEnabled) {

                // todo: check if game paused
                
                Vector3 mouseWorld = Controls.Zone.ZoneMouse.GetMouseWorldPos();
                RaycastHit2D rayHit = Physics2D.Raycast(mouseWorld, Vector2.zero);

                Controls.Zone.ZoneSelectable mouseUnit = null;
                if (rayHit.collider != null) mouseUnit = rayHit.collider.GetComponent<Controls.Zone.ZoneSelectable>();

            }
        }

        private void UpdateFadeScreen () {
            FadeScreenAlpha = Mathf.Lerp(FadeScreenAlpha, FadeScreenTarget, 0.1f);
            FadeScreen.alpha = FadeScreenAlpha;
        }

        private void UpdateMinimap () {

            MinimapRenderer.sprite = minimapBaseImage;

            // todo: resource / wreck icons

            // enemies ?

            UpdateMinimapCamera();

            MoveMinimap();
        }

        private void UpdateLeaveButtonMechanics () {
            RectTransform rt = BtnLeaveZone.GetComponent<RectTransform>();

            leaveButtonPositionT += Time.unscaledDeltaTime * 2 * (leaveButtonVisible ? 1 : -1);
            leaveButtonPositionT = Mathf.Clamp01(leaveButtonPositionT);

            float leaveButtonPosTTransf = Utility.TransformFunctions.SmoothStep(leaveButtonPositionT, 2);
            Vector2 leaveButtonCurrentPosY = Vector2.Lerp(leaveButtonTargetPos1, leaveButtonTargetPos2, leaveButtonPosTTransf);
            rt.anchorMin = new Vector2(0.7f, leaveButtonCurrentPosY.x);
            rt.anchorMax = new Vector2(0.815f, leaveButtonCurrentPosY.y);

            leaveButtonCurrentColor = Color.Lerp(leaveButtonCurrentColor, leaveButtonTargetColor, Time.deltaTime * 3);
            BtnLeaveZoneStripes.color = leaveButtonCurrentColor;

            // update popup
            LeavePopupCG.interactable = leavePopupVisible;
            LeavePopupCG.blocksRaycasts = leavePopupVisible;

            leavePopupAlpha += (leavePopupVisible ? 1 : -1) * Time.unscaledDeltaTime * 3;
            leavePopupAlpha = Mathf.Clamp01(leavePopupAlpha);
            LeavePopupCG.alpha = leavePopupAlpha;

            leavePopupTimeFactor += (leavePopupVisible ? -1 : 1) * Time.unscaledDeltaTime * 3;
            leavePopupTimeFactor = Mathf.Clamp01(leavePopupTimeFactor);
            Utility.TimeController.SetFactor(this.GetHashCode(), leavePopupTimeFactor);
        }

        // -----------------------------------------

        public void DrawSelectionBox (Vector3 p1, Vector3 p2) {
            SelectionBox.Show(p1, p2);
        }
        public void HideSelectionBox () {
            SelectionBox.Hide();
        }

        public void SelectNonVehicleUnit (Controls.Zone.ZoneSelectable unit) {
            // todo: just pull and display data from this unit on UI
            // will just get overriden by something else, so no need to store it anywhere
            // also remember to clear any references to selected vehicles
        }

        public void UpdateVehicleSelection (Vehicles.VehicleController[] selectedVehicles) {
            if (selectedVehicles == null || selectedVehicles.Length == 0) {
                // todo: everything deselected, clear the UI

            }
            else if (selectedVehicles.Length == 1) {
                // todo: selected only 1 vehicle, show data for it and unit control UI elements

            }
            else {
                // todo: selected multiple vehicles, show list of selected and group control UI elements

            }
        }

        public void MoveCameraToVehicle (Vehicles.VehicleController vehicle) {
            if(vehicle == null) return;

            // check if smoothed move
            CameraController.MoveToInstant(vehicle.transform.position);
        }

        public void SetFadeScreen (float val) {
            FadeScreenAlpha = val;
            FadeScreenTarget = val;
        }

        public static GameObject GenerateMinimapVehiclePoint (int vehicleNum) {
            GameObject go = Instantiate<GameObject>(InstanceRef.minimapVehiclePointPrefab);
            go.transform.SetParent(InstanceRef.minimapContentParent.transform, false);
            go.GetComponentInChildren<Text>().text = vehicleNum.ToString();
            return go;
        }

        // -----------------------------------------

        private void UpdateMinimapCamera () {
            if (!InputEnabled) return;

            // half the size of the vertical viewing volume (resolution?)

            float ortoSize = Camera.main.orthographicSize;
            float sizeY = ortoSize * 2;
            float sizeX = sizeY * (Screen.width / (float)Screen.height);
            Vec2Int zoneSize = World.ZoneMapGenerator.ZoneSize;
            sizeY /= zoneSize.Y;
            sizeX /= zoneSize.X;

            Vector3 posOffset = CameraController.transform.position + new Vector3(zoneSize.X * 0.5f, zoneSize.Y * 0.5f);
            posOffset = new Vector3(posOffset.x / zoneSize.X, posOffset.y / zoneSize.Y);

            RectTransform rt = minimapCamera.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(posOffset.x - (sizeX * 0.5f), posOffset.y - (sizeY * 0.5f));
            rt.anchorMax = new Vector2(posOffset.x + (sizeX * 0.5f), posOffset.y + (sizeY * 0.5f));
        }

        // -----------------------------------------

        private void MoveMinimap () {
            if (moveMinimapMouse) {

                // get mouse and minimap transforms
                Vector3 mousePos = Input.mousePosition;
                Vector3[] minimapWorldCorners = new Vector3[4];
                MinimapRenderer.rectTransform.GetWorldCorners(minimapWorldCorners);
                // note: starts bottom left and goes clockwise

                // normalize values
                float xMin = minimapWorldCorners[0].x;
                float yMin = minimapWorldCorners[0].y;
                float xDelta = minimapWorldCorners[2].x - minimapWorldCorners[1].x;
                float yDelta = minimapWorldCorners[1].y - minimapWorldCorners[0].y;
                mousePos = new Vector3((mousePos.x - xMin) / xDelta, (mousePos.y - yMin) / yDelta);

                // translate to world coordinates
                Vec2Int zoneSize = World.ZoneMapGenerator.ZoneSize;
                mousePos = new Vector3(mousePos.x * zoneSize.X - (zoneSize.X * 0.5f), mousePos.y * zoneSize.Y - (zoneSize.Y * 0.5f));

                // move player camera there
                CameraController.MoveToInstant(mousePos);
            }
        }
        public void ClickMinimapDown () {
            moveMinimapMouse = true;
        }
        public void ClickMinimapUp () {
            moveMinimapMouse = false;
        }

        // -----------------------------------------

        public void BtnLeaveZonePress () {

            // check if player vehicle in leave zone
            bool vehLeaveZone = MapController.CheckVehicleLeaveZone();
            
            // if vehicle not in leave, return
            if (!vehLeaveZone) return;

            // exit zone and go back to world view
            StartCoroutine(MapController.LeaveZone());
            return;

        }
        public void BtnLeaveZoneCancelPress () {
            leavePopupVisible = false;
        }
        public void BtnLeaveZoneLeavePress () {
            BtnLeaveLeave.interactable = false;
            BtnLeaveCancel.interactable = false;
            StartCoroutine(MapController.LeaveZone());
        }
    }
}
