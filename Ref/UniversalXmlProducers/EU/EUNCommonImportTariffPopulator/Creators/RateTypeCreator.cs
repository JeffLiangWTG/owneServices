using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class RateTypeCreator : IRateTypeCreator
	{
		public string Get(string rateCode)
		{
			switch (rateCode)
			{
				case "A30":
				case "A35":
					return "ADD";
				case "A40":
				case "A45":
					return "CVD";
				case "A00":
				case "A20":
					return "DTY";
				default:
					return string.Empty;
			}
		}
	}
}
