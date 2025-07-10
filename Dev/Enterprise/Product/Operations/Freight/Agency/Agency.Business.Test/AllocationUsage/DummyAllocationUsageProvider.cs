using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class DummyAllocationUsageProvider : IAllocationUsageProvider
	{
		public DummyAllocationUsageProvider(AllocationUsage testingUsage)
		{
			usage = testingUsage;
		}

		public void Load(JobVoyage voyage, ZGuid principalPK)
		{
		}

		public AllocationUsage GetCountryUsage(VoyageCountry country)
		{
			return usage;
		}

		public AllocationUsage GetOriginUsage(VoyageOrigin origin)
		{
			return usage;
		}

		public AllocationUsage GetSailingUsage(JobSailing sailing)
		{
			return usage;
		}

		readonly AllocationUsage usage;
	}
}
