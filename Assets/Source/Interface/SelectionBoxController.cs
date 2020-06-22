using UnityEngine;
using System.Collections;

public class SelectionBoxController : MonoBehaviour {

    float alpha = 0;
    float alphaTarget = 0;
    const float alphaLerpT = 5;

    const float SizeMin = 16;

    public Color colorBase;

    void Update () {
        UpdateColor();
    }

    // ----------------------------------
    // Public Control Methods

    public void Show (Vector3 p1, Vector3 p2) {
        alphaTarget = 1;
        SetPosition(p1, p2);
    }
    public void Hide () {
        alphaTarget = 0;
        alpha = 0;
    }

    public void SetColor (Color color) {
        colorBase = color;
    }

    // ----------------------------------
    // Private Utility Methods

    private void SetPosition (Vector3 p1, Vector3 p2) {
        // note: p1 and p2 are world pos

        Vector3 p1s = Camera.main.WorldToScreenPoint(p1);
        Vector3 p2s = Camera.main.WorldToScreenPoint(p2);

        Vector3 size = new Vector3(Mathf.Abs(p1s.x - p2s.x), Mathf.Abs(p1s.y - p2s.y), 0);

        if (size.x < SizeMin) size.x = SizeMin;
        if (size.y < SizeMin) size.y = SizeMin;

        RectTransform rt = GetComponent<RectTransform>();
        rt.position = p1s - ((p1s - p2s) / 2f);
        rt.sizeDelta = size;
    }

    private void UpdateColor () {
        alpha = Mathf.Lerp(alpha, alphaTarget, alphaLerpT * Time.unscaledDeltaTime);
        GetComponent<UnityEngine.UI.Image>().color = new Color(colorBase.r, colorBase.g, colorBase.b, alpha);
    }
}
