using UnityEngine;
using System.Collections;

namespace Controls {
    public class AxisBind : IInputBind {

        public string axis = "";
        public float sensitivity = 1;
        public bool inverted = false;
        public float deadzone = 0.05f; // 0 to 1

        public AxisBind () { }
        public AxisBind (string defaultAxis) {
            axis = defaultAxis;
        }

        public float GetValue () {
            try {
                float axisValue = UnityEngine.Input.GetAxisRaw(axis);

                if (Mathf.Abs(axisValue) <= deadzone) axisValue = 0;

                axisValue *= sensitivity;
                axisValue *= inverted ? -1 : 1;

                return axisValue;
            }
            catch (System.Exception) {
                return 0;
            }
        }
        public bool IsPressed () {
            return GetValue() != 0;
        }
        public bool IsPressedDown () {
            return false;
        }
        public bool IsPressedUp () {
            return false;
        }
        public bool Bind (params object[] parameters) // returns success
        {
            try {
                axis = (string)parameters[0];
            }
            catch (System.Exception e) {
                Debug.Log("Error while binding key: " + e.Message);
                return false;
            }

            return true;
        }
        public InputManager.InputType GetInputType () {
            return InputManager.InputType.Axis;
        }

    }
}
