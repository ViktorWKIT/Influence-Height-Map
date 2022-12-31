using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLOBALS : MonoBehaviour
{
    public Material heatmap; //The heatmap material
    public Bounds mapBounds; //The size of the map
    private static int activeAgent; //Which agent's sight is being visualized?
    private static Transform[] agents; //All agents in the scene
    public static Transform[] rewards; //All rewards in the scene
    public Camera screenshotCamera; //The camera being used to screenshot the heatmap.
    public static float viewAngle = 180; //The view angle of an agent in degrees
    public static int COUNT = 500; //How many detections should be able to be saved?
    //public static float SPEED = 1f; //Movement speed
    //public static int AVAILABLEAXIS = 4; //How many directions will be considered?
    //public static int SAMPLES = 2; //How many samples should be taken for each direction?
    public static int SUBDIVISIONS = 3; //How many subdivisions is each direction of the map sectioned into? Actual amount of subdivisions is this number squared. 

    /*PATHFINDING VARIABLES*/
    public static float INTENSITY = 10f; //How far out should a detection be considered?
    public static float AVOIDANCE = 4f; //How dangerous should enemies be considered to be?
    public static float REWARD = 10f; //How much should rewards be considered?
    public static float COHESION = 2f; //How much should walking together with a teammate be rewarded?
    public static float LAZINESS = 1f; //How much should distance be punished?
    void Awake(){
        activeAgent = 1;
        mapBounds = GameObject.Find("Map").GetComponent<Renderer>().bounds;
        agents = GameObject.Find("AGENTS").GetComponentsInChildren<Transform>();
        rewards = GameObject.Find("REWARDS").GetComponentsInChildren<Transform>();
        screenshotCamera = GameObject.Find("Camera").GetComponent<Camera>();
        screenshotCamera.orthographicSize = mapBounds.size.x / 2;
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.E)) CycleActiveAgent();
    }

    void CycleActiveAgent(){
        activeAgent++;
        if (activeAgent == agents.Length){
            activeAgent = 1;
        }
        Debug.Log(activeAgent);
    }

    public Transform GetActiveAgent(){
        return agents[activeAgent];
    }
}
