using System;
using System.Net.Http;
using CargoWise.RefDbRepo.NOReferenceData.Business;
using CargoWise.RefDbRepo.NOReferenceData.Services;

namespace CargoWise.RefDbRepo.NOReferenceData.CmdLine.Tariff
{
	sealed class DownloadData
	{
		internal static (bool IsSuccess, DateTime LastModifiedDate, T Data, string xmlAsString) TryDownloadData<T>(string dataName, string resourceUrl, string schemalocation) where T : class
		{
			var isSuccess = false;

			var client = new HttpClient();
			var downloadUrl = new Uri(ApplicationConfig.ResourceSearchUrl + resourceUrl);
			var downloadContent = DownloadContent.Download<T>(client, downloadUrl, schemalocation);

			if (downloadContent.Errors.Length == 0)
			{
				var result = downloadContent.ResourceData.LastModified.TryParseDateTime("MM/dd/yyyy HH:mm:ss");

				if (!result.SuccessfullyParsed)
				{
					isSuccess = Program.PrintErrorMessage($"Error parsing LastModifiedDate for {dataName}.");
				}

				return (isSuccess, result.DateTime, downloadContent.XmlData, downloadContent.ResourceData.FileContents);
			}

			isSuccess = Program.PrintErrorMessage($@"Error downloading {dataName}: ", downloadContent.Errors);
			return (isSuccess, DateTime.MinValue, null, null);
		}
	}
}
