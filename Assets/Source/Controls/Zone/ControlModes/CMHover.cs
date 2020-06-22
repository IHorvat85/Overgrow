using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Controls.Zone.ControlModes {
    class CMHover : IControlMode {
        public void CheckInputs (Vehicles.VehicleController veh) {
            if (!veh.isActiveAndEnabled) return;

            // calculate strafing force
            float upDownMove = KeyStorage.VehicleControls.MoveUpDown.GetValue();
            upDownMove += KeyStorage.VehicleControls.MoveUp.IsPressed() ? 1 : 0;
            upDownMove += KeyStorage.VehicleControls.MoveDown.IsPressed() ? -1 : 0;
            float leftRightMove = KeyStorage.VehicleControls.MoveLeftRight.GetValue();
            leftRightMove += KeyStorage.VehicleControls.MoveRight.IsPressed() ? 1 : 0;
            leftRightMove += KeyStorage.VehicleControls.MoveLeft.IsPressed() ? -1 : 0;

            upDownMove = Mathf.Clamp(upDownMove, -1, 1);
            leftRightMove = Mathf.Clamp(leftRightMove, -1, 1);
            Vector2 moveVector = new Vector2(leftRightMove, upDownMove).normalized;
            
            // calculate rotation (rotate towards mouse)
            Vector3 mouseWorldPos = ZoneMouse.GetMouseWorldPos();
            Vector2 posDiff = mouseWorldPos - veh.transform.position;
            Vector2 posDiffNorm = posDiff.normalized;

            float angleCurr = 360 - veh.transform.rotation.eulerAngles.z;
            float angleTarget = Vector3.Angle(Vector3.up, posDiffNorm);
            if (posDiffNorm.x < 0) angleTarget *= -1;
            float angleDelta = Mathf.DeltaAngle(angleTarget, angleCurr);

            // dampen (-angular velocity)
            float angVelocity = veh.GetComponent<Rigidbody2D>().angularVelocity;
            float angInput = angleDelta - (angVelocity * 0.5f);
            angInput = Mathf.Clamp(angInput / 20, -1, 1);

            // apply force
            veh.ApplyMovementForces(moveVector, angInput);
        }
    }
}
