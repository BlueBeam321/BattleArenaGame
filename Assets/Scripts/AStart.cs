using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    public static ArrayList FindPath(Map map, Vector3 start, Vector3 end)
    {
        List<Vector3> openSet = new List<Vector3>();
        HashSet<Vector3> closedSet = new HashSet<Vector3>();
        Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();
        Dictionary<Vector3, float> gScore = new Dictionary<Vector3, float>();
        Dictionary<Vector3, float> fScore = new Dictionary<Vector3, float>();

        openSet.Add(start);
        gScore[start] = 0;
        fScore[start] = HeuristicCostEstimate(start, end);

        while (openSet.Count > 0)
        {
            Vector3 current = GetLowestFScoreNode(fScore, openSet);
            if (current == end)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            List<Vector3> neighbors = new List<Vector3>();
            if (current.x > 0)
                neighbors.Add(new Vector3(current.x - 1, 0, current.z));
            if (current.z > 0)
                neighbors.Add(new Vector3(current.x, 0, current.z - 1));
            if (current.x < map.width - 1)
                neighbors.Add(new Vector3(current.x + 1, 0, current.z));
            if (current.z < map.height - 1)
                neighbors.Add(new Vector3(current.x, 0, current.z + 1));

            foreach (Vector3 neighbor in neighbors)
            {
                if (!map.is_walkable((int)neighbor.x, (int)neighbor.z) || closedSet.Contains(neighbor))
                    continue;

                float tentativeGScore = gScore[current] + DistanceBetween(current, neighbor);

                if (!openSet.Contains(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, end);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return new ArrayList();
    }

    private static float HeuristicCostEstimate(Vector3 start, Vector3 end)
    {
        return DistanceBetween(start, end);
    }

    private static float DistanceBetween(Vector3 nodeA, Vector3 nodeB)
    {
        float dx = nodeA.x - nodeB.x;
        float dz = nodeA.z - nodeB.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    private static Vector3 GetLowestFScoreNode(Dictionary<Vector3, float> fScore, List<Vector3> nodes)
    {
        Vector3 lowestNode = nodes[0];
        float lowestFScore = fScore[lowestNode];
        foreach (Vector3 node in nodes)
        {
            float nodeFScore = fScore[node];
            if (nodeFScore < lowestFScore)
            {
                lowestNode = node;
                lowestFScore = nodeFScore;
            }
        }

        return lowestNode;
    }

    private static ArrayList ReconstructPath(Dictionary<Vector3, Vector3> cameFrom, Vector3 current)
    {
        ArrayList path = new ArrayList();
        path.Add(current);

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }
}