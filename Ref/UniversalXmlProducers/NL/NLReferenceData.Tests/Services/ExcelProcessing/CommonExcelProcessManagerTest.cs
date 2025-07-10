using System.IO;
using System.Text;
using CargoWise.RefDbRepo.NLReferenceData.Business.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Services.Testing
{
	[TestFixture]
	sealed class CommonExcelProcessManagerTest
	{
		[Test]
		public void BuildCount()
		{
			var errorCollector = new StringBuilder();
			var builder = new CommonExcelBuilderForTest();

			var prManager = new Mock<CommonExcelProcessManagerForTest>(builder);
			prManager.Protected().Setup<bool>("SaveResourceContentToFile", ItExpr.IsAny<string>(), ItExpr.IsAny<string>()).Returns((string filename, string resource) => TestHelper.SimulateDownload(filename, resource));
			prManager.Protected().Setup<string>("EntityName").Returns(EntityName);
			prManager.Protected().Setup<string>("ExcelFileName").Returns(FileName);
			prManager.Protected().Setup<string>("ResourceContent").Returns(InputPathCorrect);

			prManager.Object.RunProcess(TempFolder, errorCollector);

			Assert.That(builder, Is.Not.Null);
			Assert.That(builder.BuildCallCount, Is.EqualTo(1));
			Assert.That(builder.BuildModelCount, Is.EqualTo(4));
		}

		[SetUp]
		public void Setup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		static string InputPathCorrect => "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.ExcelProcessing.Input.CommonExcel.xlsx";
		static string FileName => "CommonExcel.xlsx";
		static string EntityName => "Test Data";

		string TempFolder;
	}
}
