namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public interface IDataLookup : IDataLoader
	{
		string Lookup(string key);
		string ReplacementLookup(string tariffCode, string key);
	}
}
