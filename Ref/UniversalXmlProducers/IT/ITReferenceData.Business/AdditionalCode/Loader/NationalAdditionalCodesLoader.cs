using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.ITReferenceData.Services;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode
{
	public sealed class NationalAdditionalCodesLoader : IRawAdditionalCodesLoader
	{
		public NationalAdditionalCodesLoader(IDateTimeProvider dateTimeProvider, IHttpClient httpClient)
		{
			this.dateTimeProvider = Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));
			this.httpClient = Argument.NotNull(httpClient, nameof(httpClient));
		}

		readonly IDateTimeProvider dateTimeProvider;
		readonly IHttpClient httpClient;

		IEnumerable<IRawAdditionalCode> IRawAdditionalCodesLoader.GetRawAdditionalCodes()
		{
			var requestObjectParameters = GetAllRequestObjectParameters();
			return GetRawAdditionalCodes(requestObjectParameters).ToArray();
		}

		IEnumerable<IRawAdditionalCode> GetRawAdditionalCodes(IEnumerable<INationalRawAdditionalCodeRequestObjectParameters> requestObjectParameterList)
		{
			foreach (var requestObjectParameter in requestObjectParameterList)
			{
				var requestObject = new NationalAdditionalRequestObject(dateTimeProvider, requestObjectParameter).Build();
				string responseString = null;
				using (var formContent = new FormUrlEncodedContent(requestObject))
				{
					responseString = httpClient.Post(ApplicationConfig.AdditionalCodesUrl, formContent);
				}
				yield return new NationalRawAdditionalCode(responseString);
			}
		}

		IEnumerable<INationalRawAdditionalCodeRequestObjectParameters> GetAllRequestObjectParameters()
		{
			var result = new List<INationalRawAdditionalCodeRequestObjectParameters>();
			foreach (var additionalCodeType in GetAllAdditionalCodeTypes())
			{
				var matchedLinks = GetAllAdditionalCodeLinkMatchesForGivenType(additionalCodeType);

				var requestObjectListForAdditionalCodeType = ConvertMatchedLinksToRequestObjects(matchedLinks);
				result.AddRange(requestObjectListForAdditionalCodeType);
			}
			return result;
		}

		static INationalRawAdditionalCodeRequestObjectParameters[] ConvertMatchedLinksToRequestObjects(IEnumerable<Match> matchedLinks) => matchedLinks.Select(match => NationalRawAdditionalCodeRequestObjectParameters.Build(match)).ToArray();

		IEnumerable<Match> GetAllAdditionalCodeLinkMatchesForGivenType(string additionalCodeType)
		{
			var requestObject = new NationalAdditionalCodeListRequestObject(dateTimeProvider, additionalCodeType).Build();
			string responseString = null;
			using (var formContent = new FormUrlEncodedContent(requestObject))
			{
				responseString = httpClient.Post(ApplicationConfig.AdditionalCodesUrl, formContent);
			}
			return Regex.Matches(responseString, Constants.Regex.AdditionalCodeLink).Cast<Match>();
		}

		IEnumerable<string> GetAllAdditionalCodeTypes()
		{
			var requestObject = new NationalAdditionalCodeTypeListRequestObject(dateTimeProvider);
			string responseString = null;
			using (var formContent = new FormUrlEncodedContent(requestObject.Build()))
			{
				responseString = httpClient.Post(ApplicationConfig.TaricServletUrl, formContent);
			}
			var typeMatches = Regex.Matches(responseString, Constants.Regex.AdditionalCodeType);
			return typeMatches.Cast<Match>().Select(x => x.Groups["Type"].Value);
		}
	}
}
