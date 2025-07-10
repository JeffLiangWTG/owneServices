using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IOrgContactLoginAttemptRecorder
	{
		void RecordLoginAttempt(string companyCode, string username, byte[] hash);
		bool IsLockedOut(string companyCode, string username, byte[] hash);
		bool IsAnonymousUserLockedOut(string companyCode, string username);
		ZDateTime LockoutDateTimeLocal(string companyCode, string username);
		void Unlock(string companyCode, string username, bool unlockEmptyCompany);
	}
}
