using System.Collections.Generic;
using System.Net.Http;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode;

namespace CargoWise.RefDbRepo.ITReferenceData.CmdLine.AdditionalCodes
{
	public static class AdditionalCodesProgram
	{
		public static void Run(string outputPath)
		{
			using (var httpClient = new HttpClient())
			{
				var dateTimeProvider = new DateTimeProvider();
				var httpClientWrapper = new HttpClientWrapper(httpClient);

				var additionalCodes = GetRawAdditionalCodes(dateTimeProvider, httpClientWrapper);

				var publicationTimeLoader = new PublicationTimeLoader(dateTimeProvider, httpClientWrapper);
				var xmlProducerOption = new XmlProducerOption(dateTimeProvider, publicationTimeLoader);
				var xmlProducer = new AdditionalCodesXmlProducer(xmlProducerOption);
				xmlProducer.ExportToXml(additionalCodes, outputPath);
			}
		}

		static IEnumerable<RefCusCodeList> GetRawAdditionalCodes(DateTimeProvider dateTimeProvider, HttpClientWrapper httpClientWrapper)
		{
			var additionalCodesLoader = (IRawAdditionalCodesLoader)new NationalAdditionalCodesLoader(dateTimeProvider, httpClientWrapper);
			var additionalCodesProducer = new AdditionalCodesProducer(additionalCodesLoader, new RawAdditionalCodeMapper());
			return additionalCodesProducer.ProduceEntities();
		}
	}
}
