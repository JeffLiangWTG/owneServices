using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.EUReferenceData.Services.Circabc;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;

namespace CargoWise.RefDbRepo.EUReferenceData.CustomsMeursing.Business
{
	public static class ExcelDownloader
	{
		public static WebFileInfo GetWebFileInfo(CircabcService service, string rootId, string circabcDownloadUrlTemplate, string fileNameRegex, string year)
		{
			Argument.NotNull(service, nameof(service));
			Argument.NotNullOrEmpty(rootId, nameof(rootId));
			Argument.NotNullOrEmpty(fileNameRegex, nameof(fileNameRegex));
			Argument.NotNullOrEmpty(circabcDownloadUrlTemplate, nameof(circabcDownloadUrlTemplate));
			Argument.NotNullOrEmpty(year, nameof(year));
			WebFileInfo webFileInfo = null;

			var yearsResponse = service.GetFolderAsync(rootId).GetAwaiter().GetResult();

			var yearData = yearsResponse.data.SingleOrDefault(d => d.name == year);

			if (yearData != null)
			{
				var monthResponse = service.GetFolderAsync(yearData.id).GetAwaiter().GetResult();
				if (monthResponse != null)
				{
					foreach (var month in monthResponse.data)
					{
						var filesResponse = service.GetFolderAsync(month.id).GetAwaiter().GetResult();
						var file = filesResponse.data.FirstOrDefault(f => Regex.IsMatch(f.name, fileNameRegex));
						if (file != null)
						{
							webFileInfo = new WebFileInfo()
							{
								FileName = file.name,
								DownloadPath = string.Format(CultureInfo.InvariantCulture, circabcDownloadUrlTemplate, file.id),
								Month = month.name,
								Year = year,
								ModifiedDate = DateTime.Parse(file.properties.modified, CultureInfo.InvariantCulture)
							};
							break;
						}
					}
				}
			}

			return webFileInfo;
		}

		public static void DownLoadFile(WebFileInfo fileInfo, string downloadDir)
		{
			Argument.NotNull(fileInfo, nameof(fileInfo));
			Argument.NotNullOrEmpty(downloadDir, nameof(downloadDir));

			var webFileDownloader = new FileDownloaderWrapper();

			var downloadFilePath = Path.Combine(downloadDir, fileInfo.FileName);
			webFileDownloader.DownloadFile(fileInfo.DownloadPath, downloadFilePath);
		}
	}
}
