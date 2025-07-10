using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusProfileQuestionUpdaterInfo_1Fixture
	{
		[Test]
		public void Mapping()
		{
			var mapping = new RefCusProfileQuestionUpdaterInfo_1().Mapping;
			Assert.AreEqual("#TempRefCusProfileQuestion", mapping.TableName);
			Assert.AreEqual(4, mapping.RelatedFKColumnNames.Count);
			Assert.AreEqual("XQ2_XXX_ProfileType", mapping.RelatedFKColumnNames["RefCusProfileType"]);
			Assert.AreEqual("XQ4_XQ2_Question", mapping.RelatedFKColumnNames["RefCusProfileQuestionAnswerLists"]);
			Assert.AreEqual("XQ3_XQ2_Question", mapping.RelatedFKColumnNames["RefCusProfileQuestionAttributes"]);
			Assert.AreEqual("XQL_XQ2_Question", mapping.RelatedFKColumnNames["RefCusProfileQuestionLanguages"]);

			Assert.AreEqual(4, mapping.RelatedTableNames.Count);
			Assert.True(mapping.RelatedTableNames.ContainsKey("RefCusProfileType"));
			Assert.True(mapping.RelatedTableNames.ContainsKey("RefCusProfileQuestionAnswerLists"));
			Assert.True(mapping.RelatedTableNames.ContainsKey("RefCusProfileQuestionAttributes"));
			Assert.True(mapping.RelatedTableNames.ContainsKey("RefCusProfileQuestionLanguages"));
		}

		[TestCase(DbSchema.RemoteDbCollationCS)]
		[TestCase(DbSchema.RemoteDb)]
		[TransactionedTestCase]
		public void RefCusProfileQuestionUpdaterInfo_1(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.RestoreColumnNameQuestionCodeForRefCusProfileQuestion(conn, trans);

					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'ZA', 'South Africa');
INSERT INTO RefLanguageType(ZX6_PK, ZX6_Language, ZX6_Description)
VALUES(NEWID(), 'EN', 'English');
INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping)
VALUES ('7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', '1P1', '1P1','ZA');

INSERT INTO RefCusProfileType (XXX_PK,XXX_ProfileType,XXX_ZZI_TariffType,XXX_Description,XXX_ZZZ_NKDataGrouping)
VALUES ('1799FE44-08BE-42CF-B130-E89921BC3E85','A','7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','A Description','ZA');

INSERT INTO RefCusProfileQuestion (XQ2_PK,XQ2_XXX_ProfileType,XQ2_Code,XQ2_AnswerDataType,XQ2_Name,XQ2_Text,XQ2_Note,XQ2_StartDate,XQ2_EndDate,XQ2_ZZZ_NKDataGrouping)
VALUES ('C673520E-9D5E-4388-AF33-03166005DC19','1799FE44-08BE-42CF-B130-E89921BC3E85','Code1','NUMBER','Q1','Text1','Note','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA'),
('39ACB8BB-A802-4784-93B0-74DA96C675E0','1799FE44-08BE-42CF-B130-E89921BC3E85','Code2','NUMBER','Q2','Text2','Note','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA'),
('62CE5344-9EB4-450F-AF2F-5747DBD3E309','1799FE44-08BE-42CF-B130-E89921BC3E85','Code3','NUMBER','Q3','Text3','Note','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA');

