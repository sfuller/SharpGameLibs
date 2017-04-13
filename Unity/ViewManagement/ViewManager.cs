using System;
using System.Collections.Generic;
using SFuller.SharpGameLibs.Core.IOC;
using SFuller.SharpGameLibs.Core.Logging;
using SFuller.SharpGameLibs.Core.ViewManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using SFuller.SharpGameLibs.Core.Update;

namespace SFuller.SharpGameLibs.Unity.ViewManagement
{
    class Binding {
        public List<BindingTarget> Targets = new List<BindingTarget>();
    }

    [Dependencies(new Type[]{
        typeof(IUpdateManager)
    })]

    public sealed class ViewManager : IViewManager, IInitializable, IDisposable, IResourceGroupHandleOwner
    {
        public ViewManager(ViewRegistry registry) {
            _registry = registry;
        }

        public void Init(IIOCProvider systems) {
            _logger = systems.Get<Core.Logging.ILogger>();
            if (_logger == null) {
                _logger = new NullLogger();
            }

            foreach (TypeBinding bindingData in _registry.Bindings) {
                Type type = Type.GetType(bindingData.TypeName);
                if (type == null) {
                    _logger.LogError(string.Format(
                        "Could not get type for qualified name '{0}'",
                        bindingData.TypeName
                    ));
                    continue;
                }
                var binding = new Binding();
                binding.Targets.AddRange(bindingData.Targets);
                _bindings.Add(type, binding);
            }

            _scene = SceneManager.CreateScene("Managed_by_View_Manager");
            _updates = systems.Get<IUpdateManager>();
        }

        public void Dispose() {

        }

        public T Instantiate<T>() where T : IView {
            return Instantiate<T>(0);
        }

        public T Instantiate<T>(uint tag) where T : IView {
            GameObject prefab = GetPrefab<T>(tag);
            if (prefab == null) {
                _logger.LogError(
                    string.Format(
                        "Cannot instantiate view! Type: {0}, Tag: {1}\n" +
                        "Either no prefab is bound or it has not been loaded.",
                        typeof(T), tag
                    )
                );
                return default(T);
            }
            GameObject obj = InstantiatePrefab(GetPrefab<T>(tag));
            return GetComponent<T>(obj);
        }

        public void Destroy<T>(T view) where T : IView {
            MonoBehaviour behaviour = view as MonoBehaviour;
            if (behaviour != null) {
                GameObject.Destroy(behaviour.gameObject);
            }
        }

        public IResourceGroupHandle AddResourceGroup(ResourceGroup group)
        {
            var handle = new ResourceGroupHandle(group, this);
            foreach (AssetDescriptor descriptor in handle.Assets) {
                AssetData data = GetAssetData(descriptor.Type, descriptor.Tag);
                data.Handles.Add(handle);
            }
            return handle;
        }

        private GameObject InstantiatePrefab(GameObject prefab) {
            GameObject obj = GameObject.Instantiate(prefab);
            SceneManager.MoveGameObjectToScene(obj, _scene);
            return obj;
        }

        private GameObject GetPrefab<T>(uint tag) where T : IView {
            Type type = typeof(T);
            Dictionary<uint, AssetData> assetsByTag;
            if (!_assets.TryGetValue(type, out assetsByTag)) {
                return null;
            }
            AssetData data;
            if (!assetsByTag.TryGetValue(tag, out data)) {
                return null;
            }
            return data.LoadedObject;
        }

        private AssetData GetAssetData(Type type, uint tag) {
            Dictionary<uint, AssetData> assetsByTag;
            if (!_assets.TryGetValue(type, out assetsByTag)) {
                assetsByTag = new Dictionary<uint, AssetData>();
                _assets[type] = assetsByTag;
            }
            AssetData data;
            if (!assetsByTag.TryGetValue(tag, out data)) {
                data = new AssetData();
                assetsByTag[tag] = data;
            }
            return data;
        }

        private T GetComponent<T>(GameObject obj) where T : IView {
            T view = obj.GetComponent<T>();
            if (view == null) {
                _logger.LogWarning(string.Format(
                    "No component implementing {0} found on game object {1}",
                    typeof(T).FullName, obj.name
                ));
            }
            return view;
        }

        private BindingTarget GetBindingTarget(Type type, uint tag) {
            Binding binding;
            if (!_bindings.TryGetValue(type, out binding)) {
                return null;
            }
            return binding.Targets.Find(x => x.Tag == tag);
        }

