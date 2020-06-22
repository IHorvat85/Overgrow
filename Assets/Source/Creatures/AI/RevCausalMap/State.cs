using UnityEngine;
using System.Collections;

namespace Creatures.AI.RevCausalMap {
    public abstract class State {

        public enum States {

        }

        public abstract bool CheckValid ();
    }
}
