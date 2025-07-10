using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusProfileTypeUpdaterInfo_3Fixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusProfileTypeUpdaterInfo_3(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.RestoreColumnNameQuestionCodeForRefCusProfileQuestion(conn, trans);

					var refCusProfile = new RefCusProfile();
					TestDBHelper.ExecuteNonQuery(conn, SharedDbSchemaChange.GetRenameColumnIfExistsScript(refCusProfile.TableName, "XX0_AppliesToCode", "XX0_TariffCode"), trans);
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'ZA', 'South Africa');
INSERT INTO RefLanguageType(ZX6_PK, ZX6_Language, ZX6_Description)
VALUES(NEWID(), 'EN', 'English');
INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping)
VALUES ('7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', '1P1', '1P1','ZA');

INSERT INTO RefCusProfileType (XXX_PK,XXX_ProfileType,XXX_ZZI_TariffType,XXX_Description,XXX_ZZZ_NKDataGrouping)
VALUES ('1799FE44-08BE-42CF-B130-E89921BC3E85','A','7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','A Description','ZA'),
('D272AC2C-C3CB-4981-851E-764899C55D73','B','7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','B Description','ZA'),
('D1D8A0DC-8EA9-4143-9744-F2F0B2FA0F42','D','7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','D Description','ZA');

INSERT INTO RefCusProfile (XX0_PK,XX0_XXX_ProfileType,XX0_TariffCode,XX0_QuestionCode,XX0_StartDate,XX0_EndDate,XX0_ZZZ_NKDataGrouping)
VALUES ('9EC9F58A-0DE4-48AA-8081-18A45E5E216F','D272AC2C-C3CB-4981-851E-764899C55D73','1001','QC1','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA'),
('6EDA47B3-F6E3-4FAE-819E-4CD5BC107E98','D1D8A0DC-8EA9-4143-9744-F2F0B2FA0F42','1002','QC2','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA');
INSERT INTO RefCusProfileAttribute(XXY_PK,XXY_XX0_Profile,XXY_Name,XXY_Value)
VALUES (NEWID(),'6EDA47B3-F6E3-4FAE-819E-4CD5BC107E98','Name1','Value1');

INSERT INTO RefCusProfileQuestion (XQ2_PK,XQ2_XXX_ProfileType,XQ2_Code,XQ2_AnswerDataType,XQ2_Name,XQ2_Text,XQ2_Note,XQ2_StartDate,XQ2_EndDate,XQ2_ZZZ_NKDataGrouping)
VALUES ('39ACB8BB-A802-4784-93B0-74DA96C675E0','D1D8A0DC-8EA9-4143-9744-F2F0B2FA0F42','Code1','NUMBER','Q1','Text1','Note','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA');
INSERT INTO RefCusProfileQuestionAnswerList (XQ4_PK,XQ4_XQ2_Question,XQ4_Value,XQ4_Description)
VALUES ('5DDB0A8C-A200-4DDB-A3D2-8F17137FFDB1','39ACB8BB-A802-4784-93B0-74DA96C675E0','Value1','desc 1');
INSERT INTO RefCusProfileQuestionAnswerListLanguage(XAL_PK,XAL_XQ4_QuestionAnswer,XAL_Description,XAL_ZX6_NKLanguage)
VALUES (NEWID(),'5DDB0A8C-A200-4DDB-A3D2-8F17137FFDB1','desc 1','EN');
INSERT INTO RefCusProfileQuestionAttribute(XQ3_PK,XQ3_XQ2_Question,XQ3_Name,XQ3_Value)
VALUES (NEWID(),'39ACB8BB-A802-4784-93B0-74DA96C675E0','Name1','Value1');
INSERT INTO RefCusProfileQuestionLanguage(XQL_PK,XQL_XQ2_Question,XQL_Name,XQL_Text,XQL_Note,XQL_ZX6_NKLanguage)
VALUES (NEWID(),'39ACB8BB-A802-4784-93B0-74DA96C675E0','Name1','Text1','','EN');
INSERT INTO RefCusProfileQuestionPathway(XQP_PK,XQP_XQ2_QuestionParent,XQP_XQ2_QuestionChild,XQP_Description,XQP_StartDate,XQP_EndDate)
VALUES (NEWID(),'39ACB8BB-A802-4784-93B0-74DA96C675E0','39ACB8BB-A802-4784-93B0-74DA96C675E0','desc 1','2023-01-01 00:00:00.000','2079-06-06 23:59:00.000');
", trans);

					var info = new RefCusProfileTypeUpdaterInfo_3();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO #TempRefCusProfileType (XXX_PK,XXX_ProfileType,XXX_ZZI_TariffType,XXX_Description,XXX_ZZZ_NKDataGrouping,Deleted)
VALUES (NEWID(), 'A', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', 'A Description', 'ZA',1),
('0BD52681-9E16-4311-8F85-00ADC79E194D', 'B', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', 'B Description', 'ZA', 0),
('000DAEE5-D5DD-4E9D-8FE1-714476FF3A6D', 'C', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', 'C Description', 'ZA', 0),
('CACB9F09-4267-4055-AFB5-6E31967D9C6D', 'D', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', 'D Description', 'ZA', 1)
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileType WHERE XXX_ProfileType = 'A'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileType WHERE XXX_ProfileType = 'B'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfile WHERE XX0_XXX_ProfileType = 'D272AC2C-C3CB-4981-851E-764899C55D73'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileType WHERE XXX_ProfileType = 'C'", trans));

					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileType WHERE XXX_ProfileType = 'D'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfile WHERE XX0_XXX_ProfileType = 'D1D8A0DC-8EA9-4143-9744-F2F0B2FA0F42'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileAttribute WHERE XXY_Name = 'Name1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestion JOIN RefCusProfileType ON XQ2_XXX_ProfileType = XXX_PK WHERE XXX_ProfileType = 'D'", trans));
				}
			}
		}
	}
}
