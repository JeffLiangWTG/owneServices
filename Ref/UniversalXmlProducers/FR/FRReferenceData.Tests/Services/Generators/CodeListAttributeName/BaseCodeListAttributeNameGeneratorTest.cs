using System;
using System.IO;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeListAttributeName;

[TestFixture]
abstract class BaseCodeListAttributeNameGeneratorTest<T> where T : BaseCodeListAttributeNameGenerator, new()
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

	protected BaseCodeListAttributeNameGenerator Generator => new T();

	protected virtual DateTime Date => new DateTime(2025, 05, 07);
}
