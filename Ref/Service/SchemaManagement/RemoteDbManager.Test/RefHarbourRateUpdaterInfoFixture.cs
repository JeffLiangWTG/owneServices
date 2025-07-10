using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefHarbourRateUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefHarbourRateUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'ZA', 'South Africa')

INSERT RefHarbourRate (ZXF_PK, ZXF_Type, ZXF_Port, ZXF_Mode, ZXF_Commodity, ZXF_StartDate, ZXF_EndDate, ZXF_RateFormula, ZXF_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'A', 'A', 'CON', 'A', '2017/1/1', '2017/12/12', 'A', 'ZA'),
(NEWID(), 'B', 'B', 'CON', 'B', '2017/1/1', '2017/12/12', 'B', 'ZA')", trans);
					var info = new RefHarbourRateUpdaterInfo_1();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZXF_PK, ZXF_Type, ZXF_Port, ZXF_Mode, ZXF_Commodity, ZXF_StartDate, ZXF_EndDate, ZXF_RateFormula, ZXF_ZZZ_NKDataGrouping, Deleted)
VALUES  (NEWID(), 'A', 'A', 'CON', 'A', '2017/1/1', '2017/12/12', 'X', 'ZA', 0),
(NEWID(), 'B', 'B', 'CON', 'B', '2017/1/1', '2017/12/12', 'B', 'ZA', 1),
(NEWID(), 'C', 'C', 'CON', 'C', '2017/1/1', '2017/12/12', 'C', 'ZA', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefHarbourRate WHERE ZXF_Type = 'C'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefHarbourRate WHERE ZXF_Type = 'B'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefHarbourRate WHERE ZXF_Type = 'A' AND ZXF_RateFormula = 'X'", trans));
				}
			}
		}
	}
}
