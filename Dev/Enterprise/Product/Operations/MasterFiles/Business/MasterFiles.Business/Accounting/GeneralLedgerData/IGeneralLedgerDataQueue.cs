using System;
using CargoWise.Data;

namespace Enterprise.MasterFiles.Business
{
	public interface IGeneralLedgerDataQueue
	{
		void RemoveNarrowGLD(DbConnection dbConnection, Guid companyPK, DateTime startDate, DateTime endDate);

		void QueueTransaction(DbConnection dbConnection, Guid companyPK, DateTime startDate, DateTime endDate, DateTime endSystemCreateTime, bool isCheckDuplicated);
	}
}