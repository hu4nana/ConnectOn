using System.Collections.Generic;
using ConnectOn.Network.Core;

namespace ConnectOn.Network.Systems
{
    public sealed class RoutingSystem
    {
        readonly NetworkGraph graph;

        public RoutingSystem(NetworkGraph graph)
        {
            this.graph = graph;
        }

        public List<NetworkEdgeData> FindNearestServerPath(NetworkNodeData start)
        {
            if (start == null)
                return null;

            NetworkNodeData bestServer = null;
            Dictionary<NetworkNodeData, NetworkEdgeData> previousEdge;
            Dictionary<NetworkNodeData, float> distance;

            RunDijkstra(start, out previousEdge, out distance);

            float bestDistance = float.PositiveInfinity;
            IReadOnlyList<NetworkNodeData> nodes = graph.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                NetworkNodeData node = nodes[i];
                if (node.Type != NodeKind.DataServer || !distance.TryGetValue(node, out float nodeDistance))
                    continue;

                if (nodeDistance < bestDistance)
                {
                    bestDistance = nodeDistance;
                    bestServer = node;
                }
            }

            return bestServer == null ? null : BuildPath(start, bestServer, previousEdge);
        }

        public List<NetworkEdgeData> FindPath(NetworkNodeData start, NetworkNodeData end)
        {
            if (start == null || end == null)
                return null;

            RunDijkstra(start, out Dictionary<NetworkNodeData, NetworkEdgeData> previousEdge, out Dictionary<NetworkNodeData, float> distance);
            return distance.ContainsKey(end) ? BuildPath(start, end, previousEdge) : null;
        }

        void RunDijkstra(NetworkNodeData start, out Dictionary<NetworkNodeData, NetworkEdgeData> previousEdge, out Dictionary<NetworkNodeData, float> distance)
        {
            previousEdge = new Dictionary<NetworkNodeData, NetworkEdgeData>();
            distance = new Dictionary<NetworkNodeData, float>();
            List<NetworkNodeData> open = new List<NetworkNodeData>();

            distance[start] = 0f;
            open.Add(start);

            while (open.Count > 0)
            {
                NetworkNodeData current = TakeNearest(open, distance);
                IReadOnlyList<NetworkEdgeData> edges = graph.GetEdges(current);

                for (int i = 0; i < edges.Count; i++)
                {
                    NetworkEdgeData edge = edges[i];
                    NetworkNodeData next = edge.Other(current);
                    if (next == null)
                        continue;

                    float candidate = distance[current] + GetCost(edge);
                    if (distance.TryGetValue(next, out float oldDistance) && candidate >= oldDistance)
                        continue;

                    distance[next] = candidate;
                    previousEdge[next] = edge;
                    if (!open.Contains(next))
                        open.Add(next);
                }
            }
        }

        NetworkNodeData TakeNearest(List<NetworkNodeData> open, Dictionary<NetworkNodeData, float> distance)
        {
            int bestIndex = 0;
            float bestDistance = distance[open[0]];

            for (int i = 1; i < open.Count; i++)
            {
                float candidate = distance[open[i]];
                if (candidate >= bestDistance)
                    continue;

                bestDistance = candidate;
                bestIndex = i;
            }

            NetworkNodeData best = open[bestIndex];
            open.RemoveAt(bestIndex);
            return best;
        }

        float GetCost(NetworkEdgeData edge)
        {
            float congestion = edge.Capacity <= 0 ? 1000f : edge.CurrentLoad / (float)edge.Capacity;
            float averageSpeed = edge.Cable.GetSpeed(edge.Length * 0.5f);
            return edge.Length / averageSpeed + congestion;
        }

        List<NetworkEdgeData> BuildPath(NetworkNodeData start, NetworkNodeData end, Dictionary<NetworkNodeData, NetworkEdgeData> previousEdge)
        {
            List<NetworkEdgeData> path = new List<NetworkEdgeData>();
            NetworkNodeData current = end;

            while (current != start)
            {
                if (!previousEdge.TryGetValue(current, out NetworkEdgeData edge))
                    return null;

                path.Add(edge);
                current = edge.Other(current);
            }

            path.Reverse();
            return path;
        }
    }
}
