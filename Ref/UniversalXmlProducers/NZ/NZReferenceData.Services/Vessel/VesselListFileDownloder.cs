using System;
using System.IO;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.NZReferenceData.Services
{
	public static class VesselListFileDownloder
	{
		public static void Download(IHttpClientHelper clientHelper, string url, string localPath)
		{
			try
			{
				var content = clientHelper.GetWebPageAsync(url).Result;
				File.WriteAllText(localPath, content);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($@"Unable to Load NZ Vessel List from the following URL: {url}{Environment.NewLine}{ex.Message}");
			}
		}
	}
}
