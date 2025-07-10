using System.Diagnostics.Contracts;
using System.Net.Http;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class LPCODownloader
	{
		public static string DownloadModel(string model, HttpClient client, TokenDTO tokenDto)
		{
			SetupHeaderRequest(tokenDto, client);
			using (var response = client.GetAsyncEx(ConfigurationProvider.Configuration.GetSection("URL_LPCO_MODEL").Value + model)?.Result)
			{
				var content = response.Content.ReadAsStringAsync()?.Result;
				Contract.Requires(content != null && content.Length > 0);
				return content;
			}
		}

		static void SetupHeaderRequest(TokenDTO tokenDto, HttpClient client)
		{
			if (!client.DefaultRequestHeaders.Contains("Authorization"))
			{
				client.DefaultRequestHeaders.Add("Authorization", tokenDto.Authorization);
				client.DefaultRequestHeaders.Add("X-CSRF-Token", tokenDto.XToken);
			}
		}
	}
}
