using System;
using System.Collections.Generic;

namespace SFuller.SharpGameLibs.Core.IOC
{
    public enum ContainerInitStatus {
        Ok,
        MissingDependencies,
        CircularDependency
    }

    public struct ContainerInitResult {
        public ContainerInitStatus Status;
        public IEnumerable<UnitDefinition> Missing;
        public IEnumerable<CircularDependency<UnitDefinition>> Circular;
    }

    public struct CircularDependency<T> {
        public IEnumerable<T> Chain;
    }

    public class IOCContainer : IIOCProvider
    {
        private interface IRootUnit { };

        private class UnitInfo {
            public object Instance;
            public UnitDefinition Definition;
        }

        private struct DependencyInfo<T> {
            public T[] Dependencies;
        }

        public IOCContainer(IDependencyProvider dependencyProvider = null) {
            _rootDependencies = new UnitDefinition[] { _rootDefinition };

            if (dependencyProvider == null) {
                dependencyProvider = new DependencyProvider();
            }
            _dependencyProvider = dependencyProvider;
        }

        public void SetContext(Context context) {
            _context = context;
        }

        public ContainerInitResult Init() {
            var result = new ContainerInitResult();

            GenerateUnitInfo();

            // Generate the dependency graph
            var dependencies = new Dictionary<UnitDefinition, DependencyInfo<UnitDefinition>>();
            MakeDependencyInfo(_units, dependencies);
            GraphGenResult<UnitDefinition> graphResult = MakeDependencyGraph<UnitDefinition>(dependencies, _rootDefinition);
            if (graphResult.MissingDependencies.Count > 0) {
                result.Missing = graphResult.MissingDependencies;
                result.Status = ContainerInitStatus.MissingDependencies;
                return result;
            }

            // Check for circular dependencies
            var unitsWithCircularDependencies = new List<CircularDependency<UnitDefinition>>();
            DetectCircularDependencies(graphResult.Graph, unitsWithCircularDependencies);
            if (unitsWithCircularDependencies.Count > 0) {
                result.Status = ContainerInitStatus.CircularDependency;
                result.Circular = unitsWithCircularDependencies;
                return result;
            }

            // Resolve dependencies
            var unitsInOrder = new List<UnitDefinition>();
            ResolveDependencyGraph(graphResult.Root, unitsInOrder);

            // Initialize systems
            for (int i = 0, ilen = unitsInOrder.Count; i < ilen; ++i) {

                object instance = _unitMap[unitsInOrder[i].InterfaceTypes[0]].Instance;
                IInitializable initializable = instance as IInitializable;
                // TODO: Performance of this lookup, since we have the unit definition, couldn't we
                // just check if it's a weak system or not?
                if (initializable != null && _ownedSystems.Contains(instance)) {
                    initializable.Init(this);
                }
            }
            
            return result;
        }

        /// <summary>
        /// Creates a dependency graph from all registered units in the context.
        /// </summary>
        public GraphGenResult<UnitDefinition> GetDependencyGraph() {
            var dependencies = new Dictionary<UnitDefinition, DependencyInfo<UnitDefinition>>();
            GenerateUnitInfo();
            MakeDependencyInfo(_units, dependencies);
            return MakeDependencyGraph(dependencies, _rootDefinition);
        }

        public T Get<T>() {
            UnitInfo unit;
            if (!_unitMap.TryGetValue(typeof(T), out unit)) {
                return default(T);
            }
            if (unit.Definition.Mode == BindingMode.Factory) {
                object instance = unit.Definition.Factory();
                IInitializable initializable = instance as IInitializable;
                if (initializable != null) {
                    initializable.Init(this);
                }
                return (T)instance;
            }
            return (T)unit.Instance;
        }

        public void Shutdown() {
            for (int i = 0, ilen = _ownedSystems.Count; i < ilen; ++i) {
                object system = _ownedSystems[i];
                IDisposable disposable = system as IDisposable;
                if (disposable != null) {
                    disposable.Dispose();
                }
            }
        }

        public void RegisterToContextAsWeak(Context context) {
            foreach (var pair in _unitMap) {
                UnitInfo unit = pair.Value;
                UnitDefinition oldDef = unit.Definition;
                BindingMode newMode = oldDef.Mode;
                Func<object> newFactory = oldDef.Factory;

                if (oldDef.Mode == BindingMode.System) {
                    newMode = BindingMode.WeakSystem;
                    object instance = unit.Instance;
                    newFactory = () => instance;
                }

                context.AddDefinition(
                    new UnitDefinition(
                        oldDef.InterfaceTypes,
                        oldDef.ConcreteType,
                        newFactory,
                        newMode
                    )
                );
            }
        }

        private void GenerateUnitInfo() {
            _units.Clear();
            _unitMap.Clear();
            _ownedSystems.Clear();

            foreach(UnitDefinition def in _context.Definitions) {
                var unit = new UnitInfo();
                unit.Definition = def;

                if (def.Mode != BindingMode.Factory) {
                    unit.Instance = def.Factory();
                    if (def.Mode != BindingMode.WeakSystem) {
                        _ownedSystems.Add(unit.Instance);
                    }
                }

                _units.Add(unit);

                Type[] interfaceTypes = def.InterfaceTypes;
                for (int i = 0, ilen = interfaceTypes.Length; i < ilen; ++i) {
                    Type interfaceType = interfaceTypes[i];
                    _unitMap[interfaceType] = unit;
                }
            }
        }

