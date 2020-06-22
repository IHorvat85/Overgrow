using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utility {
    public static class TimeController {

        private static List<KeyValuePair<int, float>> TimeFactors;

        static TimeController () {
            TimeFactors = new List<KeyValuePair<int, float>>();
        }

        public static void SetFactor (int key, float value) {
            for (int i = 0; i < TimeFactors.Count; i++) {
                if (TimeFactors[i].Key == key) {
                    TimeFactors[i] = new KeyValuePair<int, float>(key, value);
                    UpdateTimeScale();
                    return;
                }
            }
            TimeFactors.Add(new KeyValuePair<int, float>(key, value));
            UpdateTimeScale();
        }

        // --------------------------------------------------

        private static void UpdateTimeScale () {
            float totalScale = 1;
            for (int i = 0; i < TimeFactors.Count; i++) {
                totalScale *= TimeFactors[i].Value;
            }
            Time.timeScale = totalScale;
        }

    }
}
