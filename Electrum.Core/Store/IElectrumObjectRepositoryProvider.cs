using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Store
{
    public interface IElectrumObjectRepositoryProvider
    {

        public IElectrumObjectRepository<T> GetRepo<T>() where T : class;

    }
}
