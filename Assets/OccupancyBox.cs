using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OccupancyBox : MonoBehaviour
{
    public bool isDebugMode;
    public bool is3D;

    [Header("Settings")]
    public bool followsWorldOrientation;
    [Range(3, 5)]
    public int length;
    public float cubeSize = 1.0f;
    public float offset;
    public LayerMask layerMask;

    [Header("Live Stats")]
    public float size;

    [Header("Debug Setup")]
    public Mesh mesh;
    public Material material;
    public float debugSize = 0.5f;

    Vector3[] occupancyPointList;
    bool[] occupancyPointCollisionList;

    Collider[] colliders;

    int internalLength;
    int mapMaxCount;

    MaterialPropertyBlock mpb;
    private void OnEnable()
    {
        mpb = new MaterialPropertyBlock();
    }

    public Vector3[] GetOccupancyPointList()
    {
        return occupancyPointList;
    }

    public bool[] GetOccupancyCollisionList()
    {
        return occupancyPointCollisionList;
    }

    public int GetOccupancyMapCount()
    {
        return mapMaxCount;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateOccupancyMapCount();
        occupancyPointList = new Vector3[125];
        occupancyPointCollisionList = new bool[125];
        colliders = new Collider[32];
    }

    void UpdateOccupancyMapCount()
    {
        internalLength = length;
        mapMaxCount = internalLength * internalLength * internalLength;
    }

    // Update is called once per frame
    void Update()
    {
        if (internalLength != length)
            UpdateOccupancyMapCount();

        CalculateBoundingBoxSize();
        CalculateOccupancyBox();
        CheckCollisions();
    }

    private void LateUpdate()
    {
        if (isDebugMode)
        {
            Vector3 size = Vector3.one * debugSize;
            for (int i = 0; i < mapMaxCount; i++)
            {
                Color color = occupancyPointCollisionList[i] ? Color.red : Color.green * 0.5f;
                mpb.SetColor("_Color", color); // override _Color for this draw

                Graphics.DrawMesh(mesh, Matrix4x4.TRS(
                    occupancyPointList[i], Quaternion.identity, size),
                    material, 0, null, 0, mpb);
            }
        }
    }

    void CalculateBoundingBoxSize()
    {
        int count = 0;
        Vector3 origin = transform.root.position;
        origin.y += offset;

        float dist = cubeSize * 0.5f * length;

        Vector3 dir;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (!is3D)
                {
                    dir = transform.root.InverseTransformDirection(new Vector3(i, 0, j));
                    Debug.DrawRay(origin, dir);
                    if (Physics.Raycast(origin, dir, length, layerMask))
                    {
                        ++count;
                    }
                }
                else
                {
                    for (int k = -1; k < 2; k++)
                    {
                        dir = transform.root.InverseTransformDirection(new Vector3(i, j, k));
                        Debug.DrawRay(origin, dir);
                        if (Physics.Raycast(origin, dir, length, layerMask))
                        {
                            ++count;
                        }
                    }
                }
            }
        }

        float target_size = cubeSize;
        int maxCount = is3D ? 4 : 2;
        if (count >= maxCount)
            target_size *= 0.5f;

        size = Mathf.Lerp(size, target_size, Time.deltaTime * 3.0f);
    }

    public Vector3 GetAveragePosition()
    {
        Vector3 pos = Vector3.zero;
        int count = 0;

        for (int i = 0; i < mapMaxCount; i++)
        {
            if (!occupancyPointCollisionList[i])
            {
                pos += occupancyPointList[i];
                ++count;
            }
        }

        if (count == 0)
            return transform.root.position;

        pos /= count;

        return pos;
    }

    public Vector3 PickRandomActiveNode()
    {
        List<Vector3> nodes = new List<Vector3>();
        for (int i = 0; i < mapMaxCount; i++)
        {
            if (!occupancyPointCollisionList[i])
                nodes.Add(occupancyPointList[i]);
        }

        int rand = Random.Range(0, nodes.Count);

        return nodes[rand];
    }

    void CalculateOccupancyBox()
    {
        float dist = size * 0.5f;

        for (int i = 0; i < internalLength; ++i)
        {
            for (int j = 0; j < internalLength; ++j)
            {
                for (int k = 0; k < internalLength; ++k)
                {
                    int index = i * (internalLength * internalLength) + j * internalLength + k;
                    float diff = (internalLength - 1) * 0.5f;

                    Vector3 dir = new Vector3(i - diff, j - diff, k - diff);

                    if (!followsWorldOrientation)
                        dir = transform.root.TransformDirection(dir);

                    occupancyPointList[index] = transform.root.position + dir * size;
                    occupancyPointList[index].y += offset;
                }
            }
        }
    }

    void CheckCollisions()
    {
        for (int i = 0; i < mapMaxCount; i++)
        {
            occupancyPointCollisionList[i] = false;

            int count = Physics.OverlapBoxNonAlloc(occupancyPointList[i], Vector3.one * size * 0.5f, colliders, transform.root.rotation, layerMask);

            if (count > 0)
            {
                occupancyPointCollisionList[i] = true;
            }
        }
    }

    public float OccupancyRadius
    {
        get { return cubeSize; }
        set { cubeSize = value; }
    }

    public void SetDebug()
    {
        isDebugMode = !isDebugMode;
    }
}
