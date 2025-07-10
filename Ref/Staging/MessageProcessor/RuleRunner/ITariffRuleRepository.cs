using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.Staging.Rule
{
	public interface ITariffRuleRepository
	{
		Task<IEnumerable<ITariffRuleHeaderWrapper>> GetRuleHeaderWrappersAsync(string tariffCode, string schedule, string checkdigit, string relationshipCode, string countryCode);
		Task<IEnumerable<ITariffRuleHeaderWrapper>> GetUnAppliedRuleHeaderWrappersAsync();
	}
}
