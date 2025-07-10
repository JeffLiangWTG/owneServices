using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeList
{
	[TestFixture]
	class DC44TCodeListGeneratorTest
	{
		[Test]
		public void TestGenerateFiles()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, "FR - DC44T Code Lists.xml");
			File.Delete(outputFileName);

			var error = Errors.No;
			var generator = new DC44TCodeListGeneratorForTest();
			generator.GenerateFiles(new DateTime(2024, 02, 26), ref error);

			Assert.True(File.Exists(outputFileName));

			var expectedFileContent = File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, "FR - DC44T Code Lists.xml")).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			var actualFileContent = File.ReadAllText(outputFileName).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			Assert.AreEqual(expectedFileContent, actualFileContent);
		}
	}

	class DC44TCodeListGeneratorForTest : DC44TCodeListGenerator
	{
		public override Stream GetInputFileStream() => Assembly.Load("CargoWise.RefDbRepo.FRReferenceData.Tests").GetManifestResourceStream("CargoWise.RefDbRepo.FRReferenceData.Tests.TestFiles.PNTS_CodeLists.xlsx");
	}

}
