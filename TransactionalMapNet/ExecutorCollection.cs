using System;
using System.Collections.Generic;
using System.Transactions;

namespace Igorg
{
    internal class ExecutorCollection : IExecutorCollection
    {
        private readonly Dictionary<Guid, TransactionExecutor> executors;

        public ExecutorCollection()
        {
            executors = new Dictionary<Guid, TransactionExecutor>(128);
        }

        public ITransactionContext Get(Transaction transaction)
        {
            lock(executors)
            {
                if (!executors.TryGetValue(transaction.TransactionInformation.DistributedIdentifier, out TransactionExecutor executor))
                {
                    executor = new TransactionExecutor();
                    transaction.EnlistVolatile(executor, EnlistmentOptions.EnlistDuringPrepareRequired);
                    transaction.TransactionCompleted += RemoveExecutor;
                    executors.Add(transaction.TransactionInformation.DistributedIdentifier, executor);
                }
                return executor.Context;
            }
        }

        private void RemoveExecutor(object sender, TransactionEventArgs e)
        {
            lock (executors)
            {
                if(!executors.Remove(e.Transaction.TransactionInformation.DistributedIdentifier))
                {
                    throw new InvalidOperationException("Executor didn't exist. Invalid behavior!");
                }
            }
        }
    }
}