namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ExportNomenclatureProducer : NomenclatureProducer
	{
		protected override ICompositeKeyTreeGenerator GetNewCompositeKeyTreeGenerator() => new ExportCompositeKeyTreeGenerator(new ChapterToSectionMapper());
	}
}
