using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	class Test_Consolidate_Cond_App_MultiCondValue_Lang_AutoExpiration2 : IXmlProcessIntegrationTest
	{
		public string[] FileNames => ["TestFiles\\Test_Cond_App_WithMultiValAndLang9.xml", "TestFiles\\Test_Cond_App_WithMultiValAndLang10.xml"];

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1, AssertResult_AfterProcessingXml2];

		public string TestDescription => "Test consilidating RefCusCondition(multiple, identical)/RefCusApplicability with multiple UOM when auto expiration is on";

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Console.WriteLine("Start Preparing Data");

			var statgingSql = @"insert [dbo].[DataSourceInformation] (DSI_PK,DSI_SubSource,DSI_EnableAutoExpiration) values(newid(),'Test_Cond_App_WithMultiValAndLang',1 )";
			stagingCommand.CommandText = statgingSql;
			stagingCommand.ExecuteNonQuery();

			var safeSql = @"
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (NEWID(), 23, 'RefCusTariff', 'RefCusTariff','ZZ1',0);

insert into [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
values (newid(), 'EUN', 'European Union', null);

insert into [dbo].[RefCusRateType] (ZZR_PK,ZZR_RateType,ZZR_Description,ZZR_IsPayable,ZZR_ZZZ_NKDataGrouping,ZZR_RX_NKFormulaCurrency,ZZR_CustomsValueFormula,ZZR_IsExport)
values ('D2E07963-932F-4BB4-A2E1-9C90F8848C23','DTY', 'Duty',1, 'EUN', '', 'CV',0);

insert into [dbo].[RefCusTariffType] (ZZI_PK,ZZI_TariffType,ZZI_Description,ZZI_ZZ9_NKNomenclatureGroupType,ZZI_ZZZ_NKDataGrouping,ZZI_ZZR_RateType,ZZI_HasFormulaSpecificQuestions)
values ('FC6DB681-F5F4-4B2F-9020-84AFF4F8BE98', 'IMP', 'Import Tariff', '', 'EUN', 'D2E07963-932F-4BB4-A2E1-9C90F8848C23', 0);

insert into [dbo].[RefCusTradeGroup] (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
values ('73DD6854-4168-4D35-B9B9-3F8F99DB0108', 'AD',' ERGA OMNES', '1958-01-01 00:00:00', '2079-06-06 23:59:00','EUN'),
(newid(), '1011',' ERGA OMNES1', '1958-01-01 00:00:00', '2079-06-06 23:59:00','EUN');

insert into [dbo].[RefCusConditionType] (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
values (newid(), 'CTRL', '728', 'Import control on luxury goods', 'EUN');

insert into [dbo].[RefCusConditionValueType] (ZX4_PK, ZX4_ValueType, ZX4_Description, ZX4_IsFormula, ZX4_ZZZ_NKDataGrouping)
values(newid(), 'Q', 'Presentation of an endorsed certificate/licence', 0, 'EUN');

insert into [dbo].[RefLanguageType] (ZX6_PK, ZX6_Language, ZX6_Description)
values (newid(), 'ITL', 'Italian'),
(newid(), 'GRM', 'German'),
(newid(), 'FRN', 'French'),
(newid(), 'EN', 'English'),
(newID(), 'EL', 'Greek'),
(newID(), 'DIV', 'Divehi');
";
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();

			Console.WriteLine("Preparing Data Successfully");
		}

		void AssertResult_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(stagingCommand);
				AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml1(stagingCommand);

				AssertResult_SafeDb_RefCusCondition_AfterProcessingXml1(safeCommand);
				AssertResult_SafeDb_RefCusApplicability_AfterProcessingXml1(safeCommand);
				AssertResult_SafeDb_RefCusConditionValue_AfterProcessingXml1(safeCommand);
				AssertResult_SafeDb_RefCusConditionLanguage_AfterProcessingXml1(safeCommand);
			});
		}

		void AssertResult_AfterProcessingXml2(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(stagingCommand);
				AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml2(stagingCommand);

				AssertResult_SafeDb_RefCusCondition_AfterProcessingXml2(safeCommand);
				AssertResult_SafeDb_RefCusApplicability_AfterProcessingXml2(safeCommand);
				AssertResult_SafeDb_RefCusConditionValue_AfterProcessingXml2(safeCommand);
				AssertResult_SafeDb_RefCusConditionLanguage_AfterProcessingXml2(safeCommand);
			});
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var sourceData = new SourceData();
			stagingCommand.CommandText = "SELECT * FROM dbo.SourceData";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					sourceData.SDA_PK = (Guid)reader[nameof(SourceData.SDA_PK)];
					sourceData.SDA_Status = reader[nameof(SourceData.SDA_Status)].ToString();
				}
			}
			Assert.That(sourceData.IsSourceDataMergedStatus());
		}

		void AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var dataProcessingResults = new List<DataProcessingResult>();
			stagingCommand.CommandText = "SELECT * FROM dbo.DataProcessingResult";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var dataProcessingResult = new DataProcessingResult();
					dataProcessingResult.DPR_ParentTableCode = reader[nameof(DataProcessingResult.DPR_ParentTableCode)].ToString();
					dataProcessingResult.DPR_Status = reader[nameof(DataProcessingResult.DPR_Status)].ToString();
					dataProcessingResults.Add(dataProcessingResult);
				}
			}
			Assert.That(dataProcessingResults, Has.Count.EqualTo(5));
			Assert.That(dataProcessingResults.All(x => x.DPR_Status == "QUE"));
			Assert.That(dataProcessingResults.Select(x => x.DPR_ParentTableCode).Distinct(), Is.EquivalentTo(new string[] { "ZZ1", "ZX1", "ZZT", "ZX3", "ZXJ" }));
		}

		void AssertResult_SafeDb_RefCusCondition_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var conds = new List<Safe.RefCusCondition>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusCondition";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var cond = new Safe.RefCusCondition();
					cond.ZX1_Comment = reader[nameof(Safe.RefCusCondition.ZX1_Comment)].ToString();
					conds.Add(cond);
				}
			}
			Assert.That(conds, Has.Count.EqualTo(1));
			Assert.That(conds.Select(x => x.ZX1_Comment).Distinct(), Is.EquivalentTo(new string[] { "Cond1" }));
		}

		void AssertResult_SafeDb_RefCusApplicability_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var apps = new List<Safe.RefCusApplicability>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusApplicability";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var app = new Safe.RefCusApplicability();
					app.ZZT_AdditionalCode = reader[nameof(Safe.RefCusApplicability.ZZT_AdditionalCode)].ToString();
					apps.Add(app);
				}
			}
			Assert.That(apps, Has.Count.EqualTo(1));
			Assert.That(apps.Select(x => x.ZZT_AdditionalCode).Distinct(), Is.EquivalentTo(new string[] { "U313" }));
		}

		void AssertResult_SafeDb_RefCusConditionValue_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var vals = new List<Safe.RefCusConditionValue>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusConditionValue";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var val = new Safe.RefCusConditionValue();
					val.ZX3_Value = reader[nameof(Safe.RefCusConditionValue.ZX3_Value)].ToString();
					vals.Add(val);
				}
			}
			Assert.That(vals, Has.Count.EqualTo(1));
			Assert.That(vals.Select(x => x.ZX3_Value).Distinct(), Is.EquivalentTo(new string[] { "C001" }));
		}

		void AssertResult_SafeDb_RefCusConditionLanguage_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var langs = new List<Safe.RefCusConditionLanguage>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusConditionLanguage";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var lang = new Safe.RefCusConditionLanguage();
					lang.ZXJ_ZX6_NKLanguage = reader[nameof(Safe.RefCusConditionLanguage.ZXJ_ZX6_NKLanguage)].ToString();
					langs.Add(lang);
				}
			}
			Assert.That(langs, Has.Count.EqualTo(1));
			Assert.That(langs.Select(x => x.ZXJ_ZX6_NKLanguage).Distinct(), Is.EquivalentTo(new string[] { "ITL" }));
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(IDbCommand stagingCommand)
		{
			var sourceDatas = new List<SourceData>();
			stagingCommand.CommandText = "SELECT * FROM dbo.SourceData";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var sourceData = new SourceData();
					sourceData.SDA_PK = (Guid)reader[nameof(SourceData.SDA_PK)];
					sourceData.SDA_Status = reader[nameof(SourceData.SDA_Status)].ToString();
					sourceDatas.Add(sourceData);
				}
			}
			Assert.That(sourceDatas, Has.Count.EqualTo(2));
			Assert.That(sourceDatas.All(x => x.IsSourceDataMergedStatus()));
		}

		void AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml2(IDbCommand stagingCommand)
		{
			var dataProcessingResults = new List<DataProcessingResult>();
			stagingCommand.CommandText = "SELECT * FROM dbo.DataProcessingResult";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var dataProcessingResult = new DataProcessingResult();
					dataProcessingResult.DPR_ParentTableCode = reader[nameof(DataProcessingResult.DPR_ParentTableCode)].ToString();
					dataProcessingResult.DPR_Status = reader[nameof(DataProcessingResult.DPR_Status)].ToString();
					dataProcessingResults.Add(dataProcessingResult);
				}
			}
			Assert.That(dataProcessingResults, Has.Count.EqualTo(9));
			Assert.That(dataProcessingResults.Select(x => $"{x.DPR_ParentTableCode}_{x.DPR_Status}").Distinct(), Is.EquivalentTo(new string[] {"ZZ1_QUE", "ZZT_QUE", "ZX1_QUE", "ZX3_QUE", "ZXJ_QUE" }));
		}

		void AssertResult_SafeDb_RefCusCondition_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var conds = new List<Safe.RefCusCondition>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusCondition";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var cond = new Safe.RefCusCondition();
					cond.ZX1_Comment = reader[nameof(Safe.RefCusCondition.ZX1_Comment)].ToString();
					conds.Add(cond);
				}
			}
			Assert.That(conds, Has.Count.EqualTo(2));
			Assert.That(conds.Select(x => $"{x.ZX1_Comment}").Distinct(), Is.EquivalentTo(new string[] { "Cond3" }));
		}

		void AssertResult_SafeDb_RefCusApplicability_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var apps = new List<Safe.RefCusApplicability>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusApplicability";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var app = new Safe.RefCusApplicability();
					app.ZZT_AdditionalCode = reader[nameof(Safe.RefCusApplicability.ZZT_AdditionalCode)].ToString();
					apps.Add(app);
				}
			}
			Assert.That(apps, Has.Count.EqualTo(2));
			Assert.That(apps.Select(x => $"{x.ZZT_AdditionalCode}"), Is.EquivalentTo(new string[] { "U313", "U314" }));
		}

		void AssertResult_SafeDb_RefCusConditionValue_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var vals = new List<Safe.RefCusConditionValue>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusConditionValue";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var val = new Safe.RefCusConditionValue();
					val.ZX3_Value = reader[nameof(Safe.RefCusConditionValue.ZX3_Value)].ToString();
					vals.Add(val);
				}
			}
			Assert.That(vals, Has.Count.EqualTo(2));
			Assert.That(vals.Select(x => x.ZX3_Value).Distinct(), Is.EquivalentTo(new string[] { "C003", "C004" }));
		}

		void AssertResult_SafeDb_RefCusConditionLanguage_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var langs = new List<Safe.RefCusConditionLanguage>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusConditionLanguage";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var lang = new Safe.RefCusConditionLanguage();
					lang.ZXJ_ZX6_NKLanguage = reader[nameof(Safe.RefCusConditionLanguage.ZXJ_ZX6_NKLanguage)].ToString();
					langs.Add(lang);
				}
			}
			Assert.That(langs, Has.Count.EqualTo(2));
			Assert.That(langs.Select(x => x.ZXJ_ZX6_NKLanguage).Distinct(), Is.EquivalentTo(new string[] { "FRN", "EN" }));
		}
	}
}
