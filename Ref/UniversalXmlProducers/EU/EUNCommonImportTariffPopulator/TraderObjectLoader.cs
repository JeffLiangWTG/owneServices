using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class TraderObjectLoader : ITraderObjectLoader
	{
		public TraderObjectLoader(string downloadUrl, string tmpFolder)
		{
			this.downloadUrl = downloadUrl;
			this.tmpFolder = tmpFolder;
			if (!string.IsNullOrEmpty(tmpFolder) && Directory.Exists(tmpFolder))
			{
				downloadedFiles = Directory.GetFiles(tmpFolder, "*.xml")?.Select(x => x.Substring(x.LastIndexOf('\\') + 1));
			}
		}
		readonly string downloadUrl;
		readonly string tmpFolder;
		readonly IEnumerable<string> downloadedFiles;
		export export;

		public IEnumerable<T> Get<T>(IXmlFilter<T> filter, DateTime publicationDate)
		{
			if (export == null)
			{
				Console.WriteLine($"Downloading from {downloadUrl}");
				var fileName = downloadUrl.Substring(downloadUrl.LastIndexOf('/') + 1);
				fileName = fileName.Substring(0, fileName.IndexOf(".xml", StringComparison.InvariantCultureIgnoreCase) + 4);
				if (downloadedFiles.Contains(fileName))
				{
					Console.WriteLine($"File exists, skip download. {Path.Combine(tmpFolder, fileName)}");
					export = Helper.Deserialize<export>(Path.Combine(tmpFolder, fileName));
				}
				else
				{
					export = DownloadTraderObjectExport.Download<export>(downloadUrl, tmpFolder);
				}
			}
			return export.items.OfType<T>().Where(x => filter.IsValid(x, publicationDate)).Select(x => filter.GetValidValue(x));
		}
	}
}
