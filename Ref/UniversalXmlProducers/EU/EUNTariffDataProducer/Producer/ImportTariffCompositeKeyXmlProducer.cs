using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ImportTariffCompositeKeyXmlProducer : XmlProducer<RefCusTariff>
	{
		public ImportTariffCompositeKeyXmlProducer()
		{
			XmlWriter = new XmlWriter(GetWriterConfiguration());
		}
		public override string FilePath => ApplicationConfig.ImportTariffCompositeKeyUXmlFile;
		public override string DataSource => ApplicationConfig.ImportTariffCompositeKeyDataSource;

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(false);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, "IMP");
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, "EUN");
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, "EUN");
			tariffConfiguration.IncludeColumn(x => x.ZZ1_CompositeKeyOnZZ5, false, IsDataValue.True);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfiguration);
			return writerConfiguration;
		}
	}
}
