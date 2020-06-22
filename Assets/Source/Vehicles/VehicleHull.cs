using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Vehicles {

    [XmlType("VehicleHull")]
    [Serializable]
    public class VehicleHull : Utility.IDeserialized {

        public const string XMLRoot = "VehicleHulls";

        public static Dictionary<string, VehicleHull> Database;
        static VehicleHull () {
            Database = new Dictionary<string, VehicleHull>();
        }

        public void PostDeserialize () {
            Database.Add(ID, this);
        }

        public override string ToString () {
            string text = ID + "\n" +
                ClassName + "\n" +
                GraphicIDDiffuse + "\n" +
                PowerScore + "\n" +
                HP + "\n" +
                Mass + "\n" +
                CargoVolume + "\n" +
                EngineEfficiency + "\n";
            return text;
        }

        // ---------------------------------------------

        public enum ControlMode {
            Car,
            Tank,
            Hover,
        }

        [XmlElement("ControlMode")]
        public ControlMode CtrlMode { get; set; }

        [XmlElement("HullID")]
        public string ID { get; set; }

        [XmlElement("HullClass")]
        public string ClassName { get; set; }

        [XmlElement("GraphicIDDiffuse")]
        public string GraphicIDDiffuse { get; set; }
        [XmlElement("GraphicIDEmission")]
        public string GraphicIDEmission { get; set; }

        [XmlElement("IconID")]
        public string IconID { get; set; }

        [XmlElement("PowerScore")]
        public int PowerScore { get; set; }

        [XmlElement("Size")]
        public Vector2 Size { get; set; }

        // todo: slots for modules
        // todo: slots for spaced armor, spikes, ramming grills...

        [XmlElement("NumberRendererSize")]
        public Vector2 NumberRendererSize { get; set; }
        [XmlElement("NumberRendererPos")]
        public Vector2 NumberRendererPos { get; set; }

        [XmlElement("SelectionBoxSize")]
        public Vector2 SelectionBoxSize { get; set; }

        [XmlArray("CollisionPolygon")]
        public Vector2[] CollisionPolygon { get; set; }

        // base stats

        [XmlElement("HP")]
        public int HP { get; set; }

        [XmlElement("Mass")]
        public float Mass { get; set; } // kg

        [XmlElement("CargoVolume")]
        public float CargoVolume { get; set; } // m3

        [XmlElement("EngineEfficiency")]
        public int EngineEfficiency { get; set; }
        // (fuel burned per tiles moved)

        [XmlElement("EnginePowerStrafe")]
        public float EnginePowerStrafe { get; set; }
        [XmlElement("EnginePowerTurn")]
        public float EnginePowerTurn { get; set; }

        [XmlElement("StrafeMobility")]
        public float StrafeMobility { get; set; }

    }
}
