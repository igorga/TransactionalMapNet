namespace Igorg
{
    internal struct Flags
    {
        // _owners: MSB is reserved for exclusive lock tracking, others are for readers
        // Exclusive: can be taken only when no other locks are taken, that is,
        //   when _owners is 0
        // Shared: can be taken when there are no exclusive locks being taken,
        //   that is when _owners is greater than MAX_SHARED count. Taking exclusive
        //   lock will put this value above the maximum so the simple check can be done
        // Upgradeable: can be taken when there are no exclusive or upgradeable locks
        //   held, but can be taken when there are shared locks. For the purpose of 
        //   checking shared and exclusive lock availability, upgradeable lock behaves
        //   like the shared lock, by incrementing the _owners field. For checking the
        //   upgradeable availability, _upgradeTaken is checked, as well as the shared
        //   lock availability (max shared + exclusive check)
        private uint _owners;
        private bool _upgradeTaken;

        private const uint EXCLUSIVE_FLAG = 0x80000000; // 31

        private const uint MAX_SHARED = 0x80000000 - 2;
        private const uint SHARED_MASK = 0x80000000 - 1;
        
        public bool CanTakeExclusive()
        {
            // if anyone took the lock, exclusive lock can't be taken
            return _owners == 0;
        }

        public bool CanTakeUpgradeable()
        {
            return !_upgradeTaken && CanTakeShared();
        }

        public bool CanTakeShared()
        {
            // setting the exclusive flag will increase _owners above the max shared
            // so this simple check can be used
            return _owners < MAX_SHARED;
        }

        public void TakeShared()
        {
            _owners++;
        }

        public void ReleaseShared()
        {
            _owners--;
        }

        public void TakeUpgradeable()
        {
            _owners++;
            _upgradeTaken = true;
        }

        public void ReleaseUpgradeable()
        {
            _owners--;
            _upgradeTaken = false;
        }

        public void SetExclusiveFlag()
        {
            _owners |= EXCLUSIVE_FLAG;
        }

        public void ResetExclusiveFlag()
        {
            _owners &= ~EXCLUSIVE_FLAG;
        }

    }
}