using System;
using UnityEngine;

namespace Utility {
    public class Startup : MonoBehaviour {

        void Start () {

            // todo: load language

            Controls.InputManager.Initialize();

            DefinitionLoader.LoadAllData();

            // todo: remove
            DebugStart();

        }

        void DebugStart () {

            DebugAddVehicle();

            Resources.Convoy.AddDebugResources();

        }

        void DebugAddVehicle () {
            Vehicles.Vehicle v = new Vehicles.Vehicle();

            v.Name = "Starter";
            v.VehicleHullID = "hull_Traveler";
            v.PostDeserialize();

            Resources.Convoy.ChangeVehicle(v);

            Interface.WorldMapUIController.UpdateVehiclePanels();
        }
    }
}
