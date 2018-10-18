using Igorg;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TransactionalMapTest.Isolation
{
    [TestFixture]
    class GenericLockTest
    {
#if !DEBUG
        private const int WAIT_TIMEOUT = 100;
#else
        private const int WAIT_TIMEOUT = int.MaxValue;
#endif
        private const int WAIT_TIMEOUT_EXPECTED_TIMEOUT = 100;

        private GenericLock<Guid> idLock;

        [SetUp]
        public void SetUp()
        {
            idLock = new GenericLock<Guid>();
        }

        #region Exclusive

        [Test]
        public void EnterExclusive()
        {
            var id = Guid.NewGuid();
            idLock.EnterExclusive(id);
            Assert.IsTrue(idLock.ExclusiveLockTakenBy(id));
        }

        [Test]
        public void EnterExclusive_Reentrant()
        {
            var id = Guid.NewGuid();
            idLock.EnterExclusive(id);
            Assert.Throws<InvalidOperationException>(() => idLock.EnterExclusive(id));
        }

        [Test]
        public void EnterExclusive_NotTaken()
        {
            var id = Guid.NewGuid();
            Assert.IsFalse(idLock.ExclusiveLockTakenBy(id));
        }

        [Test]
        public void ExitExclusive()
        {
            var id = Guid.NewGuid();
            idLock.EnterExclusive(id);
            idLock.ExitExclusive(id);
            Assert.IsFalse(idLock.ExclusiveLockTakenBy(id));
        }

        [Test]
        public void ExitExclusive_NotTaken()
        {
            var id = Guid.NewGuid();
            Assert.Throws<InvalidOperationException>(() => idLock.ExitExclusive(id));
        }

        [Test]
        public void ExitExclusive_WrongLock()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            idLock.EnterExclusive(id1);
            Assert.Throws<InvalidOperationException>(() => idLock.ExitExclusive(id2));
            Assert.IsTrue(idLock.ExclusiveLockTakenBy(id1));
        }

        [Test]
        public void ExclusiveLock_Read_ThreadIndependence()
        {
            SemaphoreSlim endSync = new SemaphoreSlim(0, 1);
            var id = Guid.NewGuid();

            Task.Run(() => { idLock.EnterExclusive(id); endSync.Release(); });

            Assert.IsTrue(endSync.Wait(1000), "Timeout expired");
            Assert.IsTrue(idLock.ExclusiveLockTakenBy(id));
        }

        [Test]
        public void ExclusiveLock_Exit_ThreadIndependence()
        {
            SemaphoreSlim endSync = new SemaphoreSlim(0, 1);
            var id = Guid.NewGuid();

            Task.Run(() => { idLock.EnterExclusive(id); endSync.Release(); });

            Assert.IsTrue(endSync.Wait(1000), "Timeout expired");
            Assert.DoesNotThrow(() => idLock.ExitExclusive(id));
            Assert.IsFalse(idLock.UpgradeableLockTakenBy(id));
        }

        #endregion

        #region Shared

        [Test]
        public void EnterShared()
        {
            var id = Guid.NewGuid();
            idLock.EnterShared(id);
            Assert.IsTrue(idLock.SharedLockTakenBy(id));
        }

        [Test]
        public void EnterShared_Reentrant()
        {
            var id = Guid.NewGuid();
            idLock.EnterShared(id);
            Assert.Throws<InvalidOperationException>(() => idLock.EnterShared(id));
        }

        [Test]
        public void EnterShared_NotTaken()
        {
            var id = Guid.NewGuid();
            Assert.IsFalse(idLock.SharedLockTakenBy(id));
        }

        [Test]
        public void ExitShared()
        {
            var id = Guid.NewGuid();
            idLock.EnterShared(id);
            idLock.ExitShared(id);
            Assert.IsFalse(idLock.SharedLockTakenBy(id));
        }

        [Test]
        public void ExitShared_NotTaken()
        {
            var id = Guid.NewGuid();
            Assert.Throws<InvalidOperationException>(() => idLock.ExitShared(id));
        }

        [Test]
        public void ExitShared_WrongLock()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            idLock.EnterShared(id1);
            Assert.Throws<InvalidOperationException>(() => idLock.ExitShared(id2));
            Assert.IsTrue(idLock.SharedLockTakenBy(id1));
        }

        [Test]
        public void SharedLock_Read_ThreadIndependence()
        {
            SemaphoreSlim endSync = new SemaphoreSlim(0, 1);
            var id = Guid.NewGuid();

            Task.Run(() => { idLock.EnterShared(id); endSync.Release(); });

            Assert.IsTrue(endSync.Wait(1000), "Timeout expired");
            Assert.IsTrue(idLock.SharedLockTakenBy(id));
        }

        [Test]
        public void SharedLock_Exit_ThreadIndependence()
        {
            SemaphoreSlim endSync = new SemaphoreSlim(0, 1);
            var id = Guid.NewGuid();

            Task.Run(() => { idLock.EnterShared(id); endSync.Release(); });

            Assert.IsTrue(endSync.Wait(1000), "Timeout expired");
            Assert.DoesNotThrow(() => idLock.ExitShared(id));
            Assert.IsFalse(idLock.UpgradeableLockTakenBy(id));
        }

        #endregion

        #region Upgradeable

        [Test]
        public void EnterUpgradeable()
        {
            var id = Guid.NewGuid();
            idLock.EnterUpgradeable(id);
            Assert.IsTrue(idLock.UpgradeableLockTakenBy(id));
        }

        [Test]
        public void EnterUpgradeable_Reentrant()
        {
            var id = Guid.NewGuid();
            idLock.EnterUpgradeable(id);
            Assert.Throws<InvalidOperationException>(() => idLock.EnterUpgradeable(id));
        }

        [Test]
        public void EnterUpgradeable_NotTaken()
        {
            var id = Guid.NewGuid();
            Assert.IsFalse(idLock.UpgradeableLockTakenBy(id));
        }

        [Test]
        public void EnterUpgradeable_SharedNotTaken()
        {
            var id = Guid.NewGuid();
            idLock.EnterUpgradeable(id);
            Assert.IsFalse(idLock.SharedLockTakenBy(id));
        }

        [Test]
        public void ExitUpgradeable()
        {
            var id = Guid.NewGuid();
            idLock.EnterUpgradeable(id);
            idLock.ExitUpgradeable(id);
            Assert.IsFalse(idLock.UpgradeableLockTakenBy(id));
        }

        [Test]
        public void ExitUpgradeable_NotTaken()
        {
            var id = Guid.NewGuid();
            Assert.Throws<InvalidOperationException>(() => idLock.ExitUpgradeable(id));
        }

        [Test]
        public void ExitUpgradeable_WrongLock()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            idLock.EnterUpgradeable(id1);
            Assert.Throws<InvalidOperationException>(() => idLock.ExitUpgradeable(id2));
            Assert.IsTrue(idLock.UpgradeableLockTakenBy(id1));
        }

        [Test]
        public void UpgradeableLock_Read_ThreadIndependence()
        {
            SemaphoreSlim endSync = new SemaphoreSlim(0, 1);
            var id = Guid.NewGuid();

            Task.Run(() => { idLock.EnterUpgradeable(id); endSync.Release(); });

            Assert.IsTrue(endSync.Wait(1000), "Timeout expired");
            Assert.IsTrue(idLock.UpgradeableLockTakenBy(id));
        }

        [Test]
        public void UpgradeableLock_Exit_ThreadIndependence()
        {
            SemaphoreSlim endSync = new SemaphoreSlim(0, 1);
            var id = Guid.NewGuid();

            Task.Run(() => { idLock.EnterUpgradeable(id); endSync.Release(); });

            Assert.IsTrue(endSync.Wait(1000), "Timeout expired");
            Assert.DoesNotThrow(() => idLock.ExitUpgradeable(id));
            Assert.IsFalse(idLock.UpgradeableLockTakenBy(id));
        }

        #endregion

        #region Enter different lock types with the same id

        [Test]
        public void Shared_Exclusive_NotAllowed()
        {
            Guid id = Guid.NewGuid();
            idLock.EnterShared(id);
            Assert.Throws<InvalidOperationException>(() => idLock.EnterExclusive(id));
        }

        [Test]
        public void Shared_Upgradeable_NotAllowed()
        {
            Guid id = Guid.NewGuid();
            idLock.EnterShared(id);
            Assert.Throws<InvalidOperationException>(() => idLock.EnterUpgradeable(id));
        }

        [Test]
        public void Exclusive_Shared_NotAllowed()
        {
            Guid id = Guid.NewGuid();
            idLock.EnterExclusive(id);
            Assert.Throws<InvalidOperationException>(() => idLock.EnterShared(id));
        }

        [Test]
        public void Exclusive_Upgradeable_NotAllowed()
        {
            Guid id = Guid.NewGuid();
            idLock.EnterExclusive(id);
            Assert.Throws<InvalidOperationException>(() => idLock.EnterUpgradeable(id));
        }

        [Test]
        public void Upgradeable_Shared_NotAllowed()
        {
            Guid id = Guid.NewGuid();
            idLock.EnterUpgradeable(id);
            Assert.Throws<InvalidOperationException>(() => idLock.EnterShared(id));
        }

        [Test, Timeout(WAIT_TIMEOUT)] // if we do not pass the test within the timeout period, something has deadlocked
        public void Upgradeable_Exclusive_Allowed()
        {
            Guid id = Guid.NewGuid();
            idLock.EnterUpgradeable(id);
            idLock.EnterExclusive(id);
            Assert.Pass();
        }

        #endregion

        #region Exit different lock types with the same id

        [Test]
        public void Upgradeable_Exclusive_Exit_Exclusive()
        {
            var id = Guid.NewGuid();
            idLock.EnterUpgradeable(id);
            idLock.EnterExclusive(id);

            idLock.ExitExclusive(id);

            Assert.IsFalse(idLock.ExclusiveLockTakenBy(id));
        }

        [Test]
        public void Upgradeable_Exclusive_Exit_Upgradeable()
        {
            var id = Guid.NewGuid();
            idLock.EnterUpgradeable(id);
            idLock.EnterExclusive(id);

            idLock.ExitUpgradeable(id);

            Assert.IsFalse(idLock.UpgradeableLockTakenBy(id));
        }

        #endregion

        #region Same locks different ids

        [Test, Timeout(WAIT_TIMEOUT)]
        public void Shared_Shared_Ok()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();

            idLock.EnterShared(id1);
            idLock.EnterShared(id2);

            Assert.Pass();
        }

        [Test]
        public void Upgradeable_Upgradeable_Timeout()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            AutoResetEvent ev = new AutoResetEvent(false);

            idLock.EnterUpgradeable(id1);

            Task.Run(() =>
            {
                idLock.EnterUpgradeable(id2);
                ev.Set();
            });

            if (ev.WaitOne(WAIT_TIMEOUT_EXPECTED_TIMEOUT))
            {
                Assert.Fail("Received signal. The lock was taken prematurely.");
            }

            idLock.ExitUpgradeable(id1);

            if (!ev.WaitOne(WAIT_TIMEOUT))
            {
                Assert.Fail("Never received signal. The lock was never released.");
            }

            Assert.Pass();
        }

        [Test]
        public void Exclusive_Exclusive_Timeout()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            AutoResetEvent ev = new AutoResetEvent(false);

            idLock.EnterExclusive(id1);

            Task.Run(() =>
            {
                idLock.EnterExclusive(id2);
                ev.Set();
            });

            if (ev.WaitOne(WAIT_TIMEOUT_EXPECTED_TIMEOUT))
            {
                Assert.Fail("Received signal. The lock was taken prematurely.");
            }

            idLock.ExitExclusive(id1);

            if (!ev.WaitOne(WAIT_TIMEOUT))
            {
                Assert.Fail("Never received signal. The lock was never released.");
            }

            Assert.Pass();
        }

        #endregion
    }
}
