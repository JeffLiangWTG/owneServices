using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.USSIMDataPopulator
{
	public static class XmlWriterHelper
	{
		public static XmlWriterConfiguration GerRefCusCodeListWriterConfiguration()
		{
			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "US");
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_ZZK_NKCodeType, true, "USSIM");
			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Description, false);
			codeListConfiguration.IncludeColumn(x => x.ZZD_StartDate, false);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, ApplicationConfig.DefaultEndDate);
			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes, false);

			var codeListAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			codeListAttributeConfiguration.IncludeColumn(o => o.ZZE_ZXE_NKName, true);
			codeListAttributeConfiguration.IncludeColumn(o => o.ZZE_Value, true);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListAttributeConfiguration);

			return writerConfiguration;
		}
	}
}
