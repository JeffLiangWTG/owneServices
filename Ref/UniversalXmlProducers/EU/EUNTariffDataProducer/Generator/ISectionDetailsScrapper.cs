using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface ISectionDetailsScrapper
	{
		IDictionary<int, ISection> ExtractChapterToSectionMap(string url);
	}
}
