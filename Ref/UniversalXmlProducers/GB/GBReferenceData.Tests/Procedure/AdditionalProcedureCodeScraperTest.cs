using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure
{
	[TestFixture]
	public class AdditionalProcedureCodeScraperTest
	{
		[TestCase("ImportAdditionalProcedureCodes")]
		[TestCase("InventoryImportAdditionalProcedureCodes")]
		[TestCase("FinalSupplementaryAdditionalProcedureCodes")]
		[TestCase("ExportAdditionalProcedureCodes")]
		[TestCase("ExportInventoryAdditionalProcedureCodes")]
		public void TestAdditionalProcedureCodeScraper(string filename)
		{
			var html = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Input.{filename}.html");
			var scraper = new AdditionalProcedureCodeScraperForTesting();
			var codes = scraper.ExtractCodesWithDescription(html);

			Assert.That(codes, Is.Not.Null);
			var expectedCsv = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Output.{filename}.csv");
			var resultsCsv = TestHelper.ConvertToCsv(codes, r => new[] { r.Code, r.Description });
			Assert.That(resultsCsv, Is.EqualTo(expectedCsv));
		}

		[TestCase("ImportAdditionalProcedureCodesMatrix", 66, 154,
			"0100,0121,0151,0153,0154,0171,0178,0700,0721,0751,0753,0754,0771,0778,4000,4051,4053,4054,4071,4078,4200,4221," +
			"4251,4253,4254,4271,4278,4400,4421,4422,4451,4453,4454,4471,4478,5100,5111,5121,5151,5153,5154,5171,5178,5300," +
			"5351,5353,5354,5371,5378,6110,6111,6121,6122,6123,6131,7100,7110,7121,7122,7123,7151,7153,7154,7171,7178")]
		[TestCase("InventoryImportAdditionalProcedureCodesMatrix", 10, 53, "00 02,00 03,00 04,00 05,00 06,00 07,00 08,00 09,00 20")]
		[TestCase("FinalSupplementaryAdditionalProcedureCodesMatrix", 2, 1, "0090")]
		[TestCase("ExportAdditionalProcedureCodesMatrix", 17, 61, "1007,1040,1042,1044,1100,2100,2144,2151,2154,2200,2244,2300,3151,3153,3154,3171")]
		[TestCase("InventoryExportAdditionalProcedureCodesMatrix", 8, 49, "00 12,00 14,00 15,00 16,00 17,00 18,00 19")]
		public void TestGetDocumentData(string matrixFileName, int expectedColumnCount, int expectedRowCount, string expectedFirstRowValues)
		{
			var rawData = TestHelper.ReadManifestResourceContentBytes($"CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Input.{matrixFileName}.ods");
			var result = AdditionalProcedureCodeScraperForTesting.GetDocumentData(rawData);

			Assert.That(result.Columns.Count, Is.EqualTo(expectedColumnCount));
			Assert.That(result.Rows.Count, Is.EqualTo(expectedRowCount));
			var firstRow = string.Join(",", result.Columns.Cast<DataColumn>().Skip(1).Select(col => col.ColumnName));
			Assert.That(firstRow, Is.EqualTo(expectedFirstRowValues));
		}

		[TestCase("ImportAdditionalProcedureCodesMatrix")]
		[TestCase("InventoryImportAdditionalProcedureCodesMatrix")]
		[TestCase("InventoryImportAdditionalProcedureCodesMatrix_YesNo")]
		[TestCase("FinalSupplementaryAdditionalProcedureCodesMatrix")]
		[TestCase("ExportAdditionalProcedureCodesMatrix")]
		[TestCase("InventoryExportAdditionalProcedureCodesMatrix")]
		[TestCase("20250501_Correlation_Matrix_Inventory_Imports")]
		public void TestGetMappings(string matrixFileName)
		{
			var rawData = TestHelper.ReadManifestResourceContentBytes($"CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Input.{matrixFileName}.ods");
			var documentData = AdditionalProcedureCodeScraperForTesting.GetDocumentData(rawData);
			var mappings = AdditionalProcedureCodeScraperForTesting.GetMappings(documentData);

			var expectedCsv = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Output.{matrixFileName}.csv");
			var resultsCsv = TestHelper.ConvertToCsv(mappings, m =>
			{
				var row = new List<string>() { m.ProcedureCode };
				row.AddRange(m.AdditionalProcedureCodes);
				return row.ToArray();
			});
			Assert.That(resultsCsv, Is.EqualTo(expectedCsv));
		}

		[Test]
		public void TestGetContent()
		{
			var html = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Input.CorrelationMatrixDownloadPage.html");
			var scraper = new AdditionalProcedureCodeScraperForTesting();
			var result = scraper.GetContent(html);
			var expectedBytes = Encoding.UTF8.GetBytes("Downloaded https://assets.publishing.service.gov.uk/government/uploads/system/uploads/attachment_data/file/1100405/DE-1-10-to-1-11-correlation-matrix.ods");
			Assert.That(result, Is.EqualTo(expectedBytes));
		}
	}

	class WebClientWrapperForTesting : IWebClientWrapper
	{
		public string GetContent(string url)
		{
			return "Downloaded " + url;
		}

		public byte[] GetContentAsByteArray(string url)
		{
			return Encoding.UTF8.GetBytes(GetContent(url));
		}

		public (DateTime LastModified, string Content) GetDatedContent(string url)
		{
			throw new NotImplementedException();
		}

		public (DateTime LastModified, byte[] Content) GetDatedContentAsByteArray(string url)
		{
			throw new NotImplementedException();
		}
	}

	class AdditionalProcedureCodeScraperForTesting : AdditionalProcedureCodeScraper
	{
		public AdditionalProcedureCodeScraperForTesting() : base(new WebClientWrapperForTesting())
		{
		}

		public new byte[] GetContent(string html)
		{
			return base.GetContent(html);
		}
	}
}
