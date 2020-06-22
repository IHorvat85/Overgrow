using UnityEngine;
using System.Collections;

namespace Creatures.AI.RevCausalMap {
    public class Map {

        public Link[] PlanAction (State desiredState) {

            if (desiredState.CheckValid()) return null;

            // todo: find all links that achieve desiredState

            
            

            // todo: for each (recursively) check validity of action conditions

            // todo: construct list of possible paths

            // todo: evaluate possible paths (cost and side-effects)



            return null;
        }

        private void BuildActionPathTree (State desiredState) {
            // todo: build a tree of all possible paths to desired state

            ActionPathTree tree = new ActionPathTree(desiredState);



        }

        private Link[] GetLinksThatAchieve (State resultingState) {

            // todo: get all causal links that achieve given state

            return null;
        }
    }
}
