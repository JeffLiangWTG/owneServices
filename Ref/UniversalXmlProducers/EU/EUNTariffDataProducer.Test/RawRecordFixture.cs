using System;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	sealed class RawRecordFixture
	{
		[Test]
		public void TestParse_MixExportConditionsInImportTariff()
		{
			AssertMeasureConditionRecordsDoesContain(new ImportRawRecord(), "709", "751", "765");
		}

		void AssertMeasureConditionRecordsDoesContain(RawRecord rawRecord, params string[] measureTypeIdCollection)
		{
			ApplicationConfig.SetDownloadsFolder(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles"));

			var importWebFileInfo = GetWebFileInfo("SampleDutiesImport.xlsx");
			var exportWebFileInfo = GetWebFileInfo("SampleDutiesExport.xlsx");
			var importExportWebFileInfo = GetWebFileInfo("SampleDutiesImportExport.xlsx");

			rawRecord.Parse(new IWebFileInfo[] { importWebFileInfo, exportWebFileInfo, importExportWebFileInfo });

			var measureConditionRecords = rawRecord.MeasureConditionRecords
				.Select(x => x.MeasureTypeId)
				.ToList();

			Assert.Multiple(() =>
			{
				Assert.That(measureConditionRecords.Count, Is.GreaterThan(0));
				foreach (var measureTypeId in measureTypeIdCollection)
				{
					Assert.That(measureConditionRecords.Contains(measureTypeId), Is.True, $"Measure Conditions should contain {measureTypeId}");
				}
			});
		}

		[Test]
		public void TestParse_DontMixImportConditionsInExportTariff()
		{
			AssertMeasureConditionRecordsDoesNotContain(new ExportRawRecord(), "103", "105", "106", "109", "122", "142", "410", "710", "728", "750");
		}

		void AssertMeasureConditionRecordsDoesNotContain(RawRecord rawRecord, params string[] measureTypeIdCollection)
		{
			ApplicationConfig.SetDownloadsFolder(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles"));

			var importWebFileInfo = GetWebFileInfo("SampleDutiesImport.xlsx");
			var exportWebFileInfo = GetWebFileInfo("SampleDutiesExport.xlsx");
			var importExportWebFileInfo = GetWebFileInfo("SampleDutiesImportExport.xlsx");

			rawRecord.Parse(new IWebFileInfo[] { importWebFileInfo, exportWebFileInfo, importExportWebFileInfo });

			var measureConditionRecords = rawRecord.MeasureConditionRecords
				.Select(x => x.MeasureTypeId)
				.ToList();

			Assert.Multiple(() =>
			{
				Assert.That(measureConditionRecords.Count, Is.GreaterThan(0));
				foreach (var measureTypeId in measureTypeIdCollection)
				{
					Assert.That(!measureConditionRecords.Contains(measureTypeId), Is.True, $"Measure Conditions should not contain {measureTypeId}");
				}
			});
		}

		IWebFileInfo GetWebFileInfo(string fileName)
		{
			var webFileMock = new Mock<IWebFileInfo>();
			webFileMock.Setup(x => x.FileName).Returns(fileName);
			return webFileMock.Object;
		}

		[SetUp]
		public void SetUp()
		{
			ApplicationConfig.ConfigEnvironment();
		}
	}
}
