using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


public class PlaneAgent : MonoBehaviour
{
    [Header("Settings")]
    public bool m_KeepMinAltitude = true;
    public int m_MinHeight = 1;
    public float m_Speed = 10.0f;
    public float m_MinDist = 0.1f;

    [Header("Autopilot")]
    public bool m_AutoPilot;
    public float m_DelayBetweenSelection = 1.0f;

    [Header("Details")]
    public GridPos m_Destination_Grid;
    public Vector3 m_Destination_Position;
    public int m_Destination_ID;

    bool m_PathFound = false;
    Rigidbody m_rb;
    TerrainManager m_TerrainManager;

    List<Vector3> pathNodeList = new List<Vector3>();

    float delayTimer = 0;
    bool startComputingPath = false;


    // Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_TerrainManager = GameObject.Find("TerrainManager").GetComponent<TerrainManager>();
        delayTimer = m_DelayBetweenSelection;
    }

    Coroutine pathRoutine;
    public void SetDestination(GridPos destination)
    {
        if (pathRoutine != null)
            StopCoroutine(pathRoutine);

        if (!m_TerrainManager.IsBlockValid(destination))
        {
            Debug.LogWarning("Invalid destination selected: " + destination);
            return;
        }

        if (m_KeepMinAltitude)
            destination.layer = m_MinHeight;

        m_Destination_Grid = destination;
        m_Destination_Position = m_TerrainManager.GridToWorldPos(m_Destination_Grid);

        Debug.Log("New Destination" + m_Destination_Grid);

        pathRoutine = StartCoroutine(RequestPath());
    }

    int currentPathIndex = 0;

    void MoveToDestination()
    {
        if (pathNodeList == null || pathNodeList.Count == 0)
            return;

        Vector3 target = pathNodeList[0];
        Vector3 moveDir = (target - transform.position).normalized;

        // Move
        transform.position += moveDir * m_Speed * Time.deltaTime;

        // Rotate to face movement direction
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10 * Time.deltaTime);
        }

        // Reached this point?
        if (Vector3.Distance(transform.position, target) < m_MinDist)
        {
            pathNodeList.RemoveAt(0);

            if (pathNodeList.Count == 0)
            {
                ClearPath();
                Debug.Log("Destination reached.");
            }
        }
    }


    IEnumerator RequestPath()
    {
        startComputingPath = true;
        AStarPathfinder pathfinder = new AStarPathfinder(m_TerrainManager, AgentType.Flying);
        var start = m_TerrainManager.WorldToGridPos(transform.position);
        var goal = m_Destination_Grid;

        pathNodeList = pathfinder.FindPath(start, goal);
        m_PathFound = pathNodeList.Count > 0;

        startComputingPath = false;
        yield return null;
    }


    void ClearPath()
    {
        m_PathFound = false;
        currentPathIndex = 0;
        pathNodeList.Clear();

    }

    [Header("Path Debug Settings")]
    [SerializeField] Mesh MESH_CUBE;
    [SerializeField] Material MAT_PATH;

    void LateUpdate()
    {
        List<Matrix4x4> matrices = new List<Matrix4x4>();

        for (int i = 0; i < pathNodeList.Count; i++)
        {
            matrices.Add(Matrix4x4.TRS(pathNodeList[i], Quaternion.identity, Vector3.one));

            if (matrices.Count == 1023)
            {
                Graphics.DrawMeshInstanced(MESH_CUBE, 0, MAT_PATH, matrices);
                matrices.Clear();
            }
        }

        if (matrices.Count > 0)
        {
            Graphics.DrawMeshInstanced(MESH_CUBE, 0, MAT_PATH, matrices);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_PathFound)
        {
            MoveToDestination();
        }

        else
        {
            if (m_AutoPilot)
            {
                if (!startComputingPath)
                {
                    delayTimer -= Time.deltaTime;
                    if (delayTimer < 0)
                    {
                        delayTimer = m_DelayBetweenSelection;
                        SetDestination(m_TerrainManager.GetRandomGridPosition());
                    }
                }

            }
        }
    }
}
