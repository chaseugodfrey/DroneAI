using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum AgentType
{
    Walking,
    Flying
}

public struct AStarNode
{
    public int parentID;
    public float givenCost;
    public float finalCost;
    public enum OnList
    {
        NONE,
        OPEN,
        CLOSED
    };

    public OnList onList;

    public AStarNode(int parentID, float givenCost, float finalCost, OnList onList)
    {
        this.parentID = parentID;
        this.givenCost = givenCost;
        this.finalCost = finalCost;
        this.onList = onList;
    }
};
public class AStarPathfinder
{
    private readonly TerrainManager terrain;
    private readonly float sqrt2 = 1.41421356237f;
    private readonly float sqrt3 = 1.73205080757f;

    private AStarNode[] nodeList;
    private int[] openList;
    private int openListIndex;

    int startID;
    int goalID;

    AgentType agentType;

    public AStarPathfinder(TerrainManager terrain, AgentType agentType, int maxSize = 16000)
    {
        this.terrain = terrain;
        this.agentType = agentType;
        nodeList = new AStarNode[maxSize];
        openList = new int[maxSize];
    }

    public List<Vector3> FindPath(GridPos startPos, GridPos goalPos)
    {
        startID = terrain.GridPositionToIndex(startPos);
        goalID = terrain.GridPositionToIndex(goalPos);

        Clear();

        SetData(startID, new AStarNode(0, 0, CalculateHeuristic(startPos, goalPos), AStarNode.OnList.OPEN));
        ListPush(startID);

        while (!IsListEmpty())
        {
            int parentID = ListPopCheapest();

            if (parentID == goalID)
            {
                return ReconstructPath(startID, parentID);
            }

            ClosedListPush(parentID);
            CheckNeighbours(parentID);
        }

        return new List<Vector3>(); // No path found
    }

    private List<Vector3> ReconstructPath(int startId, int currentId)
    {
        List<Vector3> path = new();
        ApplyRubberbanding();

        while (currentId != startId)
        {
            //path.Add(currentId);
            path.Add(terrain.positions.Get(currentId));
            currentId = nodeList[currentId].parentID;
        }
        path.Reverse();

        ApplySmoothing(path);

        return path;
    }


    private void Clear()
    {
        openListIndex = -1;
        System.Array.Clear(openList, 0, openList.Length);
        System.Array.Clear(nodeList, 0, nodeList.Length);
    }

    private void SetData(int id, AStarNode node) => nodeList[id] = node;
    private bool IsListEmpty() => openListIndex < 0;

    private void ListPush(int id)
    {
        if (++openListIndex >= openList.Length)
        {
            Debug.LogError("Open list overflow");
            return;
        }
        openList[openListIndex] = id;
        nodeList[id].onList = AStarNode.OnList.OPEN;
    }

    private void ClosedListPush(int id)
    {
        nodeList[id].onList = AStarNode.OnList.CLOSED;
    }

    private int ListPopCheapest()
    {
        float bestCost = float.MaxValue;
        int bestIndex = 0;

        for (int i = 0; i <= openListIndex; i++)
        {
            int id = openList[i];
            if (nodeList[id].finalCost < bestCost)
            {
                bestCost = nodeList[id].finalCost;
                bestIndex = i;
            }
        }

        int result = openList[bestIndex];
        openList[bestIndex] = openList[openListIndex--];
        return result;
    }

    private void CheckNeighbours(int id)
    {
        GridPos pos = terrain.IndexToGridPosition(id);
        GridPos goalPos = terrain.IndexToGridPosition(goalID);

        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                for (int k = -1; k <= 1; k++)
                {
                    if (i == 0 && j == 0 && k == 0) continue;

                    GridPos nPos = new(pos.row + i, pos.layer + j, pos.col + k);
                    if (!terrain.IsBlockValid(nPos) || terrain.IsBlockWall(nPos))
                        continue;

                    int nId = terrain.GridPositionToIndex(nPos);
                    int diagonal = Mathf.Abs(i) + Mathf.Abs(j) + Mathf.Abs(k);

                    if (diagonal >= 2 && IsDiagonalBlocked(pos, i, j, k)) continue;

                    AStarNode parent = nodeList[id];
                    AStarNode neighbor = nodeList[nId];

                    float cost = parent.givenCost + (diagonal > 1 ? (diagonal > 2 ? sqrt3 : sqrt2) : 1);
                    float final = cost + CalculateHeuristic(nPos, goalPos);

                    if (neighbor.onList == AStarNode.OnList.NONE)
                    {
                        SetData(nId, new AStarNode(id, cost, final, AStarNode.OnList.OPEN));
                        ListPush(nId);
                    }
                    else if (cost < neighbor.givenCost)
                    {
                        SetData(nId, new AStarNode(id, cost, final, neighbor.onList));
                        if (neighbor.onList == AStarNode.OnList.CLOSED)
                            ListPush(nId);
                    }
                }
    }

