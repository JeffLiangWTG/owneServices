using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Schema;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Mapping
{
	public static class MappingUNLOCOHelper
	{
		public static readonly List<UNLOCO> CorrectUNLOCOes = new List<UNLOCO>()
		{
			new UNLOCO()
			{
				Country = "CA",
				Location = "MTR",
				IATA = "YUL",
				IATARegionCode = "YMQ"
			}
		};

		public static UNLOCO UpdateUNLOCOWithCorrectIATA(UNLOCO unloco, UNLOCO correctUnloco)
		{
			Argument.NotNull(unloco, nameof(unloco));
			Argument.NotNull(correctUnloco, nameof(correctUnloco));

			unloco.IATA = string.IsNullOrEmpty(correctUnloco.IATA) ? unloco.IATA : correctUnloco.IATA;
			unloco.IATARegionCode = string.IsNullOrEmpty(correctUnloco.IATARegionCode) ? unloco.IATARegionCode : correctUnloco.IATARegionCode;

			return unloco;
		}
	}
}
