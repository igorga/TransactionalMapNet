using System.Transactions;

namespace Igorg
{
    internal class TransactionExecutor : IEnlistmentNotification, ITransactionExecutor
    {
        private readonly ITransactionContext context;

        public ITransactionContext Context => context;

        public void Commit(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            try
            {

            }
            catch
            {
                preparingEnlistment.ForceRollback();
            }
            finally
            {
                preparingEnlistment.Done();
            }
        }

        public void Rollback(Enlistment enlistment)
        {
            enlistment.Done();
        }
    }
}