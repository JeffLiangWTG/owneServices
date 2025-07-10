using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.DEReferenceData.Testing;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.DeTariffs.Testing;

[TestFixture]
public class DeTariffsTests
{
	[Test]
	public void ConvertToXMLFile()
	{
		Directory.CreateDirectory(inputPath);
		File.WriteAllText(
			Path.Combine(inputPath, "XD01296401_N42000_134.xml"),
	TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.DEReferenceData.Tests.Business.DeTariffs.TestFiles.Input.XD01296401_N42000_134.xml"));
		File.WriteAllText(
			Path.Combine(inputPath, "XD01296401_N42020_137.xml"),
	TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.DEReferenceData.Tests.Business.DeTariffs.TestFiles.Input.XD01296401_N42020_137.xml"));
		File.WriteAllText(
			Path.Combine(inputPath, "XD01296401_T40010_63.xml"),
	TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.DEReferenceData.Tests.Business.DeTariffs.TestFiles.Input.XD01296401_T40010_63.xml"));
		File.WriteAllText(
			Path.Combine(inputPath, "XD01296601.xml"),
	TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.DEReferenceData.Tests.Business.DeTariffs.TestFiles.Input.XD01296601.xml"));
		var outputFile = Path.Combine(outputPath, "RefCusTariff_DE.xml");

		var tariffParser = new DeTariffParser(inputPath, outputFile);
		tariffParser.ConvertToXMLFile();
		var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.DEReferenceData.Tests.Business.DeTariffs.TestFiles.Output.RefCusTariff_DE.xml");
		var actualUniversalXml = File.ReadAllText(outputFile);
		Assert.That(actualUniversalXml, Is.EqualTo(expectedXML));
	}

	[SetUp]
	public void Setup()
	{
		assembly = Assembly.GetExecutingAssembly();
		outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"DE\TestFiles\DeTariffs\Output");
		inputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"DE\TestFiles\DeTariffs\Input");
	}
	Assembly assembly;
	string outputPath;
	string inputPath;
}
