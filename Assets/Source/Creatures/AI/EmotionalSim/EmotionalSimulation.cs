using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Creatures.AI.EmotionalSim {
    public class EmotionalSimulation {

        public enum EmotionTypes {
            Fear,
            Anger,
            Happiness,
            Hunger,
            Boredom,
            Comfort, // mainly regarding shelter
        }

        private Emotion emoFear;
        private Emotion emoAnger;
        private Emotion emoHappiness;
        private Emotion emoHunger;
        private Emotion emoBoredom;
        private Emotion emoComfort;

        private Emotion[] allEmotions;
        private EventResistance[] EmoEventResists;

        private Emotion GetEmotionFromType (EmotionTypes type) {
            switch (type) {
                case EmotionTypes.Fear:
                    return emoFear;
                case EmotionTypes.Anger:
                    return emoAnger;
                case EmotionTypes.Happiness:
                    return emoHappiness;
                case EmotionTypes.Hunger:
                    return emoHunger;
                case EmotionTypes.Boredom:
                    return emoBoredom;
                case EmotionTypes.Comfort:
                    return emoComfort;
                default:
                    return null;
            }
        }

        // -----------------------------------

        public EmotionalSimulation () {
            InitializeEmotions();
            InitializeEmotionalResists();
        }

        // -----------------------------------

        private void InitializeEmotions () {
            emoFear = new Emotion(EmotionTypes.Fear, 0, 5);
            emoAnger = new Emotion(EmotionTypes.Anger, 0, 2);
            emoHappiness = new Emotion(EmotionTypes.Happiness, 50, 1);
            emoHunger = new Emotion(EmotionTypes.Hunger, 100, 0.15f); // 0.15 = 0-100 in about 10 mins
            emoBoredom = new Emotion(EmotionTypes.Boredom, 100, 1);
            emoComfort = new Emotion(EmotionTypes.Comfort, 50, 0);
            allEmotions = new Emotion[] {
                emoFear,
                emoAnger,
                emoHappiness,
                emoHunger,
                emoBoredom,
                emoComfort,
            };
        }

        private void InitializeEmotionalResists () {
            int emoEventCnt = System.Enum.GetNames(typeof(EmotionalEvent.EventID)).Length;
            EmoEventResists = new EventResistance[emoEventCnt];
            for (int i = 0; i < emoEventCnt; i++) {
                // todo: get default resist from creature archetype
                // note: softcoded xml?
                EmoEventResists[i] = new EventResistance((EmotionalEvent.EventID)i, 1);
            }
        }

        // -----------------------------------

        public void UpdateDefaulting () {
            // loop thru all emotions and update defaulting
            for (int i = 0; i < allEmotions.Length; i++) {
                allEmotions[i].UpdateDefaulting();
            }

            // update resist defaulting
            for (int i = 0; i < EmoEventResists.Length; i++) {
                EmoEventResists[i].UpdateDefaulting();
            }
        }

        public Emotion[] GetEmotions () {
            return allEmotions;
        }
        public Emotion GetEmotion (EmotionTypes emType) {
            return GetEmotionFromType(emType);
        }

        // -----------------------------------
        // Emotional Events

        public void TriggerEvent (EmotionalEvent emEvent) {
            KeyValuePair<EmotionTypes, float>[] impacts = emEvent.EmotionalImpacts;
            for (int i = 0; i < impacts.Length; i++) {
                Emotion emotion = GetEmotionFromType(impacts[i].Key);

                float realDelta = impacts[i].Value * EmoEventResists[(int)emEvent.ID].Multiplier;
                emotion.TriggerChange(realDelta);

                // todo: execute animations and stuff based on realDelta

            }

            // update resist to event
            EmoEventResists[(int)emEvent.ID].TriggerResist(emEvent.ResistMult);
        }
    }
}
