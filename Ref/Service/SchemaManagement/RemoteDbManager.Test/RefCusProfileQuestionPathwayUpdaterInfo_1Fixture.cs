using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusProfileQuestionPathwayUpdaterInfo_1Fixture
	{
		[Test]
		public void Mapping()
		{
			var mapping = new RefCusProfileQuestionPathwayUpdaterInfo_1().Mapping;
			Assert.AreEqual("#TempRefCusProfileQuestionPathway", mapping.TableName);
			Assert.AreEqual(2, mapping.RelatedFKColumnNames.Count);
			Assert.AreEqual("XQP_XQ2_QuestionParent", mapping.RelatedFKColumnNames["RefCusProfileQuestionParent"]);
			Assert.AreEqual("XQP_XQ2_QuestionChild", mapping.RelatedFKColumnNames["RefCusProfileQuestionChild"]);

			Assert.AreEqual(2, mapping.RelatedTableNames.Count);
			Assert.True(mapping.RelatedTableNames.ContainsKey("RefCusProfileQuestionParent"));
			Assert.True(mapping.RelatedTableNames.ContainsKey("RefCusProfileQuestionChild"));
		}

		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusProfileQuestionPathwayUpdaterInfo_1(DbSchema dbSchema)
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
INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping)
VALUES ('7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', '1P1', '1P1','ZA');
INSERT INTO RefCusProfileType (XXX_PK,XXX_ProfileType,XXX_ZZI_TariffType,XXX_Description,XXX_ZZZ_NKDataGrouping)
VALUES ('1799FE44-08BE-42CF-B130-E89921BC3E85','A','7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','A Description','ZA');

INSERT INTO RefCusProfileQuestion (XQ2_PK,XQ2_XXX_ProfileType,XQ2_Code,XQ2_AnswerDataType,XQ2_Name,XQ2_Text,XQ2_Note,XQ2_StartDate,XQ2_EndDate,XQ2_ZZZ_NKDataGrouping)
VALUES ('C673520E-9D5E-4388-AF33-03166005DC19','1799FE44-08BE-42CF-B130-E89921BC3E85','Code1','NUMBER','Q1','Text1','Note','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA'),
('39ACB8BB-A802-4784-93B0-74DA96C675E0','1799FE44-08BE-42CF-B130-E89921BC3E85','Code2','NUMBER','Q2','Text2','Note','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA'),
('62CE5344-9EB4-450F-AF2F-5747DBD3E309','1799FE44-08BE-42CF-B130-E89921BC3E85','Code3','NUMBER','Q3','Text3','Note','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA');

INSERT INTO RefCusProfileQuestionPathway(XQP_PK,XQP_XQ2_QuestionParent,XQP_XQ2_QuestionChild,XQP_Description,XQP_StartDate,XQP_EndDate)
VALUES (NEWID(),'C673520E-9D5E-4388-AF33-03166005DC19','39ACB8BB-A802-4784-93B0-74DA96C675E0','desc 1','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000'),
(NEWID(),'39ACB8BB-A802-4784-93B0-74DA96C675E0','62CE5344-9EB4-450F-AF2F-5747DBD3E309','desc 2','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000');
", trans);

					var info = new RefCusProfileQuestionPathwayUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO #TempRefCusProfileQuestionPathway(XQP_PK,XQP_XQ2_QuestionParent,XQP_XQ2_QuestionChild,XQP_Description,XQP_StartDate,XQP_EndDate,XQP_ConditionToProceedFormula,Deleted)
VALUES (NEWID(),'62CE5344-9EB4-450F-AF2F-5747DBD3E309','C673520E-9D5E-4388-AF33-03166005DC19','Inserted','2023-01-01 00:00:00.000','2079-06-06 23:59:00.000','',0),
(NEWID(),'C673520E-9D5E-4388-AF33-03166005DC19','39ACB8BB-A802-4784-93B0-74DA96C675E0','Updated','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','',0),
(NEWID(),'39ACB8BB-A802-4784-93B0-74DA96C675E0','62CE5344-9EB4-450F-AF2F-5747DBD3E309','desc 2','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','',1);
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionPathway WHERE XQP_Description = 'Inserted'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionPathway WHERE XQP_Description = 'Updated'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionPathway WHERE XQP_Description = 'desc 1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileQuestionPathway WHERE XQP_Description = 'desc 2'", trans));
				}
			}
		}
	}
}
