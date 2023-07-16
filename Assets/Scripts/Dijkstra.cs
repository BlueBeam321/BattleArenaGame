using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Dijkstra
{
    public static ArrayList FindPath(Map map, Vector3 start, Vector3 goal)
    {
        List<Vector3> unvisitedNodes = new List<Vector3>();
        Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();
        Dictionary<Vector3, float> distance = new Dictionary<Vector3, float>();

        // Set the distance of the start node to zero, and the distance of all other nodes to infinity
        foreach (Vector3 node in GetAllNodes(map))
        {
            if (node == start)
                distance[node] = 0f;
            else
                distance[node] = Mathf.Infinity;

            unvisitedNodes.Add(node);
        }

        while (unvisitedNodes.Count > 0)
        {
            Vector3 current = GetLowestDistanceNode(distance, unvisitedNodes);
            if (current == goal)
                return ReconstructPath(cameFrom, current);

            unvisitedNodes.Remove(current);

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
                if (!map.is_walkable((int)neighbor.x, (int)neighbor.z) || !unvisitedNodes.Contains(neighbor))
                    continue;

                float tentativeDistance = distance[current] + DistanceBetween(current, neighbor);

                if (tentativeDistance < distance[neighbor])
                {
                    cameFrom[neighbor] = current;
                    distance[neighbor] = tentativeDistance;
                }
            }
        }

        return new ArrayList();
    }

    private static float DistanceBetween(Vector3 nodeA, Vector3 nodeB)
    {
        float dx = nodeA.x - nodeB.x;
        float dz = nodeA.z - nodeB.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    private static Vector3 GetLowestDistanceNode(Dictionary<Vector3, float> distance, List<Vector3> nodes)
    {
        Vector3 lowestNode = nodes[0];
        float lowestDistance = distance[lowestNode];

        foreach (Vector3 node in nodes)
        {
            float nodeDistance = distance[node];

            if (nodeDistance < lowestDistance)
            {
                lowestNode = node;
                lowestDistance = nodeDistance;
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

    private static List<Vector3> GetAllNodes(Map map)
    {
        List<Vector3> nodes = new List<Vector3>();
        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
                nodes.Add(new Vector3(x, 0, y));
        }
        return nodes;
    }
}