using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.BEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public static class XMLGeneration
	{
		public static void GenerateXml(string dataSource, string destFileName, List<RefExchangeRateZZ> resultList)
		{
			var downloadDir = ApplicationConfig.DownloadDir;
			var xmlFileOutputFile = Path.Combine(downloadDir, destFileName);
			var writerConfiguration = GetRefExchangeRateWriterConfiguration();
			ExportToXMLFile(dataSource, xmlFileOutputFile, writerConfiguration, DateTimeOffset.Now.LocalDateTime, resultList);
		}

		public static XmlWriterConfiguration GetRefCusTradeGroupWriterConfiguration()
		{
			var writerConfig = new XmlWriterConfiguration();
			var entityConfig = new EntityTypeConfiguration<RefCusTradeGroup>(true);

			entityConfig.IncludeColumn(x => x.ZZA_TradeGroup, true);
			entityConfig.IncludeColumn(x => x.ZZA_Description);
			entityConfig.IncludeColumnWithDefaultValue(x => x.ZZA_EndDate, false, Constants.Common.MinimumDateTime);
			entityConfig.IncludeColumnWithDefaultValue(x => x.ZZA_StartDate, false, Constants.Common.MaximumDateTime);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZA_ZZZ_NKDataGrouping, true, Constants.Common.LocalCountryCode);
			entityConfig.IncludeColumn(x => x.RefCusTradeGroupCountries);
			entityConfig.IncludeColumn(x => x.RefCusTradeGroupLanguages);

			writerConfig.IncludeEntityTypeConfiguration(entityConfig);

			var countryConfig = new EntityTypeConfiguration<RefCusTradeGroupCountry>(true);
			countryConfig.IncludeColumn(x => x.ZZB_RN_NKTradeGroupCountryCode, true);
			countryConfig.IncludeColumnWithDefaultValue(x => x.ZZB_StartDate, false, Constants.Common.MinimumDateTime);
			countryConfig.IncludeColumnWithDefaultValue(x => x.ZZB_EndDate, false, Constants.Common.MaximumDateTime);

			countryConfig.IncludeColumn(x => x.ZZB_Description);

			writerConfig.IncludeEntityTypeConfiguration(countryConfig);

			var tradeGroupLanguageConfiguration = new EntityTypeConfiguration<RefCusTradeGroupLanguage>(true);
			tradeGroupLanguageConfiguration.IncludeColumn(x => x.ZXD_ZX6_NKLanguage, true);
			tradeGroupLanguageConfiguration.IncludeColumn(x => x.ZXD_Description);

			writerConfig.IncludeEntityTypeConfiguration(tradeGroupLanguageConfiguration);

			return writerConfig;
		}

		public static XmlWriterConfiguration GetRefCusTariffWriterConfiguration(string type)
		{
			bool importFlow = "EXP" != type;

			var writerConfiguration = new XmlWriterConfiguration();

			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(false);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, type);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.Common.EUNCountryCode);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.Common.EUNCountryCode);
			tariffConfiguration.IncludeColumn(x => x.RefCusConditions);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfiguration);

			var condConfiguration = new EntityTypeConfiguration<RefCusCondition>(true);
			condConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_ZZZ_NKDataGrouping, true, Constants.Common.LocalCountryCode);
			condConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZZ_NKDataGrouping, true, Constants.Common.LocalCountryCode);
			condConfiguration.IncludeColumn(x => x.ZX1_Comment);
			condConfiguration.IncludeColumn(x => x.ZX1_ZX2_NKConditionType, true);
			condConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ConditionValueTrueMeansStop, false, false);
			condConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_IsExport, false, !importFlow);
			condConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_IsImport, false, importFlow);
			condConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_StartDate, false, Constants.ZZRefCusCondition.MinimumDateTime);
			condConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_EndDate, false, Constants.Common.MaximumDateTime);
			condConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
			condConfiguration.IncludeColumn(x => x.RefCusConditionValues);
			writerConfiguration.IncludeEntityTypeConfiguration(condConfiguration);

			var condvalueConfiguration = new EntityTypeConfiguration<RefCusConditionValue>(true);
			condvalueConfiguration.IncludeColumnWithConstantValue(x => x.ZX3_ZX4_ZZZ_NKDataGrouping, true, Constants.Common.LocalCountryCode);
			condvalueConfiguration.IncludeColumn(x => x.ZX3_Value, true);
			condvalueConfiguration.IncludeColumn(x => x.ZX3_LogicalORWithinGroup, false, 0);
			condvalueConfiguration.IncludeColumnWithConstantValue(x => x.ZX3_ZX4_NKValueType, true, "SUP");
			writerConfiguration.IncludeEntityTypeConfiguration(condvalueConfiguration);

			tariffConfiguration.IncludeColumn(x => x.RefCusVATApplicabilities);
			var vatapplConfiguration = new EntityTypeConfiguration<RefCusVATApplicability>(true);
			vatapplConfiguration.IncludeColumn(x => x.ZX5_ZZF_NKTaxOrFeeCode, true);
			vatapplConfiguration.IncludeColumnWithConstantValue(x => x.ZX5_ZZZ_NKDataGrouping, true, Constants.Common.LocalCountryCode);
			vatapplConfiguration.IncludeColumn(x => x.ZX5_AdditionalCode, true);
			vatapplConfiguration.IncludeColumnWithDefaultValue(x => x.ZX5_StartDate, false, Constants.Common.MinimumDateTime);
			vatapplConfiguration.IncludeColumnWithDefaultValue(x => x.ZX5_EndDate, false, Constants.Common.MaximumDateTime);
			writerConfiguration.IncludeEntityTypeConfiguration(vatapplConfiguration);

			tariffConfiguration.IncludeColumn(x => x.RefCusRates);
			var rateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula, false);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.Common.LocalCountryCode);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.Common.LocalCountryCode);
			rateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_StartDate, false, Constants.Common.MinimumDateTime);
			rateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ2_EndDate, false, Constants.Common.MaximumDateTime);
			rateConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
			writerConfiguration.IncludeEntityTypeConfiguration(rateConfiguration);

			var excludedTradeGroupsConfiguration = new EntityTypeConfiguration<RefCusExcludedTradeGroup>(true);
			excludedTradeGroupsConfiguration.IncludeColumn(x => x.ZZC_ZZA_NKTradeGroup, true);
			excludedTradeGroupsConfiguration.IncludeColumnWithConstantValue(x => x.ZZC_ZZA_ZZZ_NKDataGrouping, true, Constants.Common.LocalCountryCode);
			writerConfiguration.IncludeEntityTypeConfiguration(excludedTradeGroupsConfiguration);

			var applConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			applConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_ZZA_NKTradeGroup, true, "1011");
			applConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, false, Constants.Common.EUNCountryCode);
			applConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_AdditionalCode, true, string.Empty);
			applConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_OrderNumber, true, string.Empty);
			applConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_StartDate, false, Constants.ZZRefCusCondition.MinimumDateTime);
			applConfiguration.IncludeColumnWithDefaultValue(x => x.ZZT_EndDate, false, Constants.Common.MaximumDateTime);
			applConfiguration.IncludeColumn(x => x.RefCusExcludedTradeGroups);
			writerConfiguration.IncludeEntityTypeConfiguration(applConfiguration);

			return writerConfiguration;
		}

		public static void GenerateXml(string dataSource, string destFileName, List<RefCusTariff> resultList, string type)
		{
			var downloadDir = ApplicationConfig.DownloadDir;
			var xmlFileOutputFile = Path.Combine(downloadDir, destFileName);
			var writerConfiguration = GetRefCusTariffWriterConfiguration(type);
			ExportToXMLFile(dataSource, xmlFileOutputFile, writerConfiguration, DateTimeOffset.Now.LocalDateTime, resultList);
		}

		public static void GenerateXml(string dataSource, string destFileName, List<RefCusCodeList> resultList, string codeType, DateTime generationTimestamp, string downloadDir)
		{
			var xmlFileOutputFile = Path.Combine(downloadDir, destFileName);
			var writerConfiguration = GetRefAddCodesWriterConfiguration(codeType);
			ExportToXMLFile(dataSource, xmlFileOutputFile, writerConfiguration, generationTimestamp, resultList);
		}

		public static void GenerateLocXml(string dataSource, string destFileName, List<RefCusCodeList> resultList)
		{
			var downloadDir = ApplicationConfig.DownloadDir;
			var xmlFileOutputFile = Path.Combine(downloadDir, destFileName);
			var writerConfiguration = GetRefLocCodesWriterConfiguration();
			ExportToXMLFile(dataSource, xmlFileOutputFile, writerConfiguration, DateTimeOffset.Now.LocalDateTime, resultList);
		}

		public static XmlWriterConfiguration GetRefExchangeRateWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, Constants.RefExchangeRate.CUSRateType);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, Constants.Common.LocalCountryCode);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_StartDate, true, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_EndDate);
			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefAddCodesWriterConfiguration(string codeType)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			var codeListLanguageConfiguration = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);

			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Description);
			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListLanguages);
			if (!string.IsNullOrEmpty(codeType))
			{
				codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			}
			else
			{
				codeListConfiguration.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, true);
			}
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.Common.LocalCountryCode);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.Common.MinimumDateTime);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.Common.MaximumDateTime);

			codeListLanguageConfiguration.IncludeColumn(x => x.ZXA_ZX6_NKLanguage, true);
			codeListLanguageConfiguration.IncludeColumn(x => x.ZXA_Description);

			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListLanguageConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefNctsCodesWriterConfiguration(string codeType)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);

			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.Common.LocalCountryCode);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Description);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.Common.MinimumDateTime);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.Common.MaximumDateTime);
			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListLanguages);
			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes);

			var codeListLanguageConfiguration = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);
			codeListLanguageConfiguration.IncludeColumn(x => x.ZXA_ZX6_NKLanguage, true);
			codeListLanguageConfiguration.IncludeColumn(x => x.ZXA_Description);

			var codeListAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			codeListAttributeConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			codeListAttributeConfiguration.IncludeColumn(x => x.ZZE_Value, true);

			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListLanguageConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListAttributeConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefNctsAddCodesWriterConfiguration()
		{
			return GetRefNctsCodesWriterConfiguration(Constants.ZZRefCusCodeList.NctsAdditionalCode);
		}

		public static XmlWriterConfiguration GetRefNctsAddInfoCodesWriterConfiguration()
		{
			return GetRefNctsCodesWriterConfiguration(Constants.ZZRefCusCodeList.NctsAdditionalInfoCode);
		}

		public static XmlWriterConfiguration GetRefNctsTraDocCodesWriterConfiguration()
		{
			return GetRefNctsCodesWriterConfiguration(Constants.ZZRefCusCodeList.NctsTransportDocumentCode);
		}

		public static XmlWriterConfiguration GetRefLocCodesWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			var codeListAttrConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);

			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Description);
			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "FAC");
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.Common.LocalCountryCode);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.Common.MinimumDateTime);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.Common.MaximumDateTime);

			codeListAttrConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			codeListAttrConfiguration.IncludeColumn(x => x.ZZE_Value);

			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListAttrConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetCodeListTypeWriterConfiguration(string codeType)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			var codeListAttrConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			var codeListLangConfiguration = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);

			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Description);
			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes);
			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListLanguages);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_EndDate, false, Constants.Common.MaximumDateTime);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_StartDate, false, Constants.Common.MinimumDateTime);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.Common.LocalCountryCode);

			codeListAttrConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			codeListAttrConfiguration.IncludeColumn(x => x.ZZE_Value, true);

			codeListLangConfiguration.IncludeColumn(x => x.ZXA_ZX6_NKLanguage, true);
			codeListLangConfiguration.IncludeColumn(x => x.ZXA_Description);

			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListAttrConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListLangConfiguration);
			return writerConfiguration;
		}


		public static void ExportToXMLFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, IEnumerable<T> codeList, UpdateType updateType = UpdateType.Full)
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

		public static string ReplaceHexadecimalSymbols(string txt)
		{
			var r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";

			return txt != null ? Regex.Replace(txt, r, "", RegexOptions.Compiled) : string.Empty;
		}
	}
}
