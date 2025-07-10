using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using WTG.Foundation.Http;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	public class TestAirBookingRequestSender : TestCaseWithFactory
	{
		public void TestAirBookingRequestSender_DoesNotCauseSocketExhaustion()
		{
			var handler = new StubHttpMessageHandler();
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => handler)))
			{
				var responses = new List<string>();

				for (var i = 0; i < 10; i++)
				{
					var sender = new AirBookingRequestApiClient(new Uri("http://test.com/"));
					var response = sender.Post("", new CancellationToken());

					responses.Add(response.Content);
				}

				AssertEquals("Should reuse one message handler, handler hash should match", 1, responses.Distinct().Count());
			}
		}

		#region Implementation

		sealed class StubHttpMessageHandler : HttpMessageHandler
		{
			public StubHttpMessageHandler()
			{
				ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent($"HTTP Handler Instance: {RuntimeHelpers.GetHashCode(this)}"),
				};
			}

			public HttpResponseMessage ResponseMessage { get; set; }

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				return await Task.FromResult(ResponseMessage);
			}
		}

		#endregion
	}
}
