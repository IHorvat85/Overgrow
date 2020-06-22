using UnityEngine;
using System.Collections;

namespace Test {
    public class MapGenerator : MonoBehaviour {

        // Use this for initialization
        void Start () {
            Regenerate();
        }

        // Update is called once per frame
        void Update () {
            if (Input.GetKeyDown(KeyCode.F)) {
                Regenerate();
            }
        }

        private void Regenerate () {

            int sizeX = 256;
            int sizeY = 256;

            int pixelCount = sizeX * sizeY;
            Color[] pixels = new Color[pixelCount];

            LibNoise.RidgedMultifractal a = new LibNoise.RidgedMultifractal();
            // todo: set values

            a.Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            a.Frequency = 0.03;
            a.Lacunarity = 2f;
            a.NoiseQuality = LibNoise.NoiseQuality.Standard;
            a.OctaveCount = 5;

            float min = float.MaxValue;
            float max = float.MinValue;

            for (int i = 0; i < pixelCount; i++) {

                int xPos = i % sizeX;
                int yPos = i / sizeX;

                float val = (float)(a.GetValue(xPos, yPos, 0) + 1) / 2.4219f;

                if (val < min) min = val;
                if (val > max) max = val;

                // is val really from 0 to 1 ?
                pixels[i] = new Color(val, val, val);
            }

            Debug.Log("Min: " + min.ToString("F4") + ", Max: " + max.ToString("F4"));

            // now create and apply texture
            Texture2D tex = new Texture2D(sizeX, sizeY);
            tex.SetPixels(pixels);
            tex.Apply();

            GetComponent<MeshRenderer>().material.mainTexture = tex;

        }
    }
}
