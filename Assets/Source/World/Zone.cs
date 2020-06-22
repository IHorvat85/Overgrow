using UnityEngine;
using System.Collections;

namespace World {
    public class Zone : Interface.ITooltipData {

        // general info on this zone
        public Resources.ResourceInfo Resources { get; set; }

        // todo: enemy count (or threat level)

        private int _Seed;
        public int Seed {
            get {
                return _Seed;
            }
        }

        public bool DataGenerated { get; set; }

        public Zone (int seed) {
            this._Seed = seed;
        }

        public string GetTooltipText () {
            return _Seed.ToString();
        }

        public Vector2 GetTooltipSize () {
            return new Vector2(100, 50);
        }

    }
}
