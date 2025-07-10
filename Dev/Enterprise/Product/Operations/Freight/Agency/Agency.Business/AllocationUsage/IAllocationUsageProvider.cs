using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public interface IAllocationUsageProvider
	{
		void Load(JobVoyage voyage, ZGuid principalPK);
		AllocationUsage GetSailingUsage(JobSailing sailing);
		AllocationUsage GetOriginUsage(VoyageOrigin origin);
		AllocationUsage GetCountryUsage(VoyageCountry country);
	}
}
