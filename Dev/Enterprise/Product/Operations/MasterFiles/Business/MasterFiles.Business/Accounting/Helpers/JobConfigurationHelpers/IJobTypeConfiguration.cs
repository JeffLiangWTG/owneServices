using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers
{
	public interface IJobTypeConfiguration
	{
		ZString JobTypeCode { get; }
		ZPropertyInfo JobTypeCodeInfo { get; }
	}
}
