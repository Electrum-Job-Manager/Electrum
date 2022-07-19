using Electrum.Core.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Store.InMemory
{
    public class InMemoryObjectRepositoryProvider : IElectrumObjectRepositoryProvider

    {
        public IElectrumObjectRepository<T> GetRepo<T>() where T : class
        {
            return new InMemoryObjectRepository<T>();
        }
    }
}
