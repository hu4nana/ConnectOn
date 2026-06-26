namespace ConnectOn.Network.Core
{
    public static class CableCatalog
    {
        public static CableSpec Get(CableKind kind)
        {
            switch (kind)
            {
                case CableKind.Fiber:
                    return new CableSpec(CableKind.Fiber, 8, 4.5f, 1.8f, 4, 0.02f);
                case CableKind.HighCapacity:
                    return new CableSpec(CableKind.HighCapacity, 7, 2.5f, 1.0f, 8, 0.06f);
                case CableKind.Submarine:
                    return new CableSpec(CableKind.Submarine, 10, 3.0f, 1.2f, 5, 0.04f);
                default:
                    return new CableSpec(CableKind.Copper, 3, 2.5f, 0.8f, 3, 0.08f);
            }
        }
    }
}
