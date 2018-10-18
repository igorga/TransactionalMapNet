namespace Igorg
{
    internal interface ITransactionExecutor
    {
        ITransactionContext Context { get; }
    }
}