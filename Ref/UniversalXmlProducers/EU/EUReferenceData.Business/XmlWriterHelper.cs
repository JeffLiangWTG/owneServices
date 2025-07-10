using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public static class XmlWriterHelper
	{
		public static XmlWriterConfiguration GetRefCusCodeListConfigurationForAES(string ListType, bool defaultStartDateWithMinimumDate = false)
		{
			var cusCodeListConfig = CusCodeListConfig(defaultStartDateWithMinimumDate);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, ListType);

			var cusCodeListLanguageConfig = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);
			cusCodeListLanguageConfig.IncludeColumn(x => x.ZXA_ZX6_NKLanguage, true);
			cusCodeListLanguageConfig.IncludeColumn(x => x.ZXA_Description, false);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListLanguageConfig);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefNctsCodesWriterConfiguration(string codeType, bool hasAttributes = true)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);

			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.Common.EUNCountryCode);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Description);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.MinimumDateTime);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListLanguages);

			var codeListLanguageConfiguration = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);
			codeListLanguageConfiguration.IncludeColumn(x => x.ZXA_ZX6_NKLanguage, true);
			codeListLanguageConfiguration.IncludeColumn(x => x.ZXA_Description);

			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListLanguageConfiguration);

			if (hasAttributes)
			{
				codeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes);
				var codeListAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
				codeListAttributeConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
				codeListAttributeConfiguration.IncludeColumn(x => x.ZZE_Value, true);
				writerConfiguration.IncludeEntityTypeConfiguration(codeListAttributeConfiguration);
			}

			return writerConfiguration;
		}

		public static void ExportToXMLFile<T>(string dataSource, string outputFilePath, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, IEnumerable<T> codeList, UpdateType updateType, Dependency[] dependencies = null)
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(updateType);

			if (dependencies != null)
			{
				foreach (var dependency in dependencies)
				{
					writer.SetDependency(dependency);
				}
			}

			var dirName = Path.GetDirectoryName(outputFilePath);
			Directory.CreateDirectory(dirName);

			foreach (var code in codeList)
			{
				writer.PopulateData(code);
			}
			writer.SaveXml(outputFilePath);
		}

		public static EntityTypeConfiguration<RefCusCodeList> CusCodeListConfig(bool defaultStartDateWithMinimumDate = false)
		{
			var cusCodeListConfig = new EntityTypeConfiguration<RefCusCodeList>(true);
			cusCodeListConfig.IncludeColumn(x => x.ZZD_Code, true);
			cusCodeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_Description, false, Constants.Common.DefaultLanguage);
			cusCodeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "EUN");
			cusCodeListConfig.IncludeColumn(x => x.RefCusCodeListLanguages, false);
			if (defaultStartDateWithMinimumDate)
			{
				cusCodeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.MinimumDateTime);
			}
			else
			{
				cusCodeListConfig.IncludeColumn(x => x.ZZD_StartDate, false);
			}
			return cusCodeListConfig;
		}
	}
}
