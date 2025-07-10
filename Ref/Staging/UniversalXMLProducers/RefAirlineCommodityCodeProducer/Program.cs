using System;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProducer
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
			var refAirlineCommodityCodes = new AirlineCommodityCodeRecordParser().Parse(ConfigurationProvider.AirlineCommodityCodesCsvFilePath);
			XmlWriterHelper.ExportToXml(XmlWriterConfiguration.GetXmlWriter(DateTime.Now), refAirlineCommodityCodes, Constants.AirlineCommodityCodesOutputFilePath);
		}
	}
}
