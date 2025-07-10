using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Enterprise.Integration;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class eAdaptorSenderHelper
	{
		public eAdaptorControllerBase Controller { get; }
		public Func<HttpResponseMessage> Send { get; set; }

		public eAdaptorSenderHelper(eAdaptorControllerBase controller, Func<HttpResponseMessage> send)
		{
			Controller = controller;
			Send = send;
		}

		public (HttpStatusCode statusCode, HttpHeaders headers, string result) SendRequest(string requestXml, IeAdaptorConfig configForTest = null)
		{
			if (configForTest != null)
			{
				Controller.SetConfigForTest(configForTest);
			}

			using (Controller)
			using (var request = new HttpRequestMessage())
			{
				Controller.Request = request;
				request.Content = new StringContent(requestXml);

				using (var response = Send())
				{
					return (
						response.StatusCode,
						response.Headers,
						response.Content.ReadAsStringAsync().Result
					);
				}
			}
		}
	}
}
