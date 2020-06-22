using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Controls.Zone {
    class ZoneMouse {

        public static Vector3 GetMouseWorldPos () {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10;
            return Camera.main.ScreenToWorldPoint(mousePos);
        }

        public static bool IsMouseOnUI () {
            if (UnityEngine.EventSystems.EventSystem.current == null) return false;
            return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
    }
}
