using FishNet.Managing;
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
            List<Task> ops = new List<Task>();
            for (; i < _assetReferences.Count; ++i)
            {
                int prefabIndex = Prefabs.Count + i;
                ops.Add(LoadPrefab(prefabIndex, _assetReferences[i]));
            }
            await Task.WhenAll(ops);
        }

        private async Task LoadPrefab(int prefabIndex, AssetReferenceNetworkObject assetRef)
        {
            NetworkObject prefab = await assetRef.GetOrLoadAssetAsync<NetworkObject>();
            ManagedObjects.InitializePrefab(prefab, prefabIndex, CollectionId);
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
        public override void Clear()
        {
            base.Clear();
            _assetReferences.Clear();
        }

        public override int GetObjectCount()
        {
            return Prefabs.Count + AssetReferences.Count;
        }

        public override NetworkObject GetObject(bool asServer, int id)
        {
            if (id < 0)
            {
                NetworkManagerExtensions.LogError($"PrefabId {id} is out of range.");
                return null;
            }
            else if (id >= GetObjectCount())
            {
                NetworkManagerExtensions.LogError($"PrefabId {id} is out of range.");
                return null;
            }
            else if (id >= Prefabs.Count)
            {
                int index = id - Prefabs.Count;
                NetworkObject nob = AssetReferences[index].GetOrLoadAsset<NetworkObject>();
                if (nob == null)
                    NetworkManagerExtensions.LogError($"Prefab on id {id} is null.");

                return nob;
            }
            else
            {
                NetworkObject nob = Prefabs[id];
                if (nob == null)
                    NetworkManagerExtensions.LogError($"Prefab on id {id} is null.");

                return nob;
            }
        }
    }
}
