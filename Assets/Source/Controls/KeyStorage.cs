using UnityEngine;
using System.Collections;

namespace Controls {
    public static class KeyStorage {

        public static void Initialize () {
            if (!TryLoadFromFile()) SetDefaults();
        }

        static bool TryLoadFromFile () {

            // todo: try load keys from file
            // if cannot for any reason, return false

            return false;
        }

        static void SetDefaults () {

            #region World Camera
            WorldCamera.ZoomCamera = new AxisBind("Mouse ScrollWheel");
            #endregion

            #region Zone Camera
            ZoneCamera.ZoomCamera = new AxisBind("Mouse ScrollWheel");
            #endregion

            #region Vehicle Controls
            VehicleControls.MoveUpDown = new AxisBind();
            VehicleControls.MoveLeftRight = new AxisBind();
            VehicleControls.MoveUp = new KeyBind(KeyCode.W, KeyCode.UpArrow);
            VehicleControls.MoveDown = new KeyBind(KeyCode.S, KeyCode.DownArrow);
            VehicleControls.MoveLeft = new KeyBind(KeyCode.A, KeyCode.LeftArrow);
            VehicleControls.MoveRight = new KeyBind(KeyCode.D, KeyCode.RightArrow);
            #endregion

        }

        // Keys, grouped by functionality

        public class WorldCamera {
            public static AxisBind ZoomCamera;
        }

        public class ZoneCamera {
            public static AxisBind ZoomCamera;
        }

        public class VehicleControls {
            public static AxisBind MoveUpDown;
            public static AxisBind MoveLeftRight;

            public static KeyBind MoveUp;
            public static KeyBind MoveDown;
            public static KeyBind MoveLeft;
            public static KeyBind MoveRight;
        }
    }
}
