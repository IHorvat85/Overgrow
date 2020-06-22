using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Creatures.AI.EmotionalSim {
    public class EmotionalEvent {

        public EmotionalEvent (EventID id, float resistMult, params KeyValuePair<EmotionalSimulation.EmotionTypes, float>[] emotionalImpacts) {
            this.ID = id;
            this.ResistMult = resistMult;
            this.EmotionalImpacts = emotionalImpacts;
        }

        public enum EventID {
            
            noiseLoud, // loud sudden noise - gunshot, thunder etc
            noiseConsistent, // prolonged irritating sound
            noiseAnnoying, // think nails on a blackboard
            
            painHeavy,
            painConsistent, // think toothache, lasting for hours

            ateFood,
            ateFoodTasty,
            ateFoodDisgusting,

            // todo: playing satisfaction
            // todo: curiousity satisfaction
        }

        public EventID ID { get; set; }
        public float ResistMult { get; set; }
        public KeyValuePair<EmotionalSimulation.EmotionTypes, float>[] EmotionalImpacts { get; set; }

    }
}
