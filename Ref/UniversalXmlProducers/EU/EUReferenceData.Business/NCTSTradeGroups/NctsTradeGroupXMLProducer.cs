using System;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public abstract class NctsTradeGroupXMLProducer
	{
		public string DownloadAndConvertToRefCusTradeGroupXML(IHttpClientHelper httpClientHelper, string outputPath)
		{
			var errorBuilder = new StringBuilder();
			var downloadFilePath = Path.GetTempFileName();

			try
			{
				var codeListDetail = CodeListDetail;
				var url = ApplicationConfig.Instance.NctsCodeListDownloadUrl;
				var downloadUrl = url.Replace("{Domain}", codeListDetail.Domain).Replace("{CodeListType}", codeListDetail.CodeListType);

				var successfullyDownloaded = FileDownloader.DownloadFile(httpClientHelper, downloadFilePath, downloadUrl, errorBuilder).GetAwaiter().GetResult();
				if (!successfullyDownloaded)
				{
					return errorBuilder.ToString();
				}

				(var sourceXml, var publicationTime) = Utils.GetXmlFromZipFileAndPublicationTime(downloadFilePath, ".xml");

				var dataParser = GetDataParser(errorBuilder);
				var refCusTradeGroups = dataParser.ParseXML(sourceXml);

				var codeType = codeListDetail.CodeType;
				var fileName = $"RefCusTradeGroupZZ_EUN_{codeType}.xml";
				var outputFile = Path.Combine(outputPath, fileName);
				XmlWriterHelper.ExportToXMLFile(
					codeListDetail.DataSource,
					outputFile,
					GetRefCusTradeGroupConfiguration(codeType),
					publicationTime,
					refCusTradeGroups,
					UpdateType.Full);
			}
			finally
			{
				if (File.Exists(downloadFilePath))
				{
					File.Delete(downloadFilePath);
				}
			}
			return errorBuilder.ToString();
		}

		protected abstract IUCCExportCodeListDetail CodeListDetail { get; }

		protected virtual NctsTradeGroupParser GetDataParser(StringBuilder errorBuilder) => new NctsTradeGroupParser(errorBuilder);

		static XmlWriterConfiguration GetRefCusTradeGroupConfiguration(string codeType)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var tradeGroupConfiguration = new EntityTypeConfiguration<RefCusTradeGroup>(true);

			tradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZA_TradeGroup, true, codeType);
			tradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZA_Description, false, Constants.UccDataSources.CountryCustomsSecurityAgreementArea);
			tradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZA_ZZZ_NKDataGrouping, true, Constants.Common.EUNCountryCode);
			tradeGroupConfiguration.IncludeColumnWithDefaultValue(x => x.ZZA_StartDate, false, Constants.MinimumDateTime);
			tradeGroupConfiguration.IncludeColumnWithDefaultValue(x => x.ZZA_EndDate, false, Constants.MaximumDateTime);
			tradeGroupConfiguration.IncludeColumn(x => x.RefCusTradeGroupCountries);

			var tradeGroupCountryConfiguration = new EntityTypeConfiguration<RefCusTradeGroupCountry>(true);
			tradeGroupCountryConfiguration.IncludeColumn(x => x.ZZB_RN_NKTradeGroupCountryCode, true);
			tradeGroupCountryConfiguration.IncludeColumn(x => x.ZZB_Description);
			tradeGroupCountryConfiguration.IncludeColumn(x => x.ZZB_StartDate, false);
			tradeGroupCountryConfiguration.IncludeColumnWithDefaultValue(x => x.ZZB_EndDate, false, Constants.MaximumDateTime);

			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupCountryConfiguration);

			return writerConfiguration;
		}
	}
}
