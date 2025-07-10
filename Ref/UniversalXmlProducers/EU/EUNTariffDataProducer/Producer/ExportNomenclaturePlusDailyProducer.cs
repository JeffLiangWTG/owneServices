namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ExportNomenclaturePlusDailyProducer : NomenclaturePlusDailyProducer
	{
		public ExportNomenclaturePlusDailyProducer(IDailyTariffUpdatesFileProvider dailyTariffUpdatesFileProvider) : base(dailyTariffUpdatesFileProvider)
		{
		}

		protected override ICompositeKeyTreeGenerator GetNewCompositeKeyTreeGenerator() => new ExportCompositeKeyTreeGenerator(new ChapterToSectionMapper());
	}
}
