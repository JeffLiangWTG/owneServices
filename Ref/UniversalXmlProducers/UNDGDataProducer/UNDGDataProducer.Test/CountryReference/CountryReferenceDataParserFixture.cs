using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using NUnit.Framework;
namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	class CountryReferenceDataParserFixture
	{
		[Test]
		public void Output()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var expectedXmlFilePath = Path.Combine(binPath, @"TestFiles\Country Reference Expected.xml");
			var resultXmlFilePath = Path.Combine(binPath, @"TestFiles\Country Reference Result.xml");
			var csvFilePath = Path.Combine(binPath, @"TestFiles\Country Reference Sample.csv");
			var csvPivotFilePath = Path.Combine(binPath, @"TestFiles\Country Reference Pivot Sample.csv");
			var imoFilePath = Path.Combine(binPath, @"TestFiles\IMO Sample For Country Reference.zip");

			var parser = new CountryReferenceParser();
			var parsedRecordsWithoutPivots = parser.Parse(Path.Combine(binPath, csvFilePath));

			var pivotParser = new SingaporePSAPivotParser(new FileDownloaderWrapper(), imoFilePath);
			var parsedRecordsWithPivots = pivotParser.ParseAndAddPivotsToReferences(Path.Combine(binPath, csvPivotFilePath), parsedRecordsWithoutPivots);

			var xmlWriter = CountryReferenceXmlWriterConfiguration.GetXmlWriter(new DateTime(1601, 1, 1));
			XmlWriterHelper.ExportToXml(xmlWriter, parsedRecordsWithPivots, resultXmlFilePath);

			var result = File.ReadAllText(resultXmlFilePath);
			var expected = File.ReadAllText(expectedXmlFilePath).TrimEnd();
			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public void FullPopulatedRowInCSV2()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var csvFilePath = Path.Combine(binPath, @"TestFiles\Country Reference Full Row Sample.csv");
			var parsedRecords = new CountryReferenceParser().Parse(csvFilePath);

			Assert.That(parsedRecords.Count(), Is.EqualTo(1));

			var element = parsedRecords.ElementAt(0);
			Assert.That(element.DCR_Type, Is.EqualTo("ICPE"));
			Assert.That(element.DCR_Code, Is.EqualTo("4736"));
			Assert.That(element.DCR_RN_NKCountry, Is.EqualTo("FR"));
			Assert.That(element.DCR_Description, Is.EqualTo("Stockage de trifluorure de bore"));
			Assert.That(element.DCR_HasFlashPointLower, Is.EqualTo(false));
			Assert.That(element.DCR_HasFlashPointUpper, Is.EqualTo(false));
			Assert.That(element.DCR_FlashPointLowerCentigrade, Is.EqualTo(0));
			Assert.That(element.DCR_FlashPointUpperCentigrade, Is.EqualTo(0));
		}

		[Test]
		public void HeaderMap()
		{
			var headerMap = new CountryReferenceParser().HeaderMap;

			Assert.That(headerMap[nameof(CountryReferenceRecord.Type)], Is.EqualTo(0));
			Assert.That(headerMap[nameof(CountryReferenceRecord.Country)], Is.EqualTo(1));
			Assert.That(headerMap[nameof(CountryReferenceRecord.Code)], Is.EqualTo(2));
			Assert.That(headerMap[nameof(CountryReferenceRecord.Description)], Is.EqualTo(3));
			Assert.That(headerMap[nameof(CountryReferenceRecord.HasFlashPointLower)], Is.EqualTo(4));
			Assert.That(headerMap[nameof(CountryReferenceRecord.FlashPointLowerCentigrade)], Is.EqualTo(5));
			Assert.That(headerMap[nameof(CountryReferenceRecord.HasFlashPointUpper)], Is.EqualTo(6));
			Assert.That(headerMap[nameof(CountryReferenceRecord.FlashPointUpperCentigrade)], Is.EqualTo(7));
		}
	}
}
