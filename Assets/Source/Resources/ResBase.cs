using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Resources {

    [XmlType("ResBase")]
    [Serializable]
    public class ResBase : Utility.IDeserialized {

        public const string XMLRoot = "Resources";

        public void PostDeserialize () {

            // get the name and description from the language table
            this.DisplayName = Language.Table.Get(ID + "Name");
            this.DisplayDescription = Language.Table.Get(ID + "Desc");

            // register in the resource database
            Database.RegisterResource(this);
        }

        public override string ToString () {
            return ID + " - " + DisplayName + " - " + DisplayDescription;
        }

        // -----------------------------------------------------

        [XmlElement("ResID")]
        public string ID { get; set; }

        [XmlElement("Name")]
        public string DisplayName { get; set; }
        [XmlElement("Description")]
        public string DisplayDescription { get; set; }

        [XmlElement("Volume")]
        public float Volume { get; set; } // per unit
        [XmlElement("Mass")]
        public float Mass { get; set; } // per unit

        [XmlElement("Stackable")]
        public bool Stackable { get; set; }

        [XmlElement("IconID")]
        public string IconID { get; set; }

        
    }
}
