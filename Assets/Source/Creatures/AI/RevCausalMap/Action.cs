using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Creatures.AI.RevCausalMap {
    public class Action {

        protected State[][] Conditions;

        public bool CheckConditions () {
            if (Conditions == null) return true;
            for (int i = 0; i < Conditions.Length; i++) {
                bool validSet = true;
                for (int j = 0; j < Conditions[i].Length; j++) {
                    if (!Conditions[i][j].CheckValid()) {
                        validSet = false;
                        break;
                    }
                }
                if (validSet) return true;
            }
            return false;
        }

    }
}
