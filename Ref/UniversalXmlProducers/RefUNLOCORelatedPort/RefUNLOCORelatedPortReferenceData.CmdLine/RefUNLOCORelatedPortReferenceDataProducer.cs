using System;
using System.Net.Http;
using CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.Business;

namespace CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.CmdLine
{
	public static class RefUNLOCORelatedPortReferenceDataProducer
	{
		public static void ProduceXml(HttpClient client)
		{
			var url = AppConfigurationProvider.AppConfiguration.PortMappingsApiUrl;
			var portMappings = PortMappingsDataLoader.GetPortMappingsAsync(client, url).Result;
			RelatedPortXmlWriter.Write(portMappings, AppConfigurationProvider.AppConfiguration.OutputPath, DateTime.Now);
		}
	}
}
