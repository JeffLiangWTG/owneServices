using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	class IntegrationTest_RefCusCodeList : IXmlProcessIntegrationTest
	{
		public string[] FileNames => ["TestFiles\\RefCusCodeList_WithStartDate.xml", "TestFiles\\RefCusCodeList_WithoutStartDate.xml"];

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1, AssertResult_AfterProcessingXml2];

		public string TestDescription => "Test RefCusCodeList XML w/o ZZD_StartDate/EndDate";

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			var safeSql = $$"""
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (newid(), 10, 'RefCusCodeList', 'RefCusCodeList','ZZD',0);

insert into [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
values (newid(), 'EUN', 'European Union', null);
insert into [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
values (newid(), 'DE', 'Germany', 'EUN');

insert into [dbo].[RefLanguageType] (ZX6_PK,ZX6_Language,ZX6_Description)
values (newid(), 'DE', 'German');

insert into [dbo].[RefCusCodeType] (ZZK_PK,ZZK_CodeType,ZZK_Description,ZZK_ZZZ_NKDataGrouping)
values (NEWID(), 'EMCPK', 'Exise Movement Control System (EMCS) Pack Types', 'EUN');
""";
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();
		}

		void AssertResult_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				var sourceID = AssertResult_StagingDb_SourceData_AfterProcessingXml1(stagingCommand);
				AssertResult_StagingDb_RefCusCodeList_AfterProcessingXml1(stagingCommand, sourceID);

				AssertResult_SafeDb_RefCusCodeList_AfterProcessingXml1(safeCommand);
			});
		}

		void AssertResult_AfterProcessingXml2(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				var sourceID = AssertResult_StagingDb_SourceData_AfterProcessingXml2(stagingCommand);
				AssertResult_StagingDb_RefCusCodeList_AfterProcessingXml2(stagingCommand, sourceID);

				AssertResult_SafeDb_RefCusCodeList_AfterProcessingXml2(safeCommand);
				AssertResult_SafeDb_RefCusCodeListLanguage_AfterProcessingXml2(safeCommand);
			});
		}

		Guid AssertResult_StagingDb_SourceData_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var sourceID = Guid.Empty;
			var status = string.Empty;
			stagingCommand.CommandText = "SELECT TOP 1 SDA_PK,SDA_Status FROM dbo.SourceData WHERE SDA_SubSource='DE EMCPK' ORDER BY SDA_CreatedTime DESC";
			using var reader = stagingCommand.ExecuteReader();
			if (reader.Read())
			{
				sourceID = reader.GetGuid(0);
				status = reader.GetString(1);
			}
			Assert.That(status, Is.EqualTo(StatusProvider.GetMERStatus()));
			return sourceID;
		}

		void AssertResult_StagingDb_RefCusCodeList_AfterProcessingXml1(IDbCommand stagingCommand, Guid sourceID)
		{
			var codeLists = new List<RefCusCodeList>();
			var sql = $$"""
select ZZD_Code,ZZD_StartDate,ZZD_EndDate from RefCusCodeList 
join DataProcessingInformation on ZZD_PK = DPI_ParentPk 
where DPI_SourceId='{{sourceID}}'
""";
			stagingCommand.CommandText = sql;
			using var reader = stagingCommand.ExecuteReader();
			while (reader.Read())
			{
				codeLists.Add(new RefCusCodeList
				{
					ZZD_Code = reader.GetString(0),
					ZZD_StartDate = reader.GetDateTime(1),
					ZZD_EndDate = reader.GetDateTime(2)
				});
			}
			Assert.AreEqual(4, codeLists.Count);
			Assert.True(codeLists.All(x => x.ZZD_EndDate == new DateTime(2079, 6, 6, 23, 59, 0)));
			var codeList_AE = codeLists.First(x => x.ZZD_Code == "AE");
			Assert.AreEqual(new DateTime(1900, 1, 1), codeList_AE.ZZD_StartDate);
			codeLists.Remove(codeList_AE);
			Assert.True(codeLists.All(x => x.ZZD_StartDate == new DateTime(2024, 1, 1)));
		}

		void AssertResult_SafeDb_RefCusCodeList_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var codeLists = new List<RefCusCodeList>();
			var sql = "select ZZD_Code,ZZD_StartDate,ZZD_EndDate from RefCusCodeList where ZZD_ZZK_NKCodeType='EMCPK' and ZZD_ZZZ_NKDataGrouping='EUN'";
			safeCommand.CommandText = sql;
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var codeList = new RefCusCodeList();
					codeList.ZZD_Code = reader.GetString(0);
					codeList.ZZD_StartDate = reader.GetDateTime(1);
					codeList.ZZD_EndDate = reader.GetDateTime(2);
					codeLists.Add(codeList);
				}
			}
			Assert.AreEqual(4, codeLists.Count);
			Assert.True(codeLists.All(x => x.ZZD_EndDate == new DateTime(2079, 6, 6, 23, 59, 0)));
			var codeList_AE = codeLists.First(x => x.ZZD_Code == "AE");
			Assert.AreEqual(new DateTime(1900, 1, 1), codeList_AE.ZZD_StartDate);
			codeLists.Remove(codeList_AE);
			Assert.True(codeLists.All(x => x.ZZD_StartDate == new DateTime(2024, 1, 1)));
		}

		Guid AssertResult_StagingDb_SourceData_AfterProcessingXml2(IDbCommand stagingCommand)
		{
			var sourceID = Guid.Empty;
			var status = string.Empty;
			stagingCommand.CommandText = "SELECT TOP 1 SDA_PK,SDA_Status FROM dbo.SourceData WHERE SDA_SubSource='DE EMCPK' ORDER BY SDA_CreatedTime DESC";
			using (var reader = stagingCommand.ExecuteReader())
			{
				if (reader.Read())
				{
					sourceID = reader.GetGuid(0);
					status = reader.GetString(1);
				}
			}
			Assert.That(status, Is.EqualTo(StatusProvider.GetMERStatus()));
			return sourceID;
		}

		void AssertResult_StagingDb_RefCusCodeList_AfterProcessingXml2(IDbCommand stagingCommand, Guid sourceID)
		{
			var now = DateTime.Now;
			var codeLists = new List<RefCusCodeList>();
			var sql = $"select ZZD_Code,ZZD_StartDate,ZZD_EndDate from RefCusCodeList join DataProcessingInformation on ZZD_PK = DPI_ParentPk where DPI_SourceId='{sourceID}'";
			stagingCommand.CommandText = sql;
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var codeList = new RefCusCodeList();
					codeList.ZZD_Code = reader.GetString(0);
					codeList.ZZD_StartDate = reader.GetDateTime(1);
					codeList.ZZD_EndDate = reader.GetDateTime(2);
					codeLists.Add(codeList);
				}
			}
			Assert.AreEqual(8, codeLists.Count);
			Assert.True(codeLists.All(x => x.ZZD_StartDate == x.ZZD_EndDate));
			Assert.LessOrEqual((now - codeLists[0].ZZD_StartDate).TotalSeconds, 30d);
		}

		void AssertResult_SafeDb_RefCusCodeList_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var codeLists = new List<RefCusCodeList>();
			var sql = "select ZZD_Code,ZZD_StartDate,ZZD_EndDate from RefCusCodeList where ZZD_ZZK_NKCodeType='EMCPK' and ZZD_ZZZ_NKDataGrouping='EUN'";
			safeCommand.CommandText = sql;
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var codeList = new RefCusCodeList();
					codeList.ZZD_Code = reader.GetString(0);
					codeList.ZZD_StartDate = reader.GetDateTime(1);
					codeList.ZZD_EndDate = reader.GetDateTime(2);
					codeLists.Add(codeList);
				}
			}
			Assert.AreEqual(4, codeLists.Count);
			Assert.True(codeLists.All(x => x.ZZD_EndDate == new DateTime(2079, 6, 6, 23, 59, 0)));
			var codeList_AE = codeLists.First(x => x.ZZD_Code == "AE");
			Assert.AreEqual(new DateTime(1900, 1, 1), codeList_AE.ZZD_StartDate);
			codeLists.Remove(codeList_AE);
			Assert.True(codeLists.All(x => x.ZZD_StartDate == new DateTime(2024, 1, 1)));
		}

		void AssertResult_SafeDb_RefCusCodeListLanguage_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var codeLists = new List<RefCusCodeList>();
			var sql = @"select ZZD_Code, ZXA_ZX6_NKLanguage from RefCusCodeListLanguage join RefCusCodeList on ZXA_ZZD_CodeList = ZZD_PK
								where ZZD_ZZK_NKCodeType='EMCPK' and ZZD_ZZZ_NKDataGrouping='EUN' and ZZD_Code like 'A%'";
			safeCommand.CommandText = sql;
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var codeList = new RefCusCodeList();
					var codeListLanguage = new RefCusCodeListLanguage();
					codeList.ZZD_Code = reader.GetString(0);
					codeListLanguage.ZXA_ZX6_NKLanguage = reader.GetString(1);
					codeList.RefCusCodeListLanguages = new[] { codeListLanguage };
					codeLists.Add(codeList);
				}
			}
			Assert.AreEqual(4, codeLists.Count);
			Assert.True(codeLists.All(x => x.RefCusCodeListLanguages.All(y => y.ZXA_ZX6_NKLanguage == "DE")));
		}
	}
}
