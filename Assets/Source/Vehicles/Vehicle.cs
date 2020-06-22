using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Resources;
using System.Linq;
using System.Xml.Serialization;

namespace Vehicles {
    public class Vehicle : Utility.IDeserialized {

        public string VehicleHullID;
        
        public VehicleHull Hull { get; set; }

        public string Name { get; set; }

        public Dictionary<string, int> Inventory;

        public Vehicle () {
            Inventory = new Dictionary<string, int>();
        }
        
        public void SetInventory (Dictionary<string, int> inventory) {
            this.Inventory = inventory;
        }

        public void PostDeserialize () {
            VehicleHull hull;
            if (VehicleHull.Database.TryGetValue(VehicleHullID, out hull)) this.Hull = hull;
            else Utility.ErrorLog.ReportError("Cannot assign hull " + VehicleHullID + " to vehicle " + Name + " - not found!");
        }

        // -----------------------------------------

        public float GetCargoVolume () {
            float totalVolume = Hull.CargoVolume;

            // todo: modifiers from modules

            return totalVolume;
        }

        public float GetInventoryVolume () {
            KeyValuePair<string, int>[] inventoryArray = Inventory.ToArray();

            float totalVolume = 0;
            for (int i = 0; i < inventoryArray.Length; i++) {
                totalVolume += Database.GetResource(inventoryArray[i].Key).Volume * inventoryArray[i].Value;
            }
            return totalVolume;
        }

        // -----------------------------------------
        // Cargo / Resources

        public int GetResourceAmount (string id) {
            int amount;
            if (Inventory.TryGetValue(id, out amount)) return amount;
            return 0;
        }

        public int TryAddResource (string id, int amount, bool onlyTest = false) {
            if (amount == 0) return 0;

            // find out how much volume this would take
            ResBase resource = Database.GetResource(id);
            if (resource == null) {
                Utility.ErrorLog.ReportError("Vehicle.TryAddResource received non-existent resource with id " + id);
                return 0;
            }

            int amountThatFits = SeeHowMuchFits(resource, amount);

            // now add amountThatFits
            if (!onlyTest) {
                int currentAmount = 0;
                if (Inventory.TryGetValue(id, out currentAmount)) Inventory.Remove(id);
                Inventory.Add(id, currentAmount + amountThatFits);
            }

            return amountThatFits;
        }
        private int SeeHowMuchFits (ResBase res, int amount) {
            float freeVolume = GetCargoVolume() - GetInventoryVolume();

            float resVolume = res.Volume;
            int amountThatFits = 0;

            for (int i = 1; i <= amount; i++) {
                float currVolume = resVolume * i;
                if (currVolume <= freeVolume) amountThatFits = i;
                else break;
            }
            return amountThatFits;
        }

        public int TryRemoveResource (string id, int amount, bool onlyTest = false) {
            if (amount == 0) return 0;

            int currAmount;
            if (Inventory.TryGetValue(id, out currAmount)) {
                int amountRemoved = currAmount >= amount ? amount : currAmount;
                if (onlyTest) return amountRemoved;
                else {
                    Inventory.Remove(id);
                    Inventory.Add(id, currAmount - amountRemoved);
                    return amountRemoved;
                }
            }
            else return 0;
        }

        // -----------------------------------------

        public int CalculateFuelUse () {
            // note: per tile moved
            return Hull.EngineEfficiency;
        }

        public float CalculateTotalMass () {

            // mass of the vehicle hull
            float totalMass = Hull.Mass;

            // todo: mass of installed modules

            // mass of all items in inventory
            KeyValuePair<string, int>[] items = Inventory.ToArray();
            for (int i = 0; i < items.Length; i++) {
                ResBase resource = Database.GetResource(items[i].Key);
                if (resource != null) totalMass += resource.Mass * items[i].Value;
            }

            return totalMass;
        }

        // -----------------------------------------
        // Movement

        public float GetStrafeForce () {
            float power = Hull.EnginePowerStrafe;
            
            // todo: apply modifiers from modules, damage and environment
            // note: ignore mass here

            return power;
        }
        public float GetRotationForce () {
            float power = Hull.EnginePowerTurn;
            
            // todo: apply modifiers from modules, damage and environment
            // note: ignore mass here

            return power;
        }

    }
}
