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
        public IEnumerable<Type> Missing;
        public IEnumerable<CircularDependency> Circular;
    }

    public struct CircularDependency {
        public IEnumerable<Type> Chain;
    }

    public class SystemContainer : IIOCProvider
    {
        private interface IRootUnit { };

        private class UnitInfo {
            public object Instance;
            public UnitDefinition Definition;
        }

        private struct DependencyInfo {
            public Type[] Dependencies;
        }

        public SystemContainer(IDependencyProvider dependencyProvider = null) {
            if (dependencyProvider == null) {
                dependencyProvider = new DependencyProvider();
            }
            _dependencyProvider = dependencyProvider;
        }

        public void SetContext(SystemContext context) {
            _context = context;
        }

        public ContainerInitResult Init() {
            var result = new ContainerInitResult();

            GenerateUnitInfo();

            // Generate the dependency graph
            var dependencies = new Dictionary<Type, DependencyInfo>();
            MakeDependencyInfo(_units, dependencies);
            GraphGenResult graphResult = MakeDependencyGraph(dependencies);
            if (graphResult.MissingDependencies.Count > 0) {
                result.Missing = graphResult.MissingDependencies;
                result.Status = ContainerInitStatus.MissingDependencies;
                return result;
            }

            // Check for circular dependencies
            var unitsWithCircularDependencies = new List<CircularDependency>();
            DetectCircularDependencies(graphResult.Graph, unitsWithCircularDependencies);
            if (unitsWithCircularDependencies.Count > 0) {
                result.Status = ContainerInitStatus.CircularDependency;
                result.Circular = unitsWithCircularDependencies;
                return result;
            }

            // Resolve dependencies
            var unitsInOrder = new List<Type>();
            ResolveDependencyGraph(graphResult.Root, unitsInOrder);

            // Initialize systems
            for (int i = 0, ilen = unitsInOrder.Count; i < ilen; ++i) {
                object instance = _units[unitsInOrder[i]].Instance;
                IInitializable initializable = instance as IInitializable;
                if (initializable != null && _ownedSystems.Contains(instance)) {
                    // TODO: Performance of this lookup
                    initializable.Init(this);
                }
            }
            
            return result;
        }

        /// <summary>
        /// Creates a dependency graph from all registered units in the context.
        /// </summary>
        public GraphGenResult GetDependencyGraph() {
            var dependencies = new Dictionary<Type, DependencyInfo>();
            GenerateUnitInfo();
            MakeDependencyInfo(_units, dependencies);
            return MakeDependencyGraph(dependencies);
        }

        public T Get<T>() {
            UnitInfo unit;
            if (!_units.TryGetValue(typeof(T), out unit)) {
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

        public void RegisterToContextAsWeak(SystemContext context) {
            foreach (var pair in _units) {
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
                        oldDef.InterfaceType,
                        oldDef.ConcreteType,
                        newFactory,
                        newMode
                    )
                );
            }
        }

        private void GenerateUnitInfo() {
            _units.Clear();
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

                _units.Add(def.InterfaceType, unit);
            }
        }

        private void MakeDependencyInfo(
            Dictionary<Type, UnitInfo> units,
            Dictionary<Type, DependencyInfo> dependencyInfo
        ) {
            foreach (var pair in units) {
                UnitInfo unit = pair.Value;

                Type concreteType = unit.Instance == null ? 
                    unit.Definition.ConcreteType : unit.Instance.GetType();
                Type[] dependencies = _dependencyProvider.Get(concreteType);
                if (dependencies == null || dependencies.Length < 1) {
                    dependencies = _rootDependencies;
                }

                var info = new DependencyInfo();
                info.Dependencies = dependencies;
                dependencyInfo[pair.Key] = info;
            }
        }

        private GraphGenResult MakeDependencyGraph(
            Dictionary<Type, DependencyInfo> units  
        ) {
            var result = new GraphGenResult();
            var nodes = new Dictionary<Type, GraphNode>();
            var root = new GraphNode();
            var missingDependencies = new List<Type>();

            nodes[typeof(IRootUnit)] = root;
            result.Root = root;
            result.Graph = nodes;
            result.MissingDependencies = missingDependencies;

            foreach (var pair in units) {
                var node = new GraphNode();
                node.UnitType = pair.Key;
                nodes[pair.Key] = node;
            }

            foreach (var pair in units) {
                GraphNode dependentNode = nodes[pair.Key];
                foreach (Type dependency in pair.Value.Dependencies) {
                    GraphNode dependencyNode;
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

        private void DetectCircularDependencies(
            Dictionary<Type, GraphNode> graph,
            List<CircularDependency> circularDependencies
        ) {
            var stack = new Stack<GraphNode>();
            var visitedNodes = new List<GraphNode>();

            foreach (var pair in graph) {
                GraphNode searchNode = pair.Value;
                stack.Push(searchNode);
                visitedNodes.Clear();

                while (stack.Count > 0) {
                    GraphNode node = stack.Pop();

                    var unvisitedNodes = new List<GraphNode>();

                    foreach (GraphNode child in node.Dependents) {
                        if (child == searchNode) {
                            var circular = new CircularDependency();
                            var chain = new List<Type>();
                            while(stack.Count > 0) {
                                chain.Add(stack.Pop().UnitType);
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
                        foreach (GraphNode child in unvisitedNodes) {
                            stack.Push(child);
                        }
                    }
                    else  if (!visitedNodes.Contains(node)) {
                        visitedNodes.Add(node);
                    }
                }
            }

        }

        private void ResolveDependencyGraph(
            GraphNode root,
            List<Type> systemsInOrder
        ) {
            var units = new List<Type>();

            Stack<GraphNode> nodeStack = new Stack<GraphNode>();
            nodeStack.Push(root);

            while (nodeStack.Count > 0)
            {
                GraphNode currentNode = nodeStack.Pop();

                List<GraphNode> unresolvedChildren = new List<GraphNode>();

                foreach (GraphNode child in currentNode.Dependents) {
                    if (!units.Contains(child.UnitType)) {
                        unresolvedChildren.Add(child);
                    }
                }

                if (unresolvedChildren.Count > 0) {
                    nodeStack.Push(currentNode);
                    foreach (GraphNode child in unresolvedChildren) {
                        nodeStack.Push(child);
                    }
                }
                else if (!units.Contains(currentNode.UnitType)) {
                    units.Add(currentNode.UnitType);
                }
            }

            // Remove the root graph node
            units.RemoveAt(units.Count - 1);

            units.Reverse();
            systemsInOrder.AddRange(units);
        }

        private readonly IDependencyProvider _dependencyProvider;
        private SystemContext _context;

        private readonly Dictionary<Type, UnitInfo> _units = new Dictionary<Type, UnitInfo>();
        private readonly List<object> _ownedSystems = new List<object>();
        private readonly Type[] _rootDependencies = new Type[] { typeof(IRootUnit) };
    }

}
