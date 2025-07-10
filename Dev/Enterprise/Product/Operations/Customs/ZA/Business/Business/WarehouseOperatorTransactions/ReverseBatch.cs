using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public static class ReverseBatch
	{
		[SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "For batch update")]
		public static void Execute(BusinessObjectFactory factory, Guid batchPK, string userCode)
		{
			const string sqlText = "EXEC dbo.ReverseCusWhsOperatorTransactionBatch @BatchPK, @UserCode, @UserTime";
			var connection = ((IDbConnected)factory).Connection;
			using (var transactionManager = connection.BeginTransactionWithManager())
			{
				using (var dbCommand = connection.Command(sqlText))
				{
					dbCommand.AddParameter("@BatchPK", SqlDbType.UniqueIdentifier, batchPK);
					dbCommand.AddParameter("@UserCode", SqlDbType.VarChar, userCode);
					dbCommand.AddParameter("@UserTime", SqlDbType.DateTime, ZDateTime.Now);
					_ = dbCommand.ExecuteNonQuery();
					transactionManager.CommitTransaction();
				}
			}
		}
	}
}
