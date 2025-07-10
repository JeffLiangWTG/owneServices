using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefExchangeRateZZUpdaterInfoFixture_3
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefExchangeRateZZUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefExchangeRateZZ (ZZN_PK, ZZN_ExRateType, ZZN_StartDate, ZZN_EndDate, ZZN_Rate, ZZN_RX_NKExCurrency, ZZN_RN_NKCountry, ZZN_AsPublished)
VALUES (NEWID(), 'BNB', '2017/1/1', '2017/12/12', 3, 'A', 'A', 1),
(NEWID(), 'BNB', '2017/1/1', '2017/12/12', 3, 'B', 'B', 1),
(NEWID(), 'BNS', '2017/1/1', '2017/12/12', 3, 'A', 'A', 1),
(NEWID(), 'BNS', '2017/1/1', '2017/12/12', 3, 'B', 'B', 1)", trans);
					var info = new RefExchangeRateZZUpdaterInfo_3();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZZN_PK, ZZN_ExRateType, ZZN_StartDate, ZZN_EndDate, ZZN_Rate, ZZN_RX_NKExCurrency, ZZN_RN_NKCountry, ZZN_AsPublished, Deleted)
VALUES (NEWID(), 'BNB', '2017/1/1', '2017/12/12', 9, 'A', 'A', 1, 0),
(NEWID(), 'BNB', '2017/1/1', '2017/12/12', 3, 'B', 'B', 1, 1),
(NEWID(), 'BNB', '2017/1/1', '2017/12/12', 3, 'C', 'C', 1, 0),
(NEWID(), 'BNS', '2017/1/1', '2017/12/12', 9, 'A', 'A', 1, 0),
(NEWID(), 'BNS', '2017/1/1', '2017/12/12', 3, 'B', 'B', 1, 1),
(NEWID(), 'BNS', '2017/1/1', '2017/12/12', 3, 'C', 'C', 1, 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(2, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefExchangeRateZZ WHERE ZZN_RX_NKExCurrency = 'C'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefExchangeRateZZ WHERE ZZN_RX_NKExCurrency = 'B'", trans));
					Assert.AreEqual(2, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefExchangeRateZZ WHERE ZZN_RX_NKExCurrency = 'A' AND ZZN_Rate = 9", trans));
				}
			}
		}
	}
}
