using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowField : MonoBehaviour
{
    [Header("Settings")]
    public bool isDebugMode;
    public float avoidanceWeight;

    [Header("Live Stats")]
    public Vector3 direction;

    [Header("Setup")]
    public Mesh mesh;
    public Material material;
    public OccupancyBox occupancyBox;
    public float debugSize;

    Vector3[] flowFieldVectorList;

    // Start is called before the first frame update
    void Start()
    {
        occupancyBox = GetComponent<OccupancyBox>();
        flowFieldVectorList = new Vector3[125];
    }

    // Update is called once per frame
    void Update()
    {
        //CalculateFlowField(new Vector3(0, 0, 50));
    }

    int Index(int x, int y, int z)
    {
        return x + occupancyBox.length * (y + occupancyBox.length * z);
    }
    public void CalculateFlowField(Vector3 dirToTarget)
    {
        int count = occupancyBox.GetOccupancyMapCount();
        var pointList = occupancyBox.GetOccupancyPointList();
        var collisionList = occupancyBox.GetOccupancyCollisionList();

        for (int i = 0; i < count; i++)
        {
            Vector3 dir = dirToTarget.normalized;

            if (collisionList[i])
            {
                if (Physics.Raycast(pointList[i], dir, out RaycastHit hit, 5f)) // Adjust max distance as needed
                {
                    Vector3 normal = hit.normal;
                    // Slide direction along the surface
                    dir = Vector3.ProjectOnPlane(dir, normal).normalized;
                }

                Vector3 dirFromCenter = pointList[i] - transform.root.position;
                float weight = 1f / (dirFromCenter.sqrMagnitude + 0.01f);
                dir -= dirFromCenter.normalized * weight * 100f; // repulsion strength
                dir = dir.normalized; // normalize after repulsion
            }

            flowFieldVectorList[i] = dir;
        }

        #region Smoothing
        Vector3[] newField = new Vector3[flowFieldVectorList.Length];


        int length = occupancyBox.length;
        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < length; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    Vector3 sum = Vector3.zero;
                    int num = 0;

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dz = -1; dz <= 1; dz++)
                            {
                                int nx = x + dx;
                                int ny = y + dy;
                                int nz = z + dz;

                                if (nx >= 0 && nx < length &&
                                    ny >= 0 && ny < length &&
                                    nz >= 0 && nz < length)
                                {
                                    int neighborIndex = Index(nx, ny, nz);
                                    sum += flowFieldVectorList[neighborIndex];
                                    num++;
                                }
                            }
                        }
                    }

                    int index = Index(x, y, z);
                    newField[index] = sum / count;
                }
            }
        }

        // Overwrite original field
        for (int i = 0; i < flowFieldVectorList.Length; i++)
        {
            flowFieldVectorList[i] = newField[i].normalized;
        }
#endregion
    }


    public Vector3 CalculateAverageDirection()
    {
        Vector3 sum = Vector3.zero;
        float totalWeight = 0f;

        var collisionList = occupancyBox.GetOccupancyCollisionList();

        for (int i = 0; i < flowFieldVectorList.Length; i++)
        {
            Vector3 dir = flowFieldVectorList[i];
            float weight = collisionList[i] ? avoidanceWeight : 0.25f;

            sum += dir * weight;
            totalWeight += weight;
        }

        if (totalWeight == 0f)
            return direction; 

        Vector3 average = sum / totalWeight;
        direction = average.normalized;
        return direction;
    }



    MaterialPropertyBlock mpb;

    private void OnEnable()
    {
        mpb = new MaterialPropertyBlock();
    }

    private void LateUpdate()
    {
        if (isDebugMode)
        {
            Vector3 size = new Vector3(0.1f, 0.1f, 1) * debugSize;
            Vector3[] occupancyPointList = occupancyBox.GetOccupancyPointList();
            bool[] collisionList = occupancyBox.GetOccupancyCollisionList();

            for (int i = 0; i < occupancyBox.GetOccupancyMapCount(); i++)
            {
                Color color = collisionList[i] ? Color.red : Color.green * 0.5f;
                mpb.SetColor("_Color", color); 

                Graphics.DrawMesh(mesh, Matrix4x4.TRS(
                    occupancyPointList[i], Quaternion.LookRotation(flowFieldVectorList[i]), size),
                    material, 0, null, 0, mpb);
            }
        }
    }

    public void SetDebug()
    {
        isDebugMode = !isDebugMode;
    }

    public float AvoidanceWeight
    {
        get { return avoidanceWeight; }
        set { avoidanceWeight = value; }
    }
}
