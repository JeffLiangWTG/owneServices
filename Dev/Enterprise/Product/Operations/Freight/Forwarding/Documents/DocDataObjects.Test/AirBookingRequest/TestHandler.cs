using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class TestHandler : DelegatingHandler
	{
		TestHandler(HttpResponseMessage mockResponseMessage)
		{
			this.mockResponseMessage = mockResponseMessage;
		}

		readonly HttpResponseMessage mockResponseMessage;

		public static TestHandler Create(HttpStatusCode statusCode, string content = null)
		{
			var response = new HttpResponseMessage
			{
				StatusCode = statusCode
			};

			if (!string.IsNullOrWhiteSpace(content))
			{
				response.Content = new StringContent(content, Encoding.UTF8, "application/xml");
			}

			return new TestHandler(response);
		}

		public HttpMethod Method => Methods.LastOrDefault();
		public string Url => Urls.LastOrDefault();
		public string Content => Contents.LastOrDefault();

		public List<HttpMethod> Methods { get; } = new List<HttpMethod>();
		public List<string> Urls { get; } = new List<string>();
		public List<string> Contents { get; } = new List<string>();

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
		{
			Methods.Add(request.Method);
			Urls.Add(request.RequestUri.ToString());
			Contents.Add(request.Content?.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult());

			return Task.FromResult(mockResponseMessage);
		}
	}
}
