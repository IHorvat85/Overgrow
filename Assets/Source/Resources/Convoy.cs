using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vehicles;

namespace Resources {
    public static class Convoy {

        public static void AddDebugResources () {
            AddResource("res_fuel", 200);
        }

        // ------------------------------------

        private static Vehicle PlayerVehicle;

        // ------------------------------------

        public static Vehicle GetVehicle () {
            return PlayerVehicle;
        }

        public static void ChangeVehicle (Vehicle newVehicle) {
            if (PlayerVehicle == null) {
                PlayerVehicle = newVehicle;
                return;
            }

            // todo: transfer inventory to newVehicle

            PlayerVehicle = newVehicle;

        }

        // ------------------------------------

        #region Resource Functions

        public static int CalculateFuelUse () {
            if (PlayerVehicle == null) return 0;
            return PlayerVehicle.CalculateFuelUse();
        }

        private static bool CheckCanAdd (string id, int amount) {
            if (PlayerVehicle == null) return false;
            amount -= PlayerVehicle.TryAddResource(id, amount, true);
            return amount <= 0;
        }

        public static int GetResourceAmount (string id) {
            if (PlayerVehicle == null) return 0;
            return PlayerVehicle.GetResourceAmount(id);
        }
        public static int AddResource (string id, int amount, bool testOnly = false) {
            if (PlayerVehicle == null) return 0;

            int originalAmount = amount;
            amount -= PlayerVehicle.TryAddResource(id, amount, testOnly);

            return originalAmount - amount;
        }
        public static int SubtractResource (string id, int amount, bool testOnly = false) {
            if (PlayerVehicle == null) return 0;

            int originalAmount = amount;
            amount -= PlayerVehicle.TryRemoveResource(id, amount, testOnly);

            return originalAmount - amount;
        }

        #endregion
    }
}
