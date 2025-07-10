using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test.IntegrationTest
{
	class Test_Expire_SetDPRStatusCorrectly : IXmlProcessIntegrationTest
	{
		public string[] FileNames => ["TestFiles\\Test_TWTariffData_1.xml", "TestFiles\\Test_TWTariffData_2.xml"];

		public string TestDescription => "Record relative DPR status will change after it's been expired, thus it will not be auto-expired in future.";

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1, AssertResult_AfterProcessingXml2];

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Console.WriteLine("Start Preparing Data");

			var stagingSql = @"
INSERT INTO DataSourceInformation (DSI_PK,DSI_SubSource,DSI_EnableAutoExpiration)
VALUES (NEWID(), 'TW Tariff', 1)
";
			stagingCommand.CommandText = stagingSql;
			stagingCommand.ExecuteNonQuery();

			var safeSql = @"
INSERT INTO RefDataSetInformation (RDS_PK, RDS_DataSetId, RDS_TableName, RDS_DataSetName, RDS_DataSetTableCode, RDS_PriorityLevel)
VALUES (NEWID(), 23, 'RefCusTariff', 'RefCusTariff', 'ZZ1', 0);

INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_NKGrouping)
VALUES (NEWID(), 'TW', 'TAIWAN', NULL);

INSERT INTO RefCusTariffType (ZZI_TariffType, ZZI_Description, ZZI_ZZ9_NKNomenclatureGroupType, ZZI_ZZZ_NKDataGrouping, ZZI_ZZR_RateType, ZZI_HasFormulaSpecificQuestions)
VALUES ('HSN', 'Taiwan Harmonized Tariff', 'TW', 'TW', NULL, 0)

INSERT INTO RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_IsPayable, ZZR_ZZZ_NKDataGrouping, ZZR_RX_NKFormulaCurrency, ZZR_CustomsValueFormula,ZZR_IsExport)
VALUES ('CE0FC46D-443A-4D79-A080-7674EC93E598', 'DTY', 'Duty', 1, 'TW', '', 'CV', 0)

INSERT INTO RefCusRateCode (ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description, ZY1_InternalUse, ZY1_ZZZ_NKDataGrouping)
VALUES ('DTA', 'CE0FC46D-443A-4D79-A080-7674EC93E598', 'Import Duty (Ad-Valorem)', '0', 'TW')

INSERT INTO RefCusTradeGroup (ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
VALUES ('ALL', 'all', '1900-01-01 00:00:00', '2079-06-06 23:59:00', 'TW')

INSERT INTO RefCusPreference (ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
VALUES ('STD', 'Column III Rates(Standard Rates)', 'TW')
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
				AssertResult_SafeDb_RefCusRate_AfterProcessingXml1(safeCommand);
				AssertResult_SafeDb_RefCusApplicability_AfterProcessingXml1(safeCommand);
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
			Assert.That(dataProcessingResults, Has.Count.EqualTo(3));
			Assert.That(dataProcessingResults.All(x => x.DPR_Status == "QUE"));
			Assert.That(dataProcessingResults.Select(x => x.DPR_ParentTableCode).Distinct(), Is.EquivalentTo(new[] { "ZZ1", "ZZ2", "ZZT" }));
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
					tariff.ZZ1_StartDate = (DateTime)reader[nameof(Safe.RefCusTariff.ZZ1_StartDate)];
					tariff.ZZ1_EndDate = (DateTime)reader[nameof(Safe.RefCusTariff.ZZ1_EndDate)];
					tariffs.Add(tariff);
				}
			}
			Assert.That(tariffs, Has.Count.EqualTo(1));
			Assert.That(tariffs.Select(x => x.ZZ1_TariffCode).Distinct(), Is.EquivalentTo(new[] { "85045090005" }));
			Assert.That(tariffs.FirstOrDefault()?.ZZ1_StartDate.DateTime, Is.EqualTo(new DateTime(2019, 7, 1, 0, 0, 0)));
			Assert.That(tariffs.FirstOrDefault()?.ZZ1_EndDate.DateTime, Is.EqualTo(new DateTime(2079, 6, 6, 23, 59, 0)));
		}

		void AssertResult_SafeDb_RefCusRate_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var cusRates = new List<Safe.RefCusRate>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusRate";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var cusRate = new Safe.RefCusRate();
					cusRate.ZZ2_EndDate = (DateTime)reader[nameof(Safe.RefCusRate.ZZ2_EndDate)];
					cusRates.Add(cusRate);
				}
			}
			Assert.That(cusRates, Has.Count.EqualTo(1));
			Assert.That(cusRates.Select(x => $"{x.ZZ2_EndDate:yyyy-MM-dd HH:mm:ss}").Distinct(), Is.EquivalentTo(new[] { "2079-06-06 00:00:00" }));
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
					app.ZZT_EndDate = (DateTime)reader[nameof(Safe.RefCusApplicability.ZZT_EndDate)];
					apps.Add(app);
				}
			}
			Assert.That(apps, Has.Count.EqualTo(1));
			Assert.That(apps.Select(x => $"{x.ZZT_EndDate:yyyy-MM-dd HH:mm:ss}").Distinct(), Is.EquivalentTo(new[] { "2079-06-06 23:59:00" }));
		}

		void AssertResult_AfterProcessingXml2(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(stagingCommand);
				AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml2(stagingCommand);

				AssertResult_SafeDb_RefCusTariff_AfterProcessingXml2(safeCommand);
				AssertResult_SafeDb_RefCusRate_AfterProcessingXml2(safeCommand);
				AssertResult_SafeDb_RefCusApplicability_AfterProcessingXml2(safeCommand);
			});
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
			Assert.That(dataProcessingResults, Has.Count.EqualTo(7));
			Assert.That(dataProcessingResults.Count(x => x.DPR_Status.Equals("PRS", StringComparison.OrdinalIgnoreCase)), Is.EqualTo(3));
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
					tariff.ZZ1_TariffCode = reader[nameof(Safe.RefCusTariff.ZZ1_TariffCode)].ToString();
					tariff.ZZ1_StartDate = (DateTime)reader[nameof(Safe.RefCusTariff.ZZ1_StartDate)];
					tariff.ZZ1_EndDate = (DateTime)reader[nameof(Safe.RefCusTariff.ZZ1_EndDate)];
					tariffs.Add(tariff);
				}
			}
			Assert.That(tariffs, Has.Count.EqualTo(2));
			Assert.That(tariffs.Select(x => x.ZZ1_TariffCode).Distinct(), Is.EquivalentTo(new[] { "85045090005" }));
			Assert.That(tariffs.Select(x => $"{x.ZZ1_EndDate:yyyy-MM-dd HH:mm:ss}").Distinct(), Is.EquivalentTo(new[] { "2079-06-06 23:59:00", "2025-01-14 23:59:00" }));
			Assert.That(tariffs.Select(x => $"{x.ZZ1_StartDate:yyyy-MM-dd HH:mm:ss}").Distinct(), Is.EquivalentTo(new[] { "2019-07-01 00:00:00", "2025-01-15 00:00:00" }));
		}
		
		void AssertResult_SafeDb_RefCusRate_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var cusRates = new List<Safe.RefCusRate>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusRate";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var cusRate = new Safe.RefCusRate();
					cusRate.ZZ2_StartDate = (DateTime)reader[nameof(RefCusRate.ZZ2_StartDate)];
					cusRate.ZZ2_EndDate = (DateTime)reader[nameof(RefCusRate.ZZ2_EndDate)];
					cusRates.Add(cusRate);
				}
			}
			Assert.That(cusRates, Has.Count.EqualTo(2));
			Assert.That(cusRates.Select(x => $"{x.ZZ2_EndDate:yyyy-MM-dd HH:mm:ss}").Distinct(), Is.EquivalentTo(new[] { "2079-06-06 00:00:00", "2025-01-10 13:21:00" }));
			Assert.That(cusRates.Select(x => $"{x.ZZ2_StartDate:yyyy-MM-dd HH:mm:ss}").Distinct(), Is.EquivalentTo(new[] { "1900-01-01 00:00:00" }));
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
					app.ZZT_StartDate = (DateTime)reader[nameof(RefCusApplicability.ZZT_StartDate)];
					app.ZZT_EndDate = (DateTime)reader[nameof(RefCusApplicability.ZZT_EndDate)];
					apps.Add(app);
				}
			}
			Assert.That(apps, Has.Count.EqualTo(2));
			Assert.That(apps.Select(x => $"{x.ZZT_StartDate:yyyy-MM-dd HH:mm:ss}").Distinct(), Is.EquivalentTo(new[] { "2019-07-01 00:00:00", "2025-01-15 00:00:00" }));
			Assert.That(apps.Select(x => $"{x.ZZT_EndDate:yyyy-MM-dd HH:mm:ss}").Distinct(), Is.EquivalentTo(new[] { "2025-01-10 13:21:00", "2079-06-06 23:59:00" }));
		}
	}
}
