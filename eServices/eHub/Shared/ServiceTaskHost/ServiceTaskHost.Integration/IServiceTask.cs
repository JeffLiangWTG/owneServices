using System.Configuration;
using System.Threading;
using Common.Logging;

namespace CargoWise.eHub.Shared.ServiceTaskHost.Integration
{
	/// <summary>
	/// Service task dll should have config file, which have appSettings named ServiceTaskName and RunIntervalInSeconds.
	/// </summary>
	public interface IServiceTask
	{
		bool Run(ILog logger, Configuration configuration, CancellationToken cancellationToken);
	}
}