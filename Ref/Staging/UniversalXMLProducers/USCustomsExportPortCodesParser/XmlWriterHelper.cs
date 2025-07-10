using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.USCustomsExportPortCodesParser
{
	public static class XmlWriterHelper
	{
		public static XmlWriterConfiguration GetRefCusCodeListWriterConfiguration()
		{
			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "US");
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_ZZK_NKCodeType, true, "CUSOF");
			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Description, false);
			codeListConfiguration.IncludeColumn(x => x.ZZD_StartDate, false);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, new DateTime(2079, 6, 6, 23, 59, 00));
			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes, false);
			codeListConfiguration.IncludeColumn(x => x.RefCusCodeOrAttributeTransportModes, false);

			var codeListAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			codeListAttributeConfiguration.IncludeColumnWithConstantValue(o => o.ZZE_ZXE_NKName, true, "ROLE");
			codeListAttributeConfiguration.IncludeColumnWithConstantValue(o => o.ZZE_Value, false, "EXP");

			var codeOrAttributeTransportModeConfiguration = new EntityTypeConfiguration<RefCusCodeOrAttributeTransportMode>(true);
			codeOrAttributeTransportModeConfiguration.IncludeColumn(o => o.ZZU_TransportMode, true);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListAttributeConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(codeOrAttributeTransportModeConfiguration);

			return writerConfiguration;
		}
	}
}
