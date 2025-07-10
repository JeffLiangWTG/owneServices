using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Tariff
{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
	internal class TariffWebClientWrapper : ITariffWebClientWrapper, System.IDisposable
	{
		public string GetContent(string url) => WebClient.DownloadString(url);

		public void DownloadFile(string url, string localFile) => WebClient.DownloadFile(url, localFile);

		public string GetContentFromPost(string url, NameValueCollection requestParams)
		{

			using (var tempWebClient = new WebClient())
			{
				tempWebClient.Headers[HttpRequestHeader.Accept] = AcceptHeader;
				var result = tempWebClient.UploadValues(url, "POST", requestParams);

				return Encoding.UTF8.GetString(result);
			}
		}

		public void SetAuthorisationHeader(string authorisationHeader)
		{
			WebClient.Headers[HttpRequestHeader.Authorization] = authorisationHeader;
		}

		WebClient WebClient
		{
			get
			{
				if (webClient == null)
				{
					webClient = new WebClient();

					webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;

					webClient.Headers[HttpRequestHeader.Accept] = AcceptHeader;
					webClient.Headers[HttpRequestHeader.ContentType] = ContentType;
				}
				return webClient;
			}
		}
		WebClient webClient;

		const string AcceptHeader = "application/vnd.hmrc.1.0+json";
		const string ContentType = "application/xml; charset=UTF-8";

		public void Dispose()
		{
			webClient?.Dispose();
		}
	}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
}
