using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using CargoWise.RefDbRepo.Staging.Common;
using CargoWise.RefDbRepo.XmlProducer.Common;
using OpenQA.Selenium;

[assembly: InternalsVisibleTo("CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer.Test")]

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer
{
	class Program
	{
		static int Main(string[] args)
		{
			return (int)ProduceXml();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		static ProducerStatus ProduceXml()
		{
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

			var filePathTariff = Path.Combine(ConfigurationProvider.OutputFilePath, $"RefCusTariff_ZA_{DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.xml");
			var filePathNomenclatureGroup = Path.Combine(ConfigurationProvider.OutputFilePath, $"RefCusNomenclatureGroup_ZA_{DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.xml");

			var nomenclatureGroupParser = new ZANomenclatureGroupParser();
			try
			{
				var fileDownloader = new FileDownloader(new Uri(ConfigurationProvider.PDFDownloadUrl));
				nomenclatureGroupParser.Parse(fileDownloader);
			}
			catch (WebDriverException wex)
			{
				Console.Error.WriteLine("Unable to connect to selenium server.");
				Console.Error.WriteLine(wex);
				return ProducerStatus.Failure;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("Error during the execution of the program.");
				Console.Error.WriteLine(ex);
				return ProducerStatus.Failure;
			}

			nomenclatureGroupParser.ExportToXml(filePathNomenclatureGroup, filePathTariff);
			return ProducerStatus.Success;
		}
	}
}
