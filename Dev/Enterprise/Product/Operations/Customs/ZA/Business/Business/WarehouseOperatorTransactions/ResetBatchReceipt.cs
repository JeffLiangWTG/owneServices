using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public static class ResetBatchReceipt
	{
		[SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "For batch update")]
		public static void Execute(BusinessObjectFactory factory, Guid batchPK, string userCode)
		{
			const string sqlText = "EXEC dbo.ResetCusWhsOperatorTransactionBatchReceipt @BatchPK, @UserCode";
			var connection = ((IDbConnected)factory).Connection;
			using (var transactionManager = connection.BeginTransactionWithManager())
			{
				using (var dbCommand = connection.Command(sqlText))
				{
					dbCommand.AddParameter("@BatchPK", SqlDbType.UniqueIdentifier, batchPK);
					dbCommand.AddParameter("@UserCode", SqlDbType.VarChar, userCode);
					_ = dbCommand.ExecuteNonQuery();
					transactionManager.CommitTransaction();
				}
			}
		}
	}
}
