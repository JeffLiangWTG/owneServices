using System.Collections.Generic;
using CargoWise.RefDbRepo.CHReferenceData.Services;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	class TariffStructure : Dictionary<string, Description>
	{
		internal TariffStructure Load(DownloadResult tariffStructureDownload)
		{
			TariffStructureLoader.Load(tariffStructureDownload, AddDescription, false);
			return this;
		}

		void AddDescription(string numm, Description description)
		{
			Add(numm, description);
		}
	}
}
