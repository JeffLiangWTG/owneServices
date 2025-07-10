using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CargoWise.Blazor.SessionBroker.Test
{
	// adapted from https://github.com/n-develop/HttpClientMock/blob/master/HttpClientMock.Tests/MockHttpMessageHandler.cs
	public class MockHttpMessageHandler : HttpMessageHandler
	{
		readonly HttpStatusCode statusCode;

		public MockHttpMessageHandler(HttpStatusCode statusCode)
		{
			this.statusCode = statusCode;
		}

		public int NumberOfCalls { get; private set; }
		public string RequestBody { get; private set; }
		public string RequestUri { get; private set; }
		public HttpMethod RequestMethod { get; private set; }

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			NumberOfCalls++;
			RequestUri = request.RequestUri?.ToString();
			RequestMethod = request.Method;

			if (request.Content != null)
			{
				RequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
			}

			return new HttpResponseMessage { StatusCode = statusCode };
		}
	}
}
