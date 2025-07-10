using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CsvHelper;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

public class C44DocumentsParserTests : TestCase
{
	[Test]
	public void TestCreateRefCusCodeListXML()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var pathPrefix = "CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CodeLists.C44Documents.TestFiles.";
		var csvCSRDT213 = TestHelper.ReadManifestResourceContent($"{pathPrefix}Input.CSRDT213.csv");
		var csvTRSUPNAC = TestHelper.ReadManifestResourceContent($"{pathPrefix}Input.TRSUPNAC.csv");
		var csvTRSUPNCA = TestHelper.ReadManifestResourceContent($"{pathPrefix}Input.TRSUPNCA.csv");
		var csvTRSUPNHO = TestHelper.ReadManifestResourceContent($"{pathPrefix}Input.TRSUPNHO.csv");
		var csvTRSUPNPA = TestHelper.ReadManifestResourceContent($"{pathPrefix}Input.TRSUPNPA.csv");

		var nctsDocuments = (csvCSRDT213, csvTRSUPNAC, csvTRSUPNCA, csvTRSUPNHO, csvTRSUPNPA);
		new C44DocumentsParser(DateProvider.Object).ConvertRecordsToXMLFile(C44DocumentsProviderTests.ExpectedDocuments, nctsDocuments, outputFile);
		var expectedXML = TestHelper.ReadManifestResourceContent($"{pathPrefix}Output.RefCusCodeListZZ_ES_C44Docs_CODES.xml");
		var xml = File.ReadAllText(outputFile);
		Assert.That(xml, Is.EqualTo(expectedXML));
	}

	[Test]
	public void TestHTMLReturnedByAeatInsteadOfCsv()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var pathPrefix = "CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CodeLists.C44Documents.TestFiles.";
		var csvCSRDT213 = TestHelper.ReadManifestResourceContent($"{pathPrefix}Input.CSRDT213.csv");
		var csvTRSUPNAC = TestHelper.ReadManifestResourceContent($"{pathPrefix}Input.TRSUPNAC.csv");
		var csvTRSUPNCA = TestHelper.ReadManifestResourceContent($"{pathPrefix}Input.TRSUPNCA.csv");
		var csvTRSUPNHO = TestHelper.ReadManifestResourceContent($"{pathPrefix}Input.TRSUPNHO.csv");
		var csvTRSUPNPA = TestHelper.ReadManifestResourceContent($"{pathPrefix}Input.TRSUPNPA_HTML.csv");

		var nctsDocuments = (csvCSRDT213, csvTRSUPNAC, csvTRSUPNCA, csvTRSUPNHO, csvTRSUPNPA);
		var errors = new C44DocumentsParser(DateProvider.Object).ConvertRecordsToXMLFile(C44DocumentsProviderTests.ExpectedDocuments, nctsDocuments, outputFile);
		Assert.That(errors, Does.Contain(string.Empty));
		var expectedXML = TestHelper.ReadManifestResourceContent($"{pathPrefix}Output.RefCusCodeListZZ_ES_C44Docs_CODES_HTML.xml");
		var xml = File.ReadAllText(outputFile);
		Assert.That(xml, Is.EqualTo(expectedXML));
	}

	[Test]
	public void TestHTMLReturnedByAeatInsteadOfCsvForAllExclusionSets()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var pathPrefix = "CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CodeLists.C44Documents.TestFiles.";
		var csvCSRDT213 = TestHelper.ReadManifestResourceContent($"{pathPrefix}Input.CSRDT213.csv");
		var csvTRSUPNAC = TestHelper.ReadManifestResourceContent($"{pathPrefix}Input.TRSUPNAC.csv");
		var html = TestHelper.ReadManifestResourceContent($"{pathPrefix}Input.TRSUPNPA_HTML.csv");
		var csvTRSUPNCA = html;
		var csvTRSUPNHO = html;
		var csvTRSUPNPA = html;

		var nctsDocuments = (csvCSRDT213, csvTRSUPNAC, csvTRSUPNCA, csvTRSUPNHO, csvTRSUPNPA);
		var errors = new C44DocumentsParser(DateProvider.Object).ConvertRecordsToXMLFile(C44DocumentsProviderTests.ExpectedDocuments, nctsDocuments, outputFile);
		Assert.That(errors, Does.Contain("Customs has returned empty CSV for all exclusion documents."));
		var expectedXML = TestHelper.ReadManifestResourceContent($"{pathPrefix}Output.RefCusCodeListZZ_ES_C44Docs_CODES_HTML_For_Exclusion.xml");
		var xml = File.ReadAllText(outputFile);
		Assert.That(xml, Is.EqualTo(expectedXML));
	}

	[Test]
	public void TestNoRecordsToProcess()
	{
		var nctsDocuments = ("", "", "", "", "");
		var errors = new C44DocumentsParser(DateProvider.Object).ConvertRecordsToXMLFile([], nctsDocuments, outputFile);
		Assert.That(errors, Does.Contain("Unable to locate any records for C44 Documents. File may only contain header record"));
	}

	[Test]
	public void TestEmptyCodeErrorMessage()
	{
		var nctsDocuments = ("", "", "", "", "");
		var errors = new C44DocumentsParser(DateProvider.Object).ConvertRecordsToXMLFile(InvalidDocuments, nctsDocuments, outputFile);
		Assert.That(errors, Does.Contain(@"Unable to import C44 Document as missing or duplicated attribute 'code' or 'description'. Details:
Code: 
Description: Certificado de autenticidad zumo de naranja concentrado"));
	}

	[Test]
	public void TestEmptyDescriptionErrorMessage()
	{
		var nctsDocuments = ("", "", "", "", "");
		var errors = new C44DocumentsParser(DateProvider.Object).ConvertRecordsToXMLFile(InvalidDocuments, nctsDocuments, outputFile);
		Assert.That(errors, Does.Contain(@"Unable to import C44 Document as missing or duplicated attribute 'code' or 'description'. Details:
Code: A009
Description: "));
	}

	[Test]
	public void TestDuplicatedCodeErrorMessage()
	{
		var nctsDocuments = ("", "", "", "", "");
		var errors = new C44DocumentsParser(DateProvider.Object).ConvertRecordsToXMLFile(InvalidDocuments, nctsDocuments, outputFile);
		Assert.That(errors, Does.Contain(@"Unable to import C44 Document as missing or duplicated attribute 'code' or 'description'. Details:
Code: A008
Description: New Certificado de autenticidad frescas naranjas dulces ""de calidad superior"""));
	}

	[Test]
	public void CSVDelimiterHasChanged()
	{
		var nctsDocuments = ("Código:Descripción::::", "", "", "", "");
		Assert.Throws<HeaderValidationException>(() => new C44DocumentsParser(DateProvider.Object).ConvertRecordsToXMLFile([], nctsDocuments, outputFile));
	}

	[Test]
	public void CSVFileHeaderChanged()
	{
		var nctsDocuments = ("WrongCode;Descripción;;;", "", "", "", "");
		Assert.Throws<HeaderValidationException>(() => new C44DocumentsParser(DateProvider.Object).ConvertRecordsToXMLFile([], nctsDocuments, outputFile));
	}

	[Test]
	public void NoRecordsToProcess()
	{
		var nctsDocuments = ("Código;Descripción;;;;", "", "", "", "");
		var expectedErrorMessage = $"Unable to locate any records for C44 Documents. File may only contain header record";
		var errors = new C44DocumentsParser(DateProvider.Object).ConvertRecordsToXMLFile([], nctsDocuments, outputFile);
		Assert.That(errors, Does.Contain(expectedErrorMessage));
	}

	public override void OneTimeSetUp()
	{
		base.OneTimeSetUp();
		outputFile = Path.Combine(FileHelper.OutputFolder, "RefCusCodeListZZ_ES.xml");
	}
	string outputFile;

	protected override string TestClassName => nameof(C44DocumentsParserTests);

	IEnumerable<(string Code, string Description)> InvalidDocuments =>
	[
		("", "Certificado de autenticidad zumo de naranja concentrado"),
		("A009", ""),
		("A008", "Certificado de autenticidad frescas naranjas dulces \"de calidad superior\""),
		("A008", "New Certificado de autenticidad frescas naranjas dulces \"de calidad superior\"")
	];
}
