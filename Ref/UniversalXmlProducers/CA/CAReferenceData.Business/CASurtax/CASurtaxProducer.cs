using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CASurtax
{
	public class CASurtaxProducer
	{
		public void QueryDataAndParseToXMLFile()
		{
			var fileDownloader = new Staging.Common.FileDownloader(new Uri(ApplicationConfig.TariffDbUrl));
			var workingDirectory = Path.Combine(Path.GetTempPath(), "CASIMAProducer");
			Directory.CreateDirectory(workingDirectory);
			var saveTo = Path.Combine(workingDirectory, "sima.zip");
			var extractedFiles = fileDownloader.SaveAndExtract(saveTo);
			var mdbFile = extractedFiles.FirstOrDefault(x => x.EndsWith(".mdb"));
			var writer = new XmlWriter(XMLWriterHelper.GetWriterConfigurationForCASIMADataAndCASurtax());
			using (var httpClient = new HttpClient())
			using (var obdbConnection = OdbcConnectionHelper.GetOdbcConnection(mdbFile))
			{
				obdbConnection.Open();
				var tariffProducer = new TariffDataProducer(obdbConnection);
				using (var stream = httpClient.GetStreamAsync(new Uri(ApplicationConfig.SurtaxUrl))?.Result)
				{
					var surTaxRateFactory = new SurTaxRatesFactory(tariffProducer);
					foreach (var surTaxText in WebSurTaxParser.Parse(stream))
					{
						foreach (var tariff in surTaxRateFactory.GetTariffs(surTaxText))
						{
							writer.PopulateData(tariff);
						}
					}
				}
			}
			writer.SetDataSource("CA Surtax");
			writer.SetPublicationTime(DateTime.Today);
			writer.SetUpdateType(UpdateType.Partial);

			writer.SaveXml(Path.Combine(ApplicationConfig.OutputPath, ApplicationConfig.CASIMAFilename));
		}
	}
}
