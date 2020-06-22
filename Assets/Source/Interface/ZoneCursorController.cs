using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Interface {
    public class ZoneCursorController : MonoBehaviour {

        public enum CursorMode {
            Hardware,
            Default,
            Select,
            Move,
            Attack,
            AttackMove,
            Ram,
            Gather,
        }
        public CursorMode currMode;
        public CursorMode dynCurrMode;

        private static ZoneCursorController instanceRef;

        [Header("Static Cursors")]
        public GameObject cursorUtilityPoint;
        public GameObject cursorUtilityPointLarge;
        public GameObject cursorDefault;
        public GameObject cursorSelect;
        public GameObject cursorMove;
        public GameObject cursorAttack;
        public GameObject cursorRam;

        [Header("Dynamic Cursors")]
        public CanvasGroup dynamicCursorParent;
        public GameObject dynOrientCenter;
        public GameObject dynOrientArrow;

        private Vector3 dynamicCursorParentPos;
        private float dynamicCursorTimer = 0;
        private float dynamicCursorSpeed = 5;

        public float orientFlashIntensity = 1.5f;

        // Use this for initialization
        void Start () {
            instanceRef = this;
            ChangeCursor(currMode);
        }

        // Update is called once per frame
        void Update () {
            if (ZoneMapUIController.InstanceRef.InputEnabled) {
                UpdateCursorAnimation();
                UpdateDynamicCursorPosition();
            }
        }

        // --------------------------------------------

        public void UpdateCursorPosition () {
            // note: called from camera's OnPreRender to avoid 1 frame lag
            this.transform.position = Input.mousePosition;
        }

        public void UpdateDynamicCursorPosition () {
            // this doesn't suffer from lag so it doesn't matter

            // update alpha
            Color baseColor = (Color)Utility.ColorData.MainGreen;
            switch (dynCurrMode) {
                case CursorMode.Attack:
                case CursorMode.AttackMove:
                    baseColor = Utility.ColorData.MainRed;
                    break;
                case CursorMode.Ram:
                    baseColor = Utility.ColorData.MainOrange;
                    break;
                case CursorMode.Gather:
                    // todo: set gathering color (blue?)
                    break;
            }

            float colorMult = 1;
            if (dynamicCursorTimer > 1.5f) {
                colorMult = (1 - (Mathf.Abs(0.5f - (dynamicCursorTimer - 1.5f)) * 2)) * orientFlashIntensity + 1;
            }
            dynamicCursorParent.alpha = dynamicCursorTimer > 1 ? 1 : dynamicCursorTimer;

            dynOrientArrow.GetComponent<Image>().color = baseColor * colorMult;
            dynOrientCenter.GetComponent<Image>().color = baseColor * colorMult;
        }

        public void ChangeCursor (CursorMode mode) {
            currMode = mode;

            cursorSelect.SetActive(mode == CursorMode.Select);
            cursorMove.SetActive(mode == CursorMode.Move || mode == CursorMode.AttackMove);
            cursorDefault.SetActive(mode == CursorMode.Default);
            cursorAttack.SetActive(mode == CursorMode.Attack || mode == CursorMode.AttackMove);
            cursorRam.SetActive(mode == CursorMode.Ram);

            // todo: other cursor modes here
            
        }
        public void SetAllCursors (bool state) {
            cursorSelect.SetActive(state);
            cursorMove.SetActive(state);
            cursorDefault.SetActive(state);
            cursorAttack.SetActive(state);
            cursorRam.SetActive(state);

            // todo: other cursor modes here

        }

        public static void SetCursor (CursorMode mode) {
            if (instanceRef == null) return;

            instanceRef.cursorUtilityPointLarge.SetActive(false);
            if (Controls.Zone.ZoneMouse.IsMouseOnUI()) instanceRef.ChangeCursor(CursorMode.Default);
            else {
                instanceRef.cursorUtilityPoint.SetActive(mode == CursorMode.Select);
                instanceRef.ChangeCursor(mode);
            }
        }
        public static void SetCursorOrient (CursorMode mode, Vector3 startDrag) {
            Vector3 orientVector = (Controls.Zone.ZoneMouse.GetMouseWorldPos() - (Vector3)startDrag).normalized;

            if (instanceRef == null) return;

            float dynDistance = Vector3.Distance(Camera.main.WorldToScreenPoint(instanceRef.dynamicCursorParentPos), Input.mousePosition);
            bool pointCursorOnly = dynDistance < 100;

            if (pointCursorOnly) instanceRef.SetAllCursors(false);
            else instanceRef.ChangeCursor(mode);

            instanceRef.dynCurrMode = mode;
            instanceRef.dynOrientArrow.SetActive(true);
            instanceRef.dynOrientCenter.SetActive(true);
            instanceRef.cursorUtilityPoint.SetActive(false);
            instanceRef.cursorUtilityPointLarge.SetActive(pointCursorOnly);
            
            RectTransform rt = instanceRef.dynOrientArrow.GetComponent<RectTransform>();

            // calculate rotation
            float angle = Vector2.Angle(orientVector, Vector2.up);
            if (orientVector.x > 0) angle *= -1;
            rt.rotation = Quaternion.Euler(0, 0, angle);

            // calculate position
            orientVector += new Vector3(1, 1);
            orientVector /= 2f;
            rt.anchorMin = orientVector;
            rt.anchorMax = orientVector;
        }

        public static void RestoreMouseCursor () {
            Cursor.visible = true;
        }

        // --------------------------------------------

        private void UpdateCursorAnimation () {
            Cursor.visible = currMode == CursorMode.Hardware;

            switch (currMode) {
                case CursorMode.Select:
                    UpdateSelectAnimation();
                    break;
                case CursorMode.Move:
                case CursorMode.Attack:
                case CursorMode.AttackMove:
                    UpdateAttackMoveAnimation();
                    break;
                case CursorMode.Ram:
                    UpdateRamAnimation();
                    break;
                case CursorMode.Gather:
                    // todo: gather cursor animation
                    break;
                default:
                    break;
            }
        }

        // --------------------------------------------

        private float animSelectT = 0;
        private const float animSelectSpeed = 2.5f;
        private const float animSelectPixels = 6;
        private void UpdateSelectAnimation () {

            animSelectT += Time.unscaledDeltaTime * animSelectSpeed;
            animSelectT %= 2;

            float state = Mathf.Abs(1 - animSelectT);

            Vector2 offset = new Vector2(state, state) * animSelectPixels;

            cursorSelect.GetComponent<RectTransform>().offsetMin = offset;
            cursorSelect.GetComponent<RectTransform>().offsetMax = -offset;
        }

        private float animAttackMoveT = 0;
        private const float animAttackMoveSpeed = 3f;
        private const float animMovePixels = 5;
        private const float animAttackPixels = 5;
        private void UpdateAttackMoveAnimation () {

            animAttackMoveT += Time.unscaledDeltaTime * animAttackMoveSpeed;
            animAttackMoveT %= 2;

            float state = Mathf.Abs(1 - animAttackMoveT);

            Vector2 offsetMove = new Vector2(state, state) * animMovePixels;
            cursorMove.GetComponent<RectTransform>().offsetMin = offsetMove;
            cursorMove.GetComponent<RectTransform>().offsetMax = -offsetMove;

            state = 1 - state;
            Vector2 offsetAttack = new Vector2(state, state) * animAttackPixels;
            cursorAttack.GetComponent<RectTransform>().offsetMin = offsetAttack;
            cursorAttack.GetComponent<RectTransform>().offsetMax = -offsetAttack;
        }

        private float animRamT = 0;
        private const float animRamSpeed = 3f;
        private const float animRamPixels = 5;
        private void UpdateRamAnimation () {
            animRamT += Time.unscaledDeltaTime * animRamSpeed;
            animRamT %= 2;

            float state = Mathf.Abs(1 - animRamT);

            Vector2 offset = new Vector2(state, state) * animRamPixels;

            cursorRam.GetComponent<RectTransform>().offsetMin = offset;
            cursorRam.GetComponent<RectTransform>().offsetMax = -offset;
        }

    }
}
