using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business
{
	public static class Helper
	{
		public static void ExportToXMLFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, UpdateType updateType, IEnumerable<T> codeList)
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(updateType);

			var dirName = Path.GetDirectoryName(outputFile);
			Directory.CreateDirectory(dirName);

			foreach (var code in codeList)
			{
				writer.PopulateData(code);
			}
			writer.SaveXml(outputFile);
		}

		public static XmlWriterConfiguration GetRefCusCodeListWriterConfigurationWithAttributes(string dataGrouping, string codeType = null, bool defaultDataGrouping = true, bool isPort = false)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(!isPort);
			DefaultRefCusCodeListWriterConfiguration(writerConfiguration, codeListConfiguration, codeType, dataGrouping, isPort, defaultDataGrouping);

			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes, false);
			var codelistAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_Value, true);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistAttributeConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCarrierCodeWriterConfiguration(string dataGrouping, string codeType = null)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfiguration = new EntityTypeConfiguration<RefCarrierCode>(true);

			codeListConfiguration.IncludeColumn(x => x.ZZ4_Code, true);
			codeListConfiguration.IncludeColumn(x => x.ZZ4_Description, false);
			codeListConfiguration.IncludeColumn(x => x.ZZ4_ZZZ_NKDataGrouping, true);
			codeListConfiguration.IncludeColumn(x => x.RefCarrierCodeAttributes, false);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);
			var codelistAttributeConfiguration = new EntityTypeConfiguration<RefCarrierCodeAttribute>(true);
			codelistAttributeConfiguration.IncludeColumn(x => x.ZZG_Name, true);
			codelistAttributeConfiguration.IncludeColumn(x => x.ZZG_Value, true);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistAttributeConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefLocoMapWriterConfiguration(string localCountryCodeGuid, string systemUsage)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfiguration = new EntityTypeConfiguration<RefLocoMap>(true);

			codeListConfiguration.IncludeColumn(x => x.RY_LocalPortCode, false);
			codeListConfiguration.IncludeColumn(x => x.RY_RL_NKLocoPort, true);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.RY_SystemUsage, true, systemUsage);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.RY_RN_NKCountryCode, true, localCountryCodeGuid);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

			return writerConfiguration;
		}

		static void DefaultRefCusCodeListWriterConfiguration(XmlWriterConfiguration writerConfiguration, EntityTypeConfiguration<RefCusCodeList> codeListConfiguration, string codeType, string dataGrouping, bool isPort, bool defaultDataGrouping = true)
		{
			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			if (!isPort)
			{
				codeListConfiguration.IncludeColumn(x => x.ZZD_Description, false);
			}
			if (string.IsNullOrWhiteSpace(codeType))
			{
				codeListConfiguration.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, true);
			}
			else
			{
				codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			}

			if (defaultDataGrouping)
			{
				codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, dataGrouping);
			}
			else
			{
				codeListConfiguration.IncludeColumn(x => x.ZZD_ZZZ_NKDataGrouping, true);
			}
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, DefaultValues.MinimumDateTime);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, DefaultValues.MaximumDateTime);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);
		}
	}
}
