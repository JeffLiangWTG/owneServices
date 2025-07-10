using System.Net.Http;

namespace Enterprise.MasterFiles.Business.Accounting
{
	public interface IHttpClientProvider
	{
		HttpClient GetHttpClient();
	}

	public class HttpClientProvider : IHttpClientProvider
	{
		public HttpClient GetHttpClient() => new HttpClient();
	}
}
