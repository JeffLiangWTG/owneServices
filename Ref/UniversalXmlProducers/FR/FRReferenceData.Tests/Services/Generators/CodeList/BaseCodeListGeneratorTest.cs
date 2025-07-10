using System;
using System.IO;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeList
{
	[TestFixture]
	abstract class BaseCodeListGeneratorTest<T> where T : CodeListDataFileGenerator, new()
	{
		[Test]
		public void TestGenerateFiles()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();

			var error = Errors.No;
			var generator = Generator;
			generator.GenerateFiles(Date, ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, OutputXMLFileName);
			Assert.True(File.Exists(outputFileName));
			var expectedFileContent = File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, OutputXMLFileName)).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			var actualFileContent = File.ReadAllText(outputFileName).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			Assert.AreEqual(expectedFileContent, actualFileContent);
		}

		protected abstract string OutputXMLFileName { get; }

		protected virtual CodeListDataFileGenerator Generator => new T();

		DateTime Date => new DateTime(2025, 05, 07);
	}
}
