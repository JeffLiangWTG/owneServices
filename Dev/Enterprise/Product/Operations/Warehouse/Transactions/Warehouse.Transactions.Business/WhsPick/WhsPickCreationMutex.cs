using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickCreationMutex : ZGlobalMutex
	{
		public WhsPickCreationMutex(WhsPickableDocket order)
			: base(MutexIDs.WhsNewPick, order.PK.ToString())
		{ }

		public string GetMessage()
		{
			string result = "";
			var info = GetLockInfo();

			if (info != null && info.UserWithLock != null)
			{
				result = Res.GetString("8d2b08e4-6ad6-4119-888d-2d058dcba871", "{0} is currently in the process of creating a new Pick for this Order. Try again later.", info.UserWithLock.GS_FullName);
			}

			return result;
		}
	}
}
