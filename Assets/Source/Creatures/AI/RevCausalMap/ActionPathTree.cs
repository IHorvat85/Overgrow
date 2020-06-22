using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creatures.AI.RevCausalMap {

    class ActionPathTreeNode {

        public ActionPathTreeNode ParentNode { get; set; }
        // note: if null its a root child

        public Link CausalLink { get; set; }
        public ActionPathTreeNode[] ChildNodes { get; set; }

        public ActionPathTreeNode (ActionPathTreeNode parent, Link link) {
            this.ParentNode = parent;
            this.CausalLink = link;

            // todo: get link action condition count
            // todo: initialize ChilDNodes array with that length

        }
    }

    class ActionPathTree {
        public State FinalState { get; set; }
        public List<ActionPathTreeNode> RootLinks { get; set; }

        public ActionPathTree (State finalState) {
            this.FinalState = finalState;
            this.RootLinks = new List<ActionPathTreeNode>();
        }
    }
}
