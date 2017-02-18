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

    public class SystemContainer
    {
        private interface IRootUnit { };

        private class UnitInfo {
            public ISystem System;
            public Type ConcreteType;
            public BindingMode Mode;
        }

        private struct GraphGenResult {
            public Dictionary<Type, GraphNode> Graph;
            public GraphNode Root;
            public List<Type> MissingDependencies;
        }

        private class GraphNode {
            public Type UnitType;
            public readonly List<GraphNode> Dependents = new List<GraphNode>(2);
        }

        private struct DependencyInfo {
            public Type[] Dependencies;
        }

        public void SetContext(SystemContext context) {
            m_Context = context;
        }

        public ContainerInitResult Init() {
            var result = new ContainerInitResult();

            _units.Clear();

            // Initialize the units
            IEnumerator<UnitDefinition> it = m_Context.Definitions.GetEnumerator();
            while (it.MoveNext()) {
                var current = it.Current;

                var unit = new UnitInfo();
                unit.Mode = current.Mode;
                unit.ConcreteType = current.ConcreteType;

                if (current.Mode != BindingMode.Factory) {
                    unit.System = current.Creator();
                    if (current.Mode != BindingMode.WeakSystem) {
                        _ownedSystems.Add(unit.System);
                    }
                }

                _units.Add(current.Type, unit);
            }

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
                ISystem system = _units[unitsInOrder[i]].System;
                
                // TODO: Performance of this lookup
                if (_ownedSystems.Contains(system)) {
                    system.Init(this);
                }
            }
            
            return result;
        }

        public T Get<T>() where T : class, IInitializable {
            UnitInfo unit;
            if (!_units.TryGetValue(typeof(T), out unit)) {
                return null;
            }
            if (unit.Mode == BindingMode.Factory) {
                T instance = (T)Activator.CreateInstance(unit.ConcreteType);
                instance.Init(this);
                return instance;
            }
            return (T)unit.System;
        }

        public void Shutdown() {
            for (int i = 0, ilen = _ownedSystems.Count; i < ilen; ++i) {
                ISystem system = _ownedSystems[i];
                system.Shutdown();
            }
        }

        public void RegisterToContextAsWeak(SystemContext context) {
            foreach (var pair in _units) {
                UnitInfo unit = pair.Value;
                if (unit.Mode == BindingMode.Factory) {
                    context.AddDefinition(
                        new UnitDefinition(pair.Key, unit.ConcreteType)
                    );
                }
                else {
                    context.AddDefinition(
                        new UnitDefinition(
                            pair.Key,
                            () => unit.System,
                            BindingMode.WeakSystem
                        )
                    );
                }
            }
        }

        private void MakeDependencyInfo(
            Dictionary<Type, UnitInfo> units,
            Dictionary<Type, DependencyInfo> dependencyInfo
        ) {
            foreach (var pair in units) {
                Type[] dependencies = null;

                UnitInfo unit = pair.Value;
                if (unit.Mode == BindingMode.Factory) {
                    object[] attributes = unit.ConcreteType.GetCustomAttributes(typeof(DependenciesAttribute), true);
                    if (attributes.Length > 0) {
                        DependenciesAttribute attribute = (DependenciesAttribute)attributes[0];
                        dependencies = attribute.Dependencies;
                    }
                }
                else {
                    dependencies = unit.System.GetDependencies();
                }

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

        private SystemContext m_Context;

        private readonly Dictionary<Type, UnitInfo> _units = new Dictionary<Type, UnitInfo>();
        private List<ISystem> _ownedSystems = new List<ISystem>();
        private readonly Type[] _rootDependencies = new Type[] { typeof(IRootUnit) };
    }

}
