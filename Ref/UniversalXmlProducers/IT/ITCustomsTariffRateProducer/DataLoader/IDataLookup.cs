namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	public interface IDataLookup
	{
		string Lookup(string key);
		string ReplacementLookup(string tariffCode, string key);
	}
}
