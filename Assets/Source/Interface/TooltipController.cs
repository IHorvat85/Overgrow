using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Interface {
    public class TooltipController : MonoBehaviour {

        public Text textBox;
        public CanvasGroup cgTooltip;

        // ------------------------------

        private float cgAlphaTarget = 0;
        private float cgAlphaLerpT = 0.1f;

        private float posLerpT = 0.35f;

        private int defaultPosOffsetX = 15;

        // ------------------------------

        void Update () {
            cgTooltip.alpha = Mathf.Lerp(cgTooltip.alpha, cgAlphaTarget, cgAlphaLerpT);
            PositionTooltip();
        }

        public void SetTooltip (ITooltipData tooltipData) {
            if (tooltipData == null) {
                cgAlphaTarget = 0;
                return;
            }

            string text = tooltipData.GetTooltipText();
            textBox.text = text;
            cgAlphaTarget = 0.8f;

            // recalculate and apply the size
            GetComponent<RectTransform>().sizeDelta = tooltipData.GetTooltipSize();

            PositionTooltip();
        }

        private void PositionTooltip () {

            Vector3 mousePos = Input.mousePosition;
            Vector2 size = GetComponent<RectTransform>().sizeDelta;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            Vector2 pos = mousePos + new Vector3(defaultPosOffsetX, size.y / 2, 0);
            
            if (pos.x + size.x > screenSize.x) pos = new Vector2(mousePos.x - size.x - defaultPosOffsetX, pos.y);

            // todo: check for out of bounds vertically.

            // -----------------------

            Vector3 currPos = GetComponent<RectTransform>().position;
            GetComponent<RectTransform>().position = Vector3.Lerp(currPos, pos, posLerpT);

        }
    }
}
