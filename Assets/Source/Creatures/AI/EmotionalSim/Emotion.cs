using UnityEngine;
using System.Collections;

namespace Creatures.AI.EmotionalSim {
    public class Emotion {

        public EmotionalSimulation.EmotionTypes EmotionType;

        public float CurrValue { get; set; }
        public float DefaultValue { get; set; }

        public float DefaultingSpeedDefault { get; set; }
        public float DefaultingSpeed { get; set; }
        // note: points per second (0-100)

        public Emotion (EmotionalSimulation.EmotionTypes type, float defaultValue, float defaultingSpeed) {
            this.EmotionType = type;
            
            this.DefaultValue = defaultValue;
            this.CurrValue = defaultValue;

            this.DefaultingSpeedDefault = defaultingSpeed;
            this.DefaultingSpeed = defaultingSpeed;
        }

        public void SetDefaultingSpeed (float val) {
            DefaultingSpeed = val;
        }

        public void UpdateDefaulting () {
            // note: should be called every frame

            float delta = DefaultValue - CurrValue;
            delta = Mathf.Clamp(delta, -DefaultingSpeed, DefaultingSpeed);
            CurrValue = CurrValue + (delta * DefaultingSpeed * Time.deltaTime);
        }

        public void SetValue (float val) {
            CurrValue = val;
            ClampValue();
        }

        public void TriggerChange (float delta) {
            CurrValue += delta;
            ClampValue();
        }

        private void ClampValue () {
            CurrValue = Mathf.Clamp(CurrValue, 0, 100);
        }

    }
}
