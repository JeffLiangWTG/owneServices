using System;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public static class CMRReferenceFileDownloader
	{
		public static (string fileContent, DateTime publishedDate) Download(IHttpClientHelper clientHelper, string fileNamePrefix, string indexUriString)
		{
			(var fileUri, var publishedDate) = DownloadHelper.DiscoverDataFile(clientHelper, fileNamePrefix, indexUriString);
			string fileContent;
			try
			{
				Console.WriteLine($"Retrieving data from {fileUri.AbsoluteUri}");
				fileContent = clientHelper.GetWebPageAsync(fileUri.AbsoluteUri).Result;
			}
			catch (Exception)
			{
				Console.Error.WriteLine($"Cannot load {fileUri.AbsoluteUri}");
				throw;
			}

			return (fileContent, publishedDate);
		}
	}
}
