using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace World.Weather {
    public class SunController : MonoBehaviour {

        public float WindSpeed = 2;
        public Vector2 WindDirection = new Vector2(-1, 0.7f);

        void Start () {

        }

        void Update () {
            transform.position += (Vector3)(WindDirection * WindSpeed * Time.deltaTime);
        }

    }
}
