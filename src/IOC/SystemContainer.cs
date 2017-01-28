
using System;
using System.Collections.Generic;

namespace SFuller.SharpGameLibs.IOC
{

    public class GraphNode
    {
        public GraphNode(Type interfaceType, ISystem system)
        {
            Type = interfaceType;
            System = system;
        }

        public GraphNode FindChild(Type type)
        {
            if(Type == type)
            {
                return this;
            }
            foreach(var child in Children)
            {
                var foundChild = child.FindChild(type);
                if(foundChild != null)
                {
                    return foundChild;
                }
            }
            return null;
        }

        public readonly Type Type;
        public readonly ISystem System;
        public readonly List<GraphNode> Children = new List<GraphNode>();
    }

    class SystemInfo
    {
        public SystemInfo(Type type, ISystem system)
        {
            Type = type;
            Dependencies = system.GetDependencies();
            System = system;
        }
        public readonly Type Type;
        public readonly Type[] Dependencies;
        public readonly ISystem System;
    }

    public class SystemContainer
    {
        public void SetContext(SystemContext context)
        {
            m_Context = context;
        }

        public bool Init()
        {
            m_Systems.Clear();
            
            // Create the systems
            var it = m_Context.Definitions.GetEnumerator();
            while(it.MoveNext())
            {
                var current = it.Current;
                var system = current.Creator();
                m_Systems.Add(current.Type, system);
                if (!current.IsWeak) {
                    m_OwnedSystems.Add(system);
                }
            }

            // Resolve dependencies
            GraphNode node;
            bool circular = MakeDependencyGraph(m_Systems, out node);
            if(circular)
            {
                return false;
            }

            // Create systems
            var systemsInResolveOrder = new List<ISystem>();
            GetSystemsInDependencyOrder(node, systemsInResolveOrder);
            for(int i = 0, ilen = systemsInResolveOrder.Count; i < ilen; ++i)
            {
                var system = systemsInResolveOrder[i];
                
                // TODO: Performance of this lookup
                if (m_OwnedSystems.Contains(system)) {
                    system.Init(this);
                }
            }
            return true;
        }

        public T Get<T>() where T : ISystem
        {
            ISystem system;
            m_Systems.TryGetValue(typeof(T), out system);
            return (T)system;
        }

        public void Shutdown() {
            for (int i = 0, ilen = m_OwnedSystems.Count; i < ilen; ++i) {
                ISystem system = m_OwnedSystems[i];
                system.Shutdown();
            }
        }

        public IEnumerable<KeyValuePair<Type, ISystem>> Systems
        {
            get {
                return m_Systems;
            }
        }

        public static bool MakeDependencyGraph(Dictionary<Type, ISystem> systems, out GraphNode graph)
        {
            var systemsToResolve = new List<SystemInfo>();
            var nodes = new Dictionary<Type, GraphNode>();
            GraphNode root = new GraphNode(null, null);
            bool circularDependencyDetected = false;

            foreach(var pair in systems)
            {
                systemsToResolve.Add(new SystemInfo(pair.Key, pair.Value));
            }

            var resolvedSystems = new List<SystemInfo>();
            while(systemsToResolve.Count > 0)
            {
                resolvedSystems.Clear(); 
                for(int i = 0, ilen = systemsToResolve.Count; i < ilen; ++i)
                {
                    var info = systemsToResolve[i];
                    if(info.Dependencies == null || info.Dependencies.Length < 1)
                    {
                        var node = new GraphNode(info.Type, info.System);
                        nodes.Add(info.Type, node);
                        root.Children.Add(node);
                        resolvedSystems.Add(info);
                        continue;
                    }
                    bool missingDependencies = false;
                    foreach(Type dependency in info.Dependencies)
                    {
                        var dependencyNode = root.FindChild(dependency);
                        if(dependencyNode == null)
                        {
                            missingDependencies = true;
                            break;
                        }
                        else
                        {
                            GraphNode nodeForSystemBeingResolved;
                            if(!nodes.TryGetValue(info.Type, out nodeForSystemBeingResolved))
                            {
                                nodeForSystemBeingResolved = new GraphNode(info.Type, info.System);
                                nodes.Add(info.Type, nodeForSystemBeingResolved);
                            }
                            dependencyNode.Children.Add(nodeForSystemBeingResolved);
                        }
                    }
                    if(!missingDependencies)
                    {
                        resolvedSystems.Add(info);
                    }
                }
                if(resolvedSystems.Count < 1)
                {
                    // There's a circular dependency somewhere. Oops.
                    circularDependencyDetected = true;
                    break;
                }
	            foreach(var info in resolvedSystems)
                {
                    systemsToResolve.Remove(info);
                }
            }
            graph = root;
            return circularDependencyDetected;
        }

        public static void GetSystemsInDependencyOrder(GraphNode node, List<ISystem> systems)
        {
            var children = node.Children;
            for(int i = 0, ilen = children.Count; i < ilen; ++i)
            {
                var child = children[i];
                if(!systems.Contains(child.System))
                {
                    systems.Add(child.System);
                }
            }
            for(int i = 0, ilen = children.Count; i < ilen; ++i)
            {
                GetSystemsInDependencyOrder(children[i], systems);
            }
        }

        private SystemContext m_Context;
        private readonly Dictionary<Type, ISystem> m_Systems = new Dictionary<Type, ISystem>();
        private List<ISystem> m_OwnedSystems = new List<ISystem>();
    }

}
