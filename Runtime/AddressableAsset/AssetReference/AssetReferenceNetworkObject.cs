using FishNet.Object;
using Insthync.AddressableAssetTools;

namespace FishNet.Insthync.AddressableAsset
{
    [System.Serializable]
    public class AssetReferenceNetworkObject : AssetReferenceComponent<NetworkObject>
    {
        public AssetReferenceNetworkObject(string guid) : base(guid)
        {
        }
    }
}
