namespace CargoWise.RefDbRepo.SourceDataBinaryScanner
{
	public interface IConfigProvider
	{
		int ExecutionTimeOutInHours { get; }
		string ConnectionString { get; }
	}
}
