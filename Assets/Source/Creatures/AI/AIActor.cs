using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Creatures.AI {
    public class AIActor : MonoBehaviour {

        private EmotionalSim.EmotionalSimulation emotionalSim;
        private NeedHierarchy.NeedHierarchy needHierarchy;

        // --------------------------------------------
        // todo: Debug, remove
        public EmotionalSim.EmotionalSimulation DebugGetEmotionalSim () {
            return emotionalSim;
        }
        public NeedHierarchy.NeedHierarchy DebugGetNeedHierarchy () {
            return needHierarchy;
        }
        // --------------------------------------------

        void Start () {
            emotionalSim = new EmotionalSim.EmotionalSimulation();
            needHierarchy = new NeedHierarchy.NeedHierarchy();
        }

        void Update () {

            // todo: check if game paused..?
            UpdateEmotions();
            UpdateNeedHierarchy();

        }

        // --------------------------------------------

        private void UpdateEmotions () {
            
            // update defaulting
            emotionalSim.UpdateDefaulting();

            // EmotionalSim.Emotion[] emotions = emotionalSim.GetEmotions();
            // todo: update animations based on current emotions
        }

        private void UpdateNeedHierarchy () {
            needHierarchy.UpdateEmotions(emotionalSim.GetEmotions());
        }
    }
}
