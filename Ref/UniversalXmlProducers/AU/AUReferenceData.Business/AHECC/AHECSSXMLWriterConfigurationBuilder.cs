using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	class AHECSSXMLWriterConfigurationBuilder
	{
		public static XmlWriterConfiguration Build(string dataGroupingCode)
		{
			var configuration = new XmlWriterConfiguration();

			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, dataGroupingCode);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffTypes.EXP);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, dataGroupingCode);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_StartDate);
			tariffConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, Constants.RefData_Common.MaximumDateTime);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_Description);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffUOMs);
			configuration.IncludeEntityTypeConfiguration(tariffConfiguration);

			var uom = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			uom.IncludeColumnWithConstantValue(x => x.ZZ8_Type, true, Constants.TariffUOMTypes.CU1);
			uom.IncludeColumn(x => x.ZZ8_UOM, false);
			uom.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, true, dataGroupingCode);
			configuration.IncludeEntityTypeConfiguration(uom);

			return configuration;
		}
	}
}
