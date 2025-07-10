using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusTaxOrFeeTypeUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusTaxOrFeeTypeUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'ZA', 'South Africa')

INSERT INTO RefLanguageType(ZX6_PK, ZX6_Language, ZX6_Description)
VALUES (NEWID(), 'EN', 'EN')

INSERT INTO RefCusTaxOrFeeType (ZX0_PK, ZX0_TaxOrFeeType, ZX0_Description)
VALUES (NEWID(), 'OTH', 'Other'),
(NEWID(), 'BBB', 'Other')

INSERT INTO RefCusTaxOrFee (ZZF_PK,ZZF_Code,ZZF_Description,ZZF_Value,ZZF_StartDate,ZZF_EndDate,ZZF_ZZZ_NKDataGrouping,ZZF_ZX0_NKTaxOrFeeType,ZZF_Minimum,ZZF_Maximum,ZZF_Threshold)
VALUES ('FBC9D1DB-B995-411A-BCE4-1D849DA928B7','MIN','Minima',0.04000000,'1900-01-01 00:00:00','2079-06-06 00:00:00','ZA','OTH',0,1,0),
('15A2CCF0-16C6-4739-87A9-22E7AFB1B22F','BBB','XBBB',0.10000000,'1900-01-01 00:00:00','2079-06-06 00:00:00','ZA','BBB',0,1,0)

INSERT INTO RefCusTaxOrFeeLanguage (ZXU_PK,ZXU_ZZF_TaxOrFee,ZXU_ZX6_NKLanguage,ZXU_Description)
VALUES (NEWID(), 'FBC9D1DB-B995-411A-BCE4-1D849DA928B7', 'EN', 'AAA'),
(NEWID(), '15A2CCF0-16C6-4739-87A9-22E7AFB1B22F', 'EN', 'XBB')
", trans);
					var info = new RefCusTaxOrFeeTypeUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO #TempRefCusTaxOrFeeType (ZX0_PK, ZX0_TaxOrFeeType, ZX0_Description, Deleted)
VALUES (NEWID(), 'OTH', 'Other', 1),
(NEWID(), 'BBB', 'XBB', 0),
(NEWID(), 'CCC', 'XCC', 0)

INSERT INTO #TempRefCusTaxOrFee (ZZF_PK,ZZF_Code,ZZF_Description,ZZF_Value,ZZF_StartDate,ZZF_EndDate,ZZF_ZZZ_NKDataGrouping,ZZF_ZX0_NKTaxOrFeeType,ZZF_Minimum,ZZF_Maximum,ZZF_Threshold)
VALUES ('D52C4ADB-81CF-42EE-B8EF-B932344C05E6','CCC','CCCC',0.04000000,'1900-01-01 00:00:00','2079-06-06 00:00:00','ZA','CCC',0,1,0),
('80887161-FC7B-4E3F-8B99-D612FEC0EA8B','BBB','BBBB',0.10000000,'1900-01-01 00:00:00','2079-06-06 00:00:00','ZA','BBB',0,1,0)

INSERT INTO #TempRefCusTaxOrFeeLanguage (ZXU_PK,ZXU_ZZF_TaxOrFee,ZXU_ZX6_NKLanguage,ZXU_Description)
VALUES (NEWID(), 'D52C4ADB-81CF-42EE-B8EF-B932344C05E6', 'EN', 'CCC'),
(NEWID(), '80887161-FC7B-4E3F-8B99-D612FEC0EA8B', 'EN', 'BBB')
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTaxOrFeeType WHERE ZX0_TaxOrFeeType = 'OTH'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTaxOrFee WHERE ZZF_Code = 'MIN'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTaxOrFeeLanguage WHERE ZXU_Description = 'AAA'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTaxOrFeeType WHERE ZX0_TaxOrFeeType = 'BBB' AND ZX0_Description = 'XBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTaxOrFee WHERE ZZF_Code = 'BBB' AND ZZF_Description = 'BBBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTaxOrFeeLanguage WHERE ZXU_Description = 'BBB'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTaxOrFeeType WHERE ZX0_TaxOrFeeType = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTaxOrFee WHERE ZZF_Code = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusTaxOrFeeLanguage WHERE ZXU_Description = 'CCC'", trans));
				}
			}
		}
	}
}
