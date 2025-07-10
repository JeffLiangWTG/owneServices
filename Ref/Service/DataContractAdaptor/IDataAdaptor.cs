namespace CargoWise.RefDbRepo.Service.DataContractAdaptor;

public interface IDataAdaptor
{
	object ToVersion<T>(T currentVersionData, string version);
	Tuple<int, int, int> ParseVersion(string version);
	int ParseToSRDbVersion(string version);
	bool IsSRDbVersion(string version);
	bool RequireTransform(string version, Type dataSetType);
}
