using SFuller.SharpGameLibs.Core.ViewManagement;
using System;
using System.Collections.Generic;

namespace SFuller.SharpGameLibs.Core.ViewManagement
{
    public struct AssetDescriptor
    {
        public Type Type;
        public uint Tag;
    }

    public class ResourceGroup
    {
        public void Add<T>() where T : IView {
            Add<T>(0);
        }

        public void Add<T>(uint tag) where T : IView {
            Assets.Add(new AssetDescriptor()
            {
                Type = typeof(T),
                Tag = tag
            });
        }

        public void Add<T>(IEnumerable<uint> tags) where T : IView {
            Type type = typeof(T);
            foreach (uint tag in tags) {
                Assets.Add(new AssetDescriptor()
                {
                    Type = type,
                    Tag = tag
                });
            }
        }

        public readonly List<AssetDescriptor> Assets = new List<AssetDescriptor>();
    }
}
