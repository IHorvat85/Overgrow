using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace World.ProcGen {
    public static class Noise {

        public static float[] GetNoiseArray (int length) {
            float[] array = new float[length];
            System.Random rand = new Random();
            for (int i = 0; i < array.Length; i++) {
                array[i] = (float)((rand.NextDouble() * 2) - 1);
            }
            return array;
        }


        public static float[] GetNoiseMapSimplex (int size, int seed, float density) {
            return GetNoiseMapSimplex(size, size, seed, density);
        }
        public static float[] GetNoiseMapSimplex (int sizeX, int sizeY, int seed, float density) {

            LibNoise.Perlin perlinNoise = new LibNoise.Perlin();
            perlinNoise.Seed = seed;
            perlinNoise.NoiseQuality = LibNoise.NoiseQuality.Standard;
            perlinNoise.Lacunarity = 2;

            int pixelCount = sizeX * sizeY;

            float[] map = new float[pixelCount];
            for (int i = 0; i < pixelCount; i++) {
                float x = (float)(i % sizeX) / sizeX * density;
                float y = (float)(i / sizeX) / sizeX * density;
                map[i] = ((float)perlinNoise.GetValue(x, y, 1) + 1) / 2;
            }

            return map;
        }

        public static float[] GetNoiseMapRandom (int size, int seed) {
            return GetNoiseMapRandom(size, size, seed);
        }
        public static float[] GetNoiseMapRandom (int sizeX, int sizeY, int seed) {
            System.Random rand = new System.Random(seed);

            int pixelCount = sizeX * sizeY;
            float[] map = new float[pixelCount];
            for (int i = 0; i < pixelCount; i++) {
                map[i] = (float)rand.NextDouble();
            }

            return map;
        }

        public static float[] GetNoiseRidgedMultifractal (int size, int seed, int octaveCount) {
            return GetNoiseRidgedMultifractal(size, size, seed, octaveCount);
        }
        public static float[] GetNoiseRidgedMultifractal (int sizeX, int sizeY, int seed, int octaveCount) {

            LibNoise.RidgedMultifractal gen = new LibNoise.RidgedMultifractal();
            // todo: set values

            gen.Seed = seed;
            gen.Frequency = 1; //  0.03;
            gen.Lacunarity = 2f;
            gen.NoiseQuality = LibNoise.NoiseQuality.Standard;
            gen.OctaveCount = 5;

            int pixelCount = sizeX * sizeY;

            float[] map = new float[pixelCount];
            for (int i = 0; i < pixelCount; i++) {
                float x = (float)(i % sizeX) / sizeX;
                float y = (float)(i / sizeX) / sizeX;
                map[i] = ((float)gen.GetValue(x, y, 1) + 1) / 2;
            }

            return map;
        }
    }
}
