using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Utility {
    public struct Vec2Int {

        public int X { get; set; }
        public int Y { get; set; }

        public Vec2Int (int x, int y) {
            this.X = x;
            this.Y = y;
        }

        public static readonly Vec2Int Zero = new Vec2Int(0, 0);

        public static readonly Vec2Int Invalid = new Vec2Int(-1, -1);

        // -----------------------------------------------------------

        public static Vec2Int operator + (Vec2Int a, Vec2Int b) {
            return new Vec2Int(a.X + b.X, a.Y + b.Y);
        }
        public static Vec2Int operator - (Vec2Int a, Vec2Int b) {
            return new Vec2Int(a.X - b.X, a.Y - b.Y);
        }
        public static Vec2Int operator * (Vec2Int a, Vec2Int b) {
            return new Vec2Int(a.X * b.X, a.Y * b.Y);
        }
        public static bool operator == (Vec2Int a, Vec2Int b) {
            return a.X == b.X && a.Y == b.Y;
        }
        public static bool operator != (Vec2Int a, Vec2Int b) {
            return a.X != b.X || a.Y != b.Y;
        }

        public static implicit operator Vector2 (Vec2Int a) {
            return new Vector2(a.X, a.Y);
        }
        public static implicit operator Vector3 (Vec2Int a) {
            return new Vector3(a.X, a.Y, 0);
        }

        public static Vec2Int operator * (Vec2Int a, int b) {
            return new Vec2Int(a.X * b, a.Y * b);
        }

        public override bool Equals (object obj) {
            return GetHashCode() == obj.GetHashCode();
        }

        // -----------------------------------------------------------
        
        public static float Distance (Vec2Int a, Vec2Int b) {
            return Mathf.Sqrt(Mathf.Pow(a.X - b.X, 2) + Mathf.Pow(a.Y - b.Y, 2));
        }
        public static float DistanceManhattan (Vec2Int a, Vec2Int b) {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        public override int GetHashCode () {
            var A = (uint)(X >= 0 ? 2 * X : -2 * X - 1);
            var B = (uint)(Y >= 0 ? 2 * Y : -2 * Y - 1);
            var C = (int)((A >= B ? A * A + A + B : A + B * B) / 2);
            return X < 0 && Y < 0 || X >= 0 && Y >= 0 ? C : -C - 1;
        }

        // -----------------------------------------------------------

        public class Vec2IntComparer : IEqualityComparer<Vec2Int> {

            public bool Equals (Vec2Int x, Vec2Int y) {
                return x.X == y.X && x.Y == y.Y;
            }

            public int GetHashCode (Vec2Int obj) {
                return obj.GetHashCode();
            }
        }

        // -----------------------------------------------------------

        public override string ToString () {
            return this.X + ":" + this.Y;
        }
    }
}
