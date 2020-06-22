using UnityEngine;
using System.Collections;

namespace TimeOfDay {
    public class TimeManager : MonoBehaviour {

        private static long Time = 0;
        private static float TimeDisplay = 0;

        private const float TimeDisplayIncrementMult = 3600;

        public struct TimeData {

            public int Day;
            public int Hours;
            public int Minutes;
            public int Seconds;

            public TimeData (long time) {
                this.Day = (int)(time / 86400) + 1;
                this.Hours = (int)((time % 86400) / 3600);
                this.Minutes = (int)((time % 3600) / 60);
                this.Seconds = (int)(time % 60);
            }

            public override string ToString () {
                return "Day " + Day + ", " + Hours + ":" + Minutes + ":" + Seconds;
            }
        }

        // --------------------------------------

        void Update () {
            UpdateTimeDisplay();
        }

        private void UpdateTimeDisplay () {
            if (Time == TimeDisplay) return;

            TimeDisplay += TimeDisplayIncrementMult * UnityEngine.Time.deltaTime;
            if (TimeDisplay > Time) TimeDisplay = Time;
        }

        // --------------------------------------

        public static void IncrementTime (int seconds) {
            Time += seconds;
        }

        public static TimeData GetTime () {
            return new TimeData(Time);
        }
        public static TimeData GetTimeDisplay () {
            return new TimeData((long)TimeDisplay);
        }

        public static void SetTime (long val) {
            Time = val;
            TimeDisplay = val;
        }

        // --------------------------------------

        public const int IncrementOnMove = 3600 * 3;

    }
}
