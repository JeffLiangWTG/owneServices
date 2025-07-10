using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefAirlineProductCodeUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefAirlineProductCodeUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
	INSERT RefAirlineProductCode (RAR_PK, RAR_AirlineID, RAR_Code, RAR_Description)
	VALUES ('EE01E9F4-E056-4DD5-8A69-D7AE5AA58379', 'A', 'AAA', 'AAAAAA'),
	('EE01E9F4-E056-4DD5-8A69-D7AE5AA58370', 'B', 'BBB', 'BBBBBB')

	INSERT RefAirlineCommodityCode (RAC_PK, RAC_AirlineID, RAC_Code, RAC_Description)
	VALUES ('8B78EF0A-B8D2-4083-871D-CBAE3C055FC7', 'AA', 'AAAA', 'AAAAAA'),
	('8B78EF0A-B8D2-4083-871D-CBAE3C055FC0', 'BB', 'BBBB', 'BBBBBB'),
	('BFCD021E-960D-4B2C-990E-44DFC32E7EAC', 'CC', 'CCCC', 'CCCCCC')

	INSERT RefAirlineProductCodeCommodityCodePivot(RPC_PK, RPC_AirlineID, RPC_RAR, RPC_RAC)
	VALUES(NEWID(), 'A', 'EE01E9F4-E056-4DD5-8A69-D7AE5AA58379', '8B78EF0A-B8D2-4083-871D-CBAE3C055FC7'),
	(NEWID(), 'B', 'EE01E9F4-E056-4DD5-8A69-D7AE5AA58370', '8B78EF0A-B8D2-4083-871D-CBAE3C055FC0')
	", trans);
					var info = new RefAirlineProductCodeUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
	INSERT #TempRefAirlineProductCode (RAR_PK, RAR_AirlineID, RAR_Code, RAR_Description, Deleted)
	VALUES ('D991BF57-4F66-486A-AD9D-A4C0043960E8', 'A', 'AAA', 'AAAAAA', 1),
	('066D8681-E1C8-4FC4-A201-49535A9A926A', 'B', 'BBB', 'TestB', 0),
	('066D8681-E1C8-4FC4-A201-49535A9A9261', 'C', 'CCC', 'TestC', 0)

	INSERT #TempRefAirlineCommodityCode (RAC_PK, RAC_AirlineID, RAC_Code)
	VALUES ('F3C49FC0-2B69-4CB8-8DA9-1F006E6FA5BE', 'BB', 'BBBB'),
	('F3C49FC0-2B69-4CB8-8DA9-1F006E6FA5B1', 'CC', 'CCCC')

	INSERT #TempRefAirlineProductCodeCommodityCodePivot(RPC_PK, RPC_AirlineID, RPC_RAR, RPC_RAC)
	VALUES(NEWID(), 'B', '066D8681-E1C8-4FC4-A201-49535A9A926A', 'F3C49FC0-2B69-4CB8-8DA9-1F006E6FA5BE'),
	(NEWID(), 'C', '066D8681-E1C8-4FC4-A201-49535A9A9261', 'F3C49FC0-2B69-4CB8-8DA9-1F006E6FA5B1')

	", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(0,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineProductCode WHERE RAR_Code = 'AAA'", trans));
					Assert.AreEqual(0,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineProductCodeCommodityCodePivot JOIN RefAirlineProductCode ON RPC_RAR = RAR_PK WHERE RAR_Description = 'AAAAAA'",
							trans));

					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineProductCode WHERE RAR_Description = 'TestB'", trans));
					Assert.AreEqual(0,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineProductCodeCommodityCodePivot JOIN RefAirlineProductCode ON RPC_RAR = RAR_PK WHERE RAR_Description = 'TestA'",
							trans));
					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineProductCodeCommodityCodePivot JOIN RefAirlineProductCode ON RPC_RAR = RAR_PK WHERE RAR_Description = 'TestC'",
							trans));

					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineProductCode WHERE RAR_Description = 'TestC'", trans));
					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineProductCodeCommodityCodePivot JOIN RefAirlineProductCode ON RPC_RAR = RAR_PK WHERE RAR_Description = 'TestC'",
							trans));
				}
			}
		}
	}
}
