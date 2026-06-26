using UnityEngine;

namespace ConnectOn.Network.Core
{
    public sealed class NetworkNodeData
    {
        public int Id { get; }
        public NodeKind Type { get; }
        public bool IsPlayerBuilt { get; }
        public bool CanDelete => IsPlayerBuilt && Type == NodeKind.Router;
        public Vector2 Position { get; set; }
        public float PendingDemand { get; set; }
        public float WarningGauge { get; set; }

        public NetworkNodeData(int id, NodeKind type, Vector2 position, bool isPlayerBuilt)
        {
            Id = id;
            Type = type;
            Position = position;
            IsPlayerBuilt = isPlayerBuilt;
        }
    }
}
