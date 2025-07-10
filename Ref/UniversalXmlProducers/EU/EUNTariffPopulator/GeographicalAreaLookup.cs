using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffPopulator
{
	public static class GeographicalAreaLookup
	{
		private static SortedDictionary<string, string> GeographicalAreas = new SortedDictionary<string, string>
		{
			{"ALLTC","1008" },
			{"CAMER","2200" },
			{"CARI","1033" },
			{"CUST 2020","1016" },
			{"ECEU","1013" },
			{"EEA","2012" },
			{"EFTA","1021" },
			{"EPA","1032" },
			{"ERGA OMNES","1011" },
			{"ESA","1034" },
			{"EU","1010" },
			{"GSP","1030" },
			{"HANDY","2301" },
			{"LOMB","2080" },
			{"LOOMS","2300" },
			{"MCH","2110" },
			{"MGB","1054" },
			{"NOWTO","2501" },
			{"PANEU","2400" },
			{"REX","2007" },
			{"REXTC","1009" },
			{"SADC EPA","1035" },
			{"SPGA","2005" },
			{"SPGE","2027" },
			{"SPGL","2020" },
			{"SURV","1005" },
			{"TRDEST","3500" },
			{"WBC","1098" },
			{"WTO","2500" }
		};

		public static string GetGeographicalCode(string value)
		{
			Argument.NotNull(value, nameof(value));
			GeographicalAreas.TryGetValue(value.ToUpper(), out string geographicalArea);
			return geographicalArea ?? value;
		}
	}
}
