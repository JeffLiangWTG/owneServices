using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class LPCOConsentingBodyDownloader
	{
		public static List<ModelDTO> DownloadModelsFromConsentingBody(HttpClient client, TokenDTO tokenDto)
		{
			SetupHeaderRequest(tokenDto, client);
			var models = new List<ModelDTO>();
			var ConsetingBodyList = new LPCOConsentingBodyList().ConsentingBodyList;
			foreach (var model in ConsetingBodyList)
			{
				models.AddRange(DownloadModel(model, client));
			}

			return models;
		}

		static List<ModelDTO> DownloadModel(string model, HttpClient client)
		{
			using (var response = client.GetAsyncEx(ConfigurationProvider.Configuration.GetSection("URL_LPCO_MODEL_SEARCH").Value + model)?.Result)
			{
				var models = new List<ModelDTO>();
				var jsonContent = response.Content?.ReadAsStringAsync()?.Result;
				if (jsonContent != null)
				{
					var modelsJson = JsonConvert.DeserializeObject<List<ModelDTO>>(jsonContent);
					if (modelsJson?.Count > 0)
					{
						models.AddRange(modelsJson);
					}
				}

				return models;
			}
		}

		static void SetupHeaderRequest(TokenDTO tokenDto, HttpClient client)
		{
			client.DefaultRequestHeaders.Add("Authorization", tokenDto.Authorization);
			client.DefaultRequestHeaders.Add("X-CSRF-Token", tokenDto.XToken);
		}
	}
}
