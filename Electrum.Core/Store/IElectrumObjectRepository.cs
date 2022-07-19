using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Store
{
    public interface IElectrumObjectRepository<T> : IEnumerable<T>, IQueryable<T> where T : class
    {
        public T Add(T entity);
        public void Remove(T entity);
    }
}
