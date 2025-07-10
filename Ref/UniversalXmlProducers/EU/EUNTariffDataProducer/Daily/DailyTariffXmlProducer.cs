using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class DailyTariffXmlProducer : XmlProducer<RefCusTariff>
	{
		public DailyTariffXmlProducer()
		{
			XmlWriter = new XmlWriter(GetWriterConfiguration());
		}
		public override string FilePath => ApplicationConfig.MonthlyTariffUXmlFile;
		public override string DataSource => ApplicationConfig.MonthlyTariffDataSource;

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			return new ImportTariffXmlProducer().GetWriterConfiguration(false, false);
		}
	}
}
