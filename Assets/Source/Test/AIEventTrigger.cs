using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Creatures.AI;
using Creatures.AI.EmotionalSim;
using Creatures.AI.NeedHierarchy;
using Creatures.AI.RevCausalMap;

public class AIEventTrigger : MonoBehaviour {

    private EmotionalSimulation emotionalSim;
    private NeedHierarchy needHierarchy;

    // ------------------------------------------

    public GUISkin guiSkin;

    [Header("Emotions DEBUG")]
    public string DebugFear = "0.00";
    public string DebugAnger = "0.00";
    public string DebugHappiness = "0.00";
    public string DebugHunger = "0.00";
    public string DebugBoredom = "0.00";
    public string DebugComfort = "0.00";

    [Header("Needs DEBUG")]
    // todo: display needs

    public string DebugNeed = "";

    // ------------------------------------------

	// Use this for initialization
	void Start () {
        emotionalSim = GetComponent<AIActor>().DebugGetEmotionalSim();
        needHierarchy = GetComponent<AIActor>().DebugGetNeedHierarchy();
        InitializeEvents();
	}
	
	// Update is called once per frame
	void Update () {
        UpdateValues();
	}

    // ------------------------------------------

    private List<KeyValuePair<string, EmotionalEvent>> EventsList;

    private void InitializeEvents () {
        EventsList = new List<KeyValuePair<string, EmotionalEvent>>();

        // events here
        EventsList.Add(new KeyValuePair<string, EmotionalEvent>("pain heavy",
            new EmotionalEvent(EmotionalEvent.EventID.painHeavy, 0.8f, new KeyValuePair<EmotionalSimulation.EmotionTypes, float>[] {
                new KeyValuePair<EmotionalSimulation.EmotionTypes, float>(EmotionalSimulation.EmotionTypes.Anger, 50),
                new KeyValuePair<EmotionalSimulation.EmotionTypes, float>(EmotionalSimulation.EmotionTypes.Fear, 25),
                new KeyValuePair<EmotionalSimulation.EmotionTypes, float>(EmotionalSimulation.EmotionTypes.Boredom, -25),
            })));
        EventsList.Add(new KeyValuePair<string, EmotionalEvent>("loud noise",
            new EmotionalEvent(EmotionalEvent.EventID.noiseLoud, 0.5f, new KeyValuePair<EmotionalSimulation.EmotionTypes, float>[] {
                new KeyValuePair<EmotionalSimulation.EmotionTypes, float>(EmotionalSimulation.EmotionTypes.Fear, 50),
                new KeyValuePair<EmotionalSimulation.EmotionTypes, float>(EmotionalSimulation.EmotionTypes.Boredom, -25),
            })));
        EventsList.Add(new KeyValuePair<string, EmotionalEvent>("eat food",
            new EmotionalEvent(EmotionalEvent.EventID.ateFood, 1, new KeyValuePair<EmotionalSimulation.EmotionTypes, float>[] {
                new KeyValuePair<EmotionalSimulation.EmotionTypes, float>(EmotionalSimulation.EmotionTypes.Hunger, -25),
                new KeyValuePair<EmotionalSimulation.EmotionTypes, float>(EmotionalSimulation.EmotionTypes.Boredom, -25),
            })));
    }

    private void UpdateValues () {

        // update emotion display
        DebugFear = emotionalSim.GetEmotion(EmotionalSimulation.EmotionTypes.Fear).CurrValue.ToString("F2");
        DebugAnger = emotionalSim.GetEmotion(EmotionalSimulation.EmotionTypes.Anger).CurrValue.ToString("F2");
        DebugHappiness = emotionalSim.GetEmotion(EmotionalSimulation.EmotionTypes.Happiness).CurrValue.ToString("F2");
        DebugHunger = emotionalSim.GetEmotion(EmotionalSimulation.EmotionTypes.Hunger).CurrValue.ToString("F2");
        DebugBoredom = emotionalSim.GetEmotion(EmotionalSimulation.EmotionTypes.Boredom).CurrValue.ToString("F2");
        DebugComfort = emotionalSim.GetEmotion(EmotionalSimulation.EmotionTypes.Comfort).CurrValue.ToString("F2");

        // todo: update need display
        Need priorityNeed = needHierarchy.GetPriorityNeed();
        DebugNeed = priorityNeed.Type.ToString();

    }

    // ------------------------------------------

    void OnGUI () {
        GUI.skin = guiSkin;

        Rect layoutRect = new Rect(Screen.width * 0.85f - 5, Screen.height * 0.4f, Screen.width * 0.1f, Screen.height * 0.2f);
        Rect boxRect = new Rect(layoutRect.x, layoutRect.y - 4, layoutRect.width, layoutRect.height + 8);

        GUI.Box(boxRect, GUIContent.none);
        GUILayout.BeginArea(layoutRect);
        for (int i = 0; i < EventsList.Count; i++) {
            if (GUILayout.Button(EventsList[i].Key)) {
                emotionalSim.TriggerEvent(EventsList[i].Value);
            }
        }
        GUILayout.EndArea();
    }
}
