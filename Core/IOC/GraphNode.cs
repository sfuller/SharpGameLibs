using System;
using System.Collections.Generic;

namespace SFuller.SharpGameLibs.Core.IOC
{
    public class GraphNode {
        public Type UnitType;
        public readonly List<GraphNode> Dependents = new List<GraphNode>(2);
    }
}