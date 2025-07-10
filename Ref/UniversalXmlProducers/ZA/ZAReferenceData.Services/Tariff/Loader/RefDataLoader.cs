using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader
{
	public class RefDataLoader : IRefDataLoader
	{
		readonly string baseUrl;
		readonly bool useSecureService;
		public RefDataLoader(string baseUrl, bool useSecureService)
		{
			this.baseUrl = baseUrl;
			this.useSecureService = useSecureService;
		}

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
					list = DeserializeObject<T>(jsonData);
				}
				else
				{
					throw new ReferenceDataException($"Failed to load reference data from \"{query}\". StatusCode: {(int)response.StatusCode} {response.ReasonPhrase}");
				}
			}

			return list;
		}

		internal static List<T> DeserializeObject<T>(string jsonData)
		{
			var list = new List<T>();
			var settings = new JsonSerializerSettings();
			settings.Converters.Add(new JsonEdmDateConverter());
			var data = JsonConvert.DeserializeObject<DataWrapper<T>>(jsonData, settings);

			if (data?.Value?.Any() ?? false)
			{
				list.AddRange(data.Value);
			}

			return list;
		}

		HttpClient CreateClient()
		{
			var client = new HttpClient();

			if (useSecureService)
			{
#pragma warning disable CA5386 // Avoid hardcoding SecurityProtocolType value
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
#pragma warning restore CA5386 // Avoid hardcoding SecurityProtocolType value
				ServicePointManager.UseNagleAlgorithm = false;
			}

			return client;
		}

		internal Uri GetQuery(string urlQuery) => new Uri($"{baseUrl.TrimEnd('/')}/{urlQuery}");

		internal class DataWrapper<T>
		{
			public T[] Value { get; set; }
		}
	}
}
