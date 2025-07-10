using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CFIAAIRSRegistrationTypes
{
	public static class XmlWriterHelper
	{
		public static XmlWriterConfiguration GetRefCusCodelistConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var codelistConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			codelistConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codelistConfiguration.IncludeColumn(x => x.ZZD_Description);
			codelistConfiguration.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, true);
			codelistConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "CA");
			codelistConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, new DateTime(1900, 1, 1));
			codelistConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.DefaultValues.MaxDateTime);
			codelistConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes, false);
			codelistConfiguration.IncludeColumn(x => x.RefCusCodeListLanguages, false);

			var codelistAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_Value, true);
			codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);

			var codelistLanguageConfiguration = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);
			codelistLanguageConfiguration.IncludeColumn(x => x.ZXA_ZX6_NKLanguage, true);
			codelistLanguageConfiguration.IncludeColumn(x => x.ZXA_Description, false);

			writerConfiguration.IncludeEntityTypeConfiguration(codelistConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistAttributeConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistLanguageConfiguration);

			return writerConfiguration;
		}
	}
}
