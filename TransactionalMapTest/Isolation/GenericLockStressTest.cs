using Igorg;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TransactionalMapTest
{
    [TestFixture]
    class GenericLockStressTest
    {
        const int iterations = 1000000;
        ManualResetEventSlim startEvent;
        GenericLock<Guid> idLock;
        CancellationTokenSource errorToken;

        const int exclusiveCnt = 5;
        const int sharedCnt = 5;
        const int upgradeableCnt = 5;

        int protectedValue;
        Exception error;

        [SetUp]
        public void SetUp()
        {
            idLock = new GenericLock<Guid>();
            startEvent = new ManualResetEventSlim(false);
            protectedValue = 0;
            errorToken = new CancellationTokenSource();
            error = null;
        }

        [Test]
        public void StressTest_Shared()
        {
            Task[] tasks = new Task[sharedCnt];

            for (int i = 0; i < sharedCnt; i++)
            {
                tasks[i] = Task.Run((Action)SharedWorker);
            }

            startEvent.Set();

            try
            {
                Task.WaitAll(tasks, errorToken.Token);
            }
            catch (OperationCanceledException)
            {
                Assert.Fail($"The test has failed. E: {error.Message}");
            }
        }

        [Test]
        public void StressTest_Exclusive()
        {
            Task[] tasks = new Task[exclusiveCnt];

            for (int i = 0; i < exclusiveCnt; i++)
            {
                tasks[i] = Task.Run((Action)ExclusiveWorker);
            }

            startEvent.Set();

            try
            {
                Task.WaitAll(tasks, errorToken.Token);
            }
            catch (OperationCanceledException)
            {
                Assert.Fail($"The test has failed. E: {error.Message}");
            }

            Assert.AreEqual(iterations * exclusiveCnt, protectedValue);
        }

        [Test]
        public void StressTest_Upgradeable()
        {
            Task[] tasks = new Task[upgradeableCnt];

            for (int i = 0; i < upgradeableCnt; i++)
            {
                tasks[i] = Task.Run((Action)UpgradeableWorker);
            }

            startEvent.Set();

            try
            {
                Task.WaitAll(tasks, errorToken.Token);
            }
            catch (OperationCanceledException)
            {
                Assert.Fail($"The test has failed. E: {error.Message}");
            }

            Assert.AreEqual(iterations * upgradeableCnt, protectedValue);
        }

        [Test]
        public void StressTest_Mixed()
        {
            Task[] tasks = new Task[sharedCnt + upgradeableCnt + exclusiveCnt];

            for (int i = 0; i < sharedCnt; i++)
            {
                tasks[i] = Task.Run((Action)SharedWorker);
            }

            for (int i = sharedCnt; i < sharedCnt + upgradeableCnt; i++)
            {
                tasks[i] = Task.Run((Action)UpgradeableWorker);
            }

            for (int i = sharedCnt + upgradeableCnt; i < sharedCnt + upgradeableCnt + exclusiveCnt; i++)
            {
                tasks[i] = Task.Run((Action)ExclusiveWorker);
            }

            foreach (var item in tasks)
            {
                Assert.NotNull(item);
            }

            startEvent.Set();

            try
            {
                Task.WaitAll(tasks, errorToken.Token);
            }
            catch (OperationCanceledException)
            {
                Assert.Fail($"The test has failed. E: {error.Message}");
            }

            Assert.AreEqual(iterations * (exclusiveCnt + upgradeableCnt), protectedValue);
        }

        private void SharedWorker()
        {
            try
            {
                startEvent.Wait();
                for (int j = 0; j < iterations; j++)
                {
                    if (errorToken.Token.IsCancellationRequested) return;
                    Guid id = Guid.NewGuid();
                    idLock.EnterShared(id);
                    SetProtectedValue();
                    idLock.ExitShared(id);
                }
            }
            catch
            {
                errorToken.Cancel();
            }
        }

        private void ExclusiveWorker()
        {
            try
            {
                startEvent.Wait();
                for (int j = 0; j < iterations; j++)
                {
                    if (errorToken.Token.IsCancellationRequested) return;
                    Guid id = Guid.NewGuid();
                    idLock.EnterExclusive(id);
                    IncrementProtectedValue();
                    idLock.ExitExclusive(id);
                }
            }
            catch(Exception e)
            {
                error = e;
                errorToken.Cancel();
            }
        }

        private void UpgradeableWorker()
        {
            try
            {
                startEvent.Wait();
                for (int j = 0; j < iterations; j++)
                {
                    if (errorToken.Token.IsCancellationRequested) return;
                    Guid id = Guid.NewGuid();
                    idLock.EnterUpgradeable(id);
                    SetProtectedValue();
                    idLock.EnterExclusive(id);
                    IncrementProtectedValue();
                    idLock.ExitExclusive(id);
                    SetProtectedValue();
                    idLock.ExitUpgradeable(id);
                }
            }
            catch (Exception e)
            {
                error = e;
                errorToken.Cancel();
            }
        }


        private void IncrementProtectedValue()
        {
            protectedValue++;
        }

        private void SetProtectedValue()
        {
#pragma warning disable CS1717 // Assignment made to same variable
            protectedValue = protectedValue; // set to the same value, this will not affect readers but will affect writer
#pragma warning restore CS1717 // Assignment made to same variable
        }
    }
}
