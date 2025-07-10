using System;
using System.Net.Http;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExchangeRates
{
	public sealed class ItalyExchangeRateMetaDataFetcher : IExchangeRateMetaDataFetcher
	{
		readonly ExchangeRateMetaData _metaData;

		public ItalyExchangeRateMetaDataFetcher(ExchangeRateMetaData metaData)
		{
			_metaData = metaData;
		}

		ExchangeRateMetaData IExchangeRateMetaDataFetcher.Fetch()
		{
			using (var httpClient = Utils.GetClient())
			{
				IHttpClient httpClientWrapper = new HttpClientWrapper(httpClient);

				var html = httpClientWrapper.Get(_metaData.Source);
				_metaData.Content = html;
			}
			return _metaData;
		}

		public ExchangeRateMetaData FetchPublicationTime()
		{
			DateTime result;
			using (var httpClient = new HttpClient())
			{
				var response = httpClient.GetAsync(new Uri(_metaData.DataLocation)).GetAwaiter().GetResult();
				result = response.Content.Headers.LastModified.GetValueOrDefault().DateTime;
			}

			_metaData.PublicationTime = result;
			return _metaData;
		}
	}
}
