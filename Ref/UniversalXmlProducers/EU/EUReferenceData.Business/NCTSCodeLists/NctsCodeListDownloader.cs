using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsCodeListDownloader
	{
		public NctsCodeListDownloader(StringBuilder errorBuilder, IHttpClientHelper httpClientHelper, IUCCExportCodeListDetail codeListDetail)
		{
			this.errorBuilder = Argument.NotNull(errorBuilder, nameof(errorBuilder));
			this.httpClientHelper = Argument.NotNull(httpClientHelper, nameof(httpClientHelper));
			this.codeListDetail = Argument.NotNull(codeListDetail, nameof(codeListDetail));
		}

		public async Task<List<ExtractedCodeListProvider>> DownloadAndConvertToRefCusCodeList()
		{
			var result = new List<ExtractedCodeListProvider>();
			var downloadFilePath = Path.GetTempFileName();
			try
			{
				var url = ApplicationConfig.Instance.NctsCodeListDownloadUrl;
				var realUrl = url.Replace("{Domain}", codeListDetail.Domain).Replace("{CodeListType}", codeListDetail.CodeListType);
				if (!string.IsNullOrWhiteSpace(downloadFilePath))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(downloadFilePath)));
				}

				var successfullyDownloaded = await FileDownloader.DownloadFile(httpClientHelper, downloadFilePath, realUrl, errorBuilder);
				if (successfullyDownloaded)
				{
					var downloadAbsolutePath = Path.GetFullPath(downloadFilePath);
					var (sourceXml, publicationTime) = Utils.GetXmlFromZipFileAndPublicationTime(downloadAbsolutePath, ".xml");
					if (sourceXml != null)
					{
						var parsedXML = new NCTSCodeListParser(errorBuilder).ParseXML(sourceXml, codeListDetail);
						if (parsedXML.Any())
						{
							result.Add(new ExtractedCodeListProvider(codeListDetail.CodeType, codeListDetail.DataSource, parsedXML.Any(x => x.RefCusCodeListAttributes.Any()), publicationTime, parsedXML));
						}
					}
				}
			}
			finally
			{
				if (File.Exists(downloadFilePath))
				{
					File.Delete(downloadFilePath);
				}
			}
			return result;
		}

		readonly StringBuilder errorBuilder;
		readonly IHttpClientHelper httpClientHelper;
		readonly IUCCExportCodeListDetail codeListDetail;
	}
}
