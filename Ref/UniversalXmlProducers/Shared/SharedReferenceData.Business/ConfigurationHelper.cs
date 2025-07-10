using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business
{
	public static class ConfigurationHelper
	{
		public static XmlWriterConfiguration GetRefCusCodeListWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Description, false);
			codeListConfiguration.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_ZZZ_NKDataGrouping, true);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.DefaultValues.MinDateTime);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.DefaultValues.MaxDateTime);
			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes, true);

			var codeListAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			codeListAttributeConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			codeListAttributeConfiguration.IncludeColumn(x => x.ZZE_Value, true);

			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListAttributeConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusCodeTypeWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeTypeConfiguration = new EntityTypeConfiguration<RefCusCodeType>(true);
			codeTypeConfiguration.IncludeColumn(x => x.ZZK_CodeType, true);
			codeTypeConfiguration.IncludeColumn(x => x.ZZK_Description, false);
			codeTypeConfiguration.IncludeColumn(x => x.ZZK_IsReadonly, false);
			codeTypeConfiguration.IncludeColumnWithDefaultValue(x => x.ZZK_MaxLength, false, 0);
			codeTypeConfiguration.IncludeColumn(x => x.ZZK_ZZZ_NKDataGrouping, true);
			writerConfiguration.IncludeEntityTypeConfiguration(codeTypeConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusCodeListAttributeNameWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListAttributeNameConfiguration = new EntityTypeConfiguration<RefCusCodeListAttributeName>(true);
			codeListAttributeNameConfiguration.IncludeColumn(x => x.ZXE_Name, true);
			codeListAttributeNameConfiguration.IncludeColumn(x => x.ZXE_Description, false);
			codeListAttributeNameConfiguration.IncludeColumn(x => x.ZXE_ZZK_NKCodeType, true);
			codeListAttributeNameConfiguration.IncludeColumn(x => x.ZXE_ZZZ_NKDataGrouping, true);
			codeListAttributeNameConfiguration.IncludeColumnWithDefaultValue(x => x.ZXE_IsMandatory, false, false);
			codeListAttributeNameConfiguration.IncludeColumnWithDefaultValue(x => x.ZXE_AllowDuplicates, false, false);
			codeListAttributeNameConfiguration.IncludeColumnWithDefaultValue(x => x.ZXE_IsValueMandatory, false, false);
			codeListAttributeNameConfiguration.IncludeColumn(x => x.ZXE_ValueDataType, false);
			codeListAttributeNameConfiguration.IncludeColumnWithDefaultValue(x => x.ZXE_MinLengthOrValue, false, 0);
			codeListAttributeNameConfiguration.IncludeColumnWithDefaultValue(x => x.ZXE_MaxLengthOrValue, false, 0);
			codeListAttributeNameConfiguration.IncludeColumnWithConstantValue(x => x.ZXE_DecimalPlaces, false, 0);
			codeListAttributeNameConfiguration.IncludeColumn(x => x.ZXE_ColumnCaption, false);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListAttributeNameConfiguration);

			return writerConfiguration;
		}
	}
}
