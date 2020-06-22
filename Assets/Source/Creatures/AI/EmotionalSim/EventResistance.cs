using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Creatures.AI.EmotionalSim {
    class EventResistance {

        public EmotionalEvent.EventID ID { get; set; }
        public float Multiplier { get; set; }
        public float DefaultMult { get; set; }

        public EventResistance (EmotionalEvent.EventID id, float defaultVal) {
            this.ID = id;
            this.DefaultMult = defaultVal;
            this.Multiplier = defaultVal;
        }

        public void UpdateDefaulting () {
            Multiplier = Mathf.Lerp(Multiplier, DefaultMult, Time.deltaTime * 0.1f);
        }

        public void TriggerResist (float newValue) {
            Multiplier = Mathf.Lerp(0, Multiplier, newValue);
        }
    }
}
