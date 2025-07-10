using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IGlbStaffLoginAttemptRecorder
	{
		void RecordLoginAttempt(string loginName, byte[] loginHash);
		bool IsLockedOut(string loginName, byte[] loginHash);
		bool IsAnonymousUserLockedOut(string loginName);
		ZDateTime LockoutDateTimeLocal(string loginName);
		void Unlock(string username);
	}
}
