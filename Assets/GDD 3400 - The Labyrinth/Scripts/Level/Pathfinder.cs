using System.Collections.Generic;
using UnityEngine;

namespace GDD3400.Labyrinth
{
    public class Pathfinder
    {
        public static List<PathNode> FindPath(PathNode startNode, PathNode endNode)
        {
            // List of the nodes we might want to take
            List<PathNode> openSet = new List<PathNode>();

            // List of the nodes we have already looked at
            List<PathNode> closedSet = new List<PathNode>();

            // Saves path information back to start
            Dictionary<PathNode, PathNode> cameFromNode = new Dictionary<PathNode, PathNode>();

            // Keeping track of our costs as we go
            Dictionary<PathNode, float> costSoFar = new Dictionary<PathNode, float>();
            Dictionary<PathNode, float> costToEnd = new Dictionary<PathNode, float>();

            // Initialize the starting information
            openSet.Add(startNode);
            costSoFar[startNode] = 0f;
            //costSoFar.Add(startNode, 0);
            costToEnd[startNode] = Heuristic(startNode, endNode);
            //costToEnd.Add(startNode, Heuristic(startNode, endNode));


            while (openSet.Count > 0)
            {
                // Get the node in the open set with the lowest cost
                PathNode currentNode = GetLowestCost(openSet, costToEnd);

                // If we reached the end node, reconstruct and return the path
                if (currentNode == endNode) return ReconstructPath(cameFromNode, currentNode);

                // Move current node from open to closed set
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                // Check each of the current node's connections
                foreach (var connection in currentNode.Connections)
                {
                    PathNode neighbor = connection.Key;

                    // If we've already evaluated this node, skip it
                    if (closedSet.Contains(neighbor)) continue;

                    // Calculate the new cost to reach this neighbor
                    float tentativeCostFromStart = costSoFar[currentNode] + connection.Value;

                    // If the neighbor is not in the open set, add it
                    if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                    // If the new cost is not better than a previously recorded cost, skip it
                    else if (tentativeCostFromStart >= costSoFar.GetValueOrDefault(neighbor, float.MaxValue)) continue;

                    // Record the best path to this neighbor
                    cameFromNode[neighbor] = currentNode;
                    costSoFar[neighbor] = tentativeCostFromStart;
                    costToEnd[neighbor] = costSoFar[neighbor] + Heuristic(neighbor, endNode);
                }
            }


            return new List<PathNode>(); // Return an empty path if no path is found
        }

        // Calculate the heuristic cost from the start node to the end node, manhattan distance
        private static float Heuristic(PathNode startNode, PathNode endNode)
        {
            return Vector3.Distance(startNode.transform.position, endNode.transform.position);
        }

        // Get the node in the provided open set with the lowest cost (eg closest to the end node)
        private static PathNode GetLowestCost(List<PathNode> openSet, Dictionary<PathNode, float> costs)
        {
            PathNode lowest = openSet[0];
            float lowestCost = costs[lowest];

            foreach (var node in openSet)
            {
                float cost = costs[node];
                if (cost < lowestCost)
                {
                    lowestCost = cost;
                    lowest = node;
                }
            }

            return lowest;
        }

        // Reconstruct the path from the cameFrom map
        private static List<PathNode> ReconstructPath(Dictionary<PathNode, PathNode> cameFrom, PathNode current)
        {
            List<PathNode> totalPath = new List<PathNode> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Insert(0, current);
            }
            return totalPath;
        }
    }
}
