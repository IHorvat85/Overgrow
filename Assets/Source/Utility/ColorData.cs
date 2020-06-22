using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utility {
    public static class ColorData {

        public static Color32 MainGreen = new Color32(0, 96, 32, 255);
        public static Color32 MainRed = new Color32(192, 32, 0, 255);
        public static Color32 MainOrange = new Color32(192, 128, 0, 255);
        public static Color32 MainBlue = new Color32(0, 128, 192, 255);

        public static Color32 GetOrderColor (Controls.Zone.Order.OrderType orderType) {
            switch (orderType) {
                case Controls.Zone.Order.OrderType.Attack:
                case Controls.Zone.Order.OrderType.AttackMove:
                case Controls.Zone.Order.OrderType.AttackMoveOrient:
                    return MainRed;
                case Controls.Zone.Order.OrderType.Ram:
                case Controls.Zone.Order.OrderType.RamOrient:
                    return MainOrange;
                case Controls.Zone.Order.OrderType.Assist:
                case Controls.Zone.Order.OrderType.Collect:
                    return MainBlue;
                default:
                    return MainGreen;
                
            }
        }

    }
}
