using UnityEngine;
using System.Collections;

namespace Controls {
    public class WorldCamera : MonoBehaviour {

        // -------------------------------
        // Position variables

        public float MoveLerpT = 0.05f;

        private Vector3 moveTarget = Vector3.zero;

        // -------------------------------
        // Zoom variables

        public float ZoomSpeed = 5;
        public float ZoomLerpT = 0.5f;

        public float ZoomMin = 3.5f;
        public float ZoomMax = 12;

        private float zoomTarget = 5;

        // -------------------------------

        void Start () {
            // set camera position and zoom to default values

            // todo: set camera to player position on world
            Camera.main.orthographicSize = zoomTarget;
        }

        void Update () {

            UpdateZoom();

            UpdatePosition();

        }

        private void UpdateZoom () {
            float zoomInput = KeyStorage.WorldCamera.ZoomCamera.GetValue();
            zoomTarget -= zoomInput * ZoomSpeed;

            if (zoomTarget < ZoomMin) zoomTarget = ZoomMin;
            if (zoomTarget > ZoomMax) zoomTarget = ZoomMax;

            float currZoom = Camera.main.orthographicSize;

            Camera.main.orthographicSize = Mathf.Lerp(currZoom, zoomTarget, ZoomLerpT);
        }

        private void UpdatePosition () {
            Utility.Vec2Int playerPos = World.WorldMapController.PlayerPos;
            moveTarget = new Vector3(playerPos.X + 0.5f, playerPos.Y + 0.5f, -10);
            transform.position = Vector3.Lerp(transform.position, moveTarget, MoveLerpT);
        }
    }
}
