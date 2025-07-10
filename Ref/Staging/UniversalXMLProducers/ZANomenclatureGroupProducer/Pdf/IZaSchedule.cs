using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer
{
	public interface IZaSchedule
	{
		List<string> GetHeaderTextForMatching();
		List<ZaColumn> GetColumnXCoordinates();
		List<RowData> GetTariffAndNomenclature(List<ZaPdfCoordinateColumnContent> pdfCoordinateColumns);
	}
}
