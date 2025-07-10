using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	internal class NomenclatureXMLWriterConfigurationBuilder
	{
		const string NomanclatureGroupType = "AU";
		public static XmlWriterConfiguration Build()
		{
			var configuration = new XmlWriterConfiguration();

			var refCusNomenclatureGroupConfiguration = new EntityTypeConfiguration<RefCusNomenclatureGroup>(true);
			refCusNomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_Value);
			refCusNomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_CompositeKey, true);
			refCusNomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_Description);
			refCusNomenclatureGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_ZZ9_NKNomenclatureGroupType, true, NomanclatureGroupType);
			refCusNomenclatureGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_ZZZ_NKDataGrouping, true, Constants.DataGrouping);
			refCusNomenclatureGroupConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ5_StartDate, false, Constants.RefData_Common.MinimumDateTime);
			refCusNomenclatureGroupConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ5_EndDate, false, Constants.RefData_Common.MaximumDateTime);
			configuration.IncludeEntityTypeConfiguration(refCusNomenclatureGroupConfiguration);

			return configuration;
		}
	}
}
