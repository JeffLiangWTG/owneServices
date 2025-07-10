using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.EUReferenceData.Services
{
	public static class FileDownloader
	{
		public static async Task<bool> DownloadRDEntryListZipFile(IHttpClientHelper httpClientHelper, string webPageUrl, string localFilePath, string fileLinkPattern, StringBuilder errorBuilder)
		{
			var result = false;
			var page = await GetWebPageHtml(webPageUrl, httpClientHelper);
			var downloadFileUrl = GetFileDownloadUrl(page, fileLinkPattern, errorBuilder);
			if (!string.IsNullOrEmpty(downloadFileUrl))
			{
				if (!string.IsNullOrWhiteSpace(localFilePath))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(localFilePath)));
				}
				result = await DownloadFile(httpClientHelper, localFilePath, downloadFileUrl, errorBuilder);
			}
			return result;
		}

		public static async Task<string> GetWebPageHtml(string webPageUrl, IHttpClientHelper httpClientHelper)
		{
			var html = await httpClientHelper.GetWebPageAsync(webPageUrl);
			return HttpUtility.HtmlDecode(html);
		}

		static string GetFileDownloadUrl(string downloadPageHtmlContent, string fileLinkPattern, StringBuilder errorBuilder)
		{
			var result = string.Empty;
			string href = new Regex(fileLinkPattern).Match(downloadPageHtmlContent).Value;

			if (!string.IsNullOrEmpty(href))
			{
				var xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(href);
				result = xmlDoc.DocumentElement.Attributes["href"].Value.TrimStart('/');
			}
			else
			{
				errorBuilder.AppendLine("The website has been modified or it's under maintenance. The download link was not found.");
			}
			return result;
		}

		public static async Task<bool> DownloadFile(IHttpClientHelper httpClientHelper, string localFilePath, string downloadFileUrl, StringBuilder errorBuilder)
		{
			var successfull = false;
			using (var response = await httpClientHelper.GetAsync(downloadFileUrl))
			{
				if (response != null)
				{
					using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write))
					{
						response.CopyTo(fileStream);
					}
					successfull = File.Exists(localFilePath);
				}
				if (!successfull)
				{
					errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"File download failed, remote url: {downloadFileUrl}, localFilePath: {localFilePath}");
				}
			}
			return successfull;
		}
	}
}
