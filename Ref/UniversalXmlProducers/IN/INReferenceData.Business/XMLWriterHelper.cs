using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public static class XMLWriterHelper
	{
		public static void ExportToXMLFile<T>(XmlWriterConfiguration xmlWriterConfig, IEnumerable<T> codeList, string dataSource, DateTime publicationDateTime, UpdateType updateType, string outputFilePath)
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(updateType);

			var dirName = Path.GetDirectoryName(outputFilePath);
			Directory.CreateDirectory(dirName);

			foreach (var code in codeList)
			{
				writer.PopulateData(code);
			}
			writer.SaveXml(outputFilePath);
		}

		public static XmlWriterConfiguration GetDBKTariffXMLWriterConfiguration(DateTime startDate)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var refTariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);
			refTariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			refTariffConfiguration.IncludeColumn(x => x.ZZ1_Description, false);
			refTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffTypes.DrawbackSchedule);
			refTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGrouping.IN);
			refTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DataGrouping.IN);
			refTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_StartDate, false, startDate);
			refTariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_EndDate, false, Constants.DefaultEndDate);
			refTariffConfiguration.IncludeColumn(x => x.RefCusRates);
			refTariffConfiguration.IncludeColumn(x => x.RefCusTariffRelationships);

			var rateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula);
			rateConfiguration.IncludeColumn(x => x.ZZ2_RateFormulaDerivedFrom);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_NKRateType, true, Constants.RateTypes.DrawbackSchedule);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.DataGrouping.IN);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.DataGrouping.IN);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_StartDate, false, startDate);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_EndDate, false, Constants.DefaultEndDate);
			rateConfiguration.IncludeColumn(x => x.RefCusRateUOMs);

			var relationshipConfiguration = new EntityTypeConfiguration<RefCusTariffRelationship>(true);
			relationshipConfiguration.IncludeColumn(x => x.ZZH_TariffCode, true);
			relationshipConfiguration.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_NKTariffType, true, Constants.TariffTypes.CTH);
			relationshipConfiguration.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGrouping.IN);

			var rateUomConfiguration = new EntityTypeConfiguration<RefCusRateUOM>(true);
			rateUomConfiguration.IncludeColumn(x => x.ZXG_UOM, true);

			writerConfiguration.IncludeEntityTypeConfiguration(refTariffConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(rateConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(relationshipConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(rateUomConfiguration);
			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetEDILocationXMLWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusCodeListConfiguration = GetRefCusCodeListConfiguration(Constants.CodeListTypes.CUSOF, Constants.DefaultStartDate, true);
			refCusCodeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes);
			refCusCodeListConfiguration.IncludeColumn(x => x.RefCusCodeOrAttributeTransportModes);

			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeListConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(GetRefCusCodeListAttributeConfiguration(Constants.CodeListAttributes.EmailAddress));
			writerConfiguration.IncludeEntityTypeConfiguration(GetRefCusCodeOrAttributeTransportModeConfiguration());

			return writerConfiguration;
		}

		static EntityTypeConfiguration<RefCusCodeList> GetRefCusCodeListConfiguration(string codeType,DateTime startDate,bool descIsKey = false)
		{
			var configuration = new EntityTypeConfiguration<RefCusCodeList>(true);

			configuration.IncludeColumn(x => x.ZZD_Code, true);
			configuration.IncludeColumn(x => x.ZZD_Description, descIsKey);
			configuration.IncludeColumnWithConstantValue(x => x.ZZD_StartDate, false, startDate);
			configuration.IncludeColumnWithConstantValue(x => x.ZZD_EndDate, false, Constants.DefaultEndDate);
			configuration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			configuration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DataGrouping.IN);

			return configuration;
		}

		static EntityTypeConfiguration<RefCusCodeListAttribute> GetRefCusCodeListAttributeConfiguration(string nkName, bool valueIsKey = true)
		{
			var configuration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);

			configuration.IncludeColumnWithConstantValue(x => x.ZZE_ZXE_NKName, true, nkName);
			configuration.IncludeColumn(x => x.ZZE_Value, valueIsKey);

			return configuration;
		}

		static EntityTypeConfiguration<RefCusCodeOrAttributeTransportMode> GetRefCusCodeOrAttributeTransportModeConfiguration()
		{
			var configuration = new EntityTypeConfiguration<RefCusCodeOrAttributeTransportMode>(true);

			configuration.IncludeColumn(x => x.ZZU_TransportMode, true);

			return configuration;
		}

		public static XmlWriterConfiguration GetRefExchangeRateWriterConfiguration(DateTime startDate, string rateType)
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(GetRefExchangeRateZZConfiguration(startDate, rateType));
			return xmlWriterConfiguration;
		}

		static EntityTypeConfiguration<RefExchangeRateZZ> GetRefExchangeRateZZConfiguration(DateTime startDate, string rateType)
		{
			var configuration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);

			configuration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, isKeyColumn: true, rateType);
			configuration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, isKeyColumn: true);
			configuration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, isKeyColumn: true, Constants.DataGrouping.IN);
			configuration.IncludeColumnWithDefaultValue(x => x.ZZN_StartDate, isKeyColumn: true,
				startDate, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0,
				"RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			configuration.IncludeColumnWithDefaultValue(x => x.ZZN_EndDate, isKeyColumn: false, startDate.AddDays(1).AddMinutes(-1));
			configuration.IncludeColumn(x => x.ZZN_Rate, isKeyColumn: false);
			configuration.IncludeColumn(x => x.ZZN_AsPublished, isKeyColumn: false);

			return configuration;
		}

		public static XmlWriterConfiguration GetRefTariffWriterConfiguration(DateTime startDate)
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(GetRefTariffConfiguration(startDate));
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(GetRefTariffUOMConfiguration());
			return xmlWriterConfiguration;
		}

		static EntityTypeConfiguration<RefCusTariff> GetRefTariffConfiguration(DateTime startDate)
		{
			var configuration = new EntityTypeConfiguration<RefCusTariff>(true);

			configuration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			configuration.IncludeColumn(x => x.ZZ1_Description, false);
			configuration.IncludeColumn(x => x.RefCusTariffUOMs, false);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ1_StartDate, false, startDate);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ1_EndDate, false, Constants.DefaultEndDate);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffTypes.CTH);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DataGrouping.IN);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGrouping.IN);

			return configuration;
		}

		static EntityTypeConfiguration<RefCusTariffUOM> GetRefTariffUOMConfiguration()
		{
			var configuration = new EntityTypeConfiguration<RefCusTariffUOM>(true);

			configuration.IncludeColumn(x => x.ZZ8_UOM, false);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ8_Type, true, Constants.CodeListTypes.CU1);
			configuration.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, true, Constants.DataGrouping.IN);

			return configuration;
		}

		public static XmlWriterConfiguration GetErrorCodesXMLWriterConfiguration(string codeType)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusCodeListConfiguration = GetRefCusCodeListConfiguration(codeType, Constants.DefaultStartDate);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeListConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetWarehouseCodeXMLWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusCodeListConfiguration = GetRefCusCodeListConfiguration(Constants.CodeListTypes.WarehouseCode, Constants.DefaultStartDate2025);
			refCusCodeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeListConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(GetRefCusCodeListAttributeConfiguration(Constants.CodeListAttributes.Address, false));

			return writerConfiguration;
		}
	}
}