        void IResourceGroupHandleOwner.Load(ResourceGroupHandle handle) {
            var prefabAssets = new List<AssetDescriptor>();
            var resourceAssets = new List<AssetDescriptor>();

            GetAssetDescriptorsByMechanism(handle.Assets, prefabAssets, resourceAssets);

            // 'Load' prefab assets
            foreach (AssetDescriptor descriptor in prefabAssets) {
                BindingTarget target = GetBindingTarget(descriptor.Type, descriptor.Tag);
                AssetData data = GetAssetData(descriptor.Type, descriptor.Tag);
                LogAssetLoadStart(descriptor);
                LogAssetLoadFinish(descriptor);
                data.LoadedObject = target.Prefab;
            }

            if (resourceAssets.Count < 1) {
                // We're all loaded. We can call back right now.
                handle.HandleLoaded();
                return;
            }

            foreach (AssetDescriptor descriptor in resourceAssets) {
                BindingTarget target = GetBindingTarget(descriptor.Type, descriptor.Tag);
                ResourceOperation op = new ResourceOperation(_updates, descriptor, target.ResourcePath);
                LogAssetLoadStart(descriptor);
                handle.Operations.AddOperation(op);
            }

            handle.StartOperations();
        }

        void IResourceGroupHandleOwner.Unload(ResourceGroupHandle handle) {
            var prefabAssets = new List<AssetDescriptor>();
            var resourceAssets = new List<AssetDescriptor>();

            GetAssetDescriptorsByMechanism(handle.Assets, prefabAssets, resourceAssets);

            // 'Unload' prefab assets
            foreach (AssetDescriptor descriptor in prefabAssets) {
                BindingTarget target = GetBindingTarget(descriptor.Type, descriptor.Tag);
                AssetData data = GetAssetData(descriptor.Type, descriptor.Tag);
                data.Handles.Remove(handle);
                if (data.Handles.Count < 1) {
                    LogAssetUnload(descriptor);
                    data.LoadedObject = null;
                }
            }

            // Unload Resource assets
            foreach (AssetDescriptor descriptor in resourceAssets) {
                BindingTarget target = GetBindingTarget(descriptor.Type, descriptor.Tag);
                AssetData data = GetAssetData(descriptor.Type, descriptor.Tag);
                data.Handles.Remove(handle);
                if (data.Handles.Count < 1) {
                    LogAssetUnload(descriptor);
                    //Resources.UnloadAsset(data.LoadedObject);
                }
            }
        }

        void IResourceGroupHandleOwner.AddLoadedAsset(ResourceGroupHandle handle, AssetDescriptorAndPrefab asset)
        {
            AssetDescriptor descriptor = asset.Descriptor;
            AssetData data = GetAssetData(descriptor.Type, descriptor.Tag);
            data.LoadedObject = asset.Prefab;
            LogAssetLoadFinish(descriptor);
        }

        void IResourceGroupHandleOwner.HandleStartingAssetLoad(AssetDescriptor descriptor)
        {
            LogAssetLoadStart(descriptor);
        }

        private void GetAssetDescriptorsByMechanism(IEnumerable<AssetDescriptor> descriptors, List<AssetDescriptor> prefabAssets, List<AssetDescriptor> resourceAssets)
        {
            foreach (AssetDescriptor descriptor in descriptors) {
                BindingTarget target = GetBindingTarget(descriptor.Type, descriptor.Tag);
                if (target == null) {
                    continue;
                }
                List<AssetDescriptor> targetList;
                switch (target.Type) {
                    case TargetType.PreloadedPrefab:
                        targetList = prefabAssets;
                        break;
                    case TargetType.Resource:
                        targetList = resourceAssets;
                        break;
                    default:
                        targetList = null;
                        break;
                }
                targetList.Add(descriptor);
            }
        }

        private void LogAssetLoadStart(AssetDescriptor descriptor)
        {
            _logger.LogInfo(string.Format("ViewManager: Starting to load asset: Type: {0}, Tag: {1}",
                descriptor.Type, descriptor.Tag));
        }

        private void LogAssetLoadFinish(AssetDescriptor descriptor)
        {
            _logger.LogInfo(string.Format("ViewManager: Finished loading asset: Type: {0}, Tag: {1}",
                descriptor.Type, descriptor.Tag));
        }

        private void LogAssetUnload(AssetDescriptor descriptor)
        {
            _logger.LogInfo(string.Format("ViewManager: Unloading asset: Type: {0}, Tag: {1}",
                descriptor.Type, descriptor.Tag));
        }

        private readonly ViewRegistry _registry;
        private Core.Logging.ILogger _logger;
        private IUpdateManager _updates;
        private readonly Dictionary<Type, Binding> _bindings = new Dictionary<Type, Binding>();
        private List<ResourceGroupHandle> _groups = new List<ResourceGroupHandle>();

        private readonly Dictionary<Type, Dictionary<uint, AssetData>> _assets 
            = new Dictionary<Type, Dictionary<uint, AssetData>>();


        private Scene _scene;
    }


    class AssetData
    {
        public GameObject LoadedObject;

