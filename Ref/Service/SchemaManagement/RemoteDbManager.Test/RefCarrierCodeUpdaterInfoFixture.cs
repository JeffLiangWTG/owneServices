using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCarrierCodeUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCarrierCodeUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
	INSERT RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
	VALUES(NEWID(), 'ZA', 'South Africa')

	INSERT RefCarrierCode (ZZ4_PK, ZZ4_Code, ZZ4_Description, ZZ4_ZZZ_NKDataGrouping, ZZ4_IsSea, ZZ4_IsRoad, ZZ4_IsAir, ZZ4_IsRail)
	VALUES ('EE01E9F4-E056-4DD5-8A69-D7AE5AA58379', 'DEND', 'Deugro A/S Denmark', 'ZA', 1, 0, 0, 0),
	('EE01E9F4-E056-4DD5-8A69-D7AE5AA58370', 'AAA', 'AAAA', 'ZA', 1, 0, 0, 0)

	INSERT RefCarrierCodeAttribute(ZZG_PK, ZZG_ZZ4_CarrierCode, ZZG_Name, ZZG_Value)
	VALUES(NEWID(), 'EE01E9F4-E056-4DD5-8A69-D7AE5AA58379', 'SEA', 'SEA'),
	(NEWID(), 'EE01E9F4-E056-4DD5-8A69-D7AE5AA58370', 'AIR', 'AIR')

	INSERT RefVesselZZ (ZZO_PK, ZZO_Code, ZZO_RadioCallSign, ZZO_VesselType, ZZO_RN_NKCountryOfReg, ZZO_LloydsNumber, ZZO_ZZZ_NKDataGrouping)
	VALUES ('8B78EF0A-B8D2-4083-871D-CBAE3C055FC7', 'SCL Andisa', 'HBEN', 'CV', '', '', 'ZA'),
	('8B78EF0A-B8D2-4083-871D-CBAE3C055FC0', 'BBB', 'BBB', 'BB', '', '', 'ZA'),
	('BFCD021E-960D-4B2C-990E-44DFC32E7EAC', 'CCC', 'CCC', 'CC', '', '', 'ZA')

	INSERT RefCarrierVesselPivot(ZZQ_PK, ZZQ_ZZ4, ZZQ_ZZO)
	VALUES(NEWID(), 'EE01E9F4-E056-4DD5-8A69-D7AE5AA58379', '8B78EF0A-B8D2-4083-871D-CBAE3C055FC7'),
	(NEWID(), 'EE01E9F4-E056-4DD5-8A69-D7AE5AA58370', '8B78EF0A-B8D2-4083-871D-CBAE3C055FC0')
	", trans);
					var info = new RefCarrierCodeUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
	INSERT #TempRefCarrierCode (ZZ4_PK, ZZ4_Code, ZZ4_Description, ZZ4_ZZZ_NKDataGrouping, Deleted, ZZ4_IsSea, ZZ4_IsRoad, ZZ4_IsAir, ZZ4_IsRail)
	VALUES ('D991BF57-4F66-486A-AD9D-A4C0043960E8', 'DEND', 'Deugro A/S Denmark', 'ZA', 1, 1, 0, 0, 0),
	('066D8681-E1C8-4FC4-A201-49535A9A926A', 'AAA', 'TestA', 'ZA', 0, 1, 0, 0, 0),
	('066D8681-E1C8-4FC4-A201-49535A9A9261', 'CCC', 'CCCC', 'ZA', 0, 1, 0, 0, 0)

	INSERT #TempRefCarrierCodeAttribute(ZZG_PK, ZZG_ZZ4_CarrierCode, ZZG_Name, ZZG_Value)
	VALUES(NEWID(), '066D8681-E1C8-4FC4-A201-49535A9A926A', 'AIR', 'AAA'),
	(NEWID(), '066D8681-E1C8-4FC4-A201-49535A9A9261', 'CCC', 'CCC')

	INSERT #TempRefVesselZZ (ZZO_PK, ZZO_Code, ZZO_RadioCallSign, ZZO_ZZZ_NKDataGrouping)
	VALUES ('F3C49FC0-2B69-4CB8-8DA9-1F006E6FA5BE', 'BBB', 'BBB', 'ZA'),
	('F3C49FC0-2B69-4CB8-8DA9-1F006E6FA5B1', 'CCC', 'CCC', 'ZA')

	INSERT #TempRefCarrierVesselPivot(ZZQ_PK, ZZQ_ZZ4, ZZQ_ZZO)
	VALUES(NEWID(), '066D8681-E1C8-4FC4-A201-49535A9A926A', 'F3C49FC0-2B69-4CB8-8DA9-1F006E6FA5BE'),
	(NEWID(), '066D8681-E1C8-4FC4-A201-49535A9A9261', 'F3C49FC0-2B69-4CB8-8DA9-1F006E6FA5B1')

	", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(0,
						TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCarrierCode WHERE ZZ4_Code = 'DEND'",
							trans));
					Assert.AreEqual(0,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefCarrierCodeAttribute WHERE ZZG_Name = 'SEA'", trans));
					Assert.AreEqual(0,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefCarrierVesselPivot JOIN RefCarrierCode ON ZZQ_ZZ4 = ZZ4_PK WHERE ZZ4_Description = 'Deugro A/S Denmark'",
							trans));

					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefCarrierCode WHERE ZZ4_Description = 'TestA'", trans));
					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefCarrierCodeAttribute WHERE ZZG_Value = 'AAA'", trans));
					Assert.AreEqual(0,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefCarrierVesselPivot JOIN RefCarrierCode ON ZZQ_ZZ4 = ZZ4_PK WHERE ZZ4_Description = 'AAAA'",
							trans));
					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefCarrierVesselPivot JOIN RefCarrierCode ON ZZQ_ZZ4 = ZZ4_PK WHERE ZZ4_Description = 'TestA'",
							trans));

					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefCarrierCode WHERE ZZ4_Description = 'CCCC'", trans));
					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefCarrierCodeAttribute WHERE ZZG_Value = 'CCC'", trans));
					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefCarrierVesselPivot JOIN RefCarrierCode ON ZZQ_ZZ4 = ZZ4_PK WHERE ZZ4_Description = 'CCCC'",
							trans));
				}
			}
		}
	}
}