INSERT INTO RefCusProfileQuestionAnswerList (XQ4_PK,XQ4_XQ2_Question,XQ4_Value,XQ4_Description)
VALUES ('5DDB0A8C-A200-4DDB-A3D2-8F17137FFDB1','39ACB8BB-A802-4784-93B0-74DA96C675E0','Value1','desc 1'),
('B49E6381-AAB7-4DDF-899D-2DCD3697A508','62CE5344-9EB4-450F-AF2F-5747DBD3E309','Value2','desc 2');
INSERT INTO RefCusProfileQuestionAnswerListLanguage(XAL_PK,XAL_XQ4_QuestionAnswer,XAL_Description,XAL_ZX6_NKLanguage)
VALUES (NEWID(),'5DDB0A8C-A200-4DDB-A3D2-8F17137FFDB1','desc 1','EN'),
(NEWID(),'B49E6381-AAB7-4DDF-899D-2DCD3697A508','desc 2','EN');
INSERT INTO RefCusProfileQuestionAttribute(XQ3_PK,XQ3_XQ2_Question,XQ3_Name,XQ3_Value)
VALUES (NEWID(),'39ACB8BB-A802-4784-93B0-74DA96C675E0','Name1','Value1'),
(NEWID(),'62CE5344-9EB4-450F-AF2F-5747DBD3E309','Name2','Value2');
INSERT INTO RefCusProfileQuestionLanguage(XQL_PK,XQL_XQ2_Question,XQL_Name,XQL_Text,XQL_Note,XQL_ZX6_NKLanguage)
VALUES (NEWID(),'39ACB8BB-A802-4784-93B0-74DA96C675E0','Name1','Text1','','EN'),
(NEWID(),'62CE5344-9EB4-450F-AF2F-5747DBD3E309','Name2','Text2','','EN');
INSERT INTO RefCusProfileQuestionPathway(XQP_PK,XQP_XQ2_QuestionParent,XQP_XQ2_QuestionChild,XQP_Description,XQP_StartDate,XQP_EndDate)
VALUES (NEWID(),'39ACB8BB-A802-4784-93B0-74DA96C675E0','C673520E-9D5E-4388-AF33-03166005DC19','desc 1','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000'),
(NEWID(),'62CE5344-9EB4-450F-AF2F-5747DBD3E309','39ACB8BB-A802-4784-93B0-74DA96C675E0','desc 2','2025-01-01 00:00:00.000','2079-06-06 23:59:00.000'),
(NEWID(),'C673520E-9D5E-4388-AF33-03166005DC19','62CE5344-9EB4-450F-AF2F-5747DBD3E309','desc 3','2026-01-01 00:00:00.000','2079-06-06 23:59:00.000');
", trans);

					var info = new RefCusProfileQuestionUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO #TempRefCusProfileQuestion (XQ2_PK,XQ2_XXX_ProfileType,XQ2_Code,XQ2_AnswerDataType,XQ2_Name,XQ2_Text,XQ2_Note,XQ2_StartDate,XQ2_EndDate,XQ2_ZZZ_NKDataGrouping,XQ2_AnswerMaxLength,XQ2_AnswerDecimalPlaces,XQ2_AnswerMask,XQ2_AllowMultipleAnswers,XQ2_IsAnswerMandatory,Deleted)
VALUES ('3457DA7A-E8A2-41A3-8158-9A2A40955E2C','1799FE44-08BE-42CF-B130-E89921BC3E85','Inserted','STRING','Inserted','Inserted','Note','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA',0,0,'',0,0,0),
('90CCBE86-D06E-4C59-9896-70BA6BAF64F9','1799FE44-08BE-42CF-B130-E89921BC3E85','Code2','BOOLEAN','Updated','Updated','Note','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA',0,0,'',0,0,0),
('90A27CCD-3C52-4664-9DE5-9F3CB723232C','1799FE44-08BE-42CF-B130-E89921BC3E85','Code3','NUMBER','Q3','Text3','Note','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA',0,0,'',0,0,1);

INSERT INTO #TempRefCusProfileQuestionAnswerList (XQ4_PK,XQ4_XQ2_Question,XQ4_Value,XQ4_Description)
VALUES ('5BB5FE11-D686-4A79-BED0-CD452F78CF42','3457DA7A-E8A2-41A3-8158-9A2A40955E2C','Inserted','Inserted Desc'),
('4999B849-8218-4F2C-AE61-0AC4F0ED9CCE','90CCBE86-D06E-4C59-9896-70BA6BAF64F9','Updated','Updated Desc');
INSERT INTO #TempRefCusProfileQuestionAnswerListLanguage (XAL_PK,XAL_XQ4_QuestionAnswer,XAL_Description,XAL_ZX6_NKLanguage)
VALUES (NEWID(),'5BB5FE11-D686-4A79-BED0-CD452F78CF42','Inserted Desc','EN'),
(NEWID(),'4999B849-8218-4F2C-AE61-0AC4F0ED9CCE','Updated Desc','EN');

