using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Enterprise.MasterFiles.Business.Accounting
{
	internal static class HttpHelper
	{
		internal static HttpRequestMessage GetJsonPostHttpRequestMessage(string uri
															, (string Name, string Value)[] requestHeaderElements
															, string content)
		{
			return GetHttpRequestMessage(uri
										, HttpMethod.Post
										, requestHeaderElements
										, MediaTypeHeaderValue.Parse("application/json")
										, new StringContent(content));
		}

		internal static async Task<(HttpStatusCode StatusCode, string ResponseText)> SendAsync(HttpClient httpClient, HttpRequestMessage request)
		{
			var response = await httpClient.SendAsync(request);
			var responseText = await response.Content.ReadAsStringAsync();
			return (response.StatusCode, responseText);
		}

		static HttpRequestMessage GetHttpRequestMessage(string uri
															, HttpMethod method
															, (string Name, string Value)[] requestHeaderElements
															, MediaTypeHeaderValue typeHeaderValue
															, HttpContent content)
		{
			var request = new HttpRequestMessage(method, uri);
			foreach (var headerElement in requestHeaderElements)
			{
				request.Headers.TryAddWithoutValidation(headerElement.Name, headerElement.Value);
			}
			request.Content = content;
			request.Content.Headers.ContentType = typeHeaderValue;
			return request;
		}
	}
}
