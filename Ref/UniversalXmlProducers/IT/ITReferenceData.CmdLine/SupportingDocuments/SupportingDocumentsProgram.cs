using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using Types = CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument.Constants.RefCusCodeListCodeTypes;

namespace CargoWise.RefDbRepo.ITReferenceData.CmdLine.SupportingDocuments
{
	public static class SupportingDocumentsProgram
	{
		public static void Run(string outputPath)
		{
			using (var httpClient = new HttpClient())
			{
				var dateTimeProvider = new DateTimeProvider();
				var httpClientWrapper = new HttpClientWrapper(httpClient);

				var supportingDocuments = GetAllSupportingDocuments(dateTimeProvider, httpClientWrapper);

				var publicationTimeLoader = new PublicationTimeLoader(dateTimeProvider, httpClientWrapper);
				var xmlProducerOption = new XmlProducerOption(dateTimeProvider, publicationTimeLoader);
				var xmlProducer = new SupportingDocumentsXmlProducer(xmlProducerOption);
				xmlProducer.ExportToXml(supportingDocuments, outputPath);
			}
		}

		static IEnumerable<RefCusCodeList> GetAllSupportingDocuments(IDateTimeProvider dateTimeProvider, HttpClientWrapper httpClientWrapper)
		{
			var result = new List<RefCusCodeList>();

			AddNationalSupportingDocuments(result, dateTimeProvider, httpClientWrapper);
			AddEuropeanSupportingDocuments(result, dateTimeProvider, httpClientWrapper);
			AddAdditionalReferenceSupportingDocuments(result, dateTimeProvider, httpClientWrapper);

			return result;
		}

		static void AddNationalSupportingDocuments(List<RefCusCodeList> items, IDateTimeProvider dateTimeProvider, HttpClientWrapper httpClientWrapper)
		{
			var nationalSupportingDocumentsLoader = new NationalSupportingDocumentsLoader(dateTimeProvider, httpClientWrapper);
			var supportingDocumentsProducer = new SupportingDocumentsProducer(nationalSupportingDocumentsLoader, new RawSupportingDocumentMapper());
			items.AddRange(supportingDocumentsProducer.ProduceEntities());
		}

		static void AddEuropeanSupportingDocuments(List<RefCusCodeList> items, IDateTimeProvider dateTimeProvider, HttpClientWrapper httpClientWrapper)
		{
			IRawSupportingDocumentsLoader refDataRepoSupportingDocumentsNctsLoader = new RefDataRepoEuropeanSupportingDocumentCachedLoader(dateTimeProvider, httpClientWrapper, Types.SupportingDocumentNcts);

			var result = GetEuropeanSupportingDocuments();
			AddCommunitySupportingDocuments(result);

			items.AddRange(result);

			List<RefCusCodeList> GetEuropeanSupportingDocuments()
			{
				var europeanSupportingDocumentsLoader = new EuropeanSupportingDocumentsLoader(dateTimeProvider, httpClientWrapper);

				var validEuropeanCodes = refDataRepoSupportingDocumentsNctsLoader
					.GetRawSupportingDocuments()
					.Select(x => x.Code)
					.ToHashSet();

				var supportingDocumentsProducer = new SupportingDocumentsProducer(europeanSupportingDocumentsLoader, new RawSupportingDocumentMapper(validEuropeanCodes));
				return supportingDocumentsProducer.ProduceEntities().ToList();
			}

			void AddCommunitySupportingDocuments(List<RefCusCodeList> europeanItems)
			{
				var excludedNctsCodes = europeanItems
					.Where(x => x.ZZD_ZZK_NKCodeType == Types.SupportingDocumentNcts)
					.Select(x => x.ZZD_Code)
					.ToHashSet();

				var refDataRepoNctsDocumentsProducer = new SupportingDocumentsProducer(refDataRepoSupportingDocumentsNctsLoader, new NctsRawSupportingDocumentMapper(excludedNctsCodes));
				items.AddRange(refDataRepoNctsDocumentsProducer.ProduceEntities());
			}
		}

		static void AddAdditionalReferenceSupportingDocuments(List<RefCusCodeList> items, IDateTimeProvider dateTimeProvider, HttpClientWrapper httpClientWrapper)
		{
			IRawSupportingDocumentsLoader refDataRepoSupportingDocumentsAdditionalReferenceLoader = new RefDataRepoEuropeanSupportingDocumentCachedLoader(dateTimeProvider, httpClientWrapper, Types.SupportingDocumentAdditionalReference);

			var refDataAdditionalReferenceCodes = refDataRepoSupportingDocumentsAdditionalReferenceLoader
				.GetRawSupportingDocuments()
				.Select(x => x.Code)
				.ToHashSet();

			IRawSupportingDocumentsLoader europeanSupportingDocumentsLoader = new EuropeanSupportingDocumentsLoader(dateTimeProvider, httpClientWrapper);
			var rawDataSource = europeanSupportingDocumentsLoader.GetRawSupportingDocuments();

			var refDataRepoNctsDocumentsProducer = new SupportingDocumentsProducer(europeanSupportingDocumentsLoader, new AdditionalReferenceRawSupportingDocumentMapper(refDataAdditionalReferenceCodes));
			items.AddRange(refDataRepoNctsDocumentsProducer.ProduceEntities());
		}
	}
}
