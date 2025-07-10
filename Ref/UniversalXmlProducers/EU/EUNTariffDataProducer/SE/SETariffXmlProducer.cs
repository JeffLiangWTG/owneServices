using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.SE
{
	public class SETariffXmlProducer : XmlProducer<RefCusTariff>
	{
		public SETariffXmlProducer()
		{
			XmlWriter = new XmlWriter(GetWriterConfiguration());
		}
		public override string FilePath => ApplicationConfig.SEAndDailyTariffUXmlFile;

		public override string DataSource => ApplicationConfig.SEAndDailyTariffDataSource;

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			return new ImportTariffXmlProducer().GetWriterConfiguration(false, true);
		}
	}
}
