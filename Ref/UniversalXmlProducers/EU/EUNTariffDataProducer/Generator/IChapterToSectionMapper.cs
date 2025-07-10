using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IChapterToSectionMapper
	{
		int GetSection(int chapterNumber);
		IEnumerable<ISection> GetAllSection();
	}
}
