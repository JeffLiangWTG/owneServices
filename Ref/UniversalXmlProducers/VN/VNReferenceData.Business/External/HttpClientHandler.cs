using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.VNReferenceData.Business
{
	public class HttpClientHandler : IHttpHandler
	{
		HttpClient Client { get; }

		public HttpClientHandler(HttpClient client = null)
		{
			Client = client ?? new HttpClient();
		}

		public async Task DownloadToFile(Uri uri, string filePath)
		{
			var response = await Client.GetAsync(uri);
			response.EnsureSuccessStatusCode();

			var contentStream = await response.Content.ReadAsStreamAsync();
			await using var fileStream = File.Create(filePath);
			await contentStream.CopyToAsync(fileStream);
		}
		public async Task<CodeListMetadata> DownloadCodeListMetadata()
		{
			var uri = new Uri(ApplicationConfig.CodeListMetadataUrl);
			var body = new Dictionary<string, string> { { "MA", ApplicationConfig.CombinationTabCategoryId } };
			using var requestBody = new StringContent(System.Text.Json.JsonSerializer.Serialize(body));
			var response = await Client.PostAsync(uri, requestBody);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadFromJsonAsync<CodeListMetadata>();
		}
	}
}