    private bool IsDiagonalBlocked(GridPos pos, int dx, int dy, int dz)
    {
        if (dx != 0 && terrain.IsBlockWall(new(pos.row + dx, pos.layer, pos.col))) return true;
        if (dy != 0 && terrain.IsBlockWall(new(pos.row, pos.layer + dy, pos.col))) return true;
        if (dz != 0 && terrain.IsBlockWall(new(pos.row, pos.layer, pos.col + dz))) return true;
        return false;
    }

    private float CalculateHeuristic(GridPos from, GridPos to)
    {
        float dx = Mathf.Abs(to.row - from.row);
        float dy = Mathf.Abs(to.layer - from.layer);
        float dz = Mathf.Abs(to.col - from.col);

        if (agentType == AgentType.Flying)
            dy *= 0.75f; // make vertical cheaper
        else if (agentType == AgentType.Walking)
            dy *= 1.25f; // penalize vertical slightly

        return dx + dy + dz; // Manhattan-style
    }



    private void ApplyRubberbanding()
    {
        int end_point = goalID;
        int mid_point = nodeList[goalID].parentID;
        int start_point = nodeList[mid_point].parentID;

        HashSet<int> visited = new();
        int safetyCounter = 0;

        while (start_point != startID && safetyCounter++ < 1000)
        {
            if (start_point < 0 || start_point >= nodeList.Length)
                break;

            if (!visited.Add(start_point))
            {
                Debug.LogError("Loop detected in parent path chain.");
                break;
            }

            GridPos cellOne = terrain.IndexToGridPosition(end_point);
            GridPos cellTwo = terrain.IndexToGridPosition(start_point);

            if (Rubberband_IsBoxValid(cellOne, cellTwo))
            {
                nodeList[end_point].parentID = start_point;
            }
            else
            {
                end_point = mid_point;
            }

            mid_point = start_point;
            start_point = nodeList[start_point].parentID;
        }

        if (safetyCounter >= 1000)
        {
            Debug.LogWarning("Rubberbanding aborted due to iteration limit.");
        }
    }


    bool Rubberband_IsBoxValid(GridPos gridLHS, GridPos gridRHS)
    {
        GridPos min = new (Mathf.Min(gridLHS.row, gridRHS.row), Mathf.Min(gridLHS.layer, gridRHS.layer), Mathf.Min(gridLHS.col, gridRHS.col));
        GridPos max = new(Mathf.Max(gridLHS.row, gridRHS.row), Mathf.Max(gridLHS.layer, gridRHS.layer), Mathf.Max(gridLHS.col, gridRHS.col));

        for (int i = min.row; i <= max.row; i++)
        {
            for (int j = min.layer; j <= max.layer; j++)
            {
                for (int k = min.col; k <= max.col; k++)
                {
                    if (terrain.IsBlockWall(i, j, k))
                        return false;

                }
            }
        }

        return true;
    }

    public void ApplySmoothing(List<Vector3> list)
    {

        List<Vector3> newList = new();
        float cellUnitSize = 1 * 1.5f;

        // ----- Step 1: Subdivision -----
        Stack<Vector3> stack = new();

        for (int i = 0; i < list.Count - 1; i++)
        {
            Vector3 target = list[i];
            Vector3 next = list[i + 1];

            stack.Clear();
            stack.Push(next);

            while (stack.Count > 0)
            {
                Vector3 current = stack.Peek();

                if (Vector3.Distance(current, target) > cellUnitSize)
                {
                    Vector3 mid = 0.5f * (current + target);
                    stack.Push(mid);
                }
                else
                {
                    newList.Add(target);
                    target = stack.Pop();
                }
            }
        }

        // Add final point
        newList.Add(list[^1]); // same as list[list.Count - 1]
        list = newList;
        newList = new(); // clear for next stage

        // ----- Step 2: Catmull-Rom smoothing -----
        int count = list.Count;
        for (int i = 0; i < count - 1; i++)
        {
            int i0 = Mathf.Max(i - 1, 0);
            int i1 = i;
            int i2 = Mathf.Min(i + 1, count - 1);
            int i3 = Mathf.Min(i + 2, count - 1);

            Vector3 p0 = list[i0];
            Vector3 p1 = list[i1];
            Vector3 p2 = list[i2];
            Vector3 p3 = list[i3];

            for (float f = 0f; f < 1.0f; f += 0.25f)
            {
                Vector3 point = CatmullRom(p0, p1, p2, p3, f);
                newList.Add(point);
            }
        }

        list = newList;
    }

    // Catmull-Rom spline interpolation
    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

}
