using Electrum.Core.Store;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Electrum.Store.InMemory
{
    public class InMemoryObjectRepository<T> : IElectrumObjectRepository<T> where T : class
    {

        static Dictionary<string, List<T>> stores = new Dictionary<string, List<T>>();
        List<T>? _list;

        public Type ElementType => List.AsQueryable().ElementType;

        public Expression Expression => List.AsQueryable().Expression;

        public IQueryProvider Provider => List.AsQueryable().Provider;

        List<T> List
        {
            get
            {
                if(_list == null)
                {
                    var storeName = nameof(T);
                    if(stores.ContainsKey(storeName))
                    {
                        _list = stores[storeName];
                    } else
                    {
                        _list = new List<T>();
                        stores.Add(storeName, _list);
                    }
                }
                return _list;
            }
        }

        public T Add(T entity)
        {
            List.Add(entity);
            return entity;
        }

        public void Remove(T entity)
        {
            List.Remove(entity);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
