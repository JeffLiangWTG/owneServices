using System;
using System.Net;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData
{
	public class GvmsWebClient : IGvmsWebClient, IDisposable
	{
		public string GetApiResponse(string url)
		{
			WebClient.Headers[HttpRequestHeader.Accept] = AcceptHeader;
			return WebClient.DownloadString(url);
		}

		WebClient WebClient
		{
			get
			{
				if (webClient == null)
				{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
					webClient = new WebClient();
#pragma warning restore SYSLIB0014 // Type or member is obsolete

					webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;

					webClient.Headers[HttpRequestHeader.Accept] = AcceptHeader;
				}
				return webClient;
			}
		}
		WebClient webClient;

		const string AcceptHeader = "application/vnd.hmrc.1.0+json";

		public void Dispose()
		{
			webClient?.Dispose();
		}
	}
}
