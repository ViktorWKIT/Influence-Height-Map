using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateHeatMap : MonoBehaviour
{
    public Material heightMaterial;

    private void Start() {
        if (heightMaterial == null)
            Debug.LogError("No material assigned to heightMaterial in GenerateHeatMap!");
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Q)){
            createHeatMap();
        }
    }

    void createHeatMap() {
        GameObject map = GameObject.Find("Map");
        GameObject projector = GameObject.Find("Projector");
        GameObject agents = GameObject.Find("AGENTS");
        Renderer mapRenderer = map.GetComponent<Renderer>();
        var mats = new Material[mapRenderer.materials.Length];

        for (int i = 0; i < mats.Length; i++)
            mats[i] = heightMaterial;

        mapRenderer.materials = mats;
        projector.SetActive(false);
        agents.SetActive(false);
        ScreenCapture.CaptureScreenshot(Application.persistentDataPath + "/heatmap.png");
    }
}