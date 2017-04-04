using System;
using System.Collections.Generic;

namespace SFuller.SharpGameLibs.Core.IOC
{
    public class GraphNode<T> {
        public T Data;
        public readonly List<GraphNode<T>> Dependents = new List<GraphNode<T>>(2);
    }
}