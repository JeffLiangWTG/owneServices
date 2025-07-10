using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	class Test_ExpireAndAutoExpireWithSameProperties : IXmlProcessIntegrationTest
	{
		public string[] FileNames => ["TestFiles\\TestExpireAndAutoExpire1.xml", "TestFiles\\TestExpireAndAutoExpire2_WithSameProperties.xml"];

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1, AssertResult_AfterProcessingXml2];

		public string TestDescription => "Test auto expire";

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Console.WriteLine("Start Preparing Data");

			var stagingSql = @"
insert DataSourceInformation (DSI_PK,DSI_SubSource,DSI_EnableAutoExpiration)
values(newid(), 'TestExpireAndAutoExpire',1)
";
			stagingCommand.CommandText = stagingSql;
			stagingCommand.ExecuteNonQuery();

			var safeSql = @"
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (NEWID(), 23, 'RefCusTariff', 'RefCusTariff','ZZ1',0);

insert into [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
values (newid(), 'EUN', 'European Union', null);

insert into [dbo].[RefCusRateType] (ZZR_PK,ZZR_RateType,ZZR_Description,ZZR_IsPayable,ZZR_ZZZ_NKDataGrouping,ZZR_RX_NKFormulaCurrency,ZZR_CustomsValueFormula,ZZR_IsExport)
values ('D2E07963-932F-4BB4-A2E1-9C90F8848C23','DTY', 'Duty',1, 'EUN', '', 'CV',0),
('775FEF08-1C97-404D-87D8-17509841D8F3','CVD', 'Countervailing Duty',1, 'EUN', 'EUR', 'CV',0);

insert into [dbo].[RefCusTariffType] (ZZI_PK,ZZI_TariffType,ZZI_Description,ZZI_ZZ9_NKNomenclatureGroupType,ZZI_ZZZ_NKDataGrouping,ZZI_ZZR_RateType,ZZI_HasFormulaSpecificQuestions)
values ('FC6DB681-F5F4-4B2F-9020-84AFF4F8BE98', 'IMP', 'Import Tariff', '', 'EUN', 'D2E07963-932F-4BB4-A2E1-9C90F8848C23', 0);

insert into [dbo].[RefCusTradeGroup] (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
values ('73DD6854-4168-4D35-B9B9-3F8F99DB0108', 'AD',' ERGA OMNES', '1958-01-01 00:00:00', '2079-06-06 23:59:00','EUN'),
(newid(), '1011',' ERGA OMNES1', '1958-01-01 00:00:00', '2079-06-06 23:59:00','EUN');

insert into [dbo].[RefCusRateCode] (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description, ZY1_InternalUse)
values (newid(), 'A00', 'D2E07963-932F-4BB4-A2E1-9C90F8848C23', 'Customs duties on industrial products', 0),
(newid(), 'EAR', 'D2E07963-932F-4BB4-A2E1-9C90F8848C23', 'Customs duties on industrial products', 0),
(newid(), 'A40', '775FEF08-1C97-404D-87D8-17509841D8F3', 'Schedule 1 Part 1', 0);
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
				AssertResult_SafeDb_RefCusRateUOM_AfterProcessingXml1(safeCommand);
			});
		}

		void AssertResult_AfterProcessingXml2(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(stagingCommand);
				AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml2(stagingCommand);
				AssertResult_StagingDb_RefCusRate_AfterProcessingXml2(stagingCommand);
				AssertResult_StagingDb_RefCusApplicability_AfterProcessingXml2(stagingCommand);
				AssertResult_StagingDb_RefCusRateUOM_AfterProcessingXml2(stagingCommand);

				var tariffs = AssertResult_SafeDb_RefCusTariff_AfterProcessingXml2(safeCommand);
				var rate = AssertResult_SafeDb_RefCusRate_AfterProcessingXml2(tariffs, safeCommand);
				AssertResult_SafeDb_RefCusApplicability_AfterProcessingXml2(tariffs, safeCommand);
				AssertResult_SafeDb_RefCusRateUOM_AfterProcessingXml2(rate, safeCommand);
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
			Assert.That(dataProcessingResults, Has.Count.EqualTo(13));
			Assert.That(dataProcessingResults.All(x => x.DPR_SubSource == "TestExpireAndAutoExpire"));
			Assert.That(dataProcessingResults.All(x => x.DPR_Status == "QUE"));
			CollectionAssert.AreEquivalent(new string[] { "ZZ1", "ZZ2", "ZZT", "ZXG" }, dataProcessingResults.Select(x => x.DPR_ParentTableCode).Distinct());
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
			CollectionAssert.AreEquivalent(new string[] { "22030", "22031", "22032" }, tariffs.Select(x => x.ZZ1_TariffCode).Distinct());
		}

		void AssertResult_SafeDb_RefCusRate_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var rates = new List<Safe.RefCusRate>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusRate";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var rate = new Safe.RefCusRate();
					rate.ZZ2_RateFormula = reader[nameof(Safe.RefCusRate.ZZ2_RateFormula)].ToString();
					rate.ZZ2_EndDate = (DateTime)reader[nameof(Safe.RefCusRate.ZZ2_EndDate)];
					rates.Add(rate);
				}
			}
			Assert.That(rates, Has.Count.EqualTo(3));
			CollectionAssert.AreEquivalent(new string[] { "2079-06-06 00:00:00" }, rates.Select(x => $"{x.ZZ2_EndDate:yyyy-MM-dd HH:mm:ss}").Distinct());
			CollectionAssert.AreEquivalent(new string[] { "43.92 * [HLT]", "47.48 * [HLT]", "50.00 * [HLT]" }, rates.Select(x => x.ZZ2_RateFormula).Distinct());
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
			CollectionAssert.AreEquivalent(new string[] { "2029-06-06 23:59:00" }, apps.Select(x => $"{x.ZZT_EndDate:yyyy-MM-dd HH:mm:ss}").Distinct());
			CollectionAssert.AreEquivalent(new string[] { "U313", "U314", "U319", "U320", "U201" }, apps.Select(x => x.ZZT_AdditionalCode).Distinct());
		}

		void AssertResult_SafeDb_RefCusRateUOM_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var uoms = new List<Safe.RefCusRateUOM>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusRateUOM";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var uom = new Safe.RefCusRateUOM();
					uom.ZXG_UOM = reader[nameof(Safe.RefCusRateUOM.ZXG_UOM)].ToString();
					uoms.Add(uom);
				}
			}
			Assert.That(uoms, Has.Count.EqualTo(2));
			CollectionAssert.AreEquivalent(new string[] { "HLT", "MIL" }, uoms.Select(x => x.ZXG_UOM).Distinct());
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
			Assert.That(dataProcessingResults, Has.Count.EqualTo(13));
			CollectionAssert.AreEquivalent(new string[] {"ZXG_QUE", "ZZ1_PRS", "ZZ1_QUE", "ZZ2_PRS", "ZZ2_QUE", "ZZT_PRS", "ZZT_QUE", "ZXG_PRS"},
				dataProcessingResults.Select(x => $"{x.DPR_ParentTableCode}_{x.DPR_Status}").Distinct());

		}

		void AssertResult_StagingDb_RefCusRate_AfterProcessingXml2(IDbCommand stagingCommand)
		{
			var rates = new List<RefCusRate>();
			stagingCommand.CommandText = "SELECT * FROM dbo.RefCusRate";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var rate = new RefCusRate();
					rate.ZZ2_RateFormula = reader[nameof(RefCusRate.ZZ2_RateFormula)].ToString();
					rate.ZZ2_EndDate = (DateTime)reader[nameof(RefCusRate.ZZ2_EndDate)];
					rates.Add(rate);
				}
			}
			Assert.That(rates, Has.Count.EqualTo(4));
			CollectionAssert.AreEquivalent(new string[] { "43.92 * [HLT]", "47.48 * [HLT]", "50.00 * [HLT]" }, rates.Select(x => x.ZZ2_RateFormula).Distinct());
			CollectionAssert.AreEquivalent(new DateTime[] { new DateTime(2029, 6, 6, 23, 59, 0, DateTimeKind.Utc) }, rates.Select(x => x.ZZ2_EndDate).Distinct());
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
			Assert.That(apps, Has.Count.EqualTo(6));
			CollectionAssert.AreEquivalent(new string[] { "U313", "U314", "U319", "U320", "U201" }, apps.Select(x => x.ZZT_AdditionalCode).Distinct());
			CollectionAssert.AreEquivalent(new DateTime[] { new DateTime(2029, 6, 6, 23, 59, 0, DateTimeKind.Utc) }, apps.Select(x => x.ZZT_EndDate).Distinct());
		}

		void AssertResult_StagingDb_RefCusRateUOM_AfterProcessingXml2(IDbCommand stagingCommand)
		{
			var uoms = new List<RefCusRateUOM>();
			stagingCommand.CommandText = "SELECT * FROM dbo.RefCusRateUOM";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var uom = new RefCusRateUOM();
					uom.ZXG_UOM = reader[nameof(RefCusRateUOM.ZXG_UOM)].ToString();
					uoms.Add(uom);
				}
			}
			Assert.That(uoms, Has.Count.EqualTo(3));
			CollectionAssert.AreEquivalent(new string[] { "MIL", "HLT" }, uoms.Select(x => x.ZXG_UOM).Distinct());
		}

		Safe.RefCusTariff[] AssertResult_SafeDb_RefCusTariff_AfterProcessingXml2(IDbCommand safeCommand)
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
			CollectionAssert.AreEquivalent(new string[] { "22030", "22031", "22032" }, tariffs.Select(x => x.ZZ1_TariffCode).Distinct());
			return tariffs.ToArray();
		}

		Safe.RefCusRate[] AssertResult_SafeDb_RefCusRate_AfterProcessingXml2(Safe.RefCusTariff[] tariffs, IDbCommand safeCommand)
		{
			var rates = new List<Safe.RefCusRate>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusRate";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var rate = new Safe.RefCusRate();
					rate.ZZ2_PK = (Guid)reader[nameof(Safe.RefCusRate.ZZ2_PK)];
					rate.ZZ2_RateFormula = reader[nameof(Safe.RefCusRate.ZZ2_RateFormula)].ToString();
					rate.ZZ2_EndDate = (DateTime)reader[nameof(Safe.RefCusRate.ZZ2_EndDate)];
					rate.ZZ2_ZZ1_Tariff = (Guid)reader[nameof(Safe.RefCusRate.ZZ2_ZZ1_Tariff)];
					rates.Add(rate);
				}
			}
			Assert.That(rates, Has.Count.EqualTo(3));
			CollectionAssert.AreEquivalent(new string[] { "43.92 * [HLT]", "47.48 * [HLT]" , "50.00 * [HLT]" }, rates.Select(x => x.ZZ2_RateFormula).Distinct());
			var rateRelatedTariff_22030 = rates.FirstOrDefault(x => x.ZZ2_ZZ1_Tariff == tariffs.First(y => y.ZZ1_TariffCode == "22030").ZZ1_PK);
			Assert.That(rateRelatedTariff_22030, Is.Not.Null);
			Assert.That(rateRelatedTariff_22030.ZZ2_EndDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), Is.EqualTo("2079-06-06 00:00:00"));

			var rateRelatedTariff_22031 = rates.FirstOrDefault(x => x.ZZ2_ZZ1_Tariff == tariffs.First(y => y.ZZ1_TariffCode == "22031").ZZ1_PK);
			Assert.That(rateRelatedTariff_22031, Is.Not.Null);
			Assert.That(rateRelatedTariff_22031.ZZ2_EndDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), Is.EqualTo("2022-03-18 13:30:25"));
			
			return rates.ToArray();
		}

		void AssertResult_SafeDb_RefCusApplicability_AfterProcessingXml2(Safe.RefCusTariff[] tariffs, IDbCommand safeCommand)
		{
			var apps = new List<Safe.RefCusApplicability>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusApplicability";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var app = new Safe.RefCusApplicability();
					app.ZZT_AdditionalCode = reader[nameof(Safe.RefCusApplicability.ZZT_AdditionalCode)].ToString();
					app.ZZT_EndDate = (DateTime)reader[nameof(RefCusApplicability.ZZT_EndDate)];
					app.ZZT_DataSetPK = (Guid)reader[nameof(Safe.RefCusApplicability.ZZT_DataSetPK)];
					apps.Add(app);
				}
			}
			Assert.That(apps, Has.Count.EqualTo(5));
			CollectionAssert.AreEquivalent(new string[] { "U313", "U314", "U319", "U320", "U201" }, apps.Select(x => x.ZZT_AdditionalCode).Distinct());
			var appAvailiby = apps.FirstOrDefault(x => x.ZZT_DataSetPK == tariffs.FirstOrDefault(y => y.ZZ1_TariffCode == "22030").ZZ1_PK && x.ZZT_AdditionalCode == "U313");
			Assert.That(appAvailiby, Is.Not.Null);
			Assert.That(appAvailiby.ZZT_EndDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), Is.EqualTo("2029-06-06 23:59:00"));

			var appOthers = apps.Except(new Safe.RefCusApplicability[] { appAvailiby });
			Assert.That(appOthers.Count, Is.EqualTo(4));
			Assert.That(appOthers.All(x => x.ZZT_EndDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) == "2022-03-18 13:30:00"));
		}

		void AssertResult_SafeDb_RefCusRateUOM_AfterProcessingXml2(Safe.RefCusRate[] rates, IDbCommand safeCommand)
		{
			var uoms = new List<Safe.RefCusRateUOM>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusRateUOM";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var uom = new Safe.RefCusRateUOM();
					uom.ZXG_UOM = reader[nameof(Safe.RefCusRateUOM.ZXG_UOM)].ToString();
					uom.ZXG_ZZ2_Rate = (Guid)reader[nameof(Safe.RefCusRateUOM.ZXG_ZZ2_Rate)];
					uoms.Add(uom);
				}
			}
			Assert.That(uoms, Has.Count.EqualTo(2));
			CollectionAssert.AreEquivalent(new[] { "HLT", "MIL" }, uoms.Select(x => x.ZXG_UOM).Distinct());
			Assert.That(uoms.First(x => x.ZXG_UOM == "MIL").ZXG_ZZ2_Rate == rates.First(x => x.ZZ2_RateFormula == "43.92 * [HLT]").ZZ2_PK);
		}
	}
}
