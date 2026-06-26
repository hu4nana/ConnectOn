using ConnectOn.Network.Core;
using UnityEngine;

namespace ConnectOn.Network.Systems
{
    public sealed class CableBuildSelection : MonoBehaviour
    {
        [SerializeField] BuildMode mode = BuildMode.Cable;
        [SerializeField] CableKind currentCableKind = CableKind.Copper;
        [SerializeField] NodeKind currentNodeKind = NodeKind.Router;

        public BuildMode Mode => mode;
        public CableKind CurrentCableKind => currentCableKind;
        public NodeKind CurrentNodeKind => currentNodeKind;

        public void Select(CableKind cableKind)
        {
            currentCableKind = cableKind;
            mode = BuildMode.Cable;
        }

        public void Select(NodeKind nodeKind)
        {
            currentNodeKind = nodeKind;
            mode = BuildMode.FunctionalNode;
        }

        public void SelectDelete()
        {
            mode = BuildMode.Delete;
        }

        public void Clear()
        {
            mode = BuildMode.None;
        }
    }
}
