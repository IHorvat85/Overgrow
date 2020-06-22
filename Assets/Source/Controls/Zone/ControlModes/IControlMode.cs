using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Controls.Zone.ControlModes {
    public interface IControlMode {

        void CheckInputs (Vehicles.VehicleController veh);

    }
}
