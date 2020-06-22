using UnityEngine;
using System.Collections;

namespace Controls {
    public class KeyBind : IInputBind {

        public KeyCode primaryKey;
        public KeyCode secondaryKey;

        public KeyBind () {
            primaryKey = KeyCode.None;
            secondaryKey = KeyCode.None;
        }
        public KeyBind (KeyCode defaultKey1) {
            primaryKey = defaultKey1;
            secondaryKey = KeyCode.None;
        }
        public KeyBind (KeyCode defaultKey1, KeyCode defaultKey2) {
            primaryKey = defaultKey1;
            secondaryKey = defaultKey2;
        }

        public float GetValue () {

            float primaryValue = 0;
            float secondaryValue = 0;

            if (primaryKey != KeyCode.None) {
                primaryValue = UnityEngine.Input.GetKey(primaryKey) ? 1 : 0;
            }
            if (secondaryKey != KeyCode.None) {
                secondaryValue = UnityEngine.Input.GetKey(secondaryKey) ? 1 : 0;
            }

            return primaryValue + secondaryValue;
        }
        public bool IsPressed () {
            return GetValue() != 0;
        }
        public bool IsPressedDown () {
            if (UnityEngine.Input.GetKeyDown(primaryKey) || UnityEngine.Input.GetKeyDown(secondaryKey)) return true;
            return false;
        }
        public bool IsPressedUp () {
            if (UnityEngine.Input.GetKeyUp(primaryKey) || UnityEngine.Input.GetKeyUp(secondaryKey)) return true;
            return false;
        }
        public bool Bind (params object[] parameters) // returns success
        {
            try {
                KeyCode key = (KeyCode)parameters[0];
                int bind = (int)parameters[1];

                if (bind == 0) {
                    primaryKey = key;
                }
                else if (bind == 1) {
                    secondaryKey = key;
                }
                else throw new System.Exception("Attempt to bind tertiary key????");
            }
            catch (System.Exception e) {
                Debug.Log("Error while binding key: " + e.Message);
                return false;
            }

            return true;
        }
        public InputManager.InputType GetInputType () {
            return InputManager.InputType.Key;
        }

    }
}
