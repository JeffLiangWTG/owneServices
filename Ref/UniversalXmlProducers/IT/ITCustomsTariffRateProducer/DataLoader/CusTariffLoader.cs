using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	public class CusTariffLoader : BaseDataLoader
	{
		IEnumerable<string> _cusTariffCodes;

		public CusTariffLoader(ILogger logger, IHttpHandler handler) : base(logger, handler)
		{
		}

		public override bool Load(string baseUrl)
		{
			var loadSuccessful = false;

			var requestUrl = baseUrl + GetQueryString();
			var response = Handler.Get(requestUrl);
			if (response == null || !response.IsSuccessStatusCode || response.Content == null)
			{
				Logger.Log(ErrorMessagesConstant.TariffCodeLoaderNoResponseFromDatabase);
				return false;
			}

			var rawData = response.Content.ReadAsStringAsync()?.Result;
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
				Logger.Log(ErrorMessagesConstant.TariffCodeLoaderUnableToLoadFromDatabase);
			}

			return loadSuccessful;
		}

		public IEnumerable<string> AllCodes => _cusTariffCodes;

		static string GetQueryString()
		{
			var today = DateTime.Today;
			var formattedDate = $"{today:s}{today.ToString("zzz", CultureInfo.InvariantCulture).Replace("+", "%2B").Replace("-", "%2D")}";

			return "RefCusTariffUpdate?$filter=ZZ1_ZZZ_NKDataGrouping%20eq%20%27" + MeasuresConstant.EuropeanUnionTradeCode + "%27" +
							"%20and%20RefCusTariffType/ZZI_TariffType%20eq%20%27" + MeasuresConstant.DefaultTariffType + "%27" +
							"%20and%20ZZ1_StartDate%20le%20" + formattedDate +
							"%20and%20ZZ1_EndDate%20ge%20" + formattedDate +
							"&$expand=RefCusTariffType" +
							"&$select=ZZ1_TariffCode";
		}
	}
}
