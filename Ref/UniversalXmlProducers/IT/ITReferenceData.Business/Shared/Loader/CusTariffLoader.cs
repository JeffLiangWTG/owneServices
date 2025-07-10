using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.ITReferenceData.Business.Shared;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public class CusTariffLoader : BaseDataLoader, ICusTariffLoader
	{
		IEnumerable<string> _cusTariffCodes;

		TariffType _type;

		public CusTariffLoader(ILogger logger, HttpClient httpClient, TariffType type) : base(logger, httpClient)
		{
			_type = type;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public override async Task<bool> LoadAsync(Uri baseUri)
		{
			var loadSuccessful = false;
			var requestUri = Utils.BuildUri(baseUri, GetQueryString());

			string rawData = null;

			try
			{

				using var response = await HttpClient.GetWithRetryAsync(requestUri);
				rawData = await response.Content.ReadAsStringAsync();
			}
			catch (HttpRequestException ex)
			{
				Logger.Log(ex, Constants.ErrorMessages.TariffCodeLoaderNoResponseFromDatabase);
				return false;
			}

			CusTariffDataWrapper cusTariffs = null;

			try
			{
				cusTariffs = JsonConvert.DeserializeObject<CusTariffDataWrapper>(rawData);
			}
			catch (Exception exception)
			{
				Logger.Log(exception);
			}

			_cusTariffCodes = cusTariffs?.Value?.Select(c => c.TariffCode);
			if (_cusTariffCodes != null)
			{
				loadSuccessful = true;
			}
			else
			{
				Logger.Log(Constants.ErrorMessages.TariffCodeLoaderUnableToLoadFromDatabase);
			}

			return loadSuccessful;
		}

		public IEnumerable<string> AllCodes => _cusTariffCodes;

		string GetQueryString()
		{
			var today = DateTime.Today;
			var formattedDate = $"{today:s}{today.ToString("zzz", CultureInfo.InvariantCulture).Replace("+", "%2B").Replace("-", "%2D")}";

			var tariffType = _type == TariffType.Import ? Constants.Measures.ImportTariffType : Constants.Measures.ExportTariffType;

			return "RefCusTariffUpdate?$filter=ZZ1_ZZZ_NKDataGrouping%20eq%20%27" + Constants.Measures.EuropeanUnionTradeCode + "%27" +
							"%20and%20RefCusTariffType/ZZI_TariffType%20eq%20%27" + tariffType + "%27" +
							"%20and%20ZZ1_StartDate%20le%20" + formattedDate +
							"%20and%20ZZ1_EndDate%20ge%20" + formattedDate +
							"&$expand=RefCusTariffType" +
							"&$select=ZZ1_TariffCode";
		}

		public enum TariffType
		{
			Import = 0,
			Export = 1
		}
	}
}
