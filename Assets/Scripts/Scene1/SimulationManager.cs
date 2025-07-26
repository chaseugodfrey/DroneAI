using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public GameObject planeAgentObj;
    public GridPos startingPosition;
    public bool planePerspective;

    [Header("Pre-Data")]
    public GameObject planeAgentPrefab;

    PlaneAgent planeAgent;
    TerrainManager terrainManager;

    // Start is called before the first frame update
    void Start()
    {
        terrainManager = GameObject.Find("TerrainManager").GetComponent<TerrainManager>();
        StartSimulation();
    }

    void StartSimulation()
    {
        terrainManager.Initialize();
        planeAgentObj = Instantiate(planeAgentPrefab, terrainManager.GridToWorldPos(startingPosition), Quaternion.identity);
        planeAgent = planeAgentObj.GetComponent<PlaneAgent>();
    }

    public void SetDestination(GridPos gridPos)
    {
        planeAgent.SetDestination(gridPos);
    }

    public void SwapCamera()
    {
        GameObject camera = planeAgent.transform.GetChild(1).gameObject; 
        camera.SetActive(!camera.activeSelf);
    }

    public void ToggleAutoPilot()
    {
        planeAgent.m_AutoPilot = !planeAgent.m_AutoPilot;
    }

    public void SwapGrounded()
    {
        planeAgent.m_KeepMinAltitude = !planeAgent.m_KeepMinAltitude;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
