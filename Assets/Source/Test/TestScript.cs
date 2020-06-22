using UnityEngine;
using System.Collections;

public class TestScript : MonoBehaviour {

    public Utility.LineRendererCustom lineRenderer;

	// Use this for initialization
	void Start () {
        Vector2[] path = new Vector2[20];
        path[0] = Vector2.zero;
        for (int i = 0; i < path.Length; i++) {
            float x = ((float)i + 1) / path.Length;
            float y = Utility.TransformFunctions.SmoothStep(x, 2);
            path[i] = new Vector2(x, y);
        }
        lineRenderer.SetAll(path, 2);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
