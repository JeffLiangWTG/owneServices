using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	public class MockHttpMessageHandler : HttpMessageHandler
	{
		public int SendAsyncCallCount { get; private set; }
		public string ReceivedRequest { get; set; }
		public HttpRequestHeaders Headers { get; set; }
		public string AbsoluteUri { get; private set; }
		readonly MockFlightScheduleClient _mockServer;

		public MockHttpMessageHandler(MockFlightScheduleClient mockServer)
		{
			_mockServer = mockServer;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			SendAsyncCallCount++;
			AbsoluteUri = request.RequestUri!.AbsoluteUri;
			Headers = request.Headers;
			ReceivedRequest = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			try
			{
				var response = HandleRequest();
				var result = new HttpResponseMessage
				{
					Content = new StringContent(response)
				};
				result.StatusCode = HttpStatusCode.OK;

				return await Task.FromResult(result);
			}
			catch(Exception)
			{
				throw;
			}
		}

		string HandleRequest()
		{
			if (Headers.GetValues("SOAPAction").First().Contains("GetFlight"))
			{
				return _mockServer.GetFlight(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>());
			}
			else if (Headers.GetValues("SOAPAction").First().Contains("SolveRouting"))
			{
				return _mockServer.SolveRouting(It.IsAny<string>(), It.IsAny<string>());
			}
			else if (Headers.GetValues("SOAPAction").First().Contains("LogInS8C"))
			{
				return _mockServer.LogInS8C(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
			}
			else
			{
				throw new NotImplementedException();
			}
		}
	}
}
