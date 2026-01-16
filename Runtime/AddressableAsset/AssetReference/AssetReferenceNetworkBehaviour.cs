using FishNet.Object;

namespace FishNet.Insthync.AddressableAsset
{
    [System.Serializable]
    public class AssetReferenceNetworkBehaviour<TBehaviour> : AssetReferenceNetworkObject
        where TBehaviour : NetworkBehaviour
    {
        public AssetReferenceNetworkBehaviour(string guid) : base(guid)
        {
        }
    }

    [System.Serializable]
    public class AssetReferenceNetworkBehaviour : AssetReferenceNetworkBehaviour<NetworkBehaviour>
    {
        public AssetReferenceNetworkBehaviour(string guid) : base(guid)
        {
        }
    }
}
