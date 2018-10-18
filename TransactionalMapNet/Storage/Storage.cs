using System;
using System.Collections.Generic;

namespace Igorg
{
    internal class Storage : IStorage
    {
        private readonly Dictionary<long, DataUnit> map;

        public Storage()
        {
            map = new Dictionary<long, DataUnit>();
        }

        public void Add(DataUnit data, ITransactionContext context)
        {
            context.Exclusive();
            map.Add(data.Key, data);
        }

        public void Update(DataUnit data, ITransactionContext context)
        {
            context.Exclusive();
            map[data.Key] = data;
        }

        public void Remove(long key, ITransactionContext context)
        {
            context.Exclusive();
            if (!map.Remove(key))
            {
                throw new ArgumentException($"Key {key} not found!");
            }
        }

        public IDataUnit Read(long key, ITransactionContext context)
        {
            context.Shared();
            return map[key];
        }

        public bool Contains(long key, ITransactionContext context)
        {
            context.Shared();
            return map.ContainsKey(key);
        }
    }
}