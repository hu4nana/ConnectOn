namespace ConnectOn.Network.Core
{
    public sealed class NetworkEdgeData
    {
        public int Id { get; }
        public NetworkNodeData A { get; }
        public NetworkNodeData B { get; }
        public float Length { get; }
        public CableSpec Cable { get; }
        public bool IsPlayerBuilt { get; }
        public bool CanDelete => IsPlayerBuilt;
        public float Speed => Cable.BaseSpeed;
        public int Capacity => Cable.Capacity;
        public int CurrentLoad { get; private set; }
        public int WaitingCount { get; private set; }

        public NetworkEdgeData(int id, NetworkNodeData a, NetworkNodeData b, float length, CableSpec cable, bool isPlayerBuilt)
        {
            Id = id;
            A = a;
            B = b;
            Length = length;
            Cable = cable;
            IsPlayerBuilt = isPlayerBuilt;
        }

        public NetworkNodeData Other(NetworkNodeData node)
        {
            if (node == A)
                return B;
            if (node == B)
                return A;
            return null;
        }

        public bool HasCapacity()
        {
            return CurrentLoad < Capacity;
        }

        public bool TryEnter()
        {
            if (!HasCapacity())
                return false;

            CurrentLoad++;
            return true;
        }

        public void Exit()
        {
            if (CurrentLoad > 0)
                CurrentLoad--;
        }

        public void AddWaiting()
        {
            WaitingCount++;
        }

        public void RemoveWaiting()
        {
            if (WaitingCount > 0)
                WaitingCount--;
        }
    }
}
