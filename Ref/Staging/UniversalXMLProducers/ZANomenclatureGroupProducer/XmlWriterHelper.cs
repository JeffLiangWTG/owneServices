using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer
{
	public static class XmlWriterHelper
	{
		public static XmlWriterConfiguration GetRefCusTariffWriterConfiguration()
		{
			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(false);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, "1P1");
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, "ZA");
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, "ZA");
			tariffConfiguration.IncludeColumn(x => x.ZZ1_CompositeKeyOnZZ5, false, IsDataValue.True);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusNomenclatureGroupConfiguration()
		{
			var nomenclatureGroupConfiguration = new EntityTypeConfiguration<RefCusNomenclatureGroup>(true);
			nomenclatureGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_ZZ9_NKNomenclatureGroupType, true, "ZA");
			nomenclatureGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_ZZZ_NKDataGrouping, true, "ZA");
			nomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_Value, false);
			nomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_Description, false);
			nomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_StartDate, false);
			nomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_EndDate, false);
			nomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_CompositeKey, true);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(nomenclatureGroupConfiguration);

			return writerConfiguration;
		}
	}
}
