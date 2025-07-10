using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.ITReferenceData.Services;
using Newtonsoft.Json.Linq;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public sealed class RefDataRepoEuropeanSupportingDocumentCachedLoader : IRawSupportingDocumentsLoader
	{
		public RefDataRepoEuropeanSupportingDocumentCachedLoader(IDateTimeProvider dateTimeProvider, IHttpClient httpClient, string codeType)
		{
			this.dateTimeProvider = Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));
			this.httpClient = Argument.NotNull(httpClient, nameof(httpClient));
			this.codeType = Argument.NotNullOrEmpty(codeType, nameof(codeType));
		}

		IEnumerable<IRawSupportingDocument> IRawSupportingDocumentsLoader.GetRawSupportingDocuments()
		{
			if(cachedResult != null)
			{
				return cachedResult;
			}

			cachedResult = new List<IRawSupportingDocument>();
			var response = httpClient.Get(GetRequestUrl());

			var jsonObj = JObject.Parse(response);
			var valueArray = (JArray)jsonObj["value"];

			foreach (var item in valueArray)
			{
				var code = item["ZZD_Code"].ToString();
				var startDate = item["ZZD_StartDate"].ToObject<DateTime>();
				var endDate = item["ZZD_EndDate"].ToObject<DateTime>();
				var description = item["ZZD_Description"].ToString();

				cachedResult.Add(new RefDataRepoRawEuropeanSupportingDocument(
					code,
					startDate,
					endDate,
					description));
			}

			return cachedResult;
		}

		string GetRequestUrl()
		{
			var urlBuilder = new StringBuilder(ApplicationConfig.RefDataRepoUrl);
			if(!urlBuilder.ToString().EndsWith("/", StringComparison.InvariantCulture))
			{
				urlBuilder.Append('/');
			}
			urlBuilder
				.Append("RefCusCodeListUpdate/Default.GetWithOptimizedExpand()?$filter=")
				.Append(CultureInfo.InvariantCulture, $"ZZD_ZZK_NKCodeType EQ '{codeType}' ")
				.Append("AND ZZD_ZZZ_NKDataGrouping EQ 'EUN' ")
				.Append(CultureInfo.InvariantCulture, $"AND ZZD_StartDate LE {dateTimeProvider.Now:yyyy-MM-ddTHH:mm:ssZ} ")
				.Append(CultureInfo.InvariantCulture, $"AND ZZD_EndDate GE {dateTimeProvider.Now:yyyy-MM-ddTHH:mm:ssZ}");
			return urlBuilder.ToString();
		}


		List<IRawSupportingDocument> cachedResult;

		readonly IDateTimeProvider dateTimeProvider;
		readonly IHttpClient httpClient;
		readonly string codeType;
	}
}
