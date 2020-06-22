using UnityEngine;
using System.Collections;

namespace Controls {
    public static class InputManager {

        public enum InputType {
            Key,
            Axis,
        }

        public static void Initialize () {
            KeyStorage.Initialize();
        }
    }
}
