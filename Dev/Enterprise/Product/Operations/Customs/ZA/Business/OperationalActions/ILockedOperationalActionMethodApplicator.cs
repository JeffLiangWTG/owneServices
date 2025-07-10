using CargoWise.Types;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.ZA.Business
{
	public interface ILockedOperationalActionMethodApplicator
	{
		ZGlobalMutex Mutex { get; }

		ZBool HasLock { get; }

		bool Lock();

		void Unlock();
	}
}
