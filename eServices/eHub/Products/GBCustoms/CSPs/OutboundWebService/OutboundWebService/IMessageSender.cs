using Common.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.eHub.Products.GBCustoms.CSP.OutboundWebService
{
	public interface IMessageSender
	{
		Task<HttpResponseMessage> SendMessage(ILog logger, string logPrefix, HttpRequestMessage httpRequestMessage, string uri);
	}
}
