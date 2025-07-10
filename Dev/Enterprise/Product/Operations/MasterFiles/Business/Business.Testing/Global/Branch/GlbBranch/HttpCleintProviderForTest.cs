using System.Net.Http;
using Enterprise.MasterFiles.Business.Accounting;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class HttpCleintProviderForTest : IHttpClientProvider
	{
		public HttpCleintProviderForTest(EInvoicingBranchRegisterHttpClientHandlerMock clientHandlerMock)
		{
			ClientHandlerMock = clientHandlerMock;
		}
		EInvoicingBranchRegisterHttpClientHandlerMock ClientHandlerMock { get; }

		public HttpClient GetHttpClient()
		{
			return new HttpClient(ClientHandlerMock);
		}
	}
}
