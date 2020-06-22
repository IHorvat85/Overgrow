using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Controls.Zone {
    public class Order {

        public enum OrderType {
            None,
            Move,
            MoveOrient,
            Attack,
            AttackMove,
            AttackMoveOrient,
            Ram,
            RamOrient,
            Assist,
            Collect,
        }

        public OrderType Action;
        public Vector2 targetLoc;
        public Vector2 orientLoc;
        public ZoneSelectable targetObj;

        public bool orientMatters;

        public Order (OrderType action, Vector2 target) {
            this.Action = action;
            this.targetLoc = target;
            this.orientLoc = Vector2.zero;
            this.targetObj = null;
            this.orientMatters = false;
        }
        public Order (OrderType action, Vector2 target, Vector2 orientTarget) {
            this.Action = action;
            this.targetLoc = target;
            this.orientLoc = orientTarget;
            this.targetObj = null;
            this.orientMatters = true;
        }
        public Order (OrderType action, ZoneSelectable target) {
            this.Action = action;
            this.targetLoc = Vector2.zero;
            this.orientLoc = Vector2.zero;
            this.targetObj = target;
            this.orientMatters = true;
        }

        public bool SupportsFormation () {
            switch (Action) {
                case OrderType.Move:
                case OrderType.MoveOrient:
                case OrderType.AttackMove:
                case OrderType.AttackMoveOrient:
                case OrderType.Ram:
                case OrderType.RamOrient:
                    return true;
                default:
                    return false;
            }
        }

        public Vector2 GetTargetPosition () {
            return targetLoc;
        }

        // NOTE: CALLED ONLY IN MAIN THREAD
        public void UpdateTargetPosition () {
            if (targetObj != null) {
                targetLoc = targetObj.transform.position;
                orientLoc = targetLoc;
            }
        }

        public override string ToString () {
            string objName = "NULL";
            if (targetObj != null) {
                objName = targetObj.name;
            }
            return Action + " on " + objName + " from " + targetLoc + " to " + orientLoc;
        }
    }
}
