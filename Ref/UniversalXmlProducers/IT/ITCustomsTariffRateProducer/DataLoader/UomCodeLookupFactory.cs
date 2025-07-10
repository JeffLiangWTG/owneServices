namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	sealed class UomCodeLookupFactory
	{
		public static IUomCodeLookup CreateLookup(string rateCode)
		{
			switch (rateCode)
			{
				case RateCode931:
					return new Rate931UomCodeLookup();

				default:
					return new UomCodeLookup();
			}
		}

		const string RateCode931 = "931";
	}
}
