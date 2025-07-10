namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	interface IUomCodeLookup
	{
		string Lookup(string rawRateFormula);
	}
}
