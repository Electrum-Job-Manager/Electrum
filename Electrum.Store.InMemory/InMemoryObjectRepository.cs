using Electrum.Core.Store;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Electrum.Store.InMemory
{
    public class InMemoryObjectRepository<T> : IElectrumObjectRepository<T> where T : class
    {

        static Dictionary<string, List<T>> stores = new Dictionary<string, List<T>>();
        static Dictionary<string, PropertyInfo?> storeKeyFields = new Dictionary<string, PropertyInfo?>();
        List<T>? _list;
        PropertyInfo? _storeKeyField;
        bool hasLoadedStoreInfo = false;

        public Type ElementType => List.AsQueryable().ElementType;

        public Expression Expression => List.AsQueryable().Expression;

        public IQueryProvider Provider => List.AsQueryable().Provider;

        List<T> List
        {
            get
            {
                if(_list == null)
                {
                    LoadStoreInfo();
                }
                return _list;
            }
        }

        PropertyInfo? storeKeyField
        {
            get
            {
                if(!hasLoadedStoreInfo)
                {
                    LoadStoreInfo();
                }
                return _storeKeyField;
            }
        }

        private void LoadStoreInfo()
        {
            var type = typeof(T);
            var storeName = type.Name;
            if (stores.ContainsKey(storeName))
            {
                _list = stores[storeName];
                _storeKeyField = storeKeyFields[storeName];
            }
            else
            {
                _list = new List<T>();
                stores.Add(storeName, _list);

                var fields = type.GetProperties();
                var fieldsWithKeyAttribute = fields.Where(x => x.GetCustomAttribute<ElectrumStoreKeyAttribute>() != null);
                _storeKeyField = fieldsWithKeyAttribute.FirstOrDefault();
                storeKeyFields.Add(storeName, _storeKeyField);

            }
            hasLoadedStoreInfo = true;
        }

        private object? GetKey(T obj)
        {
            if (storeKeyField == null) return null;
            return storeKeyField?.GetValue(obj);
        }

        public T Add(T entity)
        {
            var key = GetKey(entity);
            var elementWithSameKey = List.FirstOrDefault(x => GetKey(entity) == key);
            if(elementWithSameKey != null)
                return elementWithSameKey;
            List.Add(entity);
            return entity;
        }

        public void Remove(T entity)
        {
            List.Remove(entity);
        }

        public T Save(T entity)
        {
            var key = GetKey(entity);
            var otherValue = List.FirstOrDefault(x => key == GetKey(x));
            if(otherValue != null)
            {
                Remove(otherValue);
            }
            return Add(entity);
        }

        public T? GetByKey(object keyValue)
        {
            var keyList = List.ToDictionary(x => GetKey(x), x => x);
            return keyList[keyValue];
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
