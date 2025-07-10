using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefVesselZZUpdaterInfo_2Fixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefVesselZZUpdaterInfo_2(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES (NEWID(), 'EUN', 'EUN')

INSERT RefVesselZZ (ZZO_PK, ZZO_Code, ZZO_RadioCallSign, ZZO_VesselType, ZZO_RN_NKCountryOfReg, ZZO_LloydsNumber, ZZO_ZZZ_NKDataGrouping)
VALUES ('A3805854-E8A3-4FE8-BE68-5709748F26EC', 'A', 'AA', 'A', 'A', 'A', 'EUN'),
('296E4CF8-C747-4704-B480-C55EEDA0C48D', 'B', 'BB', 'B', 'B', 'B', 'EUN');

INSERT RefVesselArrival (ZYA_PK, ZYA_ZZO_Vessel, ZYA_VoyageNumber, ZYA_ArrivalDate, ZYA_ArrivalPort)
VALUES ('4A4494B0-9889-47C1-961A-A1256A863A4D', 'A3805854-E8A3-4FE8-BE68-5709748F26EC', '1', '2025-01-01 00:00:00', 'A'),
('4FD7C428-60A0-43CE-B6A0-E44AF679A9CD', '296E4CF8-C747-4704-B480-C55EEDA0C48D', '2', '2025-01-01 00:00:00', 'B');"
, trans);

					var info = new RefVesselZZUpdaterInfo_2();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT #TempRefVesselZZ (ZZO_PK, ZZO_Code, ZZO_RadioCallSign, ZZO_VesselType, ZZO_RN_NKCountryOfReg, ZZO_LloydsNumber, ZZO_ZZZ_NKDataGrouping, Deleted)
VALUES ('85A59220-5B33-4BA9-8A86-8280612A941D', 'A', 'AA', 'A', 'A', 'X', 'EUN', 0),
('5E50ED4B-2CB3-47E9-B1EA-5AA1DD5C33FD', 'B', 'BB', 'B', 'B', 'B', 'EUN', 1),
('525C9A0D-4EBC-4159-8315-A5BA01540D33', 'C', 'CC', 'C', 'C', 'C', 'EUN', 0);
INSERT #TempRefVesselArrival (ZYA_PK, ZYA_ZZO_Vessel, ZYA_VoyageNumber, ZYA_ArrivalDate, ZYA_ArrivalPort)
VALUES (NEWID(), '85A59220-5B33-4BA9-8A86-8280612A941D', '2', '2025-01-01 00:00:00', 'AA'),
(NEWID(), '85A59220-5B33-4BA9-8A86-8280612A941D', '3', '2025-01-02 00:00:00', 'AB');
", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);

					var vesselZZResult = TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM RefVesselZZ WHERE ZZO_Code = 'C' AND ZZO_ZZZ_NKDataGrouping='EUN'", trans);
					Assert.That(vesselZZResult, Is.EqualTo(1));
					vesselZZResult= TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM RefVesselZZ WHERE ZZO_Code = 'B' AND ZZO_ZZZ_NKDataGrouping='EUN'", trans);
					Assert.That(vesselZZResult, Is.EqualTo(0));
					vesselZZResult = TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefVesselZZ WHERE ZZO_Code = 'A'
AND ZZO_ZZZ_NKDataGrouping='EUN' AND ZZO_LloydsNumber='X'", trans);
					Assert.That(vesselZZResult, Is.EqualTo(1));

					var vesselArrivalResult = TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefVesselArrival WHERE ZYA_PK IN
('4A4494B0-9889-47C1-961A-A1256A863A4D', '4FD7C428-60A0-43CE-B6A0-E44AF679A9CD')", trans);
					Assert.That(vesselArrivalResult, Is.EqualTo(0));
					vesselArrivalResult = TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefVesselArrival JOIN RefVesselZZ ON ZYA_ZZO_Vessel=ZZO_PK
WHERE ZZO_Code='A' AND ZZO_ZZZ_NKDataGrouping='EUN' AND ZYA_VoyageNumber='2'", trans);
					Assert.That(vesselArrivalResult, Is.EqualTo(1));
					vesselArrivalResult = TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefVesselArrival JOIN RefVesselZZ ON ZYA_ZZO_Vessel=ZZO_PK
WHERE ZZO_Code='A' AND ZZO_ZZZ_NKDataGrouping='EUN' AND ZYA_VoyageNumber='3'", trans);
					Assert.That(vesselArrivalResult, Is.EqualTo(1));
				}
			}
		}
	}
}
