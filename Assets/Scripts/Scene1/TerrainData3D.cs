using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TerrainData3D<T>
{
    public int width;
    public int height;
    public int depth;
    public List<T> values;

    public TerrainData3D(int w, int h, int d)
    {
        width = w;
        height = h;
        depth = d;
        values = new List<T>(new T[w * h * d]);
    }

    public int Index(int x, int y, int z)
    {
        return x + y * width + z * width * height;
    }

    public int Index(GridPos gridPos)
    {
        return gridPos.row + gridPos.layer * width + gridPos.col * width * height;
    }

    public T Get(int x, int y, int z)
    {
        return values[Index(x, y, z)];
    }

    public T Get(GridPos gridPos)
    {
        return values[Index(gridPos.row, gridPos.layer, gridPos.col)];
    }

    public T Get(int id)
    {
        return values[id];
    }

    public void Set(int x, int y, int z, T val)
    {
        values[Index(x, y, z)] = val;
    }

    public void Set(GridPos gridPos, T val)
    {
        values[Index(gridPos.row, gridPos.layer, gridPos.col)] = val;
    }

    public void Reset()
    {
        values.Clear();
    }
}
