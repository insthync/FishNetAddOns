using FishNet.Managing.Scened;
using Insthync.AddressableAssetTools;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace FishNet.Insthync.AddressableAsset
{
    public class AddressableSceneProcessor : DefaultSceneProcessor
    {
        [SerializeField]
        private List<AssetReferenceScene> _loadableScenes = new List<AssetReferenceScene>();
        private AsyncOperationHandle<SceneInstance> _currentAddressableAsyncOp;
        private readonly List<AsyncOperationHandle<SceneInstance>> _loadingAsyncOps = new List<AsyncOperationHandle<SceneInstance>>();
        private readonly Dictionary<int, AsyncOperationHandle<SceneInstance>> _loadedAddressableScenesByHandle = new Dictionary<int, AsyncOperationHandle<SceneInstance>>();

        private static bool IsSceneInBuild(string sceneName)
        {
            int sceneCount = UnitySceneManager.sceneCountInBuildSettings;

            for (int i = 0; i < sceneCount; ++i)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = Path.GetFileNameWithoutExtension(path);
                if (name.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public override void Initialize(Managing.Scened.SceneManager manager)
        {
            base.Initialize(manager);
            foreach (var assetRef in _loadableScenes)
            {
                AddLoadableScene(assetRef);
            }
        }

        public override void LoadStart(LoadQueueData queueData)
        {
            base.LoadStart(queueData);
            ResetAddressableValues();
        }

        public override void LoadEnd(LoadQueueData queueData)
        {
            base.LoadEnd(queueData);
            ResetAddressableValues();
        }

        private void ResetAddressableValues()
        {
            _currentAddressableAsyncOp = default;
            _loadingAsyncOps.Clear();
        }

        public override void ActivateLoadedScenes()
        {
            base.ActivateLoadedScenes();
            foreach (var loadingAsyncOp in _loadingAsyncOps)
            {
                loadingAsyncOp.Result.ActivateAsync();
            }
        }

        public override void BeginLoadAsync(string sceneName, LoadSceneParameters parameters)
        {
            if (IsSceneInBuild(sceneName))
            {
                base.BeginLoadAsync(sceneName, parameters);
                return;
            }
            if (!s_SceneNameToRuntimeKey.TryGetValue(sceneName, out var runtimeKey))
            {
                Debug.LogError($"Unable to load addressable scene {sceneName}, its asset reference may not added to loadable collection, try use `AddressableSceneProcessor.AddLoadableScene()` function to add it.");
                return;
            }
            // Determine that the `sceneName` is adressable key
            var newOp = Addressables.LoadSceneAsync(runtimeKey, parameters, false);
            _loadingAsyncOps.Add(newOp);
            _currentAddressableAsyncOp = newOp;
        }

        public override void BeginUnloadAsync(Scene scene)
        {
            if (!_loadedAddressableScenesByHandle.TryGetValue(scene.handle, out var loadHandle))
            {
                // Scene is not loaded by addressable asset system?
                base.BeginUnloadAsync(scene);
                return;
            }
            // Scene is loaded by addressable asset system
            var unloadHandle = Addressables.UnloadSceneAsync(loadHandle, false);
            _currentAddressableAsyncOp = unloadHandle;
            _loadedAddressableScenesByHandle.Remove(scene.handle);
            Scenes.Remove(scene);
        }

        public override bool IsPercentComplete()
        {
            if (CurrentAsyncOperation != null)
            {
                return CurrentAsyncOperation.progress >= 0.9f;
            }
            else if (_currentAddressableAsyncOp.IsValid())
            {
                bool isDone = _currentAddressableAsyncOp.IsDone;
                if (isDone)
                {
                    Scene scene = _currentAddressableAsyncOp.Result.Scene;
                    if (_loadedAddressableScenesByHandle.TryAdd(scene.handle, _currentAddressableAsyncOp))
                        Scenes.Add(scene);
                }
                return isDone;
            }
            return false;
        }

        public override float GetPercentComplete()
        {
            if (CurrentAsyncOperation != null)
            {
                return CurrentAsyncOperation.progress;
            }
            else if (_currentAddressableAsyncOp.IsValid())
            {
                return _currentAddressableAsyncOp.PercentComplete;
            }
            return 1f;
        }

        public override IEnumerator AsyncsIsDone()
        {
            bool notDone;

            do
            {
                notDone = false;
                foreach (AsyncOperation ao in LoadingAsyncOperations)
                {

                    if (!ao.isDone)
                    {
                        notDone = true;
                        break;
                    }
                }
                yield return null;
            } while (notDone);

            do
            {
                notDone = false;
                foreach (var ao in _loadingAsyncOps)
                {

                    if (!ao.IsDone)
                    {
                        notDone = true;
                        break;
                    }
                }
                yield return null;
            } while (notDone);

            yield break;
        }


        private static Dictionary<string, object> s_SceneNameToRuntimeKey = new Dictionary<string, object>();
        private static Dictionary<object, string> s_RuntimeKeyToSceneName = new Dictionary<object, string>();
        public static string AddLoadableScene(AssetReferenceScene assetRef)
        {
            return AddLoadableScene(assetRef.RuntimeKey, assetRef.SceneName);
        }

        public static string AddLoadableScene(object runtimeKey, string sceneName)
        {
            if (s_RuntimeKeyToSceneName.ContainsKey(runtimeKey))
                return sceneName;
            s_SceneNameToRuntimeKey[sceneName] = runtimeKey;
            s_RuntimeKeyToSceneName[runtimeKey] = sceneName;
            return sceneName;
        }
    }
}
