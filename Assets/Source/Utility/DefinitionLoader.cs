using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace Utility {
    public static class DefinitionLoader {
        // reads all xml files and fills databases

        private const string FolderRoot = "definitions";

        private const string FolderResources = "Resources";
        private const string FolderGraphics = "Graphics";
        private const string FolderVehicleHulls = "VehicleHulls";
        private const string FolderVehicleModules = "VehicleModules";

        private static bool LoadCompleted = false;

        // ------------------------------------------------

        private static Dictionary<string, Texture2D> RepoImage;

        // ------------------------------------------------

        public static Texture2D GetImage (string id) {
            Texture2D tex = null;
            RepoImage.TryGetValue(id, out tex);
            return tex;
        }

        // ------------------------------------------------

        public static void LoadAllData () {
            if (LoadCompleted) return;

            // initialize repositories
            InitializeRepos();

            // graphics
            LoadGraphicsRecursively(FolderRoot + "/" + FolderGraphics);

            // resources
            LoadResources();

            // vehicle hulls
            LoadVehicleHulls();

            // modules
            LoadVehicleModules();

            LoadCompleted = true;
        }

        private static void InitializeRepos () {
            RepoImage = new Dictionary<string, Texture2D>();
            Resources.Database.Initialize();
        }

        // ------------------------------------------------

        private static bool CheckFileFormat (string path, string format) {
            return path.Split('.').Last().Trim().ToLower() == format.ToLower();
        }

        // ------------------------------------------------

        private static void LoadResources () {
            string folderPath = FolderRoot + "/" + FolderResources;
            if (!Directory.Exists(folderPath)) return;

            XmlSerializer ds = new XmlSerializer(typeof(List<Resources.ResBase>), new XmlRootAttribute(Resources.ResBase.XMLRoot));

            string[] files = Directory.GetFiles(folderPath);
            for (int i = 0; i < files.Length; i++) {

                string path = files[i];
                string fileName = path.Replace("\\", "/").Split('/').Last();

                if (!CheckFileFormat(path, "xml")) continue;

                try {
                    using (XmlReader reader = XmlReader.Create(path)) {

                        List<Resources.ResBase> list = (List<Resources.ResBase>)ds.Deserialize(reader);
                        for (int j = 0; j < list.Count; j++) {
                            list[j].PostDeserialize();
                        }
                    }
                }
                catch (Exception e) {
                    Utility.ErrorLog.ReportError("Error deserializing Resource (" + fileName + "): ", e);
                }
            }
        }

        private static void LoadVehicleHulls () {
            string folderPath = FolderRoot + "/" + FolderVehicleHulls;
            if (!Directory.Exists(folderPath)) return;

            XmlSerializer ds = new XmlSerializer(typeof(List<Vehicles.VehicleHull>), new XmlRootAttribute(Vehicles.VehicleHull.XMLRoot));
            
            string[] files = Directory.GetFiles(folderPath);
            for (int i = 0; i < files.Length; i++) {
                
                string path = files[i];
                string fileName = path.Replace("\\", "/").Split('/').Last();

                if (!CheckFileFormat(path, "xml")) continue;

                try {
                    using (XmlReader reader = XmlReader.Create(path)) {

                        List<Vehicles.VehicleHull> list = (List<Vehicles.VehicleHull>)ds.Deserialize(reader);
                        for (int j = 0; j < list.Count; j++) {
                            list[j].PostDeserialize();
                        }
                    }
                }
                catch (Exception e) {
                    Utility.ErrorLog.ReportError("Error deserializing VehicleHull (" + fileName + "): ", e);
                }
            }
        }

        private static void LoadVehicleModules () {
            // todo: this
        }

        private static void LoadGraphicsRecursively (string dir) {

            // get a list of all files in this folder
            string[] files = Directory.GetFiles(dir);
            for (int i = 0; i < files.Length; i++) {
                
                // if jpeg or png, load image into repo
                string path = files[i];
                string fileName = path.Replace("\\", "/").Split('/').Last().Split('.').First().Trim();

                if (!CheckFileFormat(path, "png")) continue;

                try {
                    // load file as byte array
                    byte[] imageData = File.ReadAllBytes(path);

                    // convert to texture and save into repo
                    Texture2D tex2D = new Texture2D(8, 8, TextureFormat.Alpha8, true);
                    if (tex2D.LoadImage(imageData)) {

                        tex2D.filterMode = FilterMode.Point;

                        RepoImage.Add(fileName, tex2D);
                    }
                }
                catch (Exception e) {
                    Utility.ErrorLog.ReportError("Error loading image data at " + path, e);
                }
            }

            // get a list of all folders in this folder
            string[] directories = Directory.GetDirectories(dir);
            for (int i = 0; i < directories.Length; i++) {
                // for each directory, call this recursively
                LoadGraphicsRecursively(directories[i]);
            }
        }
    }
}
