using System;

namespace SFuller.SharpGameLibs.Core.ViewManagement
{
    [AttributeUsage(System.AttributeTargets.Enum, Inherited = false, AllowMultiple = true)]
    public sealed class ViewTagsAttribute : Attribute { 

        public ViewTagsAttribute(Type viewType) {
            ViewType = viewType;
        }

        public readonly Type ViewType;  
    }

}
