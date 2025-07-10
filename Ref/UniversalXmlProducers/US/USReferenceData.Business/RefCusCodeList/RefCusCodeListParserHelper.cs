using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public static class RefCusCodeListParserHelper
	{
		public static XmlWriterConfiguration GetWriterConfiguration_NoDefaultDate(string codeType)
		{
			var refCusCodeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			refCusCodeList.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Description, false);
			refCusCodeList.IncludeColumn(x => x.ZZD_StartDate, false);
			refCusCodeList.IncludeColumn(x => x.ZZD_EndDate, false);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.USCountryCode);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetWriterConfiguration_HasDefaultDate(string codeType, DateTime startDate, DateTime endDate)
		{
			var refCusCodeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			refCusCodeList.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Description, false);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, startDate);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, endDate);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.USCountryCode);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetWriterConfiguration_NoDefaultDate_HasAttributes(string codeType)
		{
			var refCusCodeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			refCusCodeList.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Description, false);
			refCusCodeList.IncludeColumn(x => x.ZZD_StartDate, false);
			refCusCodeList.IncludeColumn(x => x.ZZD_EndDate, false);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.USCountryCode);
			refCusCodeList.IncludeColumn(x => x.RefCusCodeListAttributes, false);

			var refCusCodeListAttribute = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			refCusCodeListAttribute.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			refCusCodeListAttribute.IncludeColumn(x => x.ZZE_Value, true);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeListAttribute);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetWriterConfiguration_HasDefaultDate_HasAttributes(string codeType, DateTime startDate, DateTime endDate)
		{
			var refCusCodeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			refCusCodeList.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Description, false);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, startDate);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, endDate);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.USCountryCode);
			refCusCodeList.IncludeColumn(x => x.RefCusCodeListAttributes, false);

			var refCusCodeListAttribute = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			refCusCodeListAttribute.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			refCusCodeListAttribute.IncludeColumn(x => x.ZZE_Value, true);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeListAttribute);

			return writerConfiguration;
		}

		public static readonly DateTime MinSmallDateTimeValue = new DateTime(1900, 1, 1, 0, 0, 0);
		public static readonly DateTime MaxSmallDateTimeValue = new DateTime(2079, 6, 6, 23, 59, 29);
	}
}
