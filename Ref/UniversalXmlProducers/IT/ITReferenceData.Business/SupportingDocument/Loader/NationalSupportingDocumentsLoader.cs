using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.ITReferenceData.Services;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public sealed class NationalSupportingDocumentsLoader : IRawSupportingDocumentsLoader
	{
		public NationalSupportingDocumentsLoader(IDateTimeProvider dateTimeProvider, IHttpClient httpClient)
		{
			this.dateTimeProvider = Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));
			this.httpClient = Argument.NotNull(httpClient, nameof(httpClient));
		}

		readonly IDateTimeProvider dateTimeProvider;
		readonly IHttpClient httpClient;

		IEnumerable<IRawSupportingDocument> IRawSupportingDocumentsLoader.GetRawSupportingDocuments()
		{
			var requestObjectParameters = GetRawSupportingDocumentRequestObjectParameters();
			return GetRawSupportingDocuments(requestObjectParameters).ToArray();
		}

		IEnumerable<IRawSupportingDocumentRequestObjectParameters> GetRawSupportingDocumentRequestObjectParameters()
		{
			var parameterDictionary = new NationalSupportingDocumentListRequestObject(dateTimeProvider).Build();
			string responseString = null;
			using (var formContent = new FormUrlEncodedContent(parameterDictionary))
			{
				responseString = httpClient.Post(ApplicationConfig.NationalSupportingDocumentsUrl, formContent);
			}
			var certificateLinks = Regex.Matches(responseString, Constants.Regex.NationalCertificateLink);
			return certificateLinks.Cast<Match>().Select(match => RawSupportingDocumentRequestObjectParameters.Build(match));
		}

		IEnumerable<IRawSupportingDocument> GetRawSupportingDocuments(IEnumerable<IRawSupportingDocumentRequestObjectParameters> requestObjectParameterList)
		{
			foreach (var parameter in requestObjectParameterList)
			{
				yield return GetRawSupportingDocument(parameter);
			}
		}

		IRawSupportingDocument GetRawSupportingDocument(IRawSupportingDocumentRequestObjectParameters requestObjectParameters)
		{
			var parameterDictionary = new NationalSupportingDocumentRequestObject(dateTimeProvider, requestObjectParameters).Build();
			string responseString = null;
			using (var formContent = new FormUrlEncodedContent(parameterDictionary))
			{
				responseString = httpClient.Post(ApplicationConfig.NationalSupportingDocumentsUrl, formContent);
			}
			return new NationalRawSupportingDocument(responseString);
		}
	}
}
