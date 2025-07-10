using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.Rule
{
	public interface ITariffRuleHeaderWrapper : ITariffRuleWrapper
	{
		Task<RefCusTariff[]> GetMatchingTariffsWithRelatedObjects();
		void MarkAsApplied();
	}
}
