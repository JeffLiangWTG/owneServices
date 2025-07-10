using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Customs.Business.BusinessObjects.CusPermit
{
	public interface ICusPermitCusDecProcessor<T>
		where T : BusinessObject, ISavingProvider<T>
	{
		void AddPermitRecordsAndLockMutexIfNeeded();
		void AddPermitTransactions(T parentBizO, Func<T, ZString> getPermitAppId, string status = PermitTransactionStatusList.Codes.Pending);
		void UnlockPermitMutexes();
		IEnumerable<ZString> ErrorList { get; }
	}
}
