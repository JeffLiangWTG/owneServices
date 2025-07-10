using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class TariffTributaryDownloader
	{
		public static TariffTributary DownLoadZipAndExtract(HttpClient client)
		{
			string downloadUrl = ConfigurationProvider.Configuration.GetSection("URL_PORTAL_UNICO_TRATAMENTOS_TRIBUTARIOS_DOWNLOAD").Value;

			using (var response = client.GetAsync(new Uri(downloadUrl))?.Result)
			{
				Contract.Assume(response != null);

				var sZip = response.Content.ReadAsStreamAsync()?.Result;
				Contract.Assume(sZip != null);

				using (var archive = new ZipArchive(sZip))
				{
					var fileEntry = archive.Entries[0];

					if (fileEntry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
					{
						using (var stream = new StreamReader(fileEntry.Open()))
						{
							return JsonConvert.DeserializeObject<TariffTributary>(stream.ReadToEnd());
						}
					}
				}
			}

			return null;
		}
	}
}
