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
        private AsyncOperationHandle<SceneInstance> _currentAddressableAsyncOp;
        private readonly List<AsyncOperationHandle<SceneInstance>> _loadingAsyncOps = new List<AsyncOperationHandle<SceneInstance>>();

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
                if (!AddressableAssetsManager.TryGetRuntimeKeyBySceneName(sceneName, out runtimeKey))
                {
                    Debug.LogError($"Unable to load addressable scene {sceneName}, its asset reference may not added to loadable collection, try use `AddressableSceneProcessor.AddLoadableScene()` function to add it.");
                    return;
                }
                // Store to cache, so next time is faster.
                AddLoadableScene(runtimeKey, sceneName);
            }
            var newOp = Addressables.LoadSceneAsync(runtimeKey, parameters, false);
            _loadingAsyncOps.Add(newOp);
            _currentAddressableAsyncOp = newOp;
        }

        public override void BeginUnloadAsync(Scene scene)
        {
            if (!TryGetLoadingAsyncOp(scene, out var loadHandle))
            {
                base.BeginUnloadAsync(scene);
                return;
            }
            var unloadHandle = Addressables.UnloadSceneAsync(loadHandle, true);
            _loadingAsyncOps.Remove(loadHandle);
            _currentAddressableAsyncOp = unloadHandle;
        }

        private bool TryGetLoadingAsyncOp(Scene scene, out AsyncOperationHandle<SceneInstance> result)
        {
            foreach (var loadingAsyncOp in _loadingAsyncOps)
            {
                if (loadingAsyncOp.Result.Scene.handle == scene.handle)
                {
                    result = loadingAsyncOp;
                    return true;
                }
            }
            result = default;
            return false;
        }

        public override bool IsPercentComplete()
        {
            if (CurrentAsyncOperation != null)
            {
                return CurrentAsyncOperation.progress >= 0.9f;
            }
            else if (_currentAddressableAsyncOp.IsValid())
            {
                return _currentAddressableAsyncOp.IsDone;
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
