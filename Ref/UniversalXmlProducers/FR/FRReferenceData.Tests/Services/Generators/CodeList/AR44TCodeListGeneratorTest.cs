using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeList
{
	[TestFixture]
	class AR44TCodeListGeneratorTest
	{
		[Test]
		public void TestGenerateFiles()
		{
			ApplicationConfig.Instance.DownloadDirectory = "..\\..\\Test\\UxmlFiles\\Downloads";
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, "FR - AR44T Code Lists.xml");
			File.Delete(outputFileName);

			var error = Errors.No;
			var generator = new AR44TCodeListGeneratorForTest();
			generator.GenerateFiles(new DateTime(2024, 01, 08), ref error);

			Assert.True(File.Exists(outputFileName));

			var expectedFileContent = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\", "FR - AR44T Code Lists.xml")).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			var actualFileContent = File.ReadAllText(outputFileName).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			Assert.AreEqual(expectedFileContent, actualFileContent);
		}

		class AR44TCodeListGeneratorForTest : AR44TCodeListGenerator
		{
			public override Stream GetInputFileStream() => Assembly.Load("CargoWise.RefDbRepo.FRReferenceData.Tests").GetManifestResourceStream("CargoWise.RefDbRepo.FRReferenceData.Tests.TestFiles.PNTS_CodeLists.xlsx");
		}
	}
}
