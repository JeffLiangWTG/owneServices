using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefAccTaxUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefAccTaxUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefAccTaxRate(ZAT_PK, ZAT_RN_NKCountry, ZAT_ReferenceRateType, ZAT_StartDate, ZAT_EndDate, ZAT_RateNumerator, ZAT_RateDenominator)
VALUES (NEWID(), 'CN', '355', '2017/1/1', '2017/12/12', 3, 4),
(NEWID(), 'CN', '555', '2017/1/1', '2017/12/12', 3, 4)", trans);
					var info = new RefAccTaxRateUpdaterInfo_1();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZAT_PK, ZAT_RN_NKCountry, ZAT_ReferenceRateType, ZAT_StartDate, ZAT_EndDate, ZAT_RateNumerator, ZAT_RateDenominator, Deleted)
VALUES (NEWID(), 'CN', '355', '2017/1/1', '2017/12/12', 4, 5, 0),
(NEWID(), 'CN', '455', '2017/1/1', '2017/12/12', 4, 5, 0),
(NEWID(), 'CN', '555', '2017/1/1', '2017/12/12', 3, 4, 1)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefAccTaxRate WHERE ZAT_ReferenceRateType = '455'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefAccTaxRate WHERE ZAT_ReferenceRateType = '555'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefAccTaxRate WHERE ZAT_ReferenceRateType = '355' AND ZAT_RateNumerator = 4", trans));
				}
			}
		}
	}
}
