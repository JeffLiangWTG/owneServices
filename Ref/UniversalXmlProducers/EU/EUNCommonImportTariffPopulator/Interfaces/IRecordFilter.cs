using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public interface IRecordFilter : IXmlFilter<record>
	{
		object GetValidValueFromRecord(record value);
		bool IsMeasure(record value);
		bool IsGoodsNomenclature(record value);
		bool IsBaseRegulation(record value);
		bool IsModificationRegulation(record value);
	}
}
