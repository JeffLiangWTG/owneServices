using System.Collections.Generic;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public interface IOtherScraper
	{
		void Load();
		void Pack(List<DynamicCsvRecord> records);
		void Release();
	}
}
