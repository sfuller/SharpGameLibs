using System;
using System.Collections.Generic;

namespace SFuller.SharpGameLibs.Core.IOC
{
    public struct GraphGenResult {
        public Dictionary<Type, GraphNode> Graph;
        public GraphNode Root;
        public List<Type> MissingDependencies;
    }
}
