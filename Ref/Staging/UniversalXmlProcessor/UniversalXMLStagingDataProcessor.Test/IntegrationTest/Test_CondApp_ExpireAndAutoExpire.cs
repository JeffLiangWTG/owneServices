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
	class Test_CondApp_ExpireAndAutoExpire : IXmlProcessIntegrationTest
	{
		public string[] FileNames => ["TestFiles\\Test_CondApp_ExpireAndAutoExpire1.xml", "TestFiles\\Test_CondApp_ExpireAndAutoExpire2.xml"];

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1, AssertResult_AfterProcessingXml2];

		public string TestDescription => "Test expire(Identicallevel=HasChange) and auto expire for Condition-Applicability group";

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Console.WriteLine("Start Preparing Data");

			var stagingSql = @"
insert DataSourceInformation (DSI_PK,DSI_SubSource,DSI_EnableAutoExpiration)
values(newid(), 'Test CondApp ExpireAndAutoExpire',1)
";
			stagingCommand.CommandText = stagingSql;
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

				AssertResult_SafeDb_RefCusTariff_AfterProcessingXml1(safeCommand);
				AssertResult_SafeDb_RefCusCondition_AfterProcessingXml1(safeCommand);
				AssertResult_SafeDb_RefCusApplicability_AfterProcessingXml1(safeCommand);
			});
		}

		void AssertResult_AfterProcessingXml2(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(stagingCommand);
				AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml2(stagingCommand);
				AssertResult_StagingDb_RefCusCondition_AfterProcessingXml2(stagingCommand);
				AssertResult_StagingDb_RefCusApplicability_AfterProcessingXml2(stagingCommand);

				AssertResult_SafeDb_RefCusTariff_AfterProcessingXml2(safeCommand);
				AssertResult_SafeDb_RefCusCondition_AfterProcessingXml2(safeCommand);
				AssertResult_SafeDb_RefCusApplicability_AfterProcessingXml2(safeCommand);
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
					dataProcessingResult.DPR_SubSource = reader[nameof(DataProcessingResult.DPR_SubSource)].ToString();
					dataProcessingResult.DPR_ParentTableCode = reader[nameof(DataProcessingResult.DPR_ParentTableCode)].ToString();
					dataProcessingResult.DPR_Status = reader[nameof(DataProcessingResult.DPR_Status)].ToString();
					dataProcessingResults.Add(dataProcessingResult);
				}
			}
			Assert.That(dataProcessingResults, Has.Count.EqualTo(11));
			Assert.That(dataProcessingResults.All(x => x.DPR_Status == "QUE"));
			Assert.That(dataProcessingResults.Select(x => x.DPR_ParentTableCode).Distinct(), Is.EquivalentTo(new string[] { "ZZ1", "ZZT", "ZX1" }));
		}

		void AssertResult_SafeDb_RefCusTariff_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var tariffs = new List<Safe.RefCusTariff>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusTariff";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var tariff = new Safe.RefCusTariff();
					tariff.ZZ1_TariffCode = reader[nameof(Safe.RefCusTariff.ZZ1_TariffCode)].ToString();
					tariffs.Add(tariff);
				}
			}
			Assert.That(tariffs, Has.Count.EqualTo(3));
			Assert.That(tariffs.Select(x => x.ZZ1_TariffCode).Distinct(), Is.EquivalentTo(new string[] { "22030", "22031", "22032" }));
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
					cond.ZX1_EndDate = (DateTime)reader[nameof(Safe.RefCusCondition.ZX1_EndDate)];
					conds.Add(cond);
				}
			}
			Assert.That(conds, Has.Count.EqualTo(3));
			Assert.That(conds.Select(x => $"{x.ZX1_EndDate:yyyy-MM-dd HH:mm:ss}").Distinct(), Is.EquivalentTo(new string[] { "2079-06-06 00:00:00" }));
			Assert.That(conds.Select(x => x.ZX1_Comment).Distinct(), Is.EquivalentTo(new string[] { "Cond1", "Cond2", "Cond3" }));
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
					app.ZZT_EndDate = (DateTime)reader[nameof(Safe.RefCusApplicability.ZZT_EndDate)];
					apps.Add(app);
				}
			}
			Assert.That(apps, Has.Count.EqualTo(5));
			Assert.That(apps.Select(x => $"{x.ZZT_EndDate:yyyy-MM-dd HH:mm:ss}").Distinct(), Is.EquivalentTo(new string[] { "2029-06-06 23:59:00" }));
			Assert.That(apps.Select(x => x.ZZT_AdditionalCode).Distinct(), Is.EquivalentTo(new string[] { "U315", "U314", "U313", "U316", "U317" }));
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
			Assert.That(dataProcessingResults, Has.Count.EqualTo(12));
			Assert.That(dataProcessingResults.Select(x => $"{x.DPR_ParentTableCode}_{x.DPR_Status}").Distinct(), Is.EquivalentTo(new string[] { "ZZT_PRS", "ZX1_PRS", "ZZ1_PRS", "ZZ1_QUE", "ZZT_QUE", "ZX1_QUE" }));

		}

		void AssertResult_StagingDb_RefCusCondition_AfterProcessingXml2(IDbCommand stagingCommand)
		{
			var conds = new List<RefCusCondition>();
			stagingCommand.CommandText = "SELECT * FROM dbo.RefCusCondition";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var cond = new RefCusCondition();
					cond.ZX1_Comment = reader[nameof(RefCusCondition.ZX1_Comment)].ToString();
					cond.ZX1_EndDate = (DateTime)reader[nameof(RefCusCondition.ZX1_EndDate)];
					conds.Add(cond);
				}
			}
			Assert.That(conds, Has.Count.EqualTo(5));
			Assert.That(conds.Select(x => x.ZX1_Comment).Distinct(), Is.EquivalentTo(new string[] { "Cond1", "Cond2", "Cond3", "Cond1_1", "Cond3_1" }));
			Assert.That(conds.Select(x => x.ZX1_EndDate).Distinct(), Is.EquivalentTo(new DateTime[] { new DateTime(2079, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2039, 1, 1, 0, 0, 0, DateTimeKind.Utc) }));
		}

		void AssertResult_StagingDb_RefCusApplicability_AfterProcessingXml2(IDbCommand stagingCommand)
		{
			var apps = new List<RefCusApplicability>();
			stagingCommand.CommandText = "SELECT * FROM dbo.RefCusApplicability";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var app = new RefCusApplicability();
					app.ZZT_AdditionalCode = reader[nameof(RefCusApplicability.ZZT_AdditionalCode)].ToString();
					app.ZZT_EndDate = (DateTime)reader[nameof(RefCusApplicability.ZZT_EndDate)];
					apps.Add(app);
				}
			}
			Assert.That(apps, Has.Count.EqualTo(7));
			Assert.That(apps.Select(x => x.ZZT_AdditionalCode).Distinct(), Is.EquivalentTo(new string[] { "U313", "U314", "U315", "U316", "U317" }));
			Assert.That(apps.Select(x => x.ZZT_EndDate).Distinct(), Is.EquivalentTo(new DateTime[] { new DateTime(2029, 6, 6, 23, 59, 0, DateTimeKind.Utc), new DateTime(2039, 6, 6, 23, 59, 0, DateTimeKind.Utc) }));
		}

		void AssertResult_SafeDb_RefCusTariff_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var tariffs = new List<Safe.RefCusTariff>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusTariff";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var tariff = new Safe.RefCusTariff();
					tariff.ZZ1_PK = (Guid)reader[nameof(Safe.RefCusTariff.ZZ1_PK)];
					tariff.ZZ1_TariffCode = reader[nameof(Safe.RefCusTariff.ZZ1_TariffCode)].ToString();
					tariffs.Add(tariff);
				}
			}
			Assert.That(tariffs, Has.Count.EqualTo(3));
			Assert.That(tariffs.Select(x => x.ZZ1_TariffCode).Distinct(), Is.EquivalentTo(new string[] { "22030", "22031", "22032" }));
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
					cond.ZX1_StartDate = (DateTime)reader[nameof(Safe.RefCusCondition.ZX1_StartDate)];
					cond.ZX1_EndDate = (DateTime)reader[nameof(Safe.RefCusCondition.ZX1_EndDate)];
					cond.ZX1_ZZ1_Tariff = (Guid)reader[nameof(Safe.RefCusCondition.ZX1_ZZ1_Tariff)];
					conds.Add(cond);
				}
			}
			Assert.That(conds, Has.Count.EqualTo(4));
			Assert.That(conds.Select(x => $"{x.ZX1_StartDate:yyyy-MM-dd HH:mm:ss}").Distinct(), Is.EquivalentTo(new string[] { "1900-01-01 00:00:00" }));
			var condResult = conds.FirstOrDefault(x => x.ZX1_Comment == "Cond1");
			Assert.That(condResult, Is.Not.Null);
			Assert.That(condResult.ZX1_EndDate.Date, Is.EqualTo(new DateTime(2022, 3, 18, 0, 0, 0)));

			condResult = conds.FirstOrDefault(x => x.ZX1_Comment == "Cond1_1");
			Assert.That(condResult, Is.Not.Null);
			Assert.That(condResult.ZX1_EndDate.Date, Is.EqualTo(new DateTime(2079, 6, 6, 0, 0, 0)));

			condResult = conds.FirstOrDefault(x => x.ZX1_Comment == "Cond2");
			Assert.That(condResult, Is.Not.Null);
			Assert.That(condResult.ZX1_EndDate.Date, Is.EqualTo(new DateTime(2022, 3, 18, 0, 0, 0)));

			condResult = conds.FirstOrDefault(x => x.ZX1_Comment == "Cond3");
			Assert.That(condResult, Is.Null);

			condResult = conds.FirstOrDefault(x => x.ZX1_Comment == "Cond3_1");
			Assert.That(condResult, Is.Not.Null);
			Assert.That(condResult.ZX1_EndDate.Date, Is.EqualTo(new DateTime(2079, 6, 6, 0, 0, 0)));
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
					app.ZZT_StartDate = (DateTime)reader[nameof(RefCusApplicability.ZZT_StartDate)];
					app.ZZT_EndDate = (DateTime)reader[nameof(RefCusApplicability.ZZT_EndDate)];
					apps.Add(app);
				}
			}
			Assert.That(apps, Has.Count.EqualTo(5));
			var appResult = apps.FirstOrDefault(x => x.ZZT_AdditionalCode == "U313");
			Assert.That(appResult, Is.Not.Null);
			Assert.That(appResult.ZZT_EndDate.Date, Is.EqualTo(new DateTime(2039, 1, 1, 0, 0, 0)));

			appResult = apps.FirstOrDefault(x => x.ZZT_AdditionalCode == "U314");
			Assert.That(appResult, Is.Not.Null);
			Assert.That(appResult.ZZT_EndDate.Date, Is.EqualTo(new DateTime(2022, 3, 18, 0, 0, 0)));

			appResult = apps.FirstOrDefault(x => x.ZZT_AdditionalCode == "U315");
			Assert.That(appResult, Is.Not.Null);
			Assert.That(appResult.ZZT_EndDate.Date, Is.EqualTo(new DateTime(2022, 3, 18, 0, 0, 0)));

			appResult = apps.FirstOrDefault(x => x.ZZT_AdditionalCode == "U316");
			Assert.That(appResult, Is.Not.Null);
			Assert.That(appResult.ZZT_EndDate.Date, Is.EqualTo(new DateTime(2022, 3, 18, 0, 0, 0)));

			appResult = apps.FirstOrDefault(x => x.ZZT_AdditionalCode == "U317");
			Assert.That(appResult, Is.Not.Null);
			Assert.That(appResult.ZZT_EndDate.Date, Is.EqualTo(new DateTime(2039, 1, 1, 0, 0, 0)));
		}
	}
}
