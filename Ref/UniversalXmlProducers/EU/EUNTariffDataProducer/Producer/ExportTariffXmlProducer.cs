namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ExportTariffXmlProducer : TariffXmlProducer
	{
		public ExportTariffXmlProducer(bool includeCompositeKey = true)
			: base(includeCompositeKey)
		{
		}

		public override string FilePath => ApplicationConfig.ExportTariffUXmlFile;

		public override string DataSource => ApplicationConfig.ExportTariffDataSource;

		protected override string TariffType => "EXP";

		protected override bool IsImport => false;

		protected override bool IsExport => true;
	}
}
