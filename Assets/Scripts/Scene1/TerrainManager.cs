using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public struct GridPos
{
    public int row, layer, col;

    public GridPos(int row, int layer, int col)
    {
        this.row = row;
        this.layer = layer;
        this.col = col;
    }

    public static GridPos operator +(GridPos lhs, Vector3 rhs) =>
        new(lhs.row + (int)rhs.x, lhs.layer + (int)rhs.y, lhs.col + (int)rhs.z);

    public readonly override string ToString()
    {
        return "(" + row + "," + layer + "," + col + ")";
    }
}

public class TerrainManager : MonoBehaviour
{
    public float cubeSize;
    public int width, height, depth;

    public GameObject cubePrefab;
    public TerrainData3D<GameObject> chunk;
    public TerrainData3D<Vector3> positions;
    GameObject chunkParent;

    public MapLayer terrainData;

    private TerrainData3D<int> data;
    private float half_cube_size;

    [Header("Map Layers")]
    [SerializeField] Mesh cubeMesh;
    [SerializeField] Material[] debugMaterial;
    private bool[] mapLayersOn;
    [SerializeField] ParticleSystem debugParticles;

    // A list of block positions you want to visualize
    List<Vector3> debugPositions = new(); 
    private TerrainData3D<float>[] mapLayers;