INSERT INTO #TempRefCusProfileQuestionAttribute (XQ3_PK,XQ3_XQ2_Question,XQ3_Name,XQ3_Value)
VALUES (NEWID(),'3457DA7A-E8A2-41A3-8158-9A2A40955E2C','Inserted','Inserted'),
(NEWID(),'90CCBE86-D06E-4C59-9896-70BA6BAF64F9','Updated','Updated');
INSERT INTO #TempRefCusProfileQuestionLanguage (XQL_PK,XQL_XQ2_Question,XQL_Name,XQL_Text,XQL_Note,XQL_ZX6_NKLanguage)
VALUES (NEWID(),'3457DA7A-E8A2-41A3-8158-9A2A40955E2C','Inserted','Inserted','','EN'),
(NEWID(),'90CCBE86-D06E-4C59-9896-70BA6BAF64F9','Updated','Updated','','EN');
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestion WHERE XQ2_PK = 'C673520E-9D5E-4388-AF33-03166005DC19'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestion WHERE XQ2_PK = '39ACB8BB-A802-4784-93B0-74DA96C675E0'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestion WHERE XQ2_Code = 'Inserted' AND XQ2_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestion WHERE XQ2_Code = 'Code2' AND XQ2_ZZZ_NKDataGrouping = 'ZA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestion WHERE XQ2_Code = 'Code3' AND XQ2_ZZZ_NKDataGrouping = 'ZA'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionAnswerList WHERE XQ4_Value = 'Inserted'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionAnswerList WHERE XQ4_Value = 'Updated'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionAnswerList JOIN RefCusProfileQuestion ON XQ4_XQ2_Question = XQ2_PK WHERE XQ2_Code = 'Code3'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionAnswerListLanguage JOIN RefCusProfileQuestionAnswerList ON XAL_XQ4_QuestionAnswer = XQ4_PK WHERE XQ4_Value = 'Inserted'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionAnswerListLanguage JOIN RefCusProfileQuestionAnswerList ON XAL_XQ4_QuestionAnswer = XQ4_PK WHERE XQ4_Value = 'Updated'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionAttribute WHERE XQ3_Name = 'Inserted'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionAttribute WHERE XQ3_Name = 'Updated'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionAttribute JOIN RefCusProfileQuestion ON XQ3_XQ2_Question = XQ2_PK WHERE XQ2_Code = 'Code3'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionLanguage WHERE XQL_Name = 'Inserted'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionLanguage WHERE XQL_Name = 'Updated'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionLanguage JOIN RefCusProfileQuestion ON XQL_XQ2_Question = XQ2_PK WHERE XQ2_Code = 'Code3'", trans));

					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionPathway JOIN RefCusProfileQuestion ON XQP_XQ2_QuestionParent = XQ2_PK WHERE XQ2_Code = 'Code1'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionPathway JOIN RefCusProfileQuestion ON XQP_XQ2_QuestionChild = XQ2_PK WHERE XQ2_Code = 'Code1'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionPathway JOIN RefCusProfileQuestion ON XQP_XQ2_QuestionParent = XQ2_PK WHERE XQ2_Code = 'Code2'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionPathway JOIN RefCusProfileQuestion ON XQP_XQ2_QuestionChild = XQ2_PK WHERE XQ2_Code = 'Code2'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionPathway JOIN RefCusProfileQuestion ON XQP_XQ2_QuestionParent = XQ2_PK WHERE XQ2_Code = 'Code3'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionPathway JOIN RefCusProfileQuestion ON XQP_XQ2_QuestionChild = XQ2_PK WHERE XQ2_Code = 'Code3'", trans));
				}
			}
		}
	}
}
