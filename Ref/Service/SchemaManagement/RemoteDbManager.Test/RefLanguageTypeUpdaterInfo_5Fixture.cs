using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefLanguageTypeUpdaterInfo_5Fixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefLanguageTypeUpdaterInfo_5(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.RestoreColumnNameQuestionCodeForRefCusProfileQuestion(conn, trans);

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
VALUES(NEWID(), 'BB', 'A', 'A','C2CD44CA-A4D1-45B2-A3F6-5769FB817A45', 'comment');

INSERT INTO RefCusProfileType (XXX_PK,XXX_ProfileType,XXX_ZZI_TariffType,XXX_Description,XXX_ZZZ_NKDataGrouping)
VALUES ('1799FE44-08BE-42CF-B130-E89921BC3E85','A','7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','A Description','ZA');
INSERT INTO RefCusProfileQuestion (XQ2_PK,XQ2_XXX_ProfileType,XQ2_Code,XQ2_AnswerDataType,XQ2_Name,XQ2_Text,XQ2_StartDate,XQ2_EndDate,XQ2_ZZZ_NKDataGrouping)
VALUES ('C673520E-9D5E-4388-AF33-03166005DC19','1799FE44-08BE-42CF-B130-E89921BC3E85','Code1','NUMBER','Q1','Text1','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA');
INSERT INTO RefCusProfileQuestionAnswerList (XQ4_PK,XQ4_XQ2_Question,XQ4_Value,XQ4_Description)
VALUES ('5DDB0A8C-A200-4DDB-A3D2-8F17137FFDB1','C673520E-9D5E-4388-AF33-03166005DC19','Value1','desc 1');
INSERT INTO RefCusProfileQuestionAnswerListLanguage(XAL_PK,XAL_XQ4_QuestionAnswer,XAL_Description,XAL_ZX6_NKLanguage)
VALUES (NEWID(),'5DDB0A8C-A200-4DDB-A3D2-8F17137FFDB1','desc 1','BB');
INSERT INTO RefCusProfileQuestionLanguage(XQL_PK,XQL_XQ2_Question,XQL_Name,XQL_Text,XQL_Note,XQL_ZX6_NKLanguage)
VALUES (NEWID(),'C673520E-9D5E-4388-AF33-03166005DC19','Name1','Text1','','BB');
", trans);
					var info = new RefLanguageTypeUpdaterInfo_5();
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

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileType WHERE XXX_ProfileType = 'A'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileQuestion WHERE XQ2_Code = 'Code1'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileQuestionAnswerList WHERE XQ4_Value = 'Value1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileQuestionAnswerListLanguage WHERE XAL_ZX6_NKLanguage = 'BB'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(1) FROM RefCusProfileQuestionLanguage WHERE XQL_ZX6_NKLanguage = 'BB'", trans));
				}
			}
		}
	}
}
