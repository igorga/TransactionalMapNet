using System;
using System.Collections.Generic;
using System.Threading;

namespace Igorg
{
    class GenericLock<T> where T : IEquatable<T>
    {
        private SpinLock _spinLock; // lightwait lock that ensures atomicity of taking and releasing the lock
        private Flags _flags; // flags holding the state of the lock
        private T _exclusiveOwnerId; // id of the holder of the exclusive lock
        private T _upgradeableOwnerId; // id of the holder of the exclusive lock
        private HashSet<T> _sharedOwnerIds; // ids of the holders of the shared lock
        
        public GenericLock()
        {
            _sharedOwnerIds = new HashSet<T>(); // todo pool these hashsets
            _spinLock = new SpinLock();
            _flags = new Flags();
        }
        
        public bool ExclusiveLockTakenBy(T id)
        {
            return _exclusiveOwnerId.Equals(id);
        }

        public bool SharedLockTakenBy(T id)
        {
            return _sharedOwnerIds.Count > 0 && _sharedOwnerIds.Contains(id);
        }

        public bool UpgradeableLockTakenBy(T id)
        {
            return _upgradeableOwnerId.Equals(id);
        }

        private bool UpgradeableLockNotTaken => _upgradeableOwnerId.Equals(default(T));

        public void EnterExclusive(T id)
        {
            bool taken = false;
            _spinLock.Enter(ref taken);
            try
            {
                if (ExclusiveLockTakenBy(id))
                {
                    throw new InvalidOperationException("Recursion is not allowed");
                }

                if (SharedLockTakenBy(id))
                {
                    throw new InvalidOperationException("Cannot take shared lock after the exclusive was already taken.");
                }
                
                for (;;)
                {
                    if (_flags.CanTakeExclusive())
                    {
                        // from now on all others that try to enter this lock will have to wait
                        _flags.SetExclusiveFlag();
                        break;
                    }
                    
                    // at any point in time other thread can take the upgradeable lock 
                    // so it is better to perform the check here, while within the spinlock
                    if (UpgradeableLockTakenBy(id))
                    {
                        // only the upgradeable lock (with this id) is taken, it is safe
                        // to upgrade it
                        _flags.SetExclusiveFlag();
                        break;
                    }

                    // lock is taken, we have to wait
                    SpinWait();
                }
            }
            finally
            {
                _spinLock.Exit();
            }

            // we are protected here, id for exit can be set.
            _exclusiveOwnerId = id;
        }

        public void ExitExclusive(T id)
        {
            bool taken = false;
            _spinLock.Enter(ref taken);

            try
            {
                if (!ExclusiveLockTakenBy(id))
                {
                    throw new InvalidOperationException($"Exclusive lock has not been taken by {id.ToString()}.");
                }

                _flags.ResetExclusiveFlag();
                _exclusiveOwnerId = default(T);
            }
            finally
            {
                _spinLock.Exit();
            }
        }

        public void EnterShared(T id)
        {
            bool taken = false;
            _spinLock.Enter(ref taken);
            try
            {
                if (ExclusiveLockTakenBy(id))
                {
                    throw new InvalidOperationException("Shared lock is not allowed after the exclusive is already taken.");
                }
                
                if (UpgradeableLockTakenBy(id))
                {
                    throw new InvalidOperationException("Shared lock is not allowed after the upgradeable is already taken.");
                }

                if (SharedLockTakenBy(id))
                {
                    throw new InvalidOperationException("Shared lock is already taken.");
                }

                for (;;)
                {
                    if (_flags.CanTakeShared())
                    {
                        _flags.TakeShared();
                        _sharedOwnerIds.Add(id);
                        break;
                    }

                    SpinWait();
                }
            }
            finally
            {
                _spinLock.Exit();
            }
        }

        public void ExitShared(T id)
        {
            bool taken = false;
            _spinLock.Enter(ref taken);
            try
            {
                if (!_sharedOwnerIds.Remove(id))
                {
                    throw new InvalidOperationException($"Shared lock has not been taken by {id.ToString()}.");
                }

                _flags.ReleaseShared();
            }
            finally
            {
                _spinLock.Exit();
            }
        }
        
        public void EnterUpgradeable(T id)
        {
            bool taken = false;
            _spinLock.Enter(ref taken);
            try
            {
                if (UpgradeableLockTakenBy(id) || ExclusiveLockTakenBy(id) || SharedLockTakenBy(id))
                {
                    // todo better description of exceptions
                    throw new InvalidOperationException("Recursion is not allowed.");
                }

                for (;;)
                {
                    if (_flags.CanTakeUpgradeable())
                    {
                        _flags.TakeUpgradeable();
                        _upgradeableOwnerId = id;
                        break;
                    }

                    // lock is taken, we have to wait
                    SpinWait();
                }
            }
            finally
            {
                _spinLock.Exit();
            }
        }
        
        public void ExitUpgradeable(T id)
        {
            bool taken = false;
            _spinLock.Enter(ref taken);
            try
            {
                if (!_upgradeableOwnerId.Equals(id))
                {
                    throw new InvalidOperationException($"Upgradeable lock has not been taken by {id.ToString()}.");
                }
                
                _upgradeableOwnerId = default(T);
                _flags.ReleaseUpgradeable();
            }
            finally
            {
                _spinLock.Exit();
            }
        }

        private void SpinWait()
        {
            _spinLock.Exit();
            // todo switch to proper spin
            // todo introduce different mechanism to avoid spinning thaking too much CPU
            Thread.Sleep(10);
            bool taken = false;
            _spinLock.Enter(ref taken);
        }

    }
}
