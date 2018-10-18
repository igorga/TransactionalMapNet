namespace Igorg
{
    internal interface ITransactionContext
    {
        void Shared();
        void Upgradeable();
        void Exclusive();
    }
}