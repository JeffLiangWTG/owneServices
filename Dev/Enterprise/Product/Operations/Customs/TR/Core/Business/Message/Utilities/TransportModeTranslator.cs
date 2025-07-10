using System.Collections.Generic;

namespace Enterprise.Customs.TR.Business
{
	public class TransportModeTranslator : Customs.Business.TransportModeTranslator
	{
		protected override Dictionary<string, string> CargoWiseToWCO => new Dictionary<string, string>
		{
			{ "AIR", "5" },
			{ "IWT", "3" },
			{ "OTH", "_" },
			{ "RAI", "6" },
			{ "ROA", "4" },
			{ "SEA", "3" },
		};
	}
}
