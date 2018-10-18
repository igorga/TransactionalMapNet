using System.Transactions;

namespace Igorg
{    
    public class TransactionalMap
    {
        private readonly IStorage storage;
        private readonly IExecutorCollection contextCollection;

        public TransactionalMap()
        {
            storage = new Storage();
            contextCollection = new ExecutorCollection();
        }

        public void Add(DataUnit data)
        {
            using(var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                Add(data, Transaction.Current);
            }
        }

        public void Add(DataUnit data, Transaction activeTransaction)
        {
            storage.Add(data, contextCollection.Get(activeTransaction));
        }

        public void Remove(long key)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                Remove(key, Transaction.Current);
            }
        }

        public void Remove(long key, Transaction activeTransaction)
        {
            storage.Remove(key, contextCollection.Get(activeTransaction));
        }

        public IDataUnit Read(long key)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                return Read(key, Transaction.Current);
            }
        }

        public IDataUnit Read(long key, Transaction activeTransaction)
        {
            return storage.Read(key, contextCollection.Get(activeTransaction));
        }

        public bool Contains(long key)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                return Contains(key, Transaction.Current);
            }
        }

        public bool Contains(long key, Transaction activeTransaction)
        {
            return storage.Contains(key, contextCollection.Get(activeTransaction));
        }
    }
}
