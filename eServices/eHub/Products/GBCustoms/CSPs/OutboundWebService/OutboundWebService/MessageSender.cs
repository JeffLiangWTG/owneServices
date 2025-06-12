using Common.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.eHub.Products.GBCustoms.CSP.OutboundWebService
{
	public class MessageSender : IMessageSender
	{
		public async Task<HttpResponseMessage> SendMessage(ILog logger, string logPrefix, HttpRequestMessage httpRequestMessage, string uri)
		{
			httpRequestMessage.RequestUri = new Uri(uri);
			logger.Debug($"{logPrefix}Request will be sent to {uri}");
			httpRequestMessage.Headers.Remove("Host");

			using (var client = new HttpClient())
			{
				return await client.SendAsync(httpRequestMessage);
			}
		}
	}
}
