using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.IO;

public class AgentDetection : MonoBehaviour
{
    public GameObject agentContainer;
    [Tooltip("The maximum distance the agent will consider.")]
    public static float maxDistance = 10;
    private static Texture2D heightMap = null;
    [SerializeField]
    public Vector4[] detectionLocation;
    public float[] timeRemaining;
    public float[] detectionIntensity;
    private GLOBALS globals;
    private NavMeshAgent agent;
    private List<AgentDetection> teammates = new List<AgentDetection>();
    //private Vector3 moveDir = new Vector3(1, 0, 0);
    // Start is called before the first frame update
    void Awake(){
        Random.InitState(System.DateTime.Now.Millisecond);
        agent = GetComponent<NavMeshAgent>();
        globals = GameObject.Find("MANAGER").GetComponent<GLOBALS>();
        detectionLocation = new Vector4[GLOBALS.COUNT];
        timeRemaining = new float[GLOBALS.COUNT];
        detectionIntensity = new float[GLOBALS.COUNT];
        agentContainer = GameObject.Find("AGENTS");
    }
    void Start()
    {
        if(heightMap == null && File.Exists(Application.persistentDataPath + "/heatmap.png")){
            byte[] fileData = File.ReadAllBytes(Application.persistentDataPath + "/heatmap.png");
            heightMap = new Texture2D(2, 2, TextureFormat.BGRA32, false);
            heightMap.LoadImage(fileData);
        }
        else if (heightMap == null){
            Debug.LogError("No heatmap.png detected!");
        }

        if (agentContainer == null) {
            Debug.LogError("No map assigned to map in AgentDetection!");
        }
        if (globals == null) {
            Debug.LogError("No global container assigned to globals in AgentDetection!");
        }

        
        foreach (Transform agent in agentContainer.GetComponentsInChildren<Transform>()) {
            if (agent.tag == this.tag) teammates.Add(agent.GetComponent<AgentDetection>());
        }
        StartCoroutine(CheckForEnemies());
        StartCoroutine(UpdateDetections());
        StartCoroutine(WalkToOptimalSubdivision());
        //StartCoroutine(UpdateAgentHeight());
        //StartCoroutine(UpdateAgentMoveDir());
    }

    void Update(){
        //transform.position += SPEED * Time.deltaTime * moveDir;
    }

