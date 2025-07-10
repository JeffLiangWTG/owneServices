using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public static class ExchangeRateFileDownloader
	{
		public static void Download(IHttpClientHelper clientHelper, string remotePath, string localPath)
		{
			var content = clientHelper.GetWebPageAsync(remotePath).Result;
			if (string.IsNullOrEmpty(content))
			{
				throw new HttpRequestException($"Could not retrieve any content.");
			}
			File.WriteAllText(localPath, content);
		}
	}
}
