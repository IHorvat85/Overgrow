using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Controls.Zone {
    public class ZoneCamera : MonoBehaviour {

        public Interface.ZoneCursorController cursorController;

        private static Vehicles.VehicleController PlayerVehicle;

        private const float MouseOffsetFactor = 0.3f;
        private Vector3 MouseOffset = Vector3.zero;

        private const float CameraPosZ = -10;

        // Zoom variables

        public static float ZoomSpeed = 10;
        public static float ZoomLerpT = 0.5f;

        public static float ZoomMin = 10;
        public static float ZoomMax = 50;

        private float zoomTarget = 20;

        // -----------------------

        void OnPreCull () {
            CheckCameraZoom();
            transform.position = RecalculateVehicleTargetPos();
            cursorController.UpdateCursorPosition();
        }

        // -------------------------------------------------
        // Private Control Functions

        private void CheckCameraZoom () {
            float zoomInput = KeyStorage.ZoneCamera.ZoomCamera.GetValue();
            zoomTarget -= zoomInput * ZoomSpeed;

            if (zoomTarget < ZoomMin) zoomTarget = ZoomMin;
            if (zoomTarget > ZoomMax) zoomTarget = ZoomMax;

            float currZoom = Camera.main.orthographicSize;

            Camera.main.orthographicSize = Mathf.Lerp(currZoom, zoomTarget, ZoomLerpT);
        }

        // -------------------------------------------------
        // Public Control Functions

        public static void SetPlayerVehicle (Vehicles.VehicleController veh) {
            PlayerVehicle = veh;
        }

        public void MoveToInstant (Vector3 pos) {
            pos = CorrectCoord(pos);
            transform.position = pos;
        }

        // -------------------------------------------------
        // Utility Functions

        private Vector3 RecalculateVehicleTargetPos () {
            if (PlayerVehicle == null) return Vector3.zero;

            // calc camera position for camera
            // include offset for movement and mouse aim

            Vector3 vehiclePos = PlayerVehicle.transform.position;

            Vector3 mousePos = ZoneMouse.GetMouseWorldPos();
            MouseOffset = Vector3.Lerp(MouseOffset, mousePos, 2f * Time.deltaTime);

            Vector3 targetPos = Vector3.Lerp(vehiclePos, MouseOffset, MouseOffsetFactor);

            return CorrectCoord(targetPos);
        }

        private Vector3 CorrectCoord (Vector3 pos) {

            // correct Z
            pos.z = CameraPosZ;

            // clamp X and Y
            Utility.Vec2Int zoneSize = World.ZoneMapGenerator.ZoneSize;
            zoneSize = new Utility.Vec2Int(zoneSize.X / 2, zoneSize.Y / 2);
            if (pos.x < -zoneSize.X) pos.x = -zoneSize.X;
            if (pos.x > zoneSize.X) pos.x = zoneSize.X;
            if (pos.y < -zoneSize.Y) pos.y = -zoneSize.Y;
            if (pos.y > zoneSize.Y) pos.y = zoneSize.Y;

            return pos;
        }

    }
}
