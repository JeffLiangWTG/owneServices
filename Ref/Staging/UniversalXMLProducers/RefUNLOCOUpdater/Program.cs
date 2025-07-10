using System;
using System.IO;
using System.Net;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater
{
	static class Program
	{
		static int Main(string[] args)
		{
			return (int)ProduceXml(args);
		}

		static ProducerStatus ProduceXml(string[] args)
		{
			var scraper = new ScrapePageHTMLDownloader();
			using var mdbFileDownloader = new MdbFileDownloader(scraper, httpClientHelper);
			var (downloadMdbPath, publicationDate) = mdbFileDownloader.GetMdbFileAndPublicationDate();
			if (string.IsNullOrEmpty(downloadMdbPath))
			{
				Console.Error.WriteLine("Downloaded mdb file path is null or empty.");
				return ProducerStatus.Failure;
			}
			var safeRepository = new SafeRepository(new Uri(ConfigurationProvider.SafeDataUpdateUri));
			var uneceUpdater = new UNECEUpdater(safeRepository);
			uneceUpdater.Read(downloadMdbPath);
			var iataUpdater = new IATAUpdater();
			iataUpdater.Read(ConfigurationProvider.ProgramSpecificConfigurationsIATAFilePath);
			var unlocoXmlWriter = new XmlWriter(XmlWriterHelper.GetRefUNLOCOWriterConfiguration(false));
			var unlocoXmlWithoutIATAWriter = new XmlWriter(XmlWriterHelper.GetRefUNLOCOWriterConfiguration(false, false));
			var unlocoXmlWriterWithCoordinates = new XmlWriter(XmlWriterHelper.GetRefUNLOCOWriterConfiguration(true));
			if (iataUpdater.IATAs != null && uneceUpdater.UNLOCOes != null)
			{
				XmlWriterHelper.SetXMLWriter(unlocoXmlWriter, "UNLOCO Updater", publicationDate);
				unlocoXmlWriter.SetDependency(new Dependency("UN Country States", publicationDate, DependencyType.Preferred));
				XmlWriterHelper.SetXMLWriter(unlocoXmlWithoutIATAWriter, "UNLOCO Airports Without IATA", publicationDate);
				unlocoXmlWithoutIATAWriter.SetDependency(new Dependency("UN Country States", publicationDate, DependencyType.Preferred));
				XmlWriterHelper.SetXMLWriter(unlocoXmlWriterWithCoordinates, "UNLOCO Updater With Coordinates", publicationDate);
				unlocoXmlWriterWithCoordinates.SetDependency(new Dependency("UN Country States", publicationDate, DependencyType.Preferred));
				unlocoXmlWriterWithCoordinates.SetDependency(new Dependency("UNLOCO Updater", publicationDate, DependencyType.Preferred));
				unlocoXmlWriterWithCoordinates.SetDependency(new Dependency("UNLOCO Airports Without IATA", publicationDate, DependencyType.Preferred));
				var xmlParser = new UXMLParser(uneceUpdater.UNLOCOes, iataUpdater.IATAs, unlocoXmlWriter, unlocoXmlWithoutIATAWriter, unlocoXmlWriterWithCoordinates);
				xmlParser.Parse();
			}
			CountryStatesParser(publicationDate, downloadMdbPath);
			unlocoXmlWriter.SaveXml(GetXMLOutputPath("RefUNLOCO_Updater"));
			unlocoXmlWithoutIATAWriter.SaveXml(GetXMLOutputPath("RefUNLOCO_AirportsWithoutIATA"));
			unlocoXmlWriterWithCoordinates.SaveXml(GetXMLOutputPath("RefUNLOCO_Updater_WithCoordinates"));

			return ProducerStatus.Success;
		}

		static string GetXMLOutputPath(string prefix)
		{
			var outputPath = ConfigurationProvider.OutputFilePath;
			return Path.Combine(outputPath, $"{prefix}_{DateTime.UtcNow:yyyyMMdd}.xml");
		}

		static void CountryStatesParser(DateTime publicationTime, string mdbFileDownloadPath)
		{
			Argument.NotNullOrEmpty(mdbFileDownloadPath, nameof(mdbFileDownloadPath));
			var outputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationProvider.OutputFilePath, "UNCountryStates.xml");
			new CountryStatesParser(publicationTime, outputFilePath, mdbFileDownloadPath).GenerateXml();
		}

		static readonly IHttpClientHelper httpClientHelper = new HttpClientHelper(new WebProxy(
			ConfigurationProvider.ProxyService, BypassOnLocal: false, null, new NetworkCredential(ConfigurationProvider.ProxyUsername, ConfigurationProvider.ProxyPassword)));
	}
}
