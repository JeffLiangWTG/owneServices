using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	class Test_RefCusProfileQuestionPathway : IXmlProcessIntegrationTest
	{
		public string[] FileNames => ["TestFiles\\RefCusProfileQuestionPathway.xml"];
		public string TestDescription => "Test RefCusProfileQuestionPathway can be merged successfully";

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml];

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			var safeSql = @"
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (newid(), 69, 'RefCusProfileQuestionPathway', 'RefCusProfileQuestionPathway', 'XQP', 0);

insert into [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
values (newid(), 'BR', 'Brazil', null);

insert into [dbo].[RefCusTariffType] (ZZI_PK,ZZI_TariffType,ZZI_Description,ZZI_ZZ9_NKNomenclatureGroupType,ZZI_ZZZ_NKDataGrouping)
values ('85220DAB-E3E9-43F9-B5E0-23BD95F28774', 'HSN', 'Brazil Harmonized Tariff', '', 'BR');

insert into [dbo].[RefCusProfileType] (XXX_PK,XXX_ProfileType,XXX_ZZI_TariffType,XXX_Description,XXX_ZZZ_NKDataGrouping)
values ('5334117C-8179-426A-B79C-405BEAC195C7', 'NCM', '85220DAB-E3E9-43F9-B5E0-23BD95F28774', 'BR NCM Attributes', 'BR');

insert into [dbo].[RefCusProfileQuestion] (XQ2_PK,XQ2_XXX_ProfileType,XQ2_QuestionCode,XQ2_AnswerDataType,XQ2_Name,XQ2_Text,XQ2_Note,XQ2_StartDate,XQ2_EndDate,XQ2_ZZZ_NKDataGrouping)
values (newid(),'5334117C-8179-426A-B79C-405BEAC195C7','ATT_4471','LIST','4471','4471','4471','2021-04-22 00:00:00','2079-06-06 23:59:00','BR'),
(newid(),'5334117C-8179-426A-B79C-405BEAC195C7','ATT_4502','LIST','4502','4502','4502','2021-04-23 00:00:00','2079-06-06 23:59:00','BR'),
(newid(),'5334117C-8179-426A-B79C-405BEAC195C7','ATT_4504','LIST','4504','4504','4504','2021-04-23 00:00:00','2079-06-06 23:59:00','BR');
";
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();
		}

		void AssertResult_AfterProcessingXml(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml(stagingCommand);
			AssertResult_StagingDb_RefCusProfileQuestionPathway_AfterProcessingXml(stagingCommand);

			AssertResult_SafeDb_RefCusProfileQuestionPathway_AfterProcessingXml(safeCommand);
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml(IDbCommand stagingCommand)
		{
			var status = string.Empty;
			stagingCommand.CommandText = "SELECT TOP 1 SDA_Status FROM dbo.SourceData WHERE SDA_SubSource='RefCusProfileQuestionPathway Test' ORDER BY SDA_CreatedTime DESC";
			using (var reader = stagingCommand.ExecuteReader())
			{
				if (reader.Read())
				{
					status = reader.GetString(0);
				}
			}
			Assert.That(status, Is.EqualTo(StatusProvider.GetMERStatus()));
		}

		void AssertResult_StagingDb_RefCusProfileQuestionPathway_AfterProcessingXml(IDbCommand stagingCommand)
		{
			var pathwayList = new List<RefCusProfileQuestionPathway>();
			var sql = @"select XQP_XQ2_NKQuestionParent,XQP_XQ2_NKQuestionStartDateParent,XQP_XQ2_ZZZ_NKDataGroupingParent,XQP_XQ2_NKQuestionChild,XQP_XQ2_NKQuestionStartDateChild,XQP_XQ2_ZZZ_NKDataGroupingChild,
XQP_StartDate,XQP_XQ2_XXX_NKProfileType,XQP_XQ2_XXX_ZZZ_NKDataGrouping,XQP_XQ2_XXX_ZZI_NKTariffType,XQP_XQ2_XXX_ZZZ_NKDataGrouping
from RefCusProfileQuestionPathway";
			stagingCommand.CommandText = sql;
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var pathway = new RefCusProfileQuestionPathway();
					pathway.XQP_XQ2_NKQuestionParent = reader.GetString(0);
					pathway.XQP_XQ2_NKQuestionStartDateParent = reader.GetDateTime(1);
					pathway.XQP_XQ2_ZZZ_NKDataGroupingParent = reader.GetString(2);
					pathway.XQP_XQ2_NKQuestionChild = reader.GetString(3);
					pathway.XQP_XQ2_NKQuestionStartDateChild = reader.GetDateTime(4);
					pathway.XQP_XQ2_ZZZ_NKDataGroupingChild = reader.GetString(5);
					pathway.XQP_StartDate = reader.GetDateTime(6);
					pathway.XQP_XQ2_XXX_NKProfileType = reader.GetString(7);
					pathway.XQP_XQ2_XXX_ZZZ_NKDataGrouping = reader.GetString(8);
					pathway.XQP_XQ2_XXX_ZZI_NKTariffType = reader.GetString(9);
					pathway.XQP_XQ2_XXX_ZZI_ZZZ_NKDataGrouping = reader.GetString(10);
					pathwayList.Add(pathway);
				}
			}
			Assert.AreEqual(2, pathwayList.Count);
			Assert.True(pathwayList.All(p => p.XQP_XQ2_ZZZ_NKDataGroupingParent == "BR"));
			Assert.True(pathwayList.All(p => p.XQP_XQ2_ZZZ_NKDataGroupingChild == "BR"));
			Assert.True(pathwayList.All(p => p.XQP_XQ2_XXX_NKProfileType == "NCM"));
			Assert.True(pathwayList.All(p => p.XQP_XQ2_XXX_ZZZ_NKDataGrouping == "BR"));
			Assert.True(pathwayList.All(p => p.XQP_XQ2_XXX_ZZI_NKTariffType == "HSN"));
			Assert.True(pathwayList.All(p => p.XQP_XQ2_XXX_ZZI_ZZZ_NKDataGrouping == "BR"));

			var pathway1 = pathwayList.First(x => x.XQP_XQ2_NKQuestionParent == "ATT_4471");
			Assert.AreEqual(new DateTime(2021, 4, 22), pathway1.XQP_XQ2_NKQuestionStartDateParent);
			Assert.AreEqual("ATT_4502", pathway1.XQP_XQ2_NKQuestionChild);
			Assert.AreEqual(new DateTime(2021, 4, 23), pathway1.XQP_XQ2_NKQuestionStartDateChild);
			Assert.AreEqual(new DateTime(2023, 5, 24), pathway1.XQP_StartDate);

			var pathway2 = pathwayList.First(x => x.XQP_XQ2_NKQuestionParent == "ATT_4502");
			Assert.AreEqual(new DateTime(2021, 4, 23), pathway2.XQP_XQ2_NKQuestionStartDateParent);
			Assert.AreEqual("ATT_4504", pathway2.XQP_XQ2_NKQuestionChild);
			Assert.AreEqual(new DateTime(2021, 4, 23), pathway2.XQP_XQ2_NKQuestionStartDateChild);
			Assert.AreEqual(new DateTime(2023, 7, 12), pathway2.XQP_StartDate);
		}

		void AssertResult_SafeDb_RefCusProfileQuestionPathway_AfterProcessingXml(IDbCommand safeCommand)
		{
			safeCommand.CommandText = "select count(1) from RefCusProfileQuestionPathway";
			Assert.AreEqual(2, (int)safeCommand.ExecuteScalar());

			var sql = @"select count(1) from RefCusProfileQuestionPathway join RefCusProfileQuestion Q1 on XQP_XQ2_QuestionParent = Q1.XQ2_PK 
join RefCusProfileQuestion Q2 on XQP_XQ2_QuestionChild = Q2.XQ2_PK
where Q1.XQ2_QuestionCode = 'ATT_4471' and Q2.XQ2_QuestionCode = 'ATT_4502'";
			safeCommand.CommandText = sql;
			Assert.AreEqual(1, (int)safeCommand.ExecuteScalar());

			sql = @"select count(1) from RefCusProfileQuestionPathway join RefCusProfileQuestion Q1 on XQP_XQ2_QuestionParent = Q1.XQ2_PK 
join RefCusProfileQuestion Q2 on XQP_XQ2_QuestionChild = Q2.XQ2_PK
where Q1.XQ2_QuestionCode = 'ATT_4502' and Q2.XQ2_QuestionCode = 'ATT_4504'";
			safeCommand.CommandText = sql;
			Assert.AreEqual(1, (int)safeCommand.ExecuteScalar());
		}
	}
}
