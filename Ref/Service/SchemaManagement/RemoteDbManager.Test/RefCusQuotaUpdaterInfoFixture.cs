using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusQuotaUpdaterInfoFixture
	{
		[TestCase(DbSchema.RemoteDbCollationCS)]
		[TestCase(DbSchema.RemoteDb)]
		[TransactionedTestCase]
		public void RefCusQuotaUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'ZA', 'South Africa')

INSERT RefCusQuota (ZXQ_PK,ZXQ_OrderNumber,ZXQ_InitialAmount,ZXQ_UnitOfMeasure,ZXQ_Balance,ZXQ_StartDate,ZXQ_EndDate,ZXQ_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'AA', 0, 'KG', 0, '1900-01-01', '2020-12-31', 'ZA'),
(NEWID(), 'BB', 1, 'ML', 0, '2021-01-01', '2079-06-06', 'ZA')", trans);
					var info = new RefCusQuotaUpdaterInfo_1();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZXQ_PK,ZXQ_OrderNumber,ZXQ_InitialAmount,ZXQ_UnitOfMeasure,ZXQ_Balance,ZXQ_StartDate,ZXQ_EndDate,ZXQ_ZZZ_NKDataGrouping,Deleted)
VALUES (NEWID(), 'AA', 0, 'KG', 0, '1900-01-01', '2020-12-31', 'ZA', 0),
(NEWID(), 'BB', 1, 'ML', 0, '2021-01-01', '2021-12-31', 'ZA', 1),
(NEWID(), 'CC', 0, 'L', 1, '2022-01-01', '2079-06-06', 'ZA', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusQuota WHERE ZXQ_OrderNumber = 'AA' AND ZXQ_StartDate = '1900-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusQuota WHERE ZXQ_OrderNumber = 'BB' AND ZXQ_StartDate = '2021-01-01'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusQuota WHERE ZXQ_OrderNumber = 'CC' AND ZXQ_StartDate = '2022-01-01'", trans));
				}
			}
		}
	}
}
