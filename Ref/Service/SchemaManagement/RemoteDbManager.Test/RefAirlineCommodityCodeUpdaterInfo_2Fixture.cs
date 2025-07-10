using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefAirlineCommodityCodeUpdaterInfo_2Fixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefAirlineCommodityCodeUpdaterInfo_2(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
	INSERT RefAirlineCommodityCode (RAC_PK,RAC_AirlineID,RAC_Code,RAC_Description)
	VALUES ('8B78EF0A-B8D2-4083-871D-CBAE3C055FC7', 'A', 'AAA', 'AAAAAA'),
	('8B78EF0A-B8D2-4083-871D-CBAE3C055FC0', 'B', 'BB', 'BBBBB')

	INSERT RefAirlineProductCode (RAR_PK, RAR_AirlineID, RAR_Code, RAR_Description)
	VALUES ('EE01E9F4-E056-4DD5-8A69-D7AE5AA58379', 'A', '111', 'TestA'),
	('EE01E9F4-E056-4DD5-8A69-D7AE5AA58370', 'B', '222', 'TestB')

	INSERT RefAirlineProductCodeCommodityCodePivot(RPC_PK, RPC_AirlineID, RPC_RAR, RPC_RAC)
	VALUES(NEWID(), 'A', 'EE01E9F4-E056-4DD5-8A69-D7AE5AA58379', '8B78EF0A-B8D2-4083-871D-CBAE3C055FC7'),
	(NEWID(), 'B', 'EE01E9F4-E056-4DD5-8A69-D7AE5AA58370', '8B78EF0A-B8D2-4083-871D-CBAE3C055FC0')
	", trans);

					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineCommodityCode WHERE RAC_AirlineID = 'A' AND RAC_Description = 'AAAAAA'",
							trans));
					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineProductCodeCommodityCodePivot JOIN RefAirlineCommodityCode ON RPC_RAC = RAC_PK WHERE RAC_AirlineID = 'A'",
							trans));
					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineCommodityCode WHERE RAC_AirlineID = 'B'", trans));
					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineProductCodeCommodityCodePivot JOIN RefAirlineCommodityCode ON RPC_RAC = RAC_PK WHERE RAC_AirlineID = 'B'",
							trans));

					var info = new RefAirlineCommodityCodeUpdaterInfo_2();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value,
						trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
	INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (RAC_PK,RAC_AirlineID,RAC_Code,RAC_Description, Deleted)
	VALUES (NEWID(), 'A', 'AAA', '111', 0),
	(NEWID(), 'B', 'BB', 'BBBBB', 1),
	(NEWID(), 'C', 'CC', 'CCCCC', 0)
	", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(0,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineCommodityCode WHERE RAC_AirlineID = 'A' AND RAC_Description = 'AAAAAA'",
							trans));
					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineCommodityCode WHERE RAC_AirlineID = 'A' AND RAC_Description = '111'",
							trans));
					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineProductCodeCommodityCodePivot JOIN RefAirlineCommodityCode ON RPC_RAC = RAC_PK WHERE RAC_AirlineID = 'A'",
							trans));
					Assert.AreEqual(0,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineCommodityCode WHERE RAC_AirlineID = 'B'", trans));
					Assert.AreEqual(0,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineProductCodeCommodityCodePivot JOIN RefAirlineCommodityCode ON RPC_RAC = RAC_PK WHERE RAC_AirlineID = 'B'",
							trans));
					Assert.AreEqual(1,
						TestDBHelper.ExecuteScalar(conn,
							@"SELECT COUNT(*) FROM RefAirlineCommodityCode WHERE RAC_AirlineID = 'C'", trans));
				}
			}
		}
	}
}
