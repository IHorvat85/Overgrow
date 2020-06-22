using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Language {
    public class LanguageSetter : MonoBehaviour {

        public GameObject RootMenuObject;
        public GameObject TogglePrefab;
        public ToggleGroup ToggleGroup;

        void Start () {

            // TODO: remove this for build, no need to reload language file on every scene change
            Table.LoadFile("English");

            Set();

            GenerateMenuList();
        }

        public void Set () {

            Text[] textBoxes = transform.GetComponentsInChildren<Text>(false);
            if (textBoxes == null) return;
            for (int i = 0; i < textBoxes.Length; i++) {
                if (textBoxes[i] == null) continue;
                string val = Table.Get(textBoxes[i].name);
                if (textBoxes[i].name != val) textBoxes[i].text = val;
            }
        }

        private void GenerateMenuList () {
            if (RootMenuObject == null) return;
            if (TogglePrefab == null) return;

            string[] langs = Table.GetLanguages();

            // first delete any current child objects
            int childs = RootMenuObject.transform.childCount;
            for (int i = childs - 1; i >= 0; i--) {
                GameObject.Destroy(RootMenuObject.transform.GetChild(i).gameObject);
            }

            // resize the panel to fit children
            RootMenuObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 15 + langs.Length * 30);

            for (int i = 0; i < langs.Length; i++) {
                // oh boy....

                GameObject currGO = GameObject.Instantiate(TogglePrefab);

                currGO.transform.SetParent(RootMenuObject.transform);
                currGO.name = "Toggle" + langs[i];

                // set position
                RectTransform rt = currGO.GetComponent<RectTransform>();

                currGO.transform.localScale = new Vector3(1, 1, 1);

                rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, rt.rect.width);
                rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, rt.rect.height);

                rt.anchorMax = new Vector2(1, 1);

                rt.localPosition = new Vector3(0, -10 - (30 * i), 0);

                // set text

                currGO.transform.GetComponentInChildren<Text>().text = langs[i];

                // set toggle group

                currGO.transform.GetComponent<Toggle>().group = ToggleGroup;

                // set toggle state

                currGO.transform.GetComponent<Toggle>().isOn = langs[i] == Table.CurrentLanguage;

            }
        }

        public void ApplyLanguage () {
            foreach (Toggle t in ToggleGroup.ActiveToggles()) {
                if (t.isOn) {

                    string lang = t.transform.GetComponentInChildren<Text>().text;
                    Table.LoadFile(lang);
                    Set();

                    return;
                }
            }
        }
    }
}
