using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test;

class Test_SetExpirableAncestorPK_Correctly : IXmlProcessIntegrationTest
{
	public string[] FileNames => ["TestFiles\\Test_SetExpirableAncestorPK.xml"];

	public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml];

	public string TestDescription => "Test Set ExpirableAncestorPK correctly with NonPersistentObjectTransformer feature";

	public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
	{
		Console.WriteLine("Start Preparing Data");

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

insert into [dbo].[RefCusRateCode] (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description, ZY1_InternalUse)
values (newid(), 'A00', 'D2E07963-932F-4BB4-A2E1-9C90F8848C23', 'Customs duties on industrial products', 0),
 (newid(), 'EAR', 'D2E07963-932F-4BB4-A2E1-9C90F8848C23', 'Customs duties on industrial products', 0);

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

	void AssertResult_AfterProcessingXml(IDbCommand stagingCommand, IDbCommand safeCommand)
	{
		Assert.Multiple(() =>
		{
			AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml(stagingCommand);

			AssertResult_SafeDb_RefCusTariff_AfterProcessingXml(safeCommand);
			AssertResult_SafeDb_RefCusRate_AfterProcessingXml1(safeCommand);
			AssertResult_SafeDb_RefCusCondition_AfterProcessingXml(safeCommand);
			AssertResult_SafeDb_RefCusApplicability_AfterProcessingXml(safeCommand);
			AssertResult_SafeDb_RefCusRateUOM_AfterProcessingXml(safeCommand);
			AssertResult_SafeDb_RefCusConditionValue_AfterProcessingXml(safeCommand);
			AssertResult_SafeDb_RefCusConditionLanguage_AfterProcessingXml(safeCommand);

			AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml(stagingCommand);
		});
	}

	void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml(IDbCommand stagingCommand)
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

	void AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml(IDbCommand stagingCommand)
	{
		var dataProcessingResults = new List<DataProcessingResult>();
		stagingCommand.CommandText = "SELECT * FROM dbo.DataProcessingResult";
		using (var reader = stagingCommand.ExecuteReader())
		{
			while (reader.Read())
			{
				var dataProcessingResult = new DataProcessingResult
				{
					DPR_SubSource = reader[nameof(DataProcessingResult.DPR_SubSource)].ToString(),
					DPR_ParentTableCode = reader[nameof(DataProcessingResult.DPR_ParentTableCode)].ToString(),
					DPR_Status = reader[nameof(DataProcessingResult.DPR_Status)].ToString(),
					DPR_ParentPK = (Guid)reader[nameof(DataProcessingResult.DPR_ParentPK)]
				};
				if (reader[nameof(DataProcessingResult.DPR_ExpirableAncestorPK)] != DBNull.Value)
				{
					dataProcessingResult.DPR_ExpirableAncestorPK = (Guid)reader[nameof(DataProcessingResult.DPR_ExpirableAncestorPK)];
				}
				dataProcessingResults.Add(dataProcessingResult);
			}
		}
		Assert.That(dataProcessingResults, Has.Count.EqualTo(8));
		var tariffDPR = dataProcessingResults.First(x => x.DPR_ParentPK == tariff.ZZ1_PK);
		Assert.That(tariffDPR.DPR_ExpirableAncestorPK, Is.Null);
		var rateDPR = dataProcessingResults.First(x => x.DPR_ParentPK == rate.ZZ2_PK);
		Assert.That(rateDPR.DPR_ExpirableAncestorPK, Is.EqualTo(tariff.ZZ1_PK));
		var conditionDPR = dataProcessingResults.First(x => x.DPR_ParentPK == condition.ZX1_PK);
		Assert.That(conditionDPR.DPR_ExpirableAncestorPK, Is.EqualTo(tariff.ZZ1_PK));
		var app1DPR = dataProcessingResults.First(x => x.DPR_ParentPK == conditionApp.ZZT_PK);
		Assert.That(app1DPR.DPR_ExpirableAncestorPK, Is.EqualTo(condition.ZX1_PK));
		var app2DPR = dataProcessingResults.First(x => x.DPR_ParentPK == rateApp.ZZT_PK);
		Assert.That(app2DPR.DPR_ExpirableAncestorPK, Is.EqualTo(rate.ZZ2_PK));
		var rateUOMDPR = dataProcessingResults.First(x => x.DPR_ParentPK == rateUOM.ZXG_PK);
		Assert.That(rateUOMDPR.DPR_ExpirableAncestorPK, Is.EqualTo(rate.ZZ2_PK));
		var conditionLangDPR = dataProcessingResults.First(x => x.DPR_ParentPK == conditionLang.ZXJ_PK);
		Assert.That(conditionLangDPR.DPR_ExpirableAncestorPK, Is.EqualTo(condition.ZX1_PK));
		var conditionValueDPR = dataProcessingResults.First(x => x.DPR_ParentPK == conditionValue.ZX3_PK);
		Assert.That(conditionValueDPR.DPR_ExpirableAncestorPK, Is.EqualTo(condition.ZX1_PK));
	}

	void AssertResult_SafeDb_RefCusTariff_AfterProcessingXml(IDbCommand safeCommand)
	{
		var tariffs = new List<Safe.RefCusTariff>();
		safeCommand.CommandText = "SELECT * FROM dbo.RefCusTariff";
		using (var reader = safeCommand.ExecuteReader())
		{
			while (reader.Read())
			{
				tariffs.Add(new Safe.RefCusTariff
				{
					ZZ1_PK = (Guid)reader[nameof(Safe.RefCusTariff.ZZ1_PK)],
					ZZ1_TariffCode = reader[nameof(Safe.RefCusTariff.ZZ1_TariffCode)].ToString()
				});
			}
		}
		Assert.That(tariffs, Has.Count.EqualTo(1));
		tariff = tariffs.First();
	}

	void AssertResult_SafeDb_RefCusRate_AfterProcessingXml1(IDbCommand safeCommand)
	{
		var rates = new List<Safe.RefCusRate>();
		safeCommand.CommandText = "SELECT * FROM dbo.RefCusRate";
		using (var reader = safeCommand.ExecuteReader())
		{
			while (reader.Read())
			{
				rates.Add(new Safe.RefCusRate
				{
					ZZ2_PK = (Guid)reader[nameof(Safe.RefCusRate.ZZ2_PK)],
					ZZ2_RateFormula = reader[nameof(Safe.RefCusRate.ZZ2_RateFormula)].ToString(),
					ZZ2_EndDate = (DateTime)reader[nameof(Safe.RefCusRate.ZZ2_EndDate)]
				});
			}
		}
		Assert.That(rates, Has.Count.EqualTo(1));
		rate = rates.First();
	}

	void AssertResult_SafeDb_RefCusCondition_AfterProcessingXml(IDbCommand safeCommand)
	{
		var conditions = new List<Safe.RefCusCondition>();
		safeCommand.CommandText = "SELECT * FROM dbo.RefCusCondition";
		using (var reader = safeCommand.ExecuteReader())
		{
			while (reader.Read())
			{
				conditions.Add(new Safe.RefCusCondition
				{
					ZX1_PK = (Guid)reader[nameof(Safe.RefCusCondition.ZX1_PK)],
				});
			}
		}
		Assert.That(conditions, Has.Count.EqualTo(1));
		condition = conditions.First();
	}

	void AssertResult_SafeDb_RefCusApplicability_AfterProcessingXml(IDbCommand safeCommand)
	{
		var apps = new List<Safe.RefCusApplicability>();
		safeCommand.CommandText = "SELECT * FROM dbo.RefCusApplicability";
		using (var reader = safeCommand.ExecuteReader())
		{
			while (reader.Read())
			{
				apps.Add(new Safe.RefCusApplicability
				{
					ZZT_PK = (Guid)reader[nameof(Safe.RefCusApplicability.ZZT_PK)],
					ZZT_AdditionalCode = reader[nameof(Safe.RefCusApplicability.ZZT_AdditionalCode)].ToString(),
				});
			}
		}
		Assert.That(apps, Has.Count.EqualTo(2));
		conditionApp = apps.First(x => x.ZZT_AdditionalCode == "U314");
		rateApp = apps.First(x => x.ZZT_AdditionalCode == "U313");
	}

	void AssertResult_SafeDb_RefCusRateUOM_AfterProcessingXml(IDbCommand safeCommand)
	{
		var uoms = new List<Safe.RefCusRateUOM>();
		safeCommand.CommandText = "SELECT * FROM dbo.RefCusRateUOM";
		using (var reader = safeCommand.ExecuteReader())
		{
			while (reader.Read())
			{
				uoms.Add(new Safe.RefCusRateUOM
				{
					ZXG_PK = (Guid)reader[nameof(Safe.RefCusRateUOM.ZXG_PK)],
					ZXG_UOM = reader[nameof(Safe.RefCusRateUOM.ZXG_UOM)].ToString()
				});
			}
		}
		Assert.That(uoms, Has.Count.EqualTo(1));
		rateUOM = uoms.First();
	}

	void AssertResult_SafeDb_RefCusConditionLanguage_AfterProcessingXml(IDbCommand safeCommand)
	{
		var conditionLanguages = new List<Safe.RefCusConditionLanguage>();
		safeCommand.CommandText = "SELECT * FROM dbo.RefCusConditionLanguage";
		using (var reader = safeCommand.ExecuteReader())
		{
			while (reader.Read())
			{
				conditionLanguages.Add(new Safe.RefCusConditionLanguage
				{
					ZXJ_PK = (Guid)reader[nameof(Safe.RefCusConditionLanguage.ZXJ_PK)]
				});
			}
		}
		Assert.That(conditionLanguages, Has.Count.EqualTo(1));
		conditionLang = conditionLanguages.First();
	}

	void AssertResult_SafeDb_RefCusConditionValue_AfterProcessingXml(IDbCommand safeCommand)
	{
		var conditionValues = new List<Safe.RefCusConditionValue>();
		safeCommand.CommandText = "SELECT * FROM dbo.RefCusConditionValue";
		using (var reader = safeCommand.ExecuteReader())
		{
			while (reader.Read())
			{
				conditionValues.Add(new Safe.RefCusConditionValue
				{
					ZX3_PK = (Guid)reader[nameof(Safe.RefCusConditionValue.ZX3_PK)]
				});
			}
		}
		Assert.That(conditionValues, Has.Count.EqualTo(1));
		conditionValue = conditionValues.First();
	}

	Safe.RefCusTariff tariff;
	Safe.RefCusRate rate;
	Safe.RefCusCondition condition;
	Safe.RefCusApplicability conditionApp;
	Safe.RefCusApplicability rateApp;
	Safe.RefCusRateUOM rateUOM;
	Safe.RefCusConditionLanguage conditionLang;
	Safe.RefCusConditionValue conditionValue;
}
