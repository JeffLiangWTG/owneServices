using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class AgencySundryMutex : ZGlobalMutex
	{
		AgencySundryMutex(string key)
			: base(MutexIDs.AgencySundry, key) { }

		public static AgencySundryMutex New(OrgHeader billToParty)
		{
			if (billToParty == null)
			{
				throw new ArgumentNullException(nameof(billToParty));
			}

			return new AgencySundryMutex(billToParty.PK.ToString());
		}

		public string GetFriendlyMessage()
		{
			string result;
			LockInfo info = GetLockInfo();

			if (info != null && info.UserWithLock != null)
			{
				result = Res.GetString("10fee0ac-e5de-421e-8996-a0552b695354", "{0} has this bill to party locked for this company. Try again later.", info.UserWithLock.GS_FullName);
			}
			else
			{
				result = Res.GetString("d2c09062-8687-4597-8c46-6a561aa9aae5", "Another user has this bill to party locked for this company. Try again later.");
			}

			return result;
		}
	}
}
