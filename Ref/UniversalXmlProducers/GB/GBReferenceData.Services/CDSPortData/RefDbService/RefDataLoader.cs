using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.RefDbService
{
	public class RefDataLoader : IRefDataLoader
	{
		public RefDataLoader(string baseUrl, bool useSecureService)
		{
			this.baseUrl = baseUrl;
			this.useSecureService = useSecureService;
		}
		readonly string baseUrl;
		readonly bool useSecureService;


		public async Task<IEnumerable<T>> LoadData<T>(string urlQuery)
		{
			var list = new List<T>();
			using (var httpClient = CreateClient())
			{
				var query = GetQuery(urlQuery);
				var response = httpClient.GetAsync(query).Result;
				if (response?.IsSuccessStatusCode ?? false)
				{
					var jsonData = await response.Content.ReadAsStringAsync();
					var data = JsonConvert.DeserializeObject<DataWrapper<T>>(jsonData);

					if (data?.Value?.Any() ?? false)
					{
						list.AddRange(data.Value);
					}
				}
				else
				{
					throw new InvalidOperationException($"Failed to load reference data from \"{query}\". StatusCode: {(int)response.StatusCode} {response.ReasonPhrase}");
				}
			}

			return list;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5386:Avoid hardcoding SecurityProtocolType value")]
		HttpClient CreateClient()
		{
			var client = new HttpClient();

			if (useSecureService)
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
				ServicePointManager.UseNagleAlgorithm = false;
			}

			return client;
		}

		internal Uri GetQuery(string urlQuery) => new Uri($"{baseUrl.TrimEnd('/')}/{urlQuery}");

		class DataWrapper<T>
		{
			public T[] Value { get; set; }
		}
	}
}
