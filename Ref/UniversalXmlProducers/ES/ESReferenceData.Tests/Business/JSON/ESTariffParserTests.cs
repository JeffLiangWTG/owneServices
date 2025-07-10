using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

public class ESTariffParserTests : TestCase
{
	string NomenclaturesFile => "nomenclatures.jsonl";
	string SectionsFile => "sections.jsonl";

	[Test]
	public void TestValidConvertToXMLFile()
	{
		string nomenclaturesJsonContent = GetFileContent(inputPath, NomenclaturesFile);
		string sectionsJsonContent = GetFileContent(inputPath, SectionsFile);

		var errors = parser.ConvertToXMLFile(nomenclaturesJsonContent, sectionsJsonContent, outputFileTariffs, outputFileNomenclatures);
		Assert.That(errors, Is.EqualTo((string.Empty, string.Empty)));

		var expectedXMLTariffs = TestHelper.ReadManifestResourceContentUTF8("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.JSON.TestFiles.Output.RefCusTariffZZ_TariffOne_VALID_CODES.xml");
		var expectedXMLNomenclatures = TestHelper.ReadManifestResourceContentUTF8("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.JSON.TestFiles.Output.RefCusNomenclatureGroupsZZ_TariffOne_VALID_CODES.xml");
		var xmlTariffs = File.ReadAllText(outputFileTariffs);
		var xmlNomenclatures = File.ReadAllText(outputFileNomenclatures);
		Assert.That(xmlTariffs, Is.EqualTo(expectedXMLTariffs));
		Assert.That(xmlNomenclatures, Is.EqualTo(expectedXMLNomenclatures));
	}

