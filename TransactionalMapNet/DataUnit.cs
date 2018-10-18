namespace Igorg
{
    public class DataUnit : IDataUnit
    {
        public long Key { get; set; }
        public string Value { get; set; }

        public DataUnit(long key)
        {
            this.Key = key;
        }
    }
}