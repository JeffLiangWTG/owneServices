using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public interface IRefDataLoader
	{
		Task<IEnumerable<T>> LoadData<T>(string urlQuery);
	}

	public class RefDataLoader : IRefDataLoader
	{
		public RefDataLoader()
		{
		}

		public async Task<IEnumerable<T>> LoadData<T>(string urlQuery)
		{
			var list = new List<T>();
			using (var httpClient = CreateClient())
			{
				var response = httpClient.GetAsync(new System.Uri(GetQuery(urlQuery))).Result;
				if (response?.IsSuccessStatusCode ?? false)
				{
					var jsonData = await response.Content.ReadAsStringAsync();
					var data = JsonConvert.DeserializeObject<DataWrapper<T>>(jsonData);

					if (data?.Value?.Any() ?? false)
					{
						list.AddRange(data.Value);
					}
				}
			}

			return list;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5386:Avoid hardcoding SecurityProtocolType value", Justification = "Only 1 client using Tls12")]
		protected virtual HttpClient CreateClient()
		{
			var client = new HttpClient();
			client.Timeout = new System.TimeSpan(0, 10, 0);
			if (ApplicationConfig.IsRefDbServiceSecure)
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
				ServicePointManager.UseNagleAlgorithm = false;
			}

			return client;
		}

		static string GetQuery(string urlQuery) => $"{ApplicationConfig.RefDbServiceURI}{urlQuery}";

		class DataWrapper<T>
		{
			public T[] Value { get; set; }
		}
	}
}
