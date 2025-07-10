using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public abstract class NaccsXmlWriter
	{
		protected abstract string DataSource { get; }
		protected abstract string FileNameWithoutExtension { get; }
		protected abstract string DownloadUrl { get; }
		protected virtual UpdateType UpdateType => UpdateType.Full;
		protected virtual bool IsCodeTypeConstant => true;
		protected virtual bool ZZE_ValueIsKey => false;
		protected virtual bool IsStartDateUseDefaultValue => true;
		protected virtual bool IsEndUseDefaultValue => true;

		protected XmlWriterConfiguration Config
		{
			get
			{
				var writerConfiguration = new XmlWriterConfiguration();

				var config = new EntityTypeConfiguration<RefCusCodeList>(true);
				if (IsCodeTypeConstant)
				{
					config.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, RefCusCodeListParser?.CurrentCodeType);
				}
				else
				{
					config.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, true);
				}
				config.IncludeColumn(x => x.ZZD_Code, true);
				config.IncludeColumn(x => x.ZZD_Description, false);
				if (IsStartDateUseDefaultValue)
				{
					config.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, StaticResources.DefaultZZD_StartDate);
				}
				else
				{
					config.IncludeColumn(x => x.ZZD_StartDate, false);
				}

				if (IsEndUseDefaultValue)
				{
					config.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, StaticResources.DefaultZZD_EndDate);
				}
				else
				{
					config.IncludeColumn(x => x.ZZD_EndDate, false);
				}
				config.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DataGrouping.JP);
				writerConfiguration.IncludeEntityTypeConfiguration(config);

				if (RefCusCodeListParser.HasAttribute)
				{
					config.IncludeColumn(x => x.RefCusCodeListAttributes, false);
					var cusCodeListAttribute = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
					cusCodeListAttribute.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
					cusCodeListAttribute.IncludeColumn(x => x.ZZE_Value, ZZE_ValueIsKey);
					writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListAttribute);
				}

				if (RefCusCodeListParser.HasLanguage)
				{
					config.IncludeColumn(x => x.RefCusCodeListLanguages, false);
					var codelistLanguageConfiguration = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);
					var language = RefCusCodeListParser.RefCusCodeListLanguageParser.ZXA_ZX6_NKLanguage;
					codelistLanguageConfiguration.IncludeColumnWithConstantValue(x => x.ZXA_ZX6_NKLanguage, true, language);
					codelistLanguageConfiguration.IncludeColumn(x => x.ZXA_Description, false);
					writerConfiguration.IncludeEntityTypeConfiguration(codelistLanguageConfiguration);
				}

				if (RefCusCodeListParser.HasRefCusCodeOrAttributeTransportMode)
				{
					config.IncludeColumn(x => x.RefCusCodeOrAttributeTransportModes, false);
					var cusCodeOrAttributeTransportModeConfiguration = new EntityTypeConfiguration<RefCusCodeOrAttributeTransportMode>(true);
					cusCodeOrAttributeTransportModeConfiguration.IncludeColumn(x => x.ZZU_TransportMode, true);
					writerConfiguration.IncludeEntityTypeConfiguration(cusCodeOrAttributeTransportModeConfiguration);
				}

				return writerConfiguration;
			}
		}

		RefCusCodeListParser refCusCodeListParser;
		protected RefCusCodeListParser RefCusCodeListParser
		{
			get
			{
				if (refCusCodeListParser == null)
				{
					refCusCodeListParser = GetRefCusCodeListParserCore();
				}
				return refCusCodeListParser;
			}
		}
		protected abstract RefCusCodeListParser GetRefCusCodeListParserCore();

		public void WriteXml(IHttpClientHelper httpClientHelper)
		{
			var filePath = Path.GetTempFileName();

			var isDownloaded = FileDownloader.TryDownload(httpClientHelper, DownloadUrl, filePath).Result;

			if (isDownloaded)
			{
				var getRecordSuccess = GetRecords(filePath, out var records);

				var publicationDateIsRetrieved = GetPublicationDate(httpClientHelper, out var publicationDate);

				if (getRecordSuccess && publicationDateIsRetrieved)
				{
					var isParsed = RefCusCodeListParser.TryParse(records, out var refCusCodeLists);

					if (isParsed)
					{
						var updatedPublicationDate = UpdatePublicationDateIfNeeded(publicationDate);
						PopulateAndSaveXml(updatedPublicationDate, refCusCodeLists);
					}
				}
			}
		}

		protected virtual bool GetRecords(string filePath, out IList<string> records)
		{
			return CsvReaderHelper.TryRead(filePath, out records);
		}

		protected virtual bool GetPublicationDate(IHttpClientHelper httpClientHelper, out DateTime publicationDate)
		{
			return NaccsPublicationDateScraper.TryGetPublicationDate(httpClientHelper, AppConfig.NACCS.CodeLists.BaseUrl, DownloadUrl, out publicationDate);
		}

		void PopulateAndSaveXml(DateTime publicationDate, IEnumerable<RefCusCodeList> refCusCodeLists)
		{
			var xmlWriter = new XmlWriterHelper(Config, DataSource, publicationDate, UpdateType, FileNameWithoutExtension);
			xmlWriter.PopulateAndSave(refCusCodeLists);
		}

		protected virtual DateTime UpdatePublicationDateIfNeeded(DateTime publicationDate) => publicationDate;
	}
}
