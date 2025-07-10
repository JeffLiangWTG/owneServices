using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler
{
	public class WebResponseHandler : IWebResponseHandler
	{
		readonly IHttpHandler _httpHandler;
		readonly IDateTimeProvider _dateTimeProvider;

		public Dictionary<string, CertificateData> cachedCertificateData { get; set; }

		public WebResponseHandler(IHttpHandler httpHandler, IDateTimeProvider dateTimeProvider)
		{
			_httpHandler = Argument.NotNull(httpHandler, nameof(httpHandler));
			_dateTimeProvider = Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));

			cachedCertificateData = new Dictionary<string, CertificateData>();
		}

		public async Task<ResponseResult> GetWebRepsonseForTariffAsync(string requestUrl, string cusTariffCode)
		{
			if (cusTariffCode.Length < 10)
			{
				return new ResponseResult(false, ErrorMessagesConstant.InvalidLengthTariffCode, Environment.StackTrace);
			}

			var requestObjectValues = new TariffDownloadRequestObjectValues(cusTariffCode, _dateTimeProvider);
			using (var content = new FormUrlEncodedContent(requestObjectValues.AsDictionary()))
			{

				var response = await PostAsyncWithRetryPolicy(requestUrl, content);
				if (!response.IsSuccessStatusCode)
				{
					return new ResponseResult(false, ErrorMessagesConstant.TariffCodeNoResponseFromWeb, Environment.StackTrace);
				}

				var responseString = await response.Content.ReadAsStringAsync();
				return new ResponseResult(true, responseString, null);
			}
		}

		public async Task<ResponseResult> GetWebRepsonseForCertificateLinkAsync(string requestUrl, string requirementHtml)
		{
			var requestObjectValues = new CertificateLinkDownloadRequestObjectValues(requirementHtml, _dateTimeProvider);
			using (var content = new FormUrlEncodedContent(requestObjectValues.AsDictionary()))
			{
				var response = await PostAsyncWithRetryPolicy(requestUrl, content);
				if (!response.IsSuccessStatusCode)
				{
					return new ResponseResult(false, ErrorMessagesConstant.TariffCodeNoResponseFromWeb, Environment.StackTrace);
				}

				var responseString = await response.Content.ReadAsStringAsync();
				return new ResponseResult(true, responseString, null);
			}
		}

		public async Task<ResponseResult> GetWebResponseForCertificateAsync(string requestUrl, string parameterString, string requirementHtml, string tariffCode)
		{
			var requestObjectValues = new CertificateDownloadRequestObjectValues(parameterString, new CertificateLinkDownloadRequestObjectValues(requirementHtml, _dateTimeProvider).AsDictionary(), tariffCode, _dateTimeProvider);
			using (var content = new FormUrlEncodedContent(requestObjectValues.AsDictionary()))
			{
				var response = await PostAsyncWithRetryPolicy(requestUrl, content);
				if (!response.IsSuccessStatusCode)
				{
					return new ResponseResult(false, ErrorMessagesConstant.TariffCodeNoResponseFromWeb, Environment.StackTrace);
				}

				var responseString = await response.Content.ReadAsStringAsync();
				return new ResponseResult(true, responseString, null);
			}
		}

		public async Task<ResponseResult> GetWebResponseForPublicationDateAsync(string requestUrl)
		{
			var response = await GetAsyncWithRetryPolicy(requestUrl);
			if (!response.IsSuccessStatusCode)
			{
				return new ResponseResult(false, ErrorMessagesConstant.TariffCodeNoResponseFromWeb, Environment.StackTrace);
			}

			var responseString = await response.Content.ReadAsStringAsync();
			return new ResponseResult(true, responseString, null);
		}

		async Task<HttpResponseMessage> PostAsyncWithRetryPolicy(string url, HttpContent content, int currentRetryAttempt = 0)
		{
			var responseMessage = await _httpHandler.PostAsync(url, content);
			if (responseMessage.IsSuccessStatusCode || currentRetryAttempt == MaxNumberOfRetryAttempts)
			{
				return responseMessage;
			}
			return await PostAsyncWithRetryPolicy(url, content, currentRetryAttempt + 1);
		}

		async Task<HttpResponseMessage> GetAsyncWithRetryPolicy(string url, int currentRetryAttempt = 0)
		{
			var responseMessage = await _httpHandler.GetAsync(url);
			if (responseMessage.IsSuccessStatusCode || currentRetryAttempt == MaxNumberOfRetryAttempts)
			{
				return responseMessage;
			}
			return await GetAsyncWithRetryPolicy(url, currentRetryAttempt + 1);
		}

		const int MaxNumberOfRetryAttempts = 3;
	}
}
