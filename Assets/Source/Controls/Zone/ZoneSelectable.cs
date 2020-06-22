using UnityEngine;
using System.Collections;

namespace Controls.Zone {
    public class ZoneSelectable : MonoBehaviour {

        public UnityEngine.UI.Image SelectionBox;
        public UnityEngine.UI.Text NumberUI;

        public enum SelectableType {
            PointOfInterest,
            Vehicle,
            Enemy,
            Resource,
        }
        public SelectableType SelType = SelectableType.PointOfInterest;

        public Vector2 BaseSize;
        public Vector2 SelBoxTargetRotSize;
        public Vector2 SelBoxRotSize;

        private float flashT = 1;
        private const float flashSpeed = 8;
        private const float flashMult = 2;

        // Use this for initialization
        void Start () {
            Deselect();
        }

        // Update is called once per frame
        void Update () {
            
            AdjustSelectionBox();

            UpdateFlash();

        }

        // --------------------------------------------

        public Vehicles.VehicleController VehController;

        // --------------------------------------------

        public void AdjustSelectionBox () {

            // find screen position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

            SelectionBox.transform.position = screenPos;
            SelectionBox.transform.rotation = Quaternion.Euler(0, 0, 0);

            RectTransform rt = SelectionBox.GetComponent<RectTransform>();

            // calculate rotation size delta
            Vector2 forwardVector = transform.up;
            float rotFactor = Mathf.Abs(forwardVector.y);
            float xAdj = (BaseSize.x * rotFactor) + (BaseSize.y * (1 - rotFactor));
            float yAdj = (BaseSize.y * rotFactor) + (BaseSize.x * (1 - rotFactor));
            SelBoxTargetRotSize = new Vector2(xAdj, yAdj);
            SelBoxRotSize = Vector2.Lerp(SelBoxRotSize, SelBoxTargetRotSize, Time.deltaTime * 2);

            // calculate zoom size factor
            float zoomFactor = (1 - (Camera.main.orthographicSize / ZoneCamera.ZoomMax));
            zoomFactor = Mathf.Pow(zoomFactor, 1.5f);
            zoomFactor = zoomFactor * 0.7f + 0.3f;
            rt.sizeDelta = SelBoxRotSize * 3 * (zoomFactor * 0.9f);

        }

        public void Select () {
            SelectionBox.enabled = true;
            if (NumberUI != null) {
                NumberUI.enabled = true;
            }

            // flash to signal selection
            flashT = 0;
        }
        public void Deselect () {
            SelectionBox.enabled = false;
            if (NumberUI != null) {
                NumberUI.enabled = false;
            }
        }

        // --------------------------------------------

        private void UpdateFlash () {
            if (flashT >= 1) return;

            flashT += Time.deltaTime * flashSpeed;
            float val = Mathf.Sin(flashT * Mathf.PI) + 1;
            if (flashT >= 1) val = 1; // for the last tick

            SelectionBox.color = GetBaseColor() * val;

        }

        private Color GetBaseColor () {
            switch (SelType) {
                case SelectableType.PointOfInterest:
                    return new Color(0.5f, 0.5f, 0.5f, 1);
                case SelectableType.Vehicle:
                    return new Color32(0, 96, 32, 255);
                case SelectableType.Enemy:
                    return new Color32(192, 32, 0, 255);
                case SelectableType.Resource:
                    return new Color(0.5f, 0.5f, 0.5f, 1);
                default:
                    return new Color(0.5f, 0.5f, 0.5f, 1);
            }
        }
    }
}
