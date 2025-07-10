using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefHarbourRateUpdaterInfo_2Fixture
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

INSERT RefHarbourRate (ZXF_PK, ZXF_Type, ZXF_Port, ZXF_Mode, ZXF_Commodity, ZXF_StartDate, ZXF_EndDate, ZXF_RateFormula, ZXF_ZZZ_NKDataGrouping, ZXF_PortTaxType)
VALUES (NEWID(), 'A', 'A', 'CON', 'A', '2017/1/1', '2017/12/12', 'A', 'ZA', ''),
(NEWID(), 'B', 'B', 'CON', 'B', '2017/1/1', '2017/12/12', 'B', 'ZA', 'TAT')", trans);
					var info = new RefHarbourRateUpdaterInfo_2();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZXF_PK, ZXF_Type, ZXF_Port, ZXF_Mode, ZXF_Commodity, ZXF_StartDate, ZXF_EndDate, ZXF_RateFormula, ZXF_ZZZ_NKDataGrouping, ZXF_PortTaxType, Deleted)
VALUES  (NEWID(), 'A', 'A', 'CON', 'A', '2017/1/1', '2017/12/12', 'X', 'ZA', 'ATA', 0),
(NEWID(), 'B', 'B', 'CON', 'B', '2017/1/1', '2017/12/12', 'B', 'ZA', 'ASD', 1),
(NEWID(), 'C', 'C', 'CON', 'C', '2017/1/1', '2017/12/12', 'C', 'ZA', 'RTD', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefHarbourRate WHERE ZXF_PortTaxType = 'ATA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefHarbourRate WHERE ZXF_PortTaxType = 'ASD'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefHarbourRate WHERE ZXF_PortTaxType = 'RTD' AND ZXF_RateFormula = 'C'", trans));
				}
			}
		}
	}
}
