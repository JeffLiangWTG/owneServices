using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources
{
	public static class ConditionConstants
	{
		public static readonly Dictionary<string, string> conditionTypeDictionary = new Dictionary<string, string>()
		{
			{ "Altri obblighi","ALTR" },
			{ "Controlli sanitari (USMAF/PIF)","SANIT" },
			{ "Controlli sanitari", "SANIT" },
			{ "Controllo conformità esportazione ortofrutticoli","FREXP" },
			{ "Controllo conformità importazione ortofrutticoli","FRIMP" },
			{ "Controllo fitosanitario.","FITO" },
		};
	}
}
