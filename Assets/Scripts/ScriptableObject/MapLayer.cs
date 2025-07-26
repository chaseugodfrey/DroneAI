using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Create Map Layer")]
public class MapLayer : ScriptableObject
{
    public Color color;
    public TerrainData3D<int> data;
}
