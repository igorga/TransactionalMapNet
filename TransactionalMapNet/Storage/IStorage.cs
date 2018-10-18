namespace Igorg
{
    internal interface IStorage
    {
        void Add(DataUnit data, ITransactionContext context);
        bool Contains(long key, ITransactionContext context);
        IDataUnit Read(long key, ITransactionContext context);
        void Remove(long key, ITransactionContext context);
        void Update(DataUnit data, ITransactionContext context);
    }
}