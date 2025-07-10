using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProductCodeProducer
{
	class Program
	{
		static int Main(string[] args)
		{
			ProduceXmls();
			return (int)ProducerStatus.Success;
		}

		static void ProduceXmls()
		{
			string binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var csvFileName = ConfigurationProvider.ProductCommodityCodesFileToProcess;
			var csvFilePath = Path.Combine(binPath, csvFileName);

			var outputFolderPath = ConfigurationProvider.OutputFolderPath;
			var outputCommodityCodeFileName = ConfigurationProvider.OutputCommodityCodesFileName;
			var outputProductCodeFileName = ConfigurationProvider.OutputProductCodesAndPivotsFileName;

			XMLCreator.CreateXMLsFromCSVFile(csvFilePath, outputFolderPath, outputCommodityCodeFileName, outputProductCodeFileName, DateTime.Now);
		}
	}
}
