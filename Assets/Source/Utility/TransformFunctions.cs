using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utility {
    public static class TransformFunctions {

        private const float e = 2.71828f;

        // --------------------------------------

        public static float SmoothStart (float t, int pow) {
            float p = t;
            for (int i = 1; i < pow; i++) {
                p *= t;
            }
            return p;
        }
        public static float SmoothStop (float t, int pow) {
            t = 1 - t;
            t = SmoothStart(t, pow);
            return 1 - t;
        }
        public static float SmoothStep (float t, int pow) {
            float sstart = SmoothStart(t, pow);
            float sstop = SmoothStop(t, pow);
            return Mathf.Lerp(sstart, sstop, t);
        }

        // --------------------------------------

        public static float Arch (float t) {
            return t * (1 - t) * 4;
        }

    }
}
