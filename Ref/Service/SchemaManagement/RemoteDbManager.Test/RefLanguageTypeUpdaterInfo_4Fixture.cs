using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefLanguageTypeUpdaterInfo_4Fixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefLanguageTypeUpdaterInfo_4(DbSchema dbSchema)
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
VALUES (NEWID(), 'AA', 'AAA'),
(NEWID(), 'BB', 'BBB')

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping)
VALUES ('7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', '1P1', '1P1','ZA')

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType,ZZ1_ZZZ_NKDataGrouping, ZZ1_TariffCode, ZZ1_Description,ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_IAMUnique,ZZ1_StartDate,ZZ1_EndDate,ZZ1_CompositeKeyOnZZ5,ZZ1_PublishedDate)
VALUES ('FDCA07B0-8E98-4A44-A609-D441FCF0E585', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','ZA','Test','T','', 1,'1900-01-01','2079-06-06','A','1900-01-01')

INSERT RefCusConditionType (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
VALUES ('93E985C5-E990-4ABC-9E81-376FB9795AA8', 'CLASS', 'AAA', 'Test1', 'ZA')

INSERT RefCusConditionCode (ZY7_PK, ZY7_ConditionCode, ZY7_Description, ZY7_ZZZ_NKDataGrouping)
VALUES ('4CDCB28C-DD1C-4F12-A29E-42475A03186F', 'CCC', 'Test2', 'ZA')

INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZY7_NKConditionCode, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZZ_NKDataGrouping, ZX1_Source, ZX1_Comment, ZX1_IsImport, ZX1_IsExport, ZX1_ConditionValueTrueMeansStop, ZX1_LogicalANDWithinGroup, ZX1_AdditionalComment)
VALUES ('C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', '93E985C5-E990-4ABC-9E81-376FB9795AA8', 'CCC', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 'ZA', 'A', '', 1, 0, 1, 1, 'D')

INSERT RefCusConditionLanguage (ZXJ_PK, ZXJ_ZX6_NKLanguage, ZXJ_Comment, ZXJ_Source, ZXJ_ZX1_Condition, ZXJ_AdditionalComment)
VALUES(NEWID(), 'BB', 'A', 'A','C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'comment')", trans);
					var info = new RefLanguageTypeUpdaterInfo_4();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZX6_PK, ZX6_Language, ZX6_Description, Deleted)
VALUES (NEWID(), 'AA', 'XXX', 0),
(NEWID(), 'BB', 'BBB', 1),
(NEWID(), 'CC','CCC', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefLanguageType WHERE ZX6_Language = 'CC'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefLanguageType WHERE ZX6_Language = 'BB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefLanguageType WHERE ZX6_Language = 'AA' AND ZX6_Description = 'XXX'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionLanguage WHERE ZXJ_Source = 'A'", trans));
				}
			}
		}
	}
}
