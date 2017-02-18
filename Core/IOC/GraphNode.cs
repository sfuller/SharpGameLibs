//using System;
//using System.Collections.Generic;

//namespace SFuller.SharpGameLibs.Core.IOC
//{
//    class GraphNode
//    {
//        public GraphNode(Type interfaceType, UnitInfo unit)
//        {
//            Type = interfaceType;
//            Unit = unit;
//        }

//        public GraphNode FindChild(Type type)
//        {
//            if(Type == type)
//            {
//                return this;
//            }
//            foreach(var child in Children)
//            {
//                var foundChild = child.FindChild(type);
//                if(foundChild != null)
//                {
//                    return foundChild;
//                }
//            }
//            return null;
//        }

//        public readonly Type Type;
//        public readonly UnitInfo Unit;
//        public readonly List<GraphNode> Children = new List<GraphNode>();
//    }
//}