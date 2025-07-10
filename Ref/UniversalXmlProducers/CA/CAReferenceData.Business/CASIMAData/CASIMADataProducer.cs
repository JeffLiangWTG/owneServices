using System.IO;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CASIMAData
{
	public class CASIMADataProducer
	{
		public void QueryDataAndParseToXMLFile()
		{
			var helper = new HttpClientHelper();

			using (var client = new WebDriverHelper())
			{
				var parser = new WebSIMAParserV2(client, new SIMATextExtractor(helper, ApplicationConfig.EntityMatcherUri));
				ProcessCASIMA(parser);
			}
		}

		public void ProcessCASIMA(IWebSIMAParserV2 parser)
		{
			var writerConfiguration = XMLWriterHelper.GetWriterConfigurationForCASIMADataAndCASurtax();
			var writer = new XmlWriter(writerConfiguration);
			var rateFactory = new SIMARatesFactory();
			foreach (var simaText in parser.GetWebSIMAText(ApplicationConfig.SimaUrl, ApplicationConfig.SimaBaseUrl))
			{
				writer.PopulateData(rateFactory.GetSIMARate(simaText));
			}
			writer.SetDataSource("CA SIMA");
			writer.SetPublicationTime(parser.PublishedDate);
			writer.SetUpdateType(UpdateType.Full);

			writer.SaveXml(Path.Combine(ApplicationConfig.OutputPath, ApplicationConfig.CASIMAFilename));
		}
	}
}
