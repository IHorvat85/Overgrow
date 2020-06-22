using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Creatures.AI.NeedHierarchy {
    public class Need : IComparable<Need> {

        public enum Types {
            safety, // run, hide, reduce pain
            food,
            shelter, // find / build shelter
            knowledge, // curiousity, investigate
            fun, // practice various skills (hunting, fighting)
            vent, // vent anger (attack something)
        }

        public float BasePriority { get; set; }
        public float Priority { get; set; }
        
        public Types Type { get; set; }

        public Need (Types type, float basePriority) {
            this.Type = type;
            this.BasePriority = basePriority;
        }

        public int CompareTo (Need other) {
            return this.Priority.CompareTo(other.Priority);
        }
    }
}
