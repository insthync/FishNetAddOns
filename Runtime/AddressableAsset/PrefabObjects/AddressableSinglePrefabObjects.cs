using FishNet.Managing.Object;
using FishNet.Object;
using Insthync.AddressableAssetTools;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FishNet.Insthync.AddressableAsset
{
    [CreateAssetMenu(fileName = "New SinglePrefabObjects", menuName = "FishNet/Spawnable Prefabs/Addressable Single Prefab Objects")]
    public class AddressableSinglePrefabObjects : SinglePrefabObjects
    {
        [SerializeField]
        private List<AssetReferenceNetworkObject> _assetReferences = new List<AssetReferenceNetworkObject>();
        public List<AssetReferenceNetworkObject> AssetReferences => _assetReferences;

        public override async void InitializePrefabRange(int startIndex)
        {
            base.InitializePrefabRange(startIndex);
            await LoadAllPrefabs(startIndex);
        }

        private async Task LoadAllPrefabs(int startIndex)
        {
            int i = 0;
            if (startIndex > Prefabs.Count)
                i = startIndex - Prefabs.Count;
            List<Task<NetworkObject>> ops = new List<Task<NetworkObject>>();
            for (; i < _assetReferences.Count; ++i)
            {
                int prefabIndex = Prefabs.Count + i;
                if (_assetReferences[i].IsDataValid())
                    ops.Add(LoadPrefab(prefabIndex, _assetReferences[i]));
            }
            await Task.WhenAll(ops);
        }

        private async Task<NetworkObject> LoadPrefab(int prefabIndex, AssetReferenceNetworkObject assetRef)
        {
            NetworkObject prefab = await assetRef.GetOrLoadAssetAsync<NetworkObject>();
            ManagedObjects.InitializePrefab(prefab, prefabIndex, CollectionId);
            return prefab;
        }

        public override void RemoveNull()
        {
            base.RemoveNull();
            for (int i = _assetReferences.Count - 1; i >= 0; --i)
            {
                if (!_assetReferences[i].IsDataValid())
                    _assetReferences.RemoveAt(i);
            }
        }
    }
}
