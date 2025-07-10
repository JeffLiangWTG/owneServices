using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public static class Helper
	{
		public static XmlWriterConfiguration GetRefCusCodeListWriterConfiguration(string codeType)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			DefaultRefCusCodeListWriterConfiguration(writerConfiguration, new EntityTypeConfiguration<RefCusCodeList>(true), codeType);
			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetExchangeRateWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			DefaultExchangeRateWriterConfiguration(writerConfiguration, exchangeRateConfiguration);
			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusTaxOrFeeWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var refCusTaxOrFeeConfiguration = new EntityTypeConfiguration<RefCusTaxOrFee>(true);
			DefaultRefCusTaxOrFeeWriterConfiguration(writerConfiguration, refCusTaxOrFeeConfiguration);
			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusCodeListWriterConfigurationWithAttributes(bool isValueKeyColumn) => GetRefCusCodeListWriterConfigurationWithAttributes(null, isValueKeyColumn);

		public static XmlWriterConfiguration GetRefCusCodeListWriterConfigurationWithAttributes(string codeType, bool isValueKeyColumn)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			DefaultRefCusCodeListWriterConfiguration(writerConfiguration, codeListConfiguration, codeType);

			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes, false);
			var codelistAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_Value, isValueKeyColumn);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistAttributeConfiguration);

			return writerConfiguration;
		}

		static void DefaultRefCusCodeListWriterConfiguration(XmlWriterConfiguration writerConfiguration, EntityTypeConfiguration<RefCusCodeList> codeListConfiguration, string codeType)
		{
			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Description, false);
			if (string.IsNullOrWhiteSpace(codeType))
			{
				codeListConfiguration.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, true);
			}
			else
			{
				codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			}

			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "AU");
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.RefData_Common.MinimumDateTime);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.RefData_Common.MaximumDateTime);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);
		}

		static void DefaultExchangeRateWriterConfiguration(XmlWriterConfiguration writerConfiguration, EntityTypeConfiguration<RefExchangeRateZZ> exchangeRateConfiguration)
		{
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, "CUS");
			exchangeRateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZN_StartDate, true, Constants.RefData_Common.MinimumDateTime, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZN_EndDate, false, DateTime.Today);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate, false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, "AU");
			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);
		}

		static void DefaultRefCusTaxOrFeeWriterConfiguration(XmlWriterConfiguration writerConfiguration, EntityTypeConfiguration<RefCusTaxOrFee> refCusTaxOrFeeConfiguration)
		{
			refCusTaxOrFeeConfiguration.IncludeColumn(x => x.ZZF_Code, true);
			refCusTaxOrFeeConfiguration.IncludeColumn(x => x.ZZF_Description, false);
			refCusTaxOrFeeConfiguration.IncludeColumn(x => x.ZZF_Value, false);
			refCusTaxOrFeeConfiguration.IncludeColumnWithDefaultValue(x => x.ZZF_StartDate, false, Constants.RefData_Common.MinimumDateTime);
			refCusTaxOrFeeConfiguration.IncludeColumnWithDefaultValue(x => x.ZZF_EndDate, false, Constants.RefData_Common.MaximumDateTime);
			refCusTaxOrFeeConfiguration.IncludeColumnWithConstantValue(x => x.ZZF_ZZZ_NKDataGrouping, true, "AU");
			refCusTaxOrFeeConfiguration.IncludeColumnWithConstantValue(x => x.ZZF_ZX0_NKTaxOrFeeType, false, "OTH");
			refCusTaxOrFeeConfiguration.IncludeColumnWithConstantValue(x => x.ZZF_Minimum, false, 0.0m);
			refCusTaxOrFeeConfiguration.IncludeColumnWithConstantValue(x => x.ZZF_Maximum, false, 0.0m);
			refCusTaxOrFeeConfiguration.IncludeColumnWithConstantValue(x => x.ZZF_Threshold, false, 0.0m);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTaxOrFeeConfiguration);
		}

		public static XmlWriterConfiguration GetRefCusCodeTypeWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeTypeConfiguration = new EntityTypeConfiguration<RefCusCodeType>(true);
			codeTypeConfiguration.IncludeColumn(x => x.ZZK_CodeType, true);
			codeTypeConfiguration.IncludeColumn(x => x.ZZK_Description, false);
			codeTypeConfiguration.IncludeColumnWithConstantValue(x => x.ZZK_ZZZ_NKDataGrouping, true, "AU");
			writerConfiguration.IncludeEntityTypeConfiguration(codeTypeConfiguration);
			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusAUNexdocECMCodeWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var codeTypeConfiguration = new EntityTypeConfiguration<RefCusAUNexdocECMCode>(true);
			codeTypeConfiguration.IncludeColumn(x => x.ZY5_CommodityCode, true);
			codeTypeConfiguration.IncludeColumn(x => x.ZY5_PreservationCode, true);
			codeTypeConfiguration.IncludeColumn(x => x.ZY5_ProductTypeCode, true);
			codeTypeConfiguration.IncludeColumn(x => x.ZY5_PackTypeCode, true);
			codeTypeConfiguration.IncludeColumn(x => x.ZY5_SupplementaryCode, true);

			writerConfiguration.IncludeEntityTypeConfiguration(codeTypeConfiguration);
			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusCodeListAttributeNameWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListAttributeNameConfiguration = new EntityTypeConfiguration<RefCusCodeListAttributeName>(true);
			codeListAttributeNameConfiguration.IncludeColumnWithConstantValue(x => x.ZXE_Name, true, "ProductType");
			codeListAttributeNameConfiguration.IncludeColumn(x => x.ZXE_Description, false);
			codeListAttributeNameConfiguration.IncludeColumn(x => x.ZXE_ZZK_NKCodeType, true);
			codeListAttributeNameConfiguration.IncludeColumnWithConstantValue(x => x.ZXE_ZZZ_NKDataGrouping, true, "AU");
			codeListAttributeNameConfiguration.IncludeColumnWithConstantValue(x => x.ZXE_IsMandatory, false, true);
			codeListAttributeNameConfiguration.IncludeColumnWithConstantValue(x => x.ZXE_AllowDuplicates, false, false);
			codeListAttributeNameConfiguration.IncludeColumnWithConstantValue(x => x.ZXE_IsValueMandatory, false, true);
			codeListAttributeNameConfiguration.IncludeColumn(x => x.ZXE_ZZK_NKCodeTypeForValueList);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListAttributeNameConfiguration);
			return writerConfiguration;
		}

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

		public static RefCusCodeList BaseRefCusCodeList(IKeyValues keyValues, IItemCodeSet[] listItemCodeSet)
		{
			var refcusCodeList = new RefCusCodeList()
			{
				ZZD_Code = keyValues.Code,
				ZZD_Description = keyValues.Description,
				ZZD_StartDate = keyValues.StartDate
			};

			var (endDateSuccessfullyParsed, endDateTime) = listItemCodeSet.GetDateTimeCodeValue(NexDocConstants.ItemCodeSetKeys.EndDate);
			if (endDateSuccessfullyParsed)
			{
				refcusCodeList.ZZD_EndDate = endDateTime;
			}

			return refcusCodeList;
		}

		public static bool IsConditionValidAndReportError(bool condition, string errorMessage, string lineToOutput)
		{
			if (!condition)
			{
				Console.Error.WriteLine(errorMessage, lineToOutput);
			}
			return condition;
		}

		public static IEnumerable<string> NonEmptyLines(this string content)
		{
			using (var reader = new StringReader(content))
			{
				while (reader.ReadLine() is { } line)
				{
					if (!string.IsNullOrWhiteSpace(line))
					{
						yield return line;
					}
				}
			}
		}
	}
}
