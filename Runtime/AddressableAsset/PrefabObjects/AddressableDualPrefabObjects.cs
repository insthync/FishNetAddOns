using FishNet.Managing;
using FishNet.Managing.Object;
using FishNet.Object;
using Insthync.AddressableAssetTools;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FishNet.Insthync.AddressableAsset
{
    [CreateAssetMenu(fileName = "New DualPrefabObjects", menuName = "FishNet/Spawnable Prefabs/Addressable Dual Prefab Objects")]
    public class AddressableDualPrefabObjects : DualPrefabObjects
    {
        [SerializeField]
        private List<AddressableDualPrefab> _assetReferences = new List<AddressableDualPrefab>();
        public List<AddressableDualPrefab> AssetReferences => _assetReferences;

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

        private async Task LoadPrefab(int prefabIndex, AddressableDualPrefab assetRef)
        {
            NetworkObject server = await assetRef.Server.GetOrLoadAssetAsync<NetworkObject>();
            ManagedObjects.InitializePrefab(server, prefabIndex, CollectionId);
            NetworkObject client = await assetRef.Client.GetOrLoadAssetAsync<NetworkObject>();
            ManagedObjects.InitializePrefab(client, prefabIndex, CollectionId);
        }

        public override void RemoveNull()
        {
            base.RemoveNull();
            for (int i = _assetReferences.Count - 1; i >= 0; --i)
            {
                if (!_assetReferences[i].Server.IsDataValid() || !_assetReferences[i].Client.IsDataValid())
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
                AssetReferenceNetworkObject nobRef = asServer ? AssetReferences[index].Server : AssetReferences[index].Client;
                NetworkObject nob = nobRef.GetOrLoadAsset<NetworkObject>();
                if (nob == null)
                {
                    string lookupSide = asServer ? "server" : "client";
                    NetworkManagerExtensions.LogError($"Prefab for {lookupSide} on id {id} is null ");
                }

                return nob;
            }
            else
            {
                DualPrefab dp = Prefabs[id];
                NetworkObject nob = asServer ? dp.Server : dp.Client;
                if (nob == null)
                {
                    string lookupSide = asServer ? "server" : "client";
                    NetworkManagerExtensions.LogError($"Prefab for {lookupSide} on id {id} is null ");
                }

                return nob;
            }
        }
    }
}
