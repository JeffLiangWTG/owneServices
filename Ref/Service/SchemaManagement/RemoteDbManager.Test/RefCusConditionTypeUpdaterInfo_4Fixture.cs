using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusConditionTypeUpdaterInfo_4Fixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusConditionTypeUpdaterInfo_4(DbSchema dbSchema)
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

INSERT RefLanguageType (ZX6_PK, ZX6_Language, ZX6_Description)
VALUES (NEWID(), 'WW', 'WWW')

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping)
VALUES ('7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', '1P1', '1P1','ZA')

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType,ZZ1_ZZZ_NKDataGrouping, ZZ1_TariffCode, ZZ1_Description,ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_IAMUnique,ZZ1_StartDate,ZZ1_EndDate,ZZ1_CompositeKeyOnZZ5,ZZ1_PublishedDate)
VALUES ('FDCA07B0-8E98-4A44-A609-D441FCF0E585', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','ZA','Test','T','', 1,'1900-01-01','2079-06-06','A','1900-01-01')

INSERT RefCusConditionType (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
VALUES ('93E985C5-E990-4ABC-9E81-376FB9795AA8', 'CLASS', 'AAAAAA', 'Test1', 'ZA'),
('CB682E16-FE25-4A2F-9997-A5AF0CA6A37E', 'CLASS', 'BBBBBB', 'Test2', 'ZA'),
('13CB1F8E-7793-43C8-B401-B5DBE6FA9782', 'CLASS', 'DDDDDD', 'Test4', 'ZA'),
('99DD2453-AA65-44FC-AA93-90077980F5DB', 'CLASS', 'CCCCCC', 'Test3', 'ZA')

INSERT RefCusConditionTypeLanguage (ZXW_PK, ZXW_ZX2_ConditionType, ZXW_ZX6_NKLanguage, ZXW_Description)
VALUES(NEWID(),'93E985C5-E990-4ABC-9E81-376FB9795AA8','WW', 'AAA'),
(NEWID(),'CB682E16-FE25-4A2F-9997-A5AF0CA6A37E','WW','BBB'),
(NEWID(),'13CB1F8E-7793-43C8-B401-B5DBE6FA9782','WW','DDD')

INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZZ_NKDataGrouping, ZX1_Source, ZX1_Comment, ZX1_IsImport, ZX1_IsExport, ZX1_ConditionValueTrueMeansStop, ZX1_LogicalANDWithinGroup)
VALUES ('C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'CB682E16-FE25-4A2F-9997-A5AF0CA6A37E', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'ZA', 'A', '', 1, 0, 1, 1)

INSERT RefCusConditionLanguage (ZXJ_PK, ZXJ_ZX6_NKLanguage, ZXJ_Comment, ZXJ_Source, ZXJ_ZX1_Condition)
VALUES(NEWID(), 'WW', 'A', 'A','C2CD44CA-A4D1-45B2-A3F6-5769FB817A45')", trans);
					var info = new RefCusConditionTypeUpdaterInfo_4();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping, Deleted)
VALUES ('93E985C5-E990-4ABC-9E81-376FB9795AA8', 'CLASS', 'AAAAAA', 'Test1111', 'ZA', 0),
('CB682E16-FE25-4A2F-9997-A5AF0CA6A37E', 'CLASS', 'BBBBBB', 'Test2', 'ZA', 1),
(NEWID(), 'CLASS', 'DDDDDD', 'Test4', 'ZA', 0),
(NEWID(), 'RISK', 'EEEEEE', 'Test5', 'ZA', 0)

INSERT {info.PrepareTemporaryTablesScripts.LastOrDefault().Key} (ZXW_PK, ZXW_ZX2_ConditionType, ZXW_ZX6_NKLanguage, ZXW_Description)
VALUES(NEWID(),'93E985C5-E990-4ABC-9E81-376FB9795AA8','WW', 'QQQ'),
(NEWID(),'99DD2453-AA65-44FC-AA93-90077980F5DB','WW', 'CCC')", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionType WHERE ZX2_ConditionClass != 'CLASS'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionType WHERE ZX2_ConditionType = 'AAAAAA' AND ZX2_Description = 'Test1111'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionType WHERE ZX2_ConditionType = 'BBBBBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionType WHERE ZX2_ConditionType = 'CCCCCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionType WHERE ZX2_ConditionType = 'DDDDDD'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionTypeLanguage WHERE ZXW_Description = 'QQQ'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionTypeLanguage WHERE ZXW_Description = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionTypeLanguage WHERE ZXW_Description = 'CCC'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionTypeLanguage WHERE ZXW_Description = 'DDD'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCondition WHERE ZX1_Source = 'A'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionLanguage WHERE ZXJ_Source = 'A'", trans));
				}
			}
		}
	}
}
