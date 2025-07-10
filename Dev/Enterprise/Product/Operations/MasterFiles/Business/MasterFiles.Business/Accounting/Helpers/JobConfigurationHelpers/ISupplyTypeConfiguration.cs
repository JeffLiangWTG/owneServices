using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers
{
	public interface ISupplyTypeConfiguration
	{
		ZString SupplyTypeCode { get; }
		ZPropertyInfo SupplyTypeCodeInfo { get; }
	}
}