    public TerrainData3D<int> GetTerrainData()
    {
        return data;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Initialize()
    {
        ResetChunk();
        LoadChunk();
        InitializeMapLayers();
        CalculateOccupancyMap();
    }

    void InitializeMapLayers()
    {
        mapLayersOn = new bool[10];
        mapLayers = new TerrainData3D<float>[10];

        for (int i = 0; i < 10; i++)
        {
            mapLayers[i] = new TerrainData3D<float>(width, height, depth);
        }
    }

    void ResetChunk()
    {
        half_cube_size = cubeSize * 0.5f;
        chunkParent = new GameObject("ChunkParent");
        data = new TerrainData3D<int>(width, height, depth);
        chunk = new TerrainData3D<GameObject>(width, height, depth);
        positions = new TerrainData3D<Vector3>(width, height, depth);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    data.Set(x, y, z, 0);
                    chunk.Set(x, y, z, null);
                    positions.Set(x, y, z, new(x * cubeSize, y * cubeSize, z * cubeSize));
                }
            }
        }
    }

    void LoadFromMemory()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                GameObject cube = Instantiate(cubePrefab, positions.Get(x, 0, z), Quaternion.identity, chunkParent.transform);
                data.Set(x, 0, z, 1);
                chunk.Set(x, 0, z, cube);
            }
        }
    }

    void LoadChunk()
    {
        bool terrainDataExists = terrainData != null && terrainData.data != null;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    int value = 0;

                    if (terrainDataExists &&
                        x < terrainData.data.width &&
                        y < terrainData.data.height &&
                        z < terrainData.data.depth)
                    {
                        value = terrainData.data.Get(x, y, z);
                    }

                    if (y == 0 || value > 0)
                    {
                        value = 1;
                    }

                    data.Set(x, y, z, value);

                    if (value > 0.0f)
                        chunk.Set(x, y, z, Instantiate(cubePrefab, positions.Get(x, y, z), Quaternion.identity, chunkParent.transform));
                }
            }
        }
    }

    void SpawnBlock(GridPos gridPos)
    {
        if (IsBlockValid(gridPos))
        {
            GameObject block = Instantiate(cubePrefab, GridToWorldPos(gridPos), Quaternion.identity, chunkParent.transform);
            chunk.Set(gridPos, block);
            data.Set(gridPos, 1);

            CalculateOccupancyMap();
        }

        else
            Debug.Log("Invalid Grid Position.");
    }

    void DestroyBlock(GridPos gridPos)
    {
        GameObject block = chunk.Get(gridPos);
        chunk.Set(gridPos, null);
        data.Set(gridPos, 0);
        Destroy(block);
        CalculateOccupancyMap();
    }

    public GridPos IndexToGridPosition(int index)
    {
        int x = index % width;
        int y = (index / width) % height;
        int z = index / (width * height);

        return new GridPos(x, y, z);
    }

    public GridPos GetRandomGridPosition()
    {
        return new GridPos(Random.Range(0, width), Random.Range(0, height), Random.Range(0 , depth));
    }

    public int GridPositionToIndex(GridPos gridPos)
    {
        return gridPos.row + gridPos.layer * width + gridPos.col * width * height;
    }
    public GridPos WorldToGridPos(Vector3 pos)
    {
        return new GridPos((int)pos.x % width, (int)pos.y % height, (int)pos.z % depth);
    }

    public Vector3 GridToWorldPos(GridPos gridPos)
    {
        return positions.Get(gridPos.row, gridPos.layer, gridPos.col);
    }

    public bool IsBlockValid(GridPos gridPos)
    {
        if (gridPos.row < 0 || gridPos.row >= width)
            return false;
        if (gridPos.layer < 0 || gridPos.layer >= height)
            return false;
        if (gridPos.col < 0 || gridPos.col >= depth)
            return false;

        return true;
    }

    public bool IsBlockValid(int row, int layer, int col)
    {
        if (row < 0 || row >= width)
            return false;
        if (layer < 0 || layer >= height)
            return false;
        if (col < 0 || col >= depth)
            return false;

        return true;
    }

    public bool IsBlockWall(GridPos gridPos)
    {
        return (data.Get(gridPos) == 1);
    }

    public bool IsBlockWall(int row, int layer, int col)
    {
        return (data.Get(row, layer, col) == 1);
    }

    public Vector3 GetDominantAxisFromDirection(Vector3 dir)
    {
        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);
        float absZ = Mathf.Abs(dir.z);

        if (absX >= absY && absX >= absZ)
            return new Vector3(Mathf.Sign(dir.x), 0, 0);
        else if (absY >= absX && absY >= absZ)
            return new Vector3(0, Mathf.Sign(dir.y), 0);
        else
            return new Vector3(0, 0, Mathf.Sign(dir.z));
    }

    public void CreateBlock(GameObject target, Vector3 hitPoint)
    {
        Vector3 dir = hitPoint - target.transform.position;
        dir.Normalize();
        dir = GetDominantAxisFromDirection(dir);

        GridPos gridPos = WorldToGridPos(target.transform.position + dir);

        SpawnBlock(gridPos);
    }

    public void DeleteBlock(GameObject target)
    {
        GridPos gridPos = WorldToGridPos(target.transform.position);

        if (gridPos.layer == 0)
        {
            Debug.Log("Cannot remove Block in base layer.");
            return;
        }


        DestroyBlock(gridPos);
    }
    public void SaveData()
    {
        if (terrainData == null)
        {
            Debug.LogWarning("No MapLayer assigned for saving.");
            return;
        }

        terrainData.data = new TerrainData3D<int>(width, height, depth);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < depth; z++)
                    terrainData.data.Set(x, y, z, data.Get(x, y, z));

        Debug.Log("Terrain data saved to MapLayer.");
    }

    public void ClearData()
    {
        Destroy(chunkParent);

        data.Reset();
        chunk.Reset();
        positions.Reset();

        ResetChunk();
        LoadFromMemory();
    }
    
    public void ToggleMapLayer(int layer)
    {
        mapLayersOn[layer] = !mapLayersOn[layer];
    }


    public void CalculateOccupancyMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (IsBlockWall(x, y, z))
                        continue;

                    mapLayers[0].Set(x, y, z, 1.0f);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mapLayersOn[0])
            UpdateDebugParticles(mapLayers[0], positions);
        else
            debugParticles.Clear();
    }

    [Header("Random Settings")]
    public float chance;
    public void RandomGenerateMap()
    {
        ClearData();
        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    float rand = Random.Range(0, 1.0f);

                    if (rand <= chance)
                    {
                        GameObject cube = Instantiate(cubePrefab, positions.Get(x, y, z), Quaternion.identity, chunkParent.transform);
                        data.Set(x, y, z, 1);
                        chunk.Set(x, y, z, cube);
                    }
                }
            }
        }
    }

    public void UpdateDebugParticles(TerrainData3D<float> debugData, TerrainData3D<Vector3> positions, float threshold = 0.5f)
    {
        debugParticles.Clear(); // remove previous particles

        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.startColor = new Color(0f, 1f, 0f, 1f); // Green
        emitParams.startSize = cubeSize * 0.25f;

        for (int x = 0; x < debugData.width; x++)
        {
            for (int y = 0; y < debugData.height; y++)
            {
                for (int z = 0; z < debugData.depth; z++)
                {
                    float val = debugData.Get(x, y, z);
                    if (val >= threshold)
                    {
                        emitParams.position = positions.Get(x, y, z);
                        debugParticles.Emit(emitParams, 1);
                    }
                }
            }
        }
    }
}
