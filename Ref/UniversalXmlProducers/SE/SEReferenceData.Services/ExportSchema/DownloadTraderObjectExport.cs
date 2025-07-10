using System;
using System.Net.Http;

namespace CargoWise.RefDbRepo.SEReferenceData.Services
{
	public static class DownloadTraderObjectExport
	{
		public static T Download<T>(string url, string temporaryDownloadPath = null) where T : class
		{
			using (var client = new HttpClient())
			{
				return Download<T>(client, url, temporaryDownloadPath);
			}
		}

		public static T Download<T>(HttpClient client, string url, string temporaryDownloadPath = null) where T : class
		{
			try
			{
				return Helper.DownloadKryptFile<T>(client, url, temporaryDownloadPath);
			}
			catch (Exception ex)
			{
				throw new ObjectTraderExportException($"Unable to Load IncrementalObjectTraderExport XML from the following URL: {url} /r/n {ex.Message}");
			}
		}
	}
}
