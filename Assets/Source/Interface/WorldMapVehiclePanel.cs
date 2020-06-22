using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Interface {
    public class WorldMapVehiclePanel : MonoBehaviour {

        public int PanelIndex;

        private Vehicles.Vehicle assignedVehicle = null;

        public CanvasGroup cgPanelInfo;
        public CanvasGroup cgPanelLines;

        public Button button;

        public Image imageIcon;
        public Text textNumber;
        public Image imageBar;

        // ---------------------------------

        private float linesAlphaTarget = 1;
        const float linesAlphaLerpT = 0.1f;

        private float panelAnchorTarget = 0.4f;
        const float panelAnchorLerpT = 0.3f;

        // ---------------------------------

        void Start () {

        }

        void Update () {

            UpdateCGAlphas();

            UpdatePanelAnchor();

        }

        // ---------------------------------

        private void UpdateCGAlphas () {
            cgPanelInfo.alpha = Mathf.Lerp(cgPanelInfo.alpha, 1 - linesAlphaTarget, linesAlphaLerpT);
            cgPanelLines.alpha = Mathf.Lerp(cgPanelLines.alpha, linesAlphaTarget, linesAlphaLerpT);
        }

        private void UpdatePanelAnchor () {
            Vector2 currAnchorMin = GetComponent<RectTransform>().anchorMin;
            float newAnchorMinY = Mathf.Lerp(currAnchorMin.y, panelAnchorTarget, panelAnchorLerpT);
            GetComponent<RectTransform>().anchorMin = new Vector2(currAnchorMin.x, newAnchorMinY);
        }

        private void RefreshVehicleInfo () {
            // update data from assignedVehicle to UI elements
            if (assignedVehicle != null) {
                
                // update icon
                Texture2D vehIcon = Utility.DefinitionLoader.GetImage(assignedVehicle.Hull.IconID);
                Sprite s = Sprite.Create(vehIcon, new Rect(0, 0, vehIcon.width, vehIcon.height), new Vector2(0.5f, 0.5f));
                imageIcon.sprite = s;
                imageIcon.color = new Color(0.5f, 0.5f, 0.5f);

                // update number text
                textNumber.text = (PanelIndex + 1).ToString();

                // update bar ?



            }
        }

        public void AssignVehicle (Vehicles.Vehicle vehicle) {
            this.assignedVehicle = vehicle;
            
            if (vehicle == null) {
                linesAlphaTarget = 1;
                panelAnchorTarget = 1;
                button.interactable = false;
                cgPanelInfo.interactable = false;
                cgPanelInfo.blocksRaycasts = false;
            }
            else {
                linesAlphaTarget = 0;
                panelAnchorTarget = 0;
                button.interactable = true;
                cgPanelInfo.interactable = true;
                cgPanelInfo.blocksRaycasts = true;
            }

            RefreshVehicleInfo();
        }

        public void ButtonPress () {

            // todo: toggle the inventory menu for this vehicle

        }

        public void RefreshAssignedVehicle () {
            AssignVehicle(Resources.Convoy.GetVehicle());
        }
    }
}