        /// <summary>
        /// All group handles which need this object loaded.
        /// When the last handle for this asset is removed, the object is unloaded.
        /// </summary>
        public readonly List<ResourceGroupHandle> Handles = new List<ResourceGroupHandle>();
    }

    interface IResourceGroupHandleOwner
    {
        void Load(ResourceGroupHandle handle);
        void Unload(ResourceGroupHandle handle);
        void HandleStartingAssetLoad(AssetDescriptor descriptor);
        void AddLoadedAsset(ResourceGroupHandle handle, AssetDescriptorAndPrefab asset);
    }

    class ResourceGroupHandle : IResourceGroupHandle
    {
        public event Action<object, LoadedEventArgs> Loaded;

        public ResourceGroupHandle(ResourceGroup group, IResourceGroupHandleOwner owner) {
            Assets = new List<AssetDescriptor>(group.Assets);
            _owner = owner;
        }

        public void HandleLoaded() {
            var handler = Loaded;
            if (handler != null)
            {
                handler(this, new LoadedEventArgs());
            }
        }

        public void StartOperations() {
            Operations.Finished += HandleOperationFinished;
            Operations.AllFinished += HandleAllOperationsFinished;
            Operations.Start();
        }

        private void HandleStartingOperation(object sender, StartingOperationEventArgs args) {
            _owner.HandleStartingAssetLoad(args.Descriptor);
        }

        private void HandleOperationFinished(object sender, OperationFinishedEventArgs args) {
            _owner.AddLoadedAsset(this, args.Data);
        }

        private void HandleAllOperationsFinished(object sender, EventArgs args) {
            HandleLoaded();
        }

        public void Load() {
            _owner.Load(this);
        }

        public void Unload() {
            _owner.Unload(this);
        }

        private readonly IResourceGroupHandleOwner _owner;
        public readonly List<AssetDescriptor> Assets;
        public readonly OperationRunner Operations = new OperationRunner();
    }

    class OperationFinishedEventArgs : EventArgs
    {
        public AssetDescriptorAndPrefab Data;
    }

    class StartingOperationEventArgs : EventArgs
    {
        public AssetDescriptor Descriptor;
    }

    class AssetDescriptorAndPrefab
    {
        public AssetDescriptor Descriptor;
        public GameObject Prefab;
    }

    interface IAssetOperation
    {
        AssetDescriptor Descriptor { get; }
        event Action<object, OperationFinishedEventArgs> Finished;
        void Start();
    }

    class OperationRunner
    {
        public event Action<object, StartingOperationEventArgs> Starting;
        public event Action<object, OperationFinishedEventArgs> Finished;
        public event Action<object, EventArgs> AllFinished;

        public void AddOperation(IAssetOperation op) {
            _nextOperations.Enqueue(op);
        }

        public void Start() {
            HandleOperationFinished(this, new OperationFinishedEventArgs());
        }

        private void HandleOperationFinished(object sender, OperationFinishedEventArgs args) {
            if (_currentOperation != null) {
                _currentOperation.Finished -= HandleOperationFinished;
                var handler = Finished;
                if (handler != null) {
                    handler(this, args);
                }
            }

            if (_nextOperations.Count < 1) {
                // All operations finished
                var handler = AllFinished;
                if (handler != null) {
                    handler(this, EventArgs.Empty);
                }
                return;
            }

            _currentOperation = _nextOperations.Dequeue();
            _currentOperation.Finished += HandleOperationFinished;
            HandleStartingOperation(_currentOperation); ;
            _currentOperation.Start();
        }

        private void HandleStartingOperation(IAssetOperation op) {
            var handler = Starting;
            if (handler != null) {
                handler(this, new StartingOperationEventArgs() { Descriptor = op.Descriptor });
            }
        }

        private IAssetOperation _currentOperation;
        private readonly Queue<IAssetOperation> _nextOperations = new Queue<IAssetOperation>();
    }

    class ResourceOperation : IAssetOperation, IUpdatable
    {
        public event Action<object, OperationFinishedEventArgs> Finished;

        public AssetDescriptor Descriptor {  get { return _descriptor; } }

        public ResourceOperation(IUpdateManager updates, AssetDescriptor descriptor, string path) {
            _updates = updates;
            _descriptor = descriptor;
            _path = path;
        }

        public void Start() {
            _request = Resources.LoadAsync<GameObject>(_path);
            _updates.Register(this);
        }

        public void Update(float timestep) {
            if (_request.isDone) {
                _updates.Unregister(this);
                var handler = Finished;
                if (handler != null) {
                    handler(this, new OperationFinishedEventArgs() {
                        Data = new AssetDescriptorAndPrefab() {
                            Descriptor = _descriptor,
                            Prefab = _request.asset as GameObject
                        }
                    });
                }
            }
        }

        private IUpdateManager _updates;
        private ResourceRequest _request;
        private AssetDescriptor _descriptor;
        private string _path;
    }


}