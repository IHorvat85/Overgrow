using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class XMLSerializerHelper : MonoBehaviour {

	// Use this for initialization
	void Start () {

        string folderPath = "Test";
        if (!Directory.Exists(folderPath)) return;

        XmlSerializer ds = new XmlSerializer(typeof(Vehicles.VehicleHull), new XmlRootAttribute(Vehicles.VehicleHull.XMLRoot));

        string path = folderPath + "/" + "vehicleHull.xml";


        Vehicles.VehicleHull vh = new Vehicles.VehicleHull();
        vh.CollisionPolygon = new Vector2[]{
            new Vector2(0,0),
            new Vector2(1,1),
            new Vector2(2,2),
        };
        vh.CtrlMode = Vehicles.VehicleHull.ControlMode.Car;

        using (XmlWriter writer = XmlWriter.Create(path)) {
            ds.Serialize(writer, vh);
        }

	}
	
}
