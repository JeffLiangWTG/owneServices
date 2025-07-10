using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefAirlineCommodityCodeUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefAirlineCommodityCodeUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefAirlineCommodityCode (RAC_PK,RAC_AirlineID,RAC_Code,RAC_Description)
VALUES (NEWID(), 'A', 'AAA', 'AAAAAA'),
(NEWID(), 'B', 'BB', 'BBBBB')", trans);
					var info = new RefAirlineCommodityCodeUpdaterInfo_1();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value,
						trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (RAC_PK,RAC_AirlineID,RAC_Code,RAC_Description, Deleted)
VALUES (NEWID(), 'A', 'AAA', '111', 0),
(NEWID(), 'B', 'BB', 'BBBBB', 1),
(NEWID(), 'C', 'CC', 'CCCCC', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineCommodityCode WHERE RAC_AirlineID = 'C'", trans));
					Assert.AreEqual(0,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineCommodityCode WHERE RAC_AirlineID = 'B'", trans));
					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineCommodityCode WHERE RAC_AirlineID = 'A' AND RAC_Description = 111",
							trans));
				}
			}
		}
	}
}
