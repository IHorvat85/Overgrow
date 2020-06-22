using UnityEngine;
using System.Collections;

namespace Controls {
    public interface IInputBind {

        float GetValue ();
        bool IsPressed ();
        bool IsPressedDown ();
        bool IsPressedUp ();
        bool Bind (params object[] parameters);
        InputManager.InputType GetInputType ();


        /* BINDING
	
        var kbWalkForward : KeyCode;
        function OnGUI () {
            var e : Event = Event.current;
            if (e.isKey && e.keyCode != KeyCode.None) {
                Debug.Log("Detected key code: " + e.keyCode);
                kbWalkForward = e.keyCode;
            }
        }
        */
    }
}
