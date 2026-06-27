using System.Collections.Generic;

namespace ConnectOn.Network.Core
{
    public sealed class PacketData
    {
        public int Id { get; }
        public NetworkPacketKind Type { get; }
        public NetworkNodeData Source { get; }
        public NetworkNodeData Destination { get; }
        public IReadOnlyList<NetworkEdgeData> Path { get; }
        public int EdgeIndex { get; private set; }
        public float Progress { get; set; }
        public bool IsWaiting { get; set; }
        public bool IsComplete { get; private set; }

        public NetworkEdgeData CurrentEdge => EdgeIndex < Path.Count ? Path[EdgeIndex] : null;

        public PacketData(int id, NetworkPacketKind type, NetworkNodeData source, NetworkNodeData destination, IReadOnlyList<NetworkEdgeData> path)
        {
            Id = id;
            Type = type;
            Source = source;
            Destination = destination;
            Path = path;
        }

        public void AdvanceEdge()
        {
            EdgeIndex++;
            Progress = 0f;
            IsComplete = EdgeIndex >= Path.Count;
        }
    }
}
