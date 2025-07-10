using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	sealed class DailyTariffRecordsProviderFixture
	{
		const string Export = "Export";
		const string Import = "Import";

		[Test(Description = "Daily Tariff rates updates are filtered by allowed rate MeasureType")]
		[TestCase("000", "")]
		[TestCase("277", Import)]
		[TestCase("278", Import + Export)]
		[TestCase("482", Import + Export)]
		[TestCase("484", Import + Export)]
		[TestCase("495", Import)]
		[TestCase("496", Import)]
		[TestCase("765", Import + Export)]
		[TestCase("768", Import + Export)]
		[TestCase("769", Import)]
		[TestCase("776", Import)]
		[TestCase("777", Import + Export)]
		public void GetDailyRecords_FilterByMeasureTypeId(string measureTypeId, string exportOrImport)
		{
			Assert.Multiple(() =>
			{
				var actualExportRecordsCount = dailyExportRecords.Count(x => x.MeasureTypeId == measureTypeId);
				if (exportOrImport.Contains(Export))
				{
					Assert.That(actualExportRecordsCount, Is.AtLeast(1), $"{measureTypeId} is included in Export tariffs");
				}
				else
				{
					Assert.That(actualExportRecordsCount, Is.EqualTo(0), $"{measureTypeId} is excluded from Export tariffs");
				}

				var actualImportRecordsCount = dailyImportRecords.Count(x => x.MeasureTypeId == measureTypeId);
				if (exportOrImport.Contains(Import))
				{
					Assert.That(actualImportRecordsCount, Is.AtLeast(1), $"{measureTypeId} is included in Import tariffs");
				}
				else
				{
					Assert.That(actualImportRecordsCount, Is.EqualTo(0), $"{measureTypeId} is excluded from Import tariffs");
				}
			});
		}

		[Test]
		public void GetDailyRecords_IncludesAllRecordsWithSupportedMeasureTypeId()
		{
			Assert.Multiple(() =>
			{
				Assert.That(dailyExportRecords.Count(x => x.MeasureTypeId == "278"), Is.EqualTo(2));
				Assert.That(dailyImportRecords.Count(x => x.MeasureTypeId == "277"), Is.EqualTo(2));
			});
		}

		[SetUp]
		public void SetUp()
		{
			ApplicationConfig.ConfigEnvironment();

			var fileProviderMock = new Mock<IDailyTariffUpdatesFileProvider>();
			fileProviderMock
				.SetupGet(x => x.RateDailyRawRecordCollection)
				.Returns(new List<IRawRateDailyRecord>()
				{
					CreateRawRateDailyRecord("000", false),
					CreateRawRateDailyRecord("227", true),
					CreateRawRateDailyRecord("277", true, "3824301277 80"),
					CreateRawRateDailyRecord("277", true, "3824302277 80"),
					CreateRawRateDailyRecord("278", false, "3824301278 80"),
					CreateRawRateDailyRecord("278", false, "3824302278 80"),
					CreateRawRateDailyRecord("482", true),
					CreateRawRateDailyRecord("484", true),
					CreateRawRateDailyRecord("495", true),
					CreateRawRateDailyRecord("496", true),
					CreateRawRateDailyRecord("555", true),
					CreateRawRateDailyRecord("765", true),
					CreateRawRateDailyRecord("768", false),
					CreateRawRateDailyRecord("769", false),
					CreateRawRateDailyRecord("776", false),
					CreateRawRateDailyRecord("777", false),
					CreateRawRateDailyRecord("780", false),

				});

			var dailyExportRecord = new ExportRawRecordForTest();
			var dailyExportProvider = new DailyTariffRecordsProvider(fileProviderMock.Object, dailyExportRecord.ValidMeasureTypeIdsForMeasureConditionInRate);
			dailyExportRecords = dailyExportProvider.GetDailyRecords().ToList();


			var dailyImportRecord = new ImportRawRecordForTest();
			var dailyImportProvider = new DailyTariffRecordsProvider(fileProviderMock.Object, dailyImportRecord.ValidMeasureTypeIdsForMeasureConditionInRate);
			dailyImportRecords = dailyImportProvider.GetDailyRecords().ToList();

			IRawRateDailyRecord CreateRawRateDailyRecord(string measureTypeId, bool isImport, string tariffHeader = null) =>
				new RawRateDailyRecord(
					tariffHeader ?? $"3824300{measureTypeId} 80",
					additionalCode: "",
					orderNumber: "",
					startDate: DateTime.Now.AddMonths(-3).Date,
					endDate: DateTime.Now.AddMonths(3).Date,
					legalBase: "",
					tradeGroup: "EUN",
					measureTypeId: measureTypeId,
					rate: "1",
					reductionIndicator: "",
					isImport: isImport,
					operationType: "INSERT",
					sequenceNumber: 3,
					fileName: "testfile.csv");
		}

		List<IRawRateDailyRecord> dailyExportRecords;
		List<IRawRateDailyRecord> dailyImportRecords;

		sealed class ExportRawRecordForTest : ExportRawRecord
		{
			public new IEnumerable<string> ValidMeasureTypeIdsForMeasureConditionInRate => base.ValidMeasureTypeIdsForMeasureConditionInRate;
		}
		sealed class ImportRawRecordForTest : ImportRawRecord
		{
			public new IEnumerable<string> ValidMeasureTypeIdsForMeasureConditionInRate => base.ValidMeasureTypeIdsForMeasureConditionInRate;
		}
	}
}
