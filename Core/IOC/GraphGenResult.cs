using System;
using System.Collections.Generic;

namespace SFuller.SharpGameLibs.Core.IOC
{
    public struct GraphGenResult<T> {
        public Dictionary<T, GraphNode<T>> Graph;
        public GraphNode<T> Root;
        public List<T> MissingDependencies;
    }
}
