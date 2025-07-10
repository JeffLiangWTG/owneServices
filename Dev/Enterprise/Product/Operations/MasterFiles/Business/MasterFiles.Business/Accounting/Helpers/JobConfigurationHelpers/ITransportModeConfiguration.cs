using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers
{
	public interface ITransportModeConfiguration : IJobTypeConfiguration
	{
		ZPropertyInfo TransportModeInfo { get; }
	}
}
