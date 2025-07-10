using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.EUReferenceData.OfficeCodes.Business
{
	public static class OfficeCodeXMLProducer
	{
		public static string DownloadAndConvertToRefCusCodeListXML(IHttpClientHelper httpClientHelper, string outputPath)
		{
			var errorBuilder = new StringBuilder();
			var downloadFilePath = Path.GetTempFileName();

			try
			{
				var successfullyDownloaded = FileDownloader.DownloadRDEntryListZipFile(httpClientHelper, ApplicationConfig.Instance.DownloadPageUrl, downloadFilePath, ApplicationConfig.Instance.RDEntryFileLinkPattern, errorBuilder).GetAwaiter().GetResult();
				if (successfullyDownloaded)
				{
					var downloadAbsolutePath = Path.GetFullPath(downloadFilePath);
					(var sourceXml, var publicationTime) = GetXmlFromZipFileAndPublicationTime(downloadAbsolutePath);
					if (sourceXml != null)
					{
						var parsedXML = new RDEntryListCustomsOfficesParser(errorBuilder).ParseXML(sourceXml);
						if (parsedXML.Any())
						{
							var outputFile = Path.Combine(outputPath, $"RefCusCodeListZZ_EU_OfficeCodes_{publicationTime:yyyyMMdd}.xml");
							XmlWriterHelper.ExportToXMLFile("EU Office Codes", outputFile, GetRefCusCodeListConfigurationForOfficeCodes(), publicationTime, parsedXML, Common.UniversalXmlWriter.UpdateType.Full);
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
			return errorBuilder.ToString();
		}

		static (XDocument SourceXml, DateTime PublicationTime) GetXmlFromZipFileAndPublicationTime(string filePath)
		{
			using (var archive = ZipFile.OpenRead(filePath))
			{
				XDocument result = null;
				var xmlEntry = archive.Entries.FirstOrDefault(x => x.FullName.EndsWith(".xml", StringComparison.InvariantCulture));
				if (xmlEntry != null)
				{
					using (var reader = new StreamReader(xmlEntry.Open()))
					{
						result = XDocument.Load(reader);
					}
				}
				return (result, xmlEntry?.LastWriteTime.DateTime ?? DateTime.MinValue);
			}
		}

		static XmlWriterConfiguration GetRefCusCodeListConfigurationForOfficeCodes()
		{
			var cusCodeListConfig = new EntityTypeConfiguration<RefCusCodeList>(true);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "CUSOF");
			cusCodeListConfig.IncludeColumn(x => x.ZZD_Code, true);
			cusCodeListConfig.IncludeColumn(x => x.ZZD_Description, false);
			cusCodeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.MinimumDateTime);
			cusCodeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
			cusCodeListConfig.IncludeColumn(x => x.ZZD_ZZZ_NKDataGrouping, true);
			cusCodeListConfig.IncludeColumn(x => x.RefCusCodeListAttributes, false);
			cusCodeListConfig.IncludeColumn(x => x.RefCusCodeOrAttributeTransportModes, false);

			var cusCodeListAttributeConfig = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			cusCodeListAttributeConfig.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			cusCodeListAttributeConfig.IncludeColumn(x => x.ZZE_Value, true);
			cusCodeListAttributeConfig.IncludeColumn(x => x.RefCusCodeOrAttributeTransportModes, false);

			var cusCodeOrAttributeTransportMode = new EntityTypeConfiguration<RefCusCodeOrAttributeTransportMode>(true);
			cusCodeOrAttributeTransportMode.IncludeColumn(x => x.ZZU_TransportMode, true);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListAttributeConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeOrAttributeTransportMode);

			return writerConfiguration;
		}
	}
}
