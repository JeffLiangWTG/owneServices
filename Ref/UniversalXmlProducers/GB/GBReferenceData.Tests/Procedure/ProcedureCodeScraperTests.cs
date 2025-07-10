using System.Linq;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure
{
	[TestFixture]
	public class ProcedureCodeScraperTests
	{
		[TestCase("ImportProcedure.html", "ImportCategoryProcedureMapping.csv")]
		[TestCase("InventoryImportProcedure.html", "InventoryImportCategoryProcedureMapping.csv")]
		[TestCase("FinalSupplementaryImportProcedure.html", "FinalSupplementaryImportCategoryProcedureMapping.csv")]
		[TestCase("ExportProcedure.html", "ExportCategoryProcedureMapping.csv")]
		[TestCase("InventoryExportProcedure.html", "InventoryExportCategoryProcedureMapping.csv")]
		public void ExtractCategoryProcedureMapping(string inputHtmlName, string expectedCsvName)
		{
			var html = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Input.{inputHtmlName}");
			var scraper = new ProcedureCodeScraper(html);
			var results = scraper.ExtractCategoryProcedureMapping();

			var expectedCsv = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Output.{expectedCsvName}");
			var resultsCsv = TestHelper.ConvertToCsv(results, r => new[] { r.CategoryCode, r.ProcedureMapping });

			Assert.That(resultsCsv, Is.EqualTo(expectedCsv));
		}

		[TestCase("ImportProcedure.html", "ImportProcedureDescriptions.csv")]
		[TestCase("InventoryImportProcedure.html", "InventoryImportProcedureDescriptions.csv")]
		[TestCase("FinalSupplementaryImportProcedure.html", "FinalSupplementaryImportProcedureDescriptions.csv")]
		[TestCase("ExportProcedure.html", "ExportProcedureDescriptions.csv")]
		[TestCase("InventoryExportProcedure.html", "InventoryExportProcedureDescriptions.csv")]
		public void ExtractProcedureDescriptions(string inputHtmlName, string expectedCsvName)
		{
			var html = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Input.{inputHtmlName}");
			var scraper = new ProcedureCodeScraper(html);
			var results = scraper.ExtractProcedureDescriptions("IMP");

			var expectedCsv = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Output.{expectedCsvName}");
			var resultsCsv = TestHelper.ConvertToCsv(results, r => new[] { r.Code, r.Description });

			Assert.That(results.All(r => r.ShipmentType == "IMP"), Is.True);
			Assert.That(resultsCsv, Is.EqualTo(expectedCsv));
		}
	}
}
