using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Utility {
    public class LoadScreenManager : MonoBehaviour {

        private static LoadScreenManager InstanceRef = null;

        private static float CurrentProgress = 0;
        private static float ProgressSoft = 0;

        public static bool IsDone = false;
        public static bool ReceiveInput = true;

        // -------------------------------------

        public Image loadBar;

        public Text text;

        // -------------------------------------

        private float textFlashT = 0;
        private const float TextFlashSpeed = 2.5f;

        private float loadScreenAlpha = 0;
        private float loadScreenAlphaTarget = 1;

        // -------------------------------------

        void Start () {
            InstanceRef = this;

            loadScreenAlpha = 0;
            loadScreenAlphaTarget = 1;

            text.text = Language.Table.Get("gui_loading");
        }

        void Update () {

            UpdateProgressBars();

            UpdateTextFlash();

            UpdateDone();

            UpdateFade();

            UpdateActive();

        }

        private void UpdateProgressBars () {
            ProgressSoft = Mathf.Lerp(ProgressSoft, CurrentProgress, 0.1f);

            float progressFill = ProgressSoft;

            if (progressFill < 0) progressFill = 0;
            if (progressFill > 1) progressFill = 1;

            loadBar.fillAmount = progressFill;
        }

        private void UpdateTextFlash () {
            textFlashT += Time.deltaTime * TextFlashSpeed;
            float alpha = (Mathf.Sin(textFlashT) + 1) / 2;
            text.color = new Color(0.5f, 0.5f, 0.5f, 0.6f + (alpha * 0.4f));
        }

        private void UpdateDone () {
            if (IsDone && ReceiveInput) {
                InstanceRef.text.text = Language.Table.Get("gui_loadingComplete");
                CurrentProgress = 1;

                if (Input.anyKeyDown) {
                    // trigger next phase
                    loadScreenAlphaTarget = 0;
                    Interface.ZoneMapUIController.Activate();
                    ReceiveInput = false;
                }
            }
        }

        private void UpdateFade () {
            loadScreenAlpha = Mathf.Lerp(loadScreenAlpha, loadScreenAlphaTarget, 0.1f);
            GetComponent<CanvasGroup>().alpha = loadScreenAlpha;
        }

        private void UpdateActive () {
            if (loadScreenAlphaTarget <= 0 && loadScreenAlpha <= 0.01f) gameObject.SetActive(false);
        }

        public static void UpdateProgress (float value) {
            if (value > CurrentProgress) CurrentProgress = value;
        }

        public static void SetText (string val) {
            InstanceRef.text.text = val;
        }

        public static void Reset () {
            CurrentProgress = 0;
            ProgressSoft = 0;

            IsDone = false;
            ReceiveInput = true;

        }
    }
}
