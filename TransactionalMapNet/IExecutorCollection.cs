using System.Transactions;

namespace Igorg
{
    internal interface IExecutorCollection
    {
        ITransactionContext Get(Transaction transaction);
    }
}