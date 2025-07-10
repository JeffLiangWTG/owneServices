using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public sealed class SeaCarrierXmlWriter
	{
		static string CarrierDownloadUrl => AppConfig.NACCS.CodeLists.SeaCarrierCsvFileDownloadUrl;

		static string VesselDownloadUrl => AppConfig.NACCS.CodeLists.SeaVesselCsvFileDownloadUrl;

		static UpdateType UpdateType => UpdateType.Full;

		static string DataSource => "Japan Sea Carrier";

		static string FileNameWithoutExtension => "JPSeaCarrier";

		static XmlWriterConfiguration Config
		{
			get
			{
				var writerConfig = new XmlWriterConfiguration();

				var carrierConfig = new EntityTypeConfiguration<RefCarrierCode>(true);
				carrierConfig.IncludeColumn(x => x.ZZ4_Code, true);
				carrierConfig.IncludeColumn(x => x.ZZ4_Description);
				carrierConfig.IncludeColumnWithConstantValue(x => x.ZZ4_ZZZ_NKDataGrouping, true, Constants.DataGrouping.JP);
				carrierConfig.IncludeColumnWithConstantValue(x => x.ZZ4_IsSea, false, true);
				carrierConfig.IncludeColumnWithConstantValue(x => x.ZZ4_IsAir, false, false);
				carrierConfig.IncludeColumnWithConstantValue(x => x.ZZ4_IsRoad, false, false);
				carrierConfig.IncludeColumnWithConstantValue(x => x.ZZ4_IsRail, false, false);
				writerConfig.IncludeEntityTypeConfiguration(carrierConfig);

				carrierConfig.IncludeColumn(x => x.RefCarrierCodeAttributes, false);
				var refCarrierCodeAttribute = new EntityTypeConfiguration<RefCarrierCodeAttribute>(true);
				refCarrierCodeAttribute.IncludeColumn(x => x.ZZG_Name, true);
				refCarrierCodeAttribute.IncludeColumn(x => x.ZZG_Value);
				writerConfig.IncludeEntityTypeConfiguration(refCarrierCodeAttribute);

				carrierConfig.IncludeColumn(x => x.RefCarrierVesselPivots, false);
				var refCarrierVesselPivot = new EntityTypeConfiguration<RefCarrierVesselPivot>(true);
				refCarrierVesselPivot.IncludeColumn(x => x.ZZQ_ZZO_NKCode, true);
				refCarrierVesselPivot.IncludeColumn(x => x.ZZQ_ZZO_NKRadioCallSign, true);
				refCarrierVesselPivot.IncludeColumnWithConstantValue(x => x.ZZQ_ZZO_ZZZ_NKDataGrouping, true, Constants.DataGrouping.JP);

				writerConfig.IncludeEntityTypeConfiguration(refCarrierVesselPivot);

				return writerConfig;
			}
		}

		public static void WriteXml(IHttpClientHelper httpClientHelper)
		{
			var carrierFilePath = Path.GetTempFileName();
			var isCarrierDownloaded = FileDownloader.TryDownload(httpClientHelper, CarrierDownloadUrl, carrierFilePath).Result;

			var vesselFilePath = Path.GetTempFileName();
			var isVesselDownloaded = FileDownloader.TryDownload(httpClientHelper, VesselDownloadUrl, vesselFilePath).Result;

			var carrierDateIsRetrieved = ParserHelper.GetPublicationDateByDownloadUrl(httpClientHelper, CarrierDownloadUrl, out var carrierPublicationDate);
			var vesselDateIsRetrieved = ParserHelper.GetPublicationDateByDownloadUrl(httpClientHelper, VesselDownloadUrl, out var vesselPublicationDate);

			if (isCarrierDownloaded && isVesselDownloaded && carrierDateIsRetrieved && vesselDateIsRetrieved)
			{
				var publicationDate = carrierPublicationDate > vesselPublicationDate ? carrierPublicationDate : vesselPublicationDate;
				ParseAndSaveXml(carrierFilePath, vesselFilePath, publicationDate);
			}
		}

		public static void ParseAndSaveXml(string filePath, string vesselFilePath, DateTime publicationDate)
		{
			if (CsvReaderHelper.TryRead(filePath, out var records) && CsvReaderHelper.TryRead(vesselFilePath, out var vesselRecords))
			{
				SeaVesselXmlWriter.ParseAndSaveXml(vesselRecords, publicationDate);

				if (SeaCarrierParser.TryParse(records, vesselRecords, out var refCarrierCodeList))
				{
					var xmlWriter = new XmlWriterHelper(Config, DataSource, publicationDate, UpdateType, FileNameWithoutExtension, GetDependencies(publicationDate));
					xmlWriter.PopulateAndSave(refCarrierCodeList);
				}
			}
		}

		public static IEnumerable<Dependency> GetDependencies(DateTime publicationDateTime)
		{
			yield return new Dependency(SeaVesselXmlWriter.DataSource, publicationDateTime, DependencyType.Preferred);
		}
	}
}
