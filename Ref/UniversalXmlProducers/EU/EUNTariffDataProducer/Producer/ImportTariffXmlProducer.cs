namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ImportTariffXmlProducer : TariffXmlProducer
	{
		public ImportTariffXmlProducer(bool includeCompositeKey = true)
			: base(includeCompositeKey)
		{
		}

		public override string FilePath => ApplicationConfig.ImportTariffUXmlFile;

		public override string DataSource => ApplicationConfig.ImportTariffDataSource;

		protected override string TariffType => "IMP";

		protected override bool IsImport => true;

		protected override bool IsExport => false;
	}
}
