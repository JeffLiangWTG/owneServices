namespace CargoWise.eHub.Shared.ServiceTaskHost.Core
{
	public interface IRunnerConfiguration
	{
		string ServiceTaskName { get; }
		int RunIntervalInSeconds { get; }
	}
}