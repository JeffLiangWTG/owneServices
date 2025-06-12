using System.Net.Http;

namespace CargoWise.eHub.BizTalkAdapters.HttpEx
{
	internal interface IHttpExClientFactory
	{
		IHttpExClient CreateHttpExClient(WebRequestHandler handler);
	}

	internal class HttpExClientFactory : IHttpExClientFactory
	{
		public virtual IHttpExClient CreateHttpExClient(WebRequestHandler handler)
		{
			return new HttpExClient(handler);
		}
	}
}