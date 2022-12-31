using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InfluenceMap : MonoBehaviour
{
    private static GameObject agentContainer;
    private static Texture2D heightMap = null;
    private Bounds mapBounds;
    [SerializeField]
    private Vector4[] positionsArray;
    [SerializeField]
    private float[] radiuses;
    [SerializeField]
    private float[] intensities;
    float t = 0;
    public Material material;

    private const int count = 500;
    // Start is called before the first frame update
    void Start()
    {
        positionsArray = new Vector4[count];
        radiuses = new float[count];
        intensities = new float[count];
        radiuses[0] = 2f;
        intensities[0] = 1f;

        mapBounds = GameObject.Find("Map").GetComponent<Renderer>().bounds;
        agentContainer = GameObject.Find("AGENTS");

        if(File.Exists(Application.persistentDataPath + "/heatmap.png")){
            byte[] fileData = File.ReadAllBytes(Application.persistentDataPath + "/heatmap.png");
            heightMap = new Texture2D(2, 2, TextureFormat.BGRA32, false);
            heightMap.LoadImage(fileData);
        }
        else{
            Debug.LogError("No heatmap.png detected!");
        }

        if (mapBounds == null)
            Debug.LogError("No map assigned to map in InfluenceMap!");
    }

    void Update() {
        t += Time.deltaTime / 4;
        if (t > 2 * Mathf.PI)
            t -= 2 * Mathf.PI;
        Vector2 pos = new Vector2(Mathf.Sin(t) * mapBounds.size.x / 2, Mathf.Cos(t) * mapBounds.size.z / 2);
        positionsArray[0] = new Vector4(pos.x, getHeightAtPosition(pos.x, pos.y), pos.y);        
        // material.SetInt("_Points_Length", positionsArray.Length);
        // material.SetVectorArray("_Points", positionsArray);
        // material.SetFloatArray("_Radius", radiuses);
        // material.SetFloatArray("_Intensity", intensities);
    }

    float getHeightAtPosition(float xPos, float zPos){
        return heightMap.GetPixel(Mathf.FloorToInt(heightMap.width * (xPos + mapBounds.size.x / 2) / mapBounds.size.x), Mathf.FloorToInt(heightMap.height * (zPos + mapBounds.size.z / 2) / mapBounds.size.z)).r * mapBounds.size.y;
    }
}
