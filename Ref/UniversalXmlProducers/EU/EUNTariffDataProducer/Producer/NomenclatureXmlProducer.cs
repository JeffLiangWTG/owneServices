using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class NomenclatureXmlProducer : XmlProducer<RefCusNomenclatureGroup>
	{
		public NomenclatureXmlProducer()
		{
			XmlWriter = new XmlWriter(GetWriterConfiguration());
		}
		public override string FilePath => ApplicationConfig.NomenclatureUXmlFile;
		public override string DataSource => ApplicationConfig.NomenclatureDataSource;

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			var nomenclatureGroupConfiguration = new EntityTypeConfiguration<RefCusNomenclatureGroup>(true);
			nomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_CompositeKey, true);
			nomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_Value, false);
			nomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_Description, false);
			nomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_StartDate, false);
			nomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_EndDate, false);
			nomenclatureGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_ZZ9_NKNomenclatureGroupType, true, "CN");
			nomenclatureGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_ZZZ_NKDataGrouping, true, "EUN");
			nomenclatureGroupConfiguration.IncludeColumn(x => x.RefCusNomenclatureLanguages, false);

			var nomenclatureLanguageConfiguration = new EntityTypeConfiguration<RefCusNomenclatureLanguage>(true);
			nomenclatureLanguageConfiguration.IncludeColumn(x => x.ZX8_Description, false);
			nomenclatureLanguageConfiguration.IncludeColumn(x => x.ZX8_ZX6_NKLanguage, true);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(nomenclatureGroupConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(nomenclatureLanguageConfiguration);

			return writerConfiguration;
		}
	}
}
