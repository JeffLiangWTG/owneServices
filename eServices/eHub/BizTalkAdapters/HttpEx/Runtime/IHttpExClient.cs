using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.eHub.BizTalkAdapters.HttpEx
{
	internal interface IHttpExClient
	{
		Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage);
	}
}