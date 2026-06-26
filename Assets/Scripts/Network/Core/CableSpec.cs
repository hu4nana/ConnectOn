using System;

namespace ConnectOn.Network.Core
{
    [Serializable]
    public struct CableSpec
    {
        public CableKind Kind;
        public int Cost;
        public float BaseSpeed;
        public float MinSpeed;
        public int Capacity;
        public float AttenuationPerUnit;

        public CableSpec(CableKind kind, int cost, float baseSpeed, float minSpeed, int capacity, float attenuationPerUnit)
        {
            Kind = kind;
            Cost = cost;
            BaseSpeed = baseSpeed;
            MinSpeed = minSpeed;
            Capacity = capacity;
            AttenuationPerUnit = attenuationPerUnit;
        }

        public float GetSpeed(float traveledDistance)
        {
            float speed = BaseSpeed - traveledDistance * AttenuationPerUnit;
            return speed > MinSpeed ? speed : MinSpeed;
        }
    }
}
