using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Test {
    public class FramerateMonitor : MonoBehaviour {

        private const int DataPointCount = 128;
        private const int ValueResolution = 100;

        private float[] deltaT;
        private int deltaTIndex = 0;

        // -----------------------------------

        public UnityEngine.UI.RawImage Image;

        // -----------------------------------

        void Start () {
            deltaT = new float[DataPointCount];
        }

        void Update () {

            deltaT[deltaTIndex] = Time.deltaTime;
            deltaTIndex++;
            deltaTIndex %= DataPointCount;

            UpdateRenderer();

        }

        private void UpdateRenderer () {

            Color[] colorMap = new Color[DataPointCount * ValueResolution];

            Color colorA = new Color(1, 1, 1, 1);
            Color colorB = new Color(1, 1, 1, 0);
            Color currPos = new Color(1, 0, 0, 1);

            // generate horizontally for optimization
            int val60FPS = ValueResolution - 60;
            for (int i = 0; i < DataPointCount; i++) {

                float val = 1f / deltaT[i];
                val = ValueResolution - val;

                if (val < 0) val = 0;
                if (val >= ValueResolution) val = ValueResolution;

                int rowOffset = i * ValueResolution;

                int valI = (int)val;
                for (int j = 0; j < ValueResolution; j++) {
                    int cMapIndex = j + rowOffset;

                    if (i == deltaTIndex) colorMap[cMapIndex] = currPos;
                    else if (j == val60FPS) colorMap[cMapIndex] = currPos;
                    else if (j < valI) colorMap[cMapIndex] = colorB;
                    else colorMap[cMapIndex] = colorA;
                }
            }

            Texture2D tex = new Texture2D(ValueResolution, DataPointCount);
            tex.SetPixels(colorMap);

            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            tex.Apply();

            Texture currTex = Image.texture;
            if (currTex != null) {
                Image.texture = null;
                Destroy(currTex);
            }

            Image.texture = tex;
        }

        // -----------------------------------

    }
}
