using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.ExchangeRates
{
	[TestFixture]
	public class ExchangeRatesParserTests
	{
		[Test]
		public void ExchangeRatesXMLImport()
		{
			var exchangeRates = GetExchangeRatesFromEmbeddedResource("CargoWise.RefDbRepo.TRReferenceData.Tests.ExchangeRates.TestFiles.Input.download-exchange.xml");
			var expectedImportXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_TR_Import.xml");
			new ExchangeRateParser(exchangeRates).ConvertToXMLFile(importFile, Business.Constants.ExchangeRateTypes.Import);
			var actualXML = File.ReadAllText(importFile);
			Assert.That(actualXML, Is.EqualTo(expectedImportXML), "RefExchangeRateZZ_TR_Import.xml");
		}

		[Test]
		public void ExchangeRatesXMLExport()
		{
			var exchangeRates = GetExchangeRatesFromEmbeddedResource("CargoWise.RefDbRepo.TRReferenceData.Tests.ExchangeRates.TestFiles.Input.download-exchange.xml");
			var expectedExportXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_TR_Export.xml");
			new ExchangeRateParser(exchangeRates).ConvertToXMLFile(exportFile, Business.Constants.ExchangeRateTypes.Export);
			var actualXML = File.ReadAllText(exportFile);
			Assert.That(actualXML, Is.EqualTo(expectedExportXML), "RefExchangeRateZZ_TR_Export.xml");
		}

		[Test]
		public void InvalidPublicationDateFormat()
		{
			var exchangeRates = GetExchangeRatesFromEmbeddedResource("CargoWise.RefDbRepo.TRReferenceData.Tests.ExchangeRates.TestFiles.Input.InvalidTarihDateFormat.xml");
			var error = new ExchangeRateParser(exchangeRates).ConvertToXMLFile(exportFile, Business.Constants.ExchangeRateTypes.Export);
			Assert.That(error, Does.StartWith("Unable to parse publication date: 21-01-2020"));
			Assert.That(exportFile, Does.Not.Exist);
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			var outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"TR\TestFiles\ExchangeRates");
			Directory.CreateDirectory(outputPath);
			importFile = Path.Combine(outputPath, "RefExchangeRateZZ_TR_Import.xml");
			File.Delete(importFile);
			exportFile = Path.Combine(outputPath, "RefExchangeRateZZ_TR_Export.xml");
			File.Delete(exportFile);
		}
		Assembly assembly;
		string importFile;
		string exportFile;

		Tarih_Date GetExchangeRatesFromEmbeddedResource(string embeddedResourceName)
		{
			Tarih_Date exchangeRates;
			using (var inputTestStream = assembly.GetManifestResourceStream(embeddedResourceName))
			{
				var xmlData = new XmlDocument();
				xmlData.Load(inputTestStream);
				var serializer = new XmlSerializer(typeof(Tarih_Date));
				using (var stringReader = new StringReader(xmlData.OuterXml))
				using (var xmlReader = XmlReader.Create(stringReader))
				{
					exchangeRates = (Tarih_Date)serializer.Deserialize(xmlReader);
				}
			}
			return exchangeRates;
		}
	}
}
