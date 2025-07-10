using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefAccElectronicProcessingFeeUpdaterInfo_2Fixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefAccElectronicProcessingFeeUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefAccElectronicProcessingFee(EPF_PK, EPF_SystemCode, EPF_Category, EPF_Code, EPF_Description, EPF_Currency, EPF_Price, EPF_ValidFrom, EPF_CountryCode, EPF_JobDirection)
VALUES (NEWID(), 'A', 'B', 'C', 'Des1', 'E', 5, '2024/01/01', 'AU','D1'),
(NEWID(), 'A1', 'B1', 'C1', 'Des2', 'E1', 5, '2024/01/01', 'AU', 'D2')", trans);
					var info = new RefAccElectronicProcessingFeeUpdaterInfo_2();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (EPF_PK, EPF_SystemCode, EPF_Category, EPF_Code, EPF_Description, EPF_Currency, EPF_Price, EPF_ValidFrom, EPF_CountryCode, EPF_JobDirection, Deleted)
VALUES (NEWID(), 'A1', 'B1', 'C1', 'Des3', 'E1', 5, '2024/01/01', 'AU', 'D3', 0),
(NEWID(), 'A2', 'B2', 'C2', 'Des4', 'E2', 5, '2024/01/01', 'AU', 'D4', 0),
(NEWID(), 'A', 'B', 'C', 'Des1', 'E', 5, '2024/01/01', 'AU', 'D1', 1)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefAccElectronicProcessingFee WHERE EPF_Description = 'Des1'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefAccElectronicProcessingFee WHERE EPF_Description = 'Des2' AND EPF_CountryCode = 'AU' AND EPF_JobDirection = 'D2'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefAccElectronicProcessingFee WHERE EPF_Description = 'Des3' AND EPF_CountryCode = 'AU' AND EPF_JobDirection = 'D3'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefAccElectronicProcessingFee WHERE EPF_Description = 'Des4' AND EPF_CountryCode = 'AU' AND EPF_JobDirection = 'D4'", trans));
				}
			}
		}
	}
}
