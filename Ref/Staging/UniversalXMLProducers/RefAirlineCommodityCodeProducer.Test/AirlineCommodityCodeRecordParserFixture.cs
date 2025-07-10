using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProducer.Test
{
	public class AirlineCommodityCodeRecordParserFixture
	{
		[Test]
		public void Output()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			Assert.NotNull(binPath, "Bin path should not be null");

			var csvFilePath = Path.Combine(binPath, @"TestFiles\IATA Commodity Codes Sample.csv");
			var expectedXmlFilePath = Path.Combine(binPath, @"TestFiles\IATACommodityCodes Expected.xml");
			var resultXmlFilePath = Path.Combine(binPath, @"TestFiles\IATACommodityCodes Result.xml");

			var parser = new AirlineCommodityCodeRecordParser();
			var parsedRecords = parser.Parse(csvFilePath);
			var xmlWriter = XmlWriterConfiguration.GetXmlWriter(new System.DateTime(1900, 1, 1));
			XmlWriterHelper.ExportToXml(xmlWriter, parsedRecords, resultXmlFilePath);

			var expected = File.ReadAllText(expectedXmlFilePath);
			var result = File.ReadAllText(resultXmlFilePath);

			Assert.That(result, Is.EqualTo(expected));
		}
	}
}
