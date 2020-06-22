using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Creatures.AI.NeedHierarchy {
    public class NeedHierarchy {

        private List<Need> Needs;

        public NeedHierarchy () {
            InitializeNeeds();
        }

        private void InitializeNeeds () {

            needFood = new Need(Need.Types.food, 0);
            needSafety = new Need(Need.Types.safety, 0);
            needShelter = new Need(Need.Types.shelter, 0);
            needFun = new Need(Need.Types.fun, 0);
            needKnowledge = new Need(Need.Types.knowledge, 0);
            needVent = new Need(Need.Types.vent, 0);

            Needs = new List<Need>();
            Needs.Add(needFood);
            Needs.Add(needSafety);
            Needs.Add(needShelter);
            Needs.Add(needFun);
            Needs.Add(needKnowledge);
            Needs.Add(needVent);
        }

        private Need needFood;
        private Need needSafety;
        private Need needShelter;
        private Need needFun;
        private Need needKnowledge;
        private Need needVent;

        // ----------------------------------------------

        public void UpdateEmotions (EmotionalSim.Emotion[] emotions) {
            // update need priority based on emotions

            #region first get emotion values
            float fearVal = 0;
            float angerVal = 0;
            float happinessVal = 0;
            float hungerVal = 0;
            float boredomVal = 0;
            float comfortVal = 0;
            for (int i = 0; i < emotions.Length; i++) {
                EmotionalSim.Emotion currEmo = emotions[i];
                switch (currEmo.EmotionType) {
                    case Creatures.AI.EmotionalSim.EmotionalSimulation.EmotionTypes.Fear:
                        fearVal = currEmo.CurrValue;
                        break;
                    case Creatures.AI.EmotionalSim.EmotionalSimulation.EmotionTypes.Anger:
                        angerVal = currEmo.CurrValue;
                        break;
                    case Creatures.AI.EmotionalSim.EmotionalSimulation.EmotionTypes.Happiness:
                        // note: care, this is 50 +/-
                        happinessVal = currEmo.CurrValue;
                        break;
                    case Creatures.AI.EmotionalSim.EmotionalSimulation.EmotionTypes.Hunger:
                        hungerVal = currEmo.CurrValue;
                        break;
                    case Creatures.AI.EmotionalSim.EmotionalSimulation.EmotionTypes.Boredom:
                        boredomVal = currEmo.CurrValue;
                        break;
                    case Creatures.AI.EmotionalSim.EmotionalSimulation.EmotionTypes.Comfort:
                        // note: care, this is 50 +/-
                        comfortVal = currEmo.CurrValue;
                        break;
                }
            }
            #endregion

            float shelterPriority = comfortVal - angerVal + fearVal;
            float foodPriority = hungerVal;
            float funPriority = boredomVal - fearVal - angerVal;
            float knowledgePriority = -angerVal;
            float safetyPriority = fearVal - angerVal - hungerVal - boredomVal;
            float ventPriority = angerVal;

            needShelter.Priority = shelterPriority;
            needSafety.Priority = safetyPriority;
            needFood.Priority = foodPriority;
            needFun.Priority = funPriority;
            needKnowledge.Priority = knowledgePriority;
            needVent.Priority = ventPriority;

        }

        public Need GetPriorityNeed () {
            Needs.Sort();
            return Needs.Last(); // sort is ascending
        }
    }
}
