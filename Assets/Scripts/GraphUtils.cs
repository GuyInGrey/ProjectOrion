using System.Collections.Generic;
using System.Linq;

public class Edge
{
    public int A;
    public int B;
}

public static class GraphUtils
{
    public static List<Edge> FilterConnectedComponent(List<Edge> edges, int startNode)
    {
        // Build adjacency list
        var adjacency = new Dictionary<int, List<int>>();
        foreach (var e in edges)
        {
            if (!adjacency.ContainsKey(e.A))
                adjacency[e.A] = new List<int>();
            if (!adjacency.ContainsKey(e.B))
                adjacency[e.B] = new List<int>();
            adjacency[e.A].Add(e.B);
            adjacency[e.B].Add(e.A);
        }

        // If the start node isn't in the graph, return empty
        if (!adjacency.ContainsKey(startNode))
            return new List<Edge>();

        // BFS or DFS to collect reachable nodes
        var visited = new HashSet<int>();
        var stack = new Stack<int>();
        stack.Push(startNode);
        visited.Add(startNode);

        while (stack.Count > 0)
        {
            int current = stack.Pop();
            foreach (var neighbor in adjacency[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    stack.Push(neighbor);
                }
            }
        }

        // Keep only edges where both ends are visited
        return edges.Where(e => visited.Contains(e.A) && visited.Contains(e.B)).ToList();
    }

    public static List<Edge> BuildConnections(List<Edge> edges)
    {
        // Collect all unique node IDs
        var nodeIds = edges.SelectMany(e => new[] { e.A, e.B }).Distinct().ToList();

        // Map arbitrary node IDs to sequential indices
        var idToIndex = nodeIds
            .Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => x.index);

        var parent = Enumerable.Range(0, nodeIds.Count).ToArray();
        var result = new List<Edge>();

        foreach (var e in edges)
        {
            // Skip edges with missing or invalid nodes
            if (!idToIndex.ContainsKey(e.A) || !idToIndex.ContainsKey(e.B))
                continue;

            int rootA = Find(parent, idToIndex[e.A]);
            int rootB = Find(parent, idToIndex[e.B]);

            if (rootA == rootB)
                continue;

            Union(parent, rootA, rootB);
            result.Add(e);
        }

        return result;
    }

    private static int Find(int[] parent, int i)
    {
        if (parent[i] != i)
            parent[i] = Find(parent, parent[i]);
        return parent[i];
    }

    private static void Union(int[] parent, int a, int b)
    {
        parent[Find(parent, b)] = Find(parent, a);
    }
}