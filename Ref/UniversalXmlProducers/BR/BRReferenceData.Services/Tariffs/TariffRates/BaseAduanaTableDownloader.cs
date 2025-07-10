using System.Net.Http;
using System.Text;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class BaseAduanaTableDownloader
	{
		protected static HttpResponseMessage DoLogin(HttpClient client)
		{
			var needsLogin = true;
			HttpResponseMessage responseMessage = null;

			if (client.DefaultRequestHeaders.Contains("Cookie"))
			{
				var response = IsLoggedIn(client);
				needsLogin = !response.Item1;
				if (!needsLogin)
				{
					responseMessage = response.Item2;
				}
			}

			if (needsLogin)
			{
				var responseData = CaptchaSolvingUtils.Instance.GetLoginResponse(client);
				using var loginResponse = responseData;
				if (loginResponse != null && loginResponse.IsSuccessStatusCode)
				{
					return client.GetSyncEx(URL_LVL6_SUBITEM);
				}
				else
				{
					responseMessage = new HttpResponseMessage
					{
						StatusCode = System.Net.HttpStatusCode.Unauthorized
					};
					return responseMessage;
				}
			}
			else
			{
				return responseMessage;
			}
		}

		protected static string ViewId(HttpResponseMessage response) => CaptchaSolvingUtils.GetViewId(ReadResponseAsString(response));

		protected static string ReadResponseAsString(HttpResponseMessage response) => response.Content.ReadAsStringAsync()?.Result;

		protected static bool IsConnected(HttpResponseMessage response) => response.IsSuccessStatusCode && ReadResponseAsString(response).Contains("6 - SubItem");

		protected static (bool, HttpResponseMessage) IsLoggedIn(HttpClient client)
		{
			var response = client.GetSyncEx(URL_LVL6_SUBITEM);
			return (IsConnected(response), response);
		}

		protected static string URL_LVL6_SUBITEM => ConfigurationProvider.Configuration.GetSection("URL_TABELAS_ADUANEIRAS_DOWNLOAD_NCM_LVL6_SUBITEM").Value;

		protected void AddLog(string text)
		{
			Logger.AppendLine(text);
		}

		protected StringBuilder Logger => fLogger ??= new StringBuilder();
		StringBuilder fLogger;
	}
}
