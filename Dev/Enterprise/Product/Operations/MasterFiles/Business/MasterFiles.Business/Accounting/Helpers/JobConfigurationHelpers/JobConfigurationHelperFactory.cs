namespace Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers
{
	public interface IJobConfigurationHelperFactory
	{
		IJobTypeConfigurationHelper GetJobTypeHelper(IJobTypeConfiguration parent);
		ITransportModeConfigurationHelper GetTransportModeHelper(ITransportModeConfiguration parent);
		IJobConfigurationHelper GetSupplyTypeHelper(ISupplyTypeConfiguration parent);
		IDuplicateValidationHelper GetDuplicateValidationHelper();
	}

	public class JobConfigurationHelperFactory : IJobConfigurationHelperFactory
	{
		IJobTypeConfigurationHelper IJobConfigurationHelperFactory.GetJobTypeHelper(IJobTypeConfiguration parent) => new JobTypeConfigurationBizoImplementationHelper(parent);

		ITransportModeConfigurationHelper IJobConfigurationHelperFactory.GetTransportModeHelper(ITransportModeConfiguration parent) => new TransportModeConfigurationBizoImplementationHelper(parent);

		IJobConfigurationHelper IJobConfigurationHelperFactory.GetSupplyTypeHelper(ISupplyTypeConfiguration parent) => new SupplyTypeConfigurationBizoImplementationHelper(parent);

		IDuplicateValidationHelper IJobConfigurationHelperFactory.GetDuplicateValidationHelper() => new DuplicateValidationHelper();
	}
}
