using UnityEngine;

namespace ConnectOn.Network.Systems
{
    public sealed class EconomySystem : MonoBehaviour
    {
        [SerializeField] int initialCredits = 50;
        [SerializeField] int rewardPerResponse = 1;

        public int Credits { get; private set; }

        void Awake()
        {
            Credits = initialCredits;
        }

        public bool CanAfford(int cost)
        {
            return Credits >= cost;
        }

        public bool TrySpend(int cost)
        {
            if (!CanAfford(cost))
                return false;

            Credits -= cost;
            return true;
        }

        public void AddResponseReward()
        {
            Credits += rewardPerResponse;
        }
    }
}