    IEnumerator CheckForEnemies() {
        while (true){
            foreach (Transform agent in agentContainer.GetComponentsInChildren<Transform>()) {
                if (agent.name == "AGENTS") continue;
                if (agent.tag == this.tag) continue;

                Vector3 direction = agent.position - transform.position;

                if (Vector3.Angle(
                    Vector3.ProjectOnPlane(transform.forward, Vector3.up), 
                    Vector3.ProjectOnPlane(direction, Vector3.up)) < GLOBALS.viewAngle) {
                    Physics.Raycast(transform.position, direction, out RaycastHit hit, maxDistance);
                    if (hit.collider == agent.GetComponent<Collider>()){
                        foreach(AgentDetection teammate in teammates){
                            teammate.DetectAtPosition(hit.point.x, hit.point.z);
                        }
                    }
                }
            }
            if (globals.GetActiveAgent() == this.transform) {   
                globals.heatmap.SetInt("_Points_Length", detectionLocation.Length);
                globals.heatmap.SetVectorArray("_Points", detectionLocation);
                globals.heatmap.SetFloatArray("_Radius", timeRemaining);
                globals.heatmap.SetFloatArray("_Intensity", detectionIntensity);
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator UpdateDetections() {
        while (true) {
            for (int location = 0; location < GLOBALS.COUNT; location++){
                if (detectionLocation[location] == new Vector4(0, 0, 0, 0)) continue;
                timeRemaining[location] -= 0.2f;
                if (timeRemaining[location] <= 0f) {
                    detectionLocation[location] = new Vector4(0, 0, 0, 0);
                    timeRemaining[location] = 0f;
                    detectionIntensity[location] = 0f;
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator UpdateAgentHeight(){
        while (true){
            transform.position = new Vector3(transform.position.x, getHeightAtPosition(transform.position.x, transform.position.z) + 1, transform.position.z);
            yield return new WaitForSeconds(0.1f);
        }
    }

    // IEnumerator UpdateAgentMoveDir(){

    //     float offset = 360/(AVAILABLEAXIS * 2);
    //     while (true){
    //         Vector3 chosenAxis = new Vector3(0, 0, 0);
    //         float bestScore = -1;
    //         for (int i = 0; i < AVAILABLEAXIS; i++){
    //             Vector3 axis = Quaternion.AngleAxis(i * offset * 2, Vector3.up) * Vector3.forward;

    //             float axisScore = 0;

    //             for (int j = 0; j < SAMPLES; j++) { //Monte Carlo sampling
    //                 Vector3 randomDir = Quaternion.AngleAxis(Random.Range(-offset, offset), Vector3.up) * axis;
    //                 Vector3 endPoint = new Vector3(0, 0, 0);

    //                 float dist = Mathf.Max(randomDir.x / ((globals.mapBounds.size.x / 2) - transform.position.x * Mathf.Sign(randomDir.x)), randomDir.z / ((globals.mapBounds.size.z / 2) - transform.position.z * Mathf.Sign(randomDir.z)));

    //                 endPoint = transform.position + (randomDir * dist);
    //                 Vector3 randomPos = Vector3.Lerp(transform.position, endPoint, Random.Range(0f,1f));
    //                 axisScore += getHeightAtPosition(randomPos.x, randomPos.z);
    //                 for (int k = 0; j < detectionLocation.Length; j++) {
    //                     if(detectionLocation[j] == new Vector4(0, 0, 0, 0)) continue;
    //                     Vector3 detectionXYZ = new Vector3(detectionLocation[k].x, detectionLocation[k].y, detectionLocation[k].z);
    //                     float distance = Vector3.Distance(randomPos, detectionXYZ);
    //                     if(distance > INTENSITY) continue;

    //                     axisScore -= AVOIDANCE - distance;
    //                 }
    //             }

    //             if (axisScore > bestScore){
    //                 bestScore = axisScore;
    //                 chosenAxis = axis;
    //             }
    //         }
    //         moveDir = chosenAxis;
    //         yield return new WaitForSeconds(0.1f);
    //     }
    // }

    void DetectAtPosition(float xPos, float zPos){
        int location = 0;
        for (; location < GLOBALS.COUNT; location++){
            if (detectionLocation[location] == new Vector4(0, 0, 0, 0)) break;
        }
        detectionLocation[location] = new Vector4(xPos, getHeightAtPosition(xPos, zPos), zPos, 0);
        detectionIntensity[location] = GLOBALS.INTENSITY;
        timeRemaining[location] = 1f;
    }

    float getHeightAtPosition(float xPos, float zPos){
        return heightMap.GetPixel(Mathf.FloorToInt(heightMap.width * (xPos + globals.mapBounds.size.x / 2) / globals.mapBounds.size.x), Mathf.FloorToInt(heightMap.height * (zPos + globals.mapBounds.size.z / 2) / globals.mapBounds.size.z)).r * globals.mapBounds.size.y;
    }

    IEnumerator WalkToOptimalSubdivision(){
        while (true) {
            float subdivisionSizeX = globals.mapBounds.size.x / GLOBALS.SUBDIVISIONS;
            float subdivisionSizeZ = globals.mapBounds.size.z / GLOBALS.SUBDIVISIONS;

            Vector3 bestPos = new Vector3(0, 0, 0);
            float bestScore = 0;

            for (int i = 0; i < GLOBALS.SUBDIVISIONS; i++){
                for (int j = 0; j < GLOBALS.SUBDIVISIONS; j++){
                    float score = 0;
                    float randomPosX = i * subdivisionSizeX + Random.Range(0f, subdivisionSizeX) - (globals.mapBounds.size.x / 2);
                    float randomPosZ = j * subdivisionSizeZ + Random.Range(0f, subdivisionSizeZ) - (globals.mapBounds.size.z / 2);

                    score += getHeightAtPosition(randomPosX, randomPosZ);
                    Vector3 pos = new Vector3(randomPosX, score, randomPosZ);
                    score -= Mathf.Sqrt(Vector2.Distance(new Vector2(randomPosX, randomPosZ), new Vector2(transform.position.x, transform.position.z)) * GLOBALS.LAZINESS);
                    for (int k = 0; k < detectionLocation.Length; k++) {
                        if(detectionLocation[k] == new Vector4(0, 0, 0, 0)) continue;
                        Vector3 detectionXZ = new Vector3(detectionLocation[k].x, detectionLocation[k].z);
                        float distance = Vector2.Distance(new Vector2(randomPosX, randomPosZ), detectionXZ);
                        if(distance > GLOBALS.INTENSITY) continue;

                        score -= (1 - (distance / GLOBALS.INTENSITY)) * GLOBALS.AVOIDANCE;
                    }
                    for (int k = 1; k < GLOBALS.rewards.Length; k++){
                        Vector2 detectionXZ = new Vector3(GLOBALS.rewards[k].position.x, GLOBALS.rewards[k].position.z);
                        float distance = Vector2.Distance(new Vector2(randomPosX, randomPosZ), detectionXZ);

                        score += Mathf.Sqrt(GLOBALS.REWARD / distance) * GLOBALS.REWARD;
                    }
                    for (int k = 0; k < teammates.Count; k++){
                        if(teammates[k].gameObject.name == gameObject.name) continue;
                        Vector2 detectionXZ = new Vector3(teammates[k].transform.position.x, teammates[k].transform.position.z);
                        float distance = Vector2.Distance(new Vector2(randomPosX, randomPosZ), detectionXZ);

                        score += Mathf.Sqrt(GLOBALS.COHESION / distance) * GLOBALS.COHESION;
                    }
                    if (score > bestScore) {
                        bestScore = score;
                        bestPos = pos;
                    }
                }
            }
            if (globals.GetActiveAgent() == this.transform) Debug.Log(bestPos);
            NavMesh.SamplePosition(bestPos, out NavMeshHit hit, subdivisionSizeX / 2, NavMesh.AllAreas);

            agent.SetDestination(hit.position);
            yield return new WaitForSeconds(0.1f);
        }
    }

    void DrawPlane(Vector3 position, Vector3 normal) {
 
        Vector3 v3;
        
        if (normal.normalized != Vector3.forward)
            v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
        else
            v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude;;
            
        var corner0 = position + v3;
        var corner2 = position - v3;
        var q = Quaternion.AngleAxis(90.0f, normal);
        v3 = q * v3;
        var corner1 = position + v3;
        var corner3 = position - v3;
        
        Debug.DrawLine(corner0, corner2, Color.green, 1000);
        Debug.DrawLine(corner1, corner3, Color.green, 1000);
        Debug.DrawLine(corner0, corner1, Color.green, 1000);
        Debug.DrawLine(corner1, corner2, Color.green, 1000);
        Debug.DrawLine(corner2, corner3, Color.green, 1000);
        Debug.DrawLine(corner3, corner0, Color.green, 1000);
        Debug.DrawRay(position, normal, Color.red, 1000);
    }
}
