using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.ITReferenceData.Services;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public sealed class EuropeanSupportingDocumentsLoader : IRawSupportingDocumentsLoader
	{
		public EuropeanSupportingDocumentsLoader(IDateTimeProvider dateTimeProvider, IHttpClient httpClient)
		{
			this.dateTimeProvider = Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));
			this.httpClient = Argument.NotNull(httpClient, nameof(httpClient));
		}

		readonly IDateTimeProvider dateTimeProvider;
		readonly IHttpClient httpClient;

		IEnumerable<IRawSupportingDocument> IRawSupportingDocumentsLoader.GetRawSupportingDocuments()
		{
			(var requestObjectParameters, var preProccesedRawSupportingDocuments) = GetRequestObjectParametersAndPreProcessedItems();

			var rawSupportingDocuments = GetRawSupportingDocuments(requestObjectParameters).ToList();
			rawSupportingDocuments.AddRange(preProccesedRawSupportingDocuments);
			return rawSupportingDocuments;
		}

		(List<IEuropeanRawSupportingDocumentRequestObjectParameters> RequestObjectParameters, List<IRawSupportingDocument> RawSupportingDocuments) GetRequestObjectParametersAndPreProcessedItems()
		{
			var requestObjectParameters = new List<IEuropeanRawSupportingDocumentRequestObjectParameters>();
			var rawSupportingDocuments = new List<IRawSupportingDocument>();

			foreach (var certificateType in GetAllSupportingDocumentTypes())
			{
				var parameterDictionary = new EuropeanSupportingDocumentListRequestObject(dateTimeProvider, certificateType).Build();
				string responseString = null;
				using (var formContent = new FormUrlEncodedContent(parameterDictionary))
				{
					responseString = httpClient.Post(ApplicationConfig.EuropeanSupportingDocumentsUrl, formContent);
				}
				var certificateLinks = Regex.Matches(responseString, Constants.Regex.EuropeanCertificateLink).Cast<Match>();
				if (!certificateLinks.Any())
				{
					TryPreProcessSupportingDocument(rawSupportingDocuments, responseString);
				}
				else
				{
					requestObjectParameters.AddRange(certificateLinks.Select(match => EuropeanRawSupportingDocumentRequestObjectParameters.Build(match)));
				}
			}
			return (requestObjectParameters, rawSupportingDocuments);
		}

		static void TryPreProcessSupportingDocument(List<IRawSupportingDocument> rawSupportingDocuments, string responseString)
		{
			var supportingDocument = ParseEuropeanRawSupportingDocument(responseString);
			if (!string.IsNullOrWhiteSpace(supportingDocument.Code))
			{
				rawSupportingDocuments.Add(supportingDocument);
			}
		}

		IEnumerable<string> GetAllSupportingDocumentTypes()
		{
			var typesReqObject = new EuropeanSupportingDocumentTypeListRequestObject(dateTimeProvider);
			string responseString = null;
			using (var formContent = new FormUrlEncodedContent(typesReqObject.Build()))
			{
				responseString = httpClient.Post(ApplicationConfig.TaricServletUrl, formContent);
			}
			var certificateLinks = Regex.Matches(responseString, Constants.Regex.EuropeanCertificateType);
			return certificateLinks.Cast<Match>().Select(x => x.Groups["Type"].Value);
		}

		IEnumerable<IRawSupportingDocument> GetRawSupportingDocuments(IEnumerable<IEuropeanRawSupportingDocumentRequestObjectParameters> requestObjectParameterList)
		{
			foreach (var parameter in requestObjectParameterList)
			{
				yield return GetRawSupportingDocument(parameter);
			}
		}

		IRawSupportingDocument GetRawSupportingDocument(IEuropeanRawSupportingDocumentRequestObjectParameters requestObjectParameters)
		{
			var parameterDictionary = new EuropeanSupportingDocumentRequestObject(dateTimeProvider, requestObjectParameters).Build();
			string responseString = null;
			using (var formContent = new FormUrlEncodedContent(parameterDictionary))
			{
				responseString = httpClient.Post(ApplicationConfig.EuropeanSupportingDocumentsUrl, formContent);
			}
			return ParseEuropeanRawSupportingDocument(responseString);
		}

		static IRawSupportingDocument ParseEuropeanRawSupportingDocument(string responseString) => new EuropeanRawSupportingDocument(responseString);
	}
}
