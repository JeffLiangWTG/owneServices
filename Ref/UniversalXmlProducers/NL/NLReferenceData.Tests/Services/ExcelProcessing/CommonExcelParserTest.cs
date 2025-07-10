using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.NLReferenceData.Business.Testing;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Services.Testing
{
	[TestFixture]
	public class CommonExcelParserTest
	{
		[Test]
		public void TestReadXlsFile()
		{
			TestHelper.SimulateDownload(TempFolder + "CommonExcel.xlsx", "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.ExcelProcessing.Input.CommonExcel.xlsx");
			var xlsFiles = new List<string> { TempFolder + "CommonExcel.xlsx" };

			var parser = new ExcelParserForTest();

			var result = parser.ReadXlsFile(xlsFiles);

			Assert.IsNotNull(result);
			Assert.That(result.Count.Equals(4), "There should be 4 results in the excel");

			var result0 = result[0];
			var result3 = result[3];

			Assert.That(result0.Code.Equals("Code 1", System.StringComparison.Ordinal), "Code of result 0 should be Code1 ");
			Assert.That(result0.Description.Equals("Description 1", System.StringComparison.Ordinal), "Description of result 0 should be Description 1");
			Assert.That(result0.Kind.Equals("Kind 1", System.StringComparison.Ordinal), "Kind of result 0 should be Kind 1");
			Assert.That(result0.Type.Equals("Type 1", System.StringComparison.Ordinal), "Type of result 0 should be Type 1");
			Assert.That(result0.List.Equals("List 1", System.StringComparison.Ordinal), "List of result 0 should be List 1");

			Assert.That(result3.Code.Equals("Code 4", System.StringComparison.Ordinal), "Code of result 3 should be Code4 ");
			Assert.That(result3.Description.Equals("Description 4", System.StringComparison.Ordinal), "Description of result 3 should be Description 4");
			Assert.That(result3.Kind.Equals("Kind 4", System.StringComparison.Ordinal), "Kind of result 3 should be Kind 4");
			Assert.That(result3.Type.Equals("Type 4", System.StringComparison.Ordinal), "Type of result 3 should be Type 4");
			Assert.That(result3.List.Equals("List 4", System.StringComparison.Ordinal), "List of result 3 should be List 4");
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}
	}
}
