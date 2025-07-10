using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace CargoWise.RefDbRepo.VNReferenceData.Business
{
	public static class MetadataHelper
	{
		public static CodeListMetadataItem FindMetadataItem(CodeListMetadata metadata, int metadataItemId)
		{
			return metadata.DataItems.FirstOrDefault(i => i.Id == metadataItemId)
			       ?? throw new MetadataItemNotFoundException(metadata, metadataItemId);
		}

		public static DateTime ParsePublicationTime(CodeListMetadataItem metadataItem, DateTime defaultPublicationTime)
		{
			// parse from metadata
			var metadataUpdatedDate = metadataItem.UpdatedDate;
			if (!string.IsNullOrEmpty(metadataUpdatedDate))
			{
				if (DateTime.TryParseExact(metadataUpdatedDate, "MMM dd, yyyy", CultureInfo.InvariantCulture,
					    DateTimeStyles.None, out var metadataTime))
				{
					return metadataTime;
				}
			}

			// fallback to fileName
			var uri = new Uri(metadataItem.FileDownloadUrl);
			var localPath = HttpUtility.UrlDecode(uri.LocalPath);
			var fileName = Path.GetFileName(localPath);
			var fileNameMatch = Regex.Match(fileName, @"(\d{8})\.xlsx$");
			if (fileNameMatch.Success)
			{
				if (DateTime.TryParseExact(fileNameMatch.Groups[1].Value, "ddMMyyyy", CultureInfo.InvariantCulture,
					    DateTimeStyles.None, out var dateTimeFromFileName))
				{
					return dateTimeFromFileName;
				}
			}

			// fallback to url data
			var urlPathMatch = Regex.Match(localPath, @"(\d{4}\/\d{1,2}\/\d{1,2})");
			if (urlPathMatch.Success)
			{
				string[] urlDateFormats = { "yyyy/M/d", "yyyy/MM/dd", "yyyy/M/dd", "yyyy/MM/d" };
				if (DateTime.TryParseExact(urlPathMatch.Groups[1].Value, urlDateFormats, CultureInfo.InvariantCulture,
					    DateTimeStyles.None, out var dateTimeFromUrl))
				{
					return dateTimeFromUrl;
				}
			}

			return defaultPublicationTime;
		}

		public static Uri ParseDownloadUrl(CodeListMetadataItem metadataItem)
		{
			var downloadUrl = metadataItem.FileDownloadUrl;

			if (downloadUrl.StartsWith(ApplicationConfig.CodeListDownloadUrlPrefix, StringComparison.Ordinal))
			{
				return new Uri(downloadUrl);
			}

			var regex = @"^http:\/\/.+\/resources";
			downloadUrl = Regex.Replace(downloadUrl, regex, ApplicationConfig.CodeListDownloadUrlPrefix);
			return new Uri(downloadUrl);
		}
	}
}
