using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utility {
    public class Textures {

        public static Color[,] NormalFromAlpha (Color[,] tex, int sizeX, int sizeY) {

            Color[,] newMap = new Color[sizeX, sizeY];

            for (int x = 0; x < sizeX; x++) {
                for (int y = 0; y < sizeY; y++) {

                    float x1 = GetPixel(tex, x - 1, y, sizeX, sizeY).a;
                    float x2 = GetPixel(tex, x + 1, y, sizeX, sizeY).a;
                    float y1 = GetPixel(tex, x, y - 1, sizeX, sizeY).a;
                    float y2 = GetPixel(tex, x, y + 1, sizeX, sizeY).a;

                    float xDelta = ((x1 - x2) + 1) * 0.5f;
                    float yDelta = ((y1 - y2) + 1) * 0.5f;

                    newMap[x, y] = new Color(xDelta, yDelta, 1, 1);
                }
            }

            return newMap;
        }

        private static Color GetPixel (Color[,] map, int x, int y, int sizeX, int sizeY) {
            if (x < 0) x = sizeX - x;
            if (y < 0) y = sizeY - y;
            x = x % sizeX;
            y = y % sizeY;
            return map[x, y];
        }

        public static Color[] ConvertMapToArray (Color[,] map, int sizeX, int sizeY) {
            Color[] array = new Color[sizeX * sizeY];
            for (int i = 0; i < array.Length; i++) {
                int x = i % sizeX;
                int y = i / sizeX;
                array[i] = map[x, y];
            }
            return array;
        }

        public static Texture2D GenerateTexture (Color[,] pixels, int sizeX, int sizeY) {
            return GenerateTexture(ConvertMapToArray(pixels, sizeX, sizeY), sizeX, sizeY);
        }
        public static Texture2D GenerateTexture (Color[] pixels, int sizeX, int sizeY) {
            Texture2D tex = new Texture2D(sizeX, sizeY);
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

    }
}
