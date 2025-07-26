using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public int size = 16;
    public int[,,] blocks;
    public float blockSize = 1f;

    void Start()
    {
        blocks = new int[size, size, size];
        GenerateBlockData();
        GenerateMesh();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (!Camera.main) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Vector3 hitPoint = hit.point;
            Vector3Int blockPos = Vector3Int.FloorToInt(hitPoint - transform.position - hit.normal * 0.5f);
            Vector3Int placePos = Vector3Int.FloorToInt(hitPoint - transform.position + hit.normal * 0.5f);

            if (Input.GetMouseButtonDown(0)) // Destroy
            {
                SetBlock(blockPos, 0);
            }
            else if (Input.GetMouseButtonDown(1)) // Place
            {
                SetBlock(placePos, 1);
            }
        }
    }

    void SetBlock(Vector3Int pos, int type)
    {
        if (InBounds(pos))
        {
            blocks[pos.x, pos.y, pos.z] = type;
            GenerateMesh();
        }
    }

    bool InBounds(Vector3Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 && pos.x < size && pos.y < size && pos.z < size;
    }

    void GenerateBlockData()
    {
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                    blocks[x, y, z] = y < size / 2 ? 1 : 0; // simple ground
    }

    void GenerateMesh()
    {
        List<Vector3> verts = new();
        List<int> tris = new();
        List<Vector2> uvs = new();

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                {
                    if (blocks[x, y, z] == 0) continue;

                    Vector3 blockPos = new Vector3(x, y, z) * blockSize;

                    for (int i = 0; i < 6; i++)
                    {
                        Vector3Int dir = directions[i];
                        int nx = x + dir.x, ny = y + dir.y, nz = z + dir.z;
                        if (nx < 0 || ny < 0 || nz < 0 || nx >= size || ny >= size || nz >= size || blocks[nx, ny, nz] == 0)
                        {
                            AddFace(verts, tris, uvs, blockPos, i);
                        }
                    }
                }

        Mesh mesh = new();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        MeshFilter filter = GetComponent<MeshFilter>();
        MeshCollider collider = GetComponent<MeshCollider>();
        filter.mesh = mesh;
        collider.sharedMesh = mesh;
    }

    void AddFace(List<Vector3> verts, List<int> tris, List<Vector2> uvs, Vector3 offset, int dir)
    {
        int vertIndex = verts.Count;
        for (int i = 0; i < 4; i++)
        {
            verts.Add(offset + faceVertices[dir, i] * blockSize);
            uvs.Add(faceUVs[i]);
        }
        tris.Add(vertIndex); tris.Add(vertIndex + 1); tris.Add(vertIndex + 2);
        tris.Add(vertIndex); tris.Add(vertIndex + 2); tris.Add(vertIndex + 3);
    }

    static readonly Vector3[,] faceVertices = new Vector3[6, 4]
    {
        { new(1,0,0), new(1,1,0), new(1,1,1), new(1,0,1) }, // Right
        { new(0,0,1), new(0,1,1), new(0,1,0), new(0,0,0) }, // Left
        { new(0,1,1), new(1,1,1), new(1,1,0), new(0,1,0) }, // Top
        { new(0,0,0), new(1,0,0), new(1,0,1), new(0,0,1) }, // Bottom
        { new(1,0,1), new(1,1,1), new(0,1,1), new(0,0,1) }, // Front
        { new(0,0,0), new(0,1,0), new(1,1,0), new(1,0,0) }, // Back
    };

    static readonly Vector3Int[] directions = new Vector3Int[6]
    {
        new(1,0,0), new(-1,0,0), new(0,1,0), new(0,-1,0), new(0,0,1), new(0,0,-1)
    };

    static readonly Vector2[] faceUVs = new Vector2[4]
    {
        new(0,0), new(0,1), new(1,1), new(1,0)
    };
}
