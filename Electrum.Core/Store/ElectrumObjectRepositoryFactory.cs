using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Store
{
    public class ElectrumObjectRepositoryFactory
    {

        public ElectrumObjectRepositoryFactory(IElectrumObjectRepositoryProvider provider)
        {
            Provider = provider;
        }

        internal IElectrumObjectRepositoryProvider Provider { get; set; }

        public IElectrumObjectRepository<T> GetRepo<T>() where T : class
        {
            return Provider.GetRepo<T>();
        }

    }
}
