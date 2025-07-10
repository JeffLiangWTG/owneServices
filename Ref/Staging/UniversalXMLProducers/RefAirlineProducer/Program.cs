using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineProducer
{
	class Program
	{
		static int Main(string[] args)
		{
			ProduceXml();
			return (int)ProducerStatus.Success;
		}

		static void ProduceXml()
		{
			var airlineFile = ConfigurationProvider.AirlineFilePath;
			var airlineParser = new RefAirlineParser(airlineFile);

			var airlinesWithNumericalOrThreeLetterCode = airlineParser.GetAirlinesWithNumericalOrThreeLetterCode();
			var xmlWriter = RefAirlineXmlConfiguration.GetXmlWriter(airlineFile, true);
			XmlWriterHelper.ExportToXml(xmlWriter, airlinesWithNumericalOrThreeLetterCode, ConfigurationProvider.AirlineWithMultipleKeysOutputPath);

			var airlinesWithAirlineName1 = airlineParser.GetAirlinesWithAirlineName1();
			xmlWriter = RefAirlineXmlConfiguration.GetXmlWriter(airlineFile, false);
			XmlWriterHelper.ExportToXml(xmlWriter, airlinesWithAirlineName1, ConfigurationProvider.AirlineName1OutputPath);
		}
	}
}
