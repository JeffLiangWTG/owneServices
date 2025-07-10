using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefVesselZZUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefVesselZZUpdaterInfo(DbSchema dbSchema)
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

INSERT RefVesselZZ (ZZO_PK, ZZO_Code, ZZO_RadioCallSign, ZZO_VesselType, ZZO_RN_NKCountryOfReg, ZZO_LloydsNumber, ZZO_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'A','A','A','A','A','ZA'),
(NEWID(), 'B','B','A','A','A','ZA')", trans);
					var info = new RefVesselZZUpdaterInfo_1();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZZO_PK, ZZO_Code, ZZO_RadioCallSign, ZZO_VesselType, ZZO_RN_NKCountryOfReg, ZZO_LloydsNumber, ZZO_ZZZ_NKDataGrouping, Deleted)
VALUES (NEWID(), 'A','A','A','A','X','ZA', 0),
(NEWID(), 'B','B','A','A','A','ZA', 1),
(NEWID(), 'C','C','A','A','A','ZA', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefVesselZZ WHERE ZZO_Code = 'C'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefVesselZZ WHERE ZZO_Code = 'B'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefVesselZZ WHERE ZZO_Code = 'A' AND ZZO_LloydsNumber = 'X'", trans));
				}
			}
		}
	}
}
