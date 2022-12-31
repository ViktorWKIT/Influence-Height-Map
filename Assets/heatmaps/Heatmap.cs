// Original script by Alan Zucconi
// www.alanzucconi.com
using UnityEngine;
using System.Collections;

public class Heatmap : MonoBehaviour
{

    public Vector4[] positions;
    public float[] radiuses;
    public float[] intensities;

    public Material material;

    public GameObject ground;

    public int count = 50;

    void Start ()
    {
        positions = new Vector4[count];
        radiuses = new float[count];
        intensities= new float[count];

        ground = GameObject.Find("Map");

        Vector3 groundSizes = ground.GetComponent<Renderer>().bounds.size;

        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = new Vector3(Random.Range(-5f, 5f), 0.4f, Random.Range(-5f, 5f));
            radiuses[i] = Random.Range(1.25f, 1.25f);
            intensities[i] = Random.Range(1.5f, 2f);
        }
    }

    void Update()
    {
        material.SetInt("_Points_Length", positions.Length);
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] += new Vector4(Random.Range(-0.1f,+0.1f), 0, Random.Range(-0.1f, +0.1f)) * Time.deltaTime;
        }
            material.SetVectorArray("_Points", positions);
            material.SetFloatArray("_Radius", radiuses);
            material.SetFloatArray("_Intensity", intensities);
    }
}