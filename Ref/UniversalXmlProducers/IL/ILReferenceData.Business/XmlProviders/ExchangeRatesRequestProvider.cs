using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.ILReferenceData.Business.ExchangeRatesRequest;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public class ExchangeRatesRequestProvider
	{
		public ExchangeRatesRequestProvider(ICurrencyRateSearchParam currencyRateSearchParam)
		{
			this.currencyRateSearchParam = Argument.NotNull(currencyRateSearchParam, nameof(currencyRateSearchParam));
		}

		public string GetRequestXml()
		{
			var request = BuildRequest();

			var xml = Helpers.Serialize(request);
			return xml;
		}

		CD_NG_8347_Web01_CurrencyRateSearchParam BuildRequest()
		{
			var currencyRateSearchParam = new CD_NG_8347_Web01_CurrencyRateSearchParam();

			var requestContentHeader = this.currencyRateSearchParam.RequestContentHeader;

			currencyRateSearchParam.RequestContentHeader = new RequestContentHeader()
			{
				TransmitionDateTime = requestContentHeader.TransmitionDateTime,
				RecieverID = new[] { requestContentHeader.RecieverID },
				SenderID = requestContentHeader.SenderID
			};

			var currencyRate = this.currencyRateSearchParam.CurrencyRate;
			currencyRateSearchParam.CurrencyRate = new CD_NG_8347_Web01_CurrencyRateSearchParamCurrencyRate()
			{
				currencyTypeID = currencyRate.currencyTypeID,

				fromDate = currencyRate.fromDate,
				toDate = currencyRate.toDate,
			};
			return currencyRateSearchParam;
		}


		readonly ICurrencyRateSearchParam currencyRateSearchParam;
	}
}