        private void MakeDependencyInfo(
            IEnumerable<UnitInfo> units,
            Dictionary<UnitDefinition, DependencyInfo<UnitDefinition>> dependencyInfo
        ) {
            foreach (UnitInfo unit in units) {
                Type concreteType = unit.Instance == null ? 
                    unit.Definition.ConcreteType : unit.Instance.GetType();
                Type[] dependencies = _dependencyProvider.Get(concreteType);
                UnitDefinition[] dependencyUnits;
                if (dependencies == null || dependencies.Length < 1) {
                    dependencyUnits = _rootDependencies;
                }
                else {
                    var unitDefs = new List<UnitDefinition>();
                    for (int i = 0, ilen = dependencies.Length; i < ilen; ++i) {
                        UnitInfo dependencyUnitInfo;
                        UnitDefinition dependencyUnit;
                        if (_unitMap.TryGetValue(dependencies[i], out dependencyUnitInfo)) {
                            dependencyUnit = dependencyUnitInfo.Definition;
                        }
                        else {
                            dependencyUnit = new UnitDefinition(null, null, null, BindingMode.System);
                        }
                        if (!unitDefs.Contains(dependencyUnit)) {
                            unitDefs.Add(dependencyUnit);
                        }
                    }
                    dependencyUnits = unitDefs.ToArray();
                }

                var info = new DependencyInfo<UnitDefinition>();
                info.Dependencies = dependencyUnits;
                dependencyInfo[unit.Definition] = info;
            }
        }

        private GraphGenResult<T> MakeDependencyGraph<T>(
            Dictionary<T, DependencyInfo<T>> units,
            T rootData
        ) {
            var result = new GraphGenResult<T>();
            var nodes = new Dictionary<T, GraphNode<T>>();
            var root = new GraphNode<T>();
            var missingDependencies = new List<T>();

            nodes[rootData] = root;
            result.Root = root;
            result.Graph = nodes;
            result.MissingDependencies = missingDependencies;

            foreach (var pair in units) {
                var node = new GraphNode<T>();
                node.Data = pair.Key;
                nodes[pair.Key] = node;
            }

            foreach (var pair in units) {
                GraphNode<T> dependentNode = nodes[pair.Key];
                foreach (T dependency in pair.Value.Dependencies) {
                    GraphNode<T> dependencyNode;
                    if (nodes.TryGetValue(dependency, out dependencyNode)) {
                        dependencyNode.Dependents.Add(dependentNode);
                    }
                    else {
                        missingDependencies.Add(dependency);
                    }
                }
            }

            return result;
        }

        private void DetectCircularDependencies<T>(
            Dictionary<T, GraphNode<T>> graph,
            List<CircularDependency<T>> circularDependencies
        ) {
            var stack = new Stack<GraphNode<T>>();
            var visitedNodes = new List<GraphNode<T>>();

            foreach (var pair in graph) {
                GraphNode<T> searchNode = pair.Value;
                stack.Push(searchNode);
                visitedNodes.Clear();

                while (stack.Count > 0) {
                    GraphNode<T> node = stack.Pop();

                    var unvisitedNodes = new List<GraphNode<T>>();

                    foreach (GraphNode<T> child in node.Dependents) {
                        if (child == searchNode) {
                            var circular = new CircularDependency<T>();
                            var chain = new List<T>();
                            while(stack.Count > 0) {
                                chain.Add(stack.Pop().Data);
                            }
                            chain.Reverse();
                            circular.Chain = chain;
                            circularDependencies.Add(circular);
                            break;
                        }
                        else if (!stack.Contains(child) && !visitedNodes.Contains(child)) {
                            unvisitedNodes.Add(child);
                        }
                    }

                    if (unvisitedNodes.Count > 0) {
                        stack.Push(node);
                        foreach (GraphNode<T> child in unvisitedNodes) {
                            stack.Push(child);
                        }
                    }
                    else  if (!visitedNodes.Contains(node)) {
                        visitedNodes.Add(node);
                    }
                }
            }

        }

        private void ResolveDependencyGraph<T>(
            GraphNode<T> root,
            List<T> unitsInOrder
        ) {
            var units = new List<T>();

            Stack<GraphNode<T>> nodeStack = new Stack<GraphNode<T>>();
            nodeStack.Push(root);

            while (nodeStack.Count > 0)
            {
                GraphNode<T> currentNode = nodeStack.Pop();

                List<GraphNode<T>> unresolvedChildren = new List<GraphNode<T>>();

                foreach (GraphNode<T> child in currentNode.Dependents) {
                    if (!units.Contains(child.Data)) {
                        unresolvedChildren.Add(child);
                    }
                }

                if (unresolvedChildren.Count > 0) {
                    nodeStack.Push(currentNode);
                    foreach (GraphNode<T> child in unresolvedChildren) {
                        nodeStack.Push(child);
                    }
                }
                else if (!units.Contains(currentNode.Data)) {
                    units.Add(currentNode.Data);
                }
            }

            // Remove the root graph node
            units.RemoveAt(units.Count - 1);

            units.Reverse();
            unitsInOrder.AddRange(units);
        }

        private readonly IDependencyProvider _dependencyProvider;
        private Context _context;

        private readonly Dictionary<Type, UnitInfo> _unitMap = new Dictionary<Type, UnitInfo>();
        private readonly List<UnitInfo> _units = new List<UnitInfo>();
        private readonly List<object> _ownedSystems = new List<object>();
        private readonly UnitDefinition[] _rootDependencies;
        private readonly UnitDefinition _rootDefinition = new UnitDefinition(
            new Type[] { typeof(IRootUnit) }, null, null, BindingMode.System
        );
    }

}
