using Igorg;
using NUnit.Framework;
using System;

namespace TransactionalMapTest
{
    [TestFixture]
    class TransactionMapTest
    {
        private TransactionalMap map;

        [SetUp]
        public void SetUp()
        {
            map = new TransactionalMap();
        }

        [Test]
        public void Add()
        {
            var data = new DataUnit(1);
            map.Add(data);
            Assert.IsTrue(map.Contains(1)); 
        }

        [Test]
        public void AddExisting_Exception()
        {
            var data = new DataUnit(1);
            map.Add(data);
            Assert.Throws<ArgumentException>(() => map.Add(data));
        }

        [Test]
        public void Remove()
        {
            var data = new DataUnit(1);
            map.Add(data);
            map.Remove(1);
            Assert.IsFalse(map.Contains(data.Key));
        }

        [Test]
        public void RemoveNonExisting_Exception()
        {
            Assert.Throws<ArgumentException>(() => map.Remove(1));
        }
    }
}
