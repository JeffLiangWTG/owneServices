using System.IO;

namespace CargoWise.eHub.Products.NZCustoms.PullService
{
	public interface IConfigurationProvider
	{
		string PartnerID { get; }
		int PullInterval { get; }
		string[] ExceptionMessagePatterns { get; }
		DirectoryInfo FailedToDeliverMessageFolder { get; }
		int RetryCount { get; }
	}
}
