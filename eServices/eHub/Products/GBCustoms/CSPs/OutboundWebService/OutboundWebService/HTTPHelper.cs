using Common.Logging;
using System.Net;
using System.Net.Http;

namespace CargoWise.eHub.Products.GBCustoms.CSP.OutboundWebService
{
	public static class HTTPHelper
	{
		public static HttpResponseMessage CreateFailureResponse(ILog logger, string logPrefix, HttpStatusCode httpStatusCode, string errorDescription)
		{
			logger.Error($"{logPrefix}{errorDescription}");
			var httpResponseMessage = new HttpResponseMessage(httpStatusCode);
			httpResponseMessage.Content = new StringContent(errorDescription);
			return httpResponseMessage;
		}
	}
}