	[Test]
	public void TestNoRecords()
	{
		string nomenclaturesJsonContent = "{}";
		string sectionsJsonContent = GetFileContent(inputPath, SectionsFile);
		var errors = parser.ConvertToXMLFile(nomenclaturesJsonContent, sectionsJsonContent, outputFileTariffs, outputFileNomenclatures);

		Assert.That(errors.errors, Does.Contain(@"Nomenclatures load failed. Details:
Number of records: 0
Json Content: {}"));
		Assert.That(errors.logs, Is.EqualTo(string.Empty));
	}

	[Test]
	public void TestInvalidJsonl_Array()
	{
		string nomenclaturesJsonContent = GetFileContent(inputPath, "invalid_array.jsonl");
		string sectionsJsonContent = GetFileContent(inputPath, SectionsFile);
		var errors = parser.ConvertToXMLFile(nomenclaturesJsonContent, sectionsJsonContent, outputFileTariffs, outputFileNomenclatures);

		Assert.That(errors.errors, Does.Contain(@"Invalid json format.
Exception Details: Newtonsoft.Json.JsonSerializationException"));
		Assert.That(errors.logs, Is.EqualTo(string.Empty));
	}

	[Test]
	public void TestDifferentJsonl_Indentation()
	{
		string nomenclaturesJsonContent = GetFileContent(inputPath, "different_indentation_nomenclatures.jsonl");
		string sectionsJsonContent = GetFileContent(inputPath, SectionsFile);
		var errors = parser.ConvertToXMLFile(nomenclaturesJsonContent, sectionsJsonContent, outputFileTariffs, outputFileNomenclatures);

		Assert.That(errors.errors, Does.Contain(string.Empty));
		Assert.That(errors.logs, Is.EqualTo(string.Empty));
		var xmlTariffs = File.ReadAllText(outputFileTariffs);
		var xmlNomenclatures = File.ReadAllText(outputFileNomenclatures);
		Assert.That(xmlTariffs, Is.Not.EqualTo(string.Empty));
		Assert.That(xmlNomenclatures, Is.Not.EqualTo(string.Empty));
	}

	[Test]
	public void TestInvalidDescription()
	{
		string nomenclaturesJsonContent = GetFileContent(inputPath, "invalid_data_nomenclatures.jsonl");
		string sectionsJsonContent = GetFileContent(inputPath, SectionsFile);
		var errors = parser.ConvertToXMLFile(nomenclaturesJsonContent, sectionsJsonContent, outputFileTariffs, outputFileNomenclatures);

		Assert.That(errors.errors, Does.Contain(@"Tariff processing failed. Details:
startDate: 1/1/1900 0:00:00
endDate: 6/6/2079 23:59:00
goods_id: 9920000000
suffix: 80
hierarchy: 4
level: 0
description: "));
		Assert.That(errors.errors, Does.Contain(@"Tariff processing failed. Details:
startDate: 1/1/1900 0:00:00
endDate: 6/6/2079 23:59:00
goods_id: 9920420000
suffix: 80
hierarchy: 6
level: 1
description: "));
		Assert.That(errors.errors, Does.Contain(@"Tariff processing failed. Details:
startDate: 1/1/1900 0:00:00
endDate: 6/6/2079 23:59:00
goods_id: 9920950000
suffix: 80
hierarchy: 6
level: 1
description: "));
		Assert.That(errors.logs, Is.EqualTo(string.Empty));
	}

	[Test]
	public void TestInvalidGoodsId()
	{
		string nomenclaturesJsonContent = GetFileContent(inputPath, "invalid_data_nomenclatures.jsonl");
		string sectionsJsonContent = GetFileContent(inputPath, SectionsFile);
		var errors = parser.ConvertToXMLFile(nomenclaturesJsonContent, sectionsJsonContent, outputFileTariffs, outputFileNomenclatures);

		Assert.That(errors.errors, Does.Contain(@"Tariff processing failed. Details:
startDate: 1/1/1900 0:00:00
endDate: 6/6/2079 23:59:00
goods_id: 
suffix: 80
hierarchy: 6
level: 1
description: Goods exported by postal service cap 92."));
		Assert.That(errors.logs, Is.EqualTo(string.Empty));
	}

	[Test]
	public void TestInvalidLevel()
	{
		string nomenclaturesJsonContent = GetFileContent(inputPath, "invalid_data_nomenclatures.jsonl");
		string sectionsJsonContent = GetFileContent(inputPath, SectionsFile);
		var errors = parser.ConvertToXMLFile(nomenclaturesJsonContent, sectionsJsonContent, outputFileTariffs, outputFileNomenclatures);

		Assert.That(errors.errors, Does.Contain(@"Tariff processing failed. Details:
startDate: 1/1/1900 0:00:00
endDate: 6/6/2079 23:59:00
goods_id: 9920940000
suffix: 80
hierarchy: 6
level: 
description: Goods exported by postal service cap 94."));
		Assert.That(errors.logs, Is.EqualTo(string.Empty));
	}

	[Test]
	public void TestInvalidHierarchy()
	{
		string nomenclaturesJsonContent = GetFileContent(inputPath, "invalid_data_nomenclatures.jsonl");
		string sectionsJsonContent = GetFileContent(inputPath, SectionsFile);
		var errors = parser.ConvertToXMLFile(nomenclaturesJsonContent, sectionsJsonContent, outputFileTariffs, outputFileNomenclatures);

		Assert.That(errors.errors, Does.Contain(@"Tariff processing failed. Details:
startDate: 1/1/1900 0:00:00
endDate: 6/6/2079 23:59:00
goods_id: 9920290000
suffix: 80
hierarchy: 
level: 1
description: Goods exported by postal service cap 29."));
		Assert.That(errors.logs, Is.EqualTo(string.Empty));
	}

	[Test]
	public void TestInvalidSuffix()
	{
		string nomenclaturesJsonContent = GetFileContent(inputPath, "invalid_data_nomenclatures.jsonl");
		string sectionsJsonContent = GetFileContent(inputPath, SectionsFile);
		var errors = parser.ConvertToXMLFile(nomenclaturesJsonContent, sectionsJsonContent, outputFileTariffs, outputFileNomenclatures);

		Assert.That(errors.errors, Does.Contain(@"Tariff processing failed. Details:
startDate: 1/1/1900 0:00:00
endDate: 6/6/2079 23:59:00
goods_id: 9920960000
suffix: 
hierarchy: 6
level: 1
description: Goods exported by postal service cap 96."));
		Assert.That(errors.logs, Is.EqualTo(string.Empty));
	}

	static string GetFileContent(string inputPath, string fileName) => File.ReadAllText(Path.Combine(inputPath, fileName));

	public override void OneTimeSetUp()
	{
		base.OneTimeSetUp();
		outputFileTariffs = Path.Combine(FileHelper.OutputFolder, "RefCusTariffZZ_TariffOne_VALID_CODES.xml");
		outputFileNomenclatures = Path.Combine(FileHelper.OutputFolder, "RefCusNomenclatureGroupsZZ_TariffOne_VALID_CODES.xml");
		inputPath = Path.Combine(Path.GetDirectoryName(ExecutingAssembly.Location), @"Business\JSON\TestFiles\Input\TariffOne");

		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2021, 3, 25));
	}

	[SetUp]
	public void SetUp()
	{
		parser = new ESTariffParser(DateProvider.Object);
	}

	string outputFileTariffs;
	string outputFileNomenclatures;
	string inputPath;
	ESTariffParser parser;

	protected override string TestClassName => nameof(ESTariffParserTests);
}
