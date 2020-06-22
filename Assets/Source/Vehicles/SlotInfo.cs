using UnityEngine;
using System.Collections;

namespace Vehicles {
    public class SlotInfo {

        public enum SlotTypes {
            Weapon, // ranged
            Defense, // extra armor, shield, spikes (reflect melee dmg), energized hull (better spikes doing electric dmg), flamethrowers
            Ramming, // spikes, dozer blades, ramming grills
            Engineering, // various utility stuff

        }

        public SlotTypes SlotType { get; set; }




    }
}
