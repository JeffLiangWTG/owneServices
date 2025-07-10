using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common
{
	public class HttpClientHelper : IHttpClientHelper
	{
		public HttpClientHelper(WebProxy webProxy = null)
		{
			if (webProxy != null)
			{
				clientWithProxy = clientWithProxy ?? GetHttpClientWithProxy(webProxy);
				client = clientWithProxy;
			}
			else
			{
				client = clientWithoutProxy;
			}
			var securityProtocols = CommonApplicationConfig.SecurityProtocols;
			if (!string.IsNullOrEmpty(securityProtocols))
			{
				Console.WriteLine($"SecurityProtocol will be specified to {securityProtocols}.");
				var securityProtocolsToAdd = securityProtocols.Split(',').Select(o => o.Trim()).ToArray();
				if (securityProtocolsToAdd != null && securityProtocolsToAdd.Any())
				{
					foreach (var securityProtocol in securityProtocolsToAdd)
					{
						SecurityProtocolType securityProtocolEnum;
						if (Enum.TryParse(securityProtocol, out securityProtocolEnum))
						{
							ServicePointManager.SecurityProtocol |= securityProtocolEnum;
							Console.WriteLine($"{securityProtocolEnum} has been added to SecurityProtocol.");
						}
					}
				}
			}
		}

#if DEBUG
		public
#endif
		HttpClient client
		{ get; }
		static readonly HttpClient clientWithoutProxy = new HttpClient();
		static HttpClient clientWithProxy;
		static HttpClientHandler httpClientHandler;

		static HttpClient GetHttpClientWithProxy(WebProxy webProxy)
		{
			httpClientHandler = new HttpClientHandler { Proxy = webProxy, UseProxy = true, CheckCertificateRevocationList = true };
			var httpClient = new HttpClient(httpClientHandler);
			httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
			return httpClient;
		}

		public async Task<TRead> PostAndReadAsAsync<TRead, TPost>(string url, TPost value)
		{
			using (var content = new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json"))
			{
				var response = await client.PostAsync(new Uri(url), content);
				return JsonConvert.DeserializeObject<TRead>(await response.Content.ReadAsStringAsync());
			}
		}

		public async Task<string> PostAndReadAsAsyncString(string url, string value, string mediaTypeName)
		{
			using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, url))
			{
				requestMessage.Headers.Add("User-Agent", "WiseTech Tools");
				requestMessage.Content = new StringContent(value, Encoding.UTF8, mediaTypeName);
				var result = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead);
				return await result.Content.ReadAsStringAsync();
			}
		}

		public async Task<string> GetWebPageAsync(string url)
		{
			using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
			{
				requestMessage.Headers.Add("User-Agent", "WiseTech Tools");
				var result = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead);
				return await result.Content.ReadAsStringAsync();
			}
		}

		public async Task<string> GetWebPageAsync(string url, string charset = "utf-8")
		{
			using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
			{
				requestMessage.Headers.Add("User-Agent", "WiseTech Tools");
				var result = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead);
				var bytes = await result.Content.ReadAsByteArrayAsync();
				var encoding = Encoding.GetEncoding(charset);
				return encoding.GetString(bytes);
			}
		}

		public async Task<Stream> GetAsync(string url)
		{
			var result = await client.GetAsync(new Uri(url));
			return await result.Content.ReadAsStreamAsync();
		}

		public async Task<string[]> GetMatchedEntityCodesAsync(string url, string[] entityNames)
		{
			Argument.NotNullOrEmpty(url, nameof(url));
			Argument.NotNull(entityNames, nameof(entityNames));

			url += string.Concat(entityNames.Select(x => $"&names={x}"));
			var result = await client.GetAsync(new Uri(url));
			return JsonConvert.DeserializeObject<string[]>(await result.Content.ReadAsStringAsync());
		}

		public void CloseConnection()
		{
			client.DefaultRequestHeaders.ConnectionClose = true;
		}
	}
}
