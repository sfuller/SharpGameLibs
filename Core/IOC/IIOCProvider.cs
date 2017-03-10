using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFuller.SharpGameLibs.Core.IOC
{
    public interface IIOCProvider
    {
        T Get<T>();
    }
}
