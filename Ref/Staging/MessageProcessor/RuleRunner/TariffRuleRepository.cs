using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.Rule
{
	public class TariffRuleRepository : ITariffRuleRepository
	{
		public TariffRuleRepository(ISafeRepository safeRepo)
		{
			Argument.NotNull(safeRepo, nameof(safeRepo));
			this.safeRepo = safeRepo;
		}

		readonly ISafeRepository safeRepo;

		public async Task<IEnumerable<ITariffRuleHeaderWrapper>> GetUnAppliedRuleHeaderWrappersAsync()
		{
			var result = new List<ITariffRuleHeaderWrapper>();
			var rules = await safeRepo.Get<RefCusTariffRule>().Expand(x => x.RefCusTariffType).Expand(x => x.RefCusTariffAttributeRules)
				.Expand(x => x.RefCusTariffRelationshipRules).Where(x => !x.ZZ1_Applied).ExecuteAsync();
			foreach (var rule in rules)
			{
				result.Add(new TariffRuleHeaderWrapper(rule, safeRepo));
			}
			return result;
		}

		//TODO : Investigate observable to iterate async
		public async Task<IEnumerable<ITariffRuleHeaderWrapper>> GetRuleHeaderWrappersAsync(string tariffCode, string schedule, string checkdigit, string relationshipCode, string countryCode)
		{
			Argument.NotNullOrEmpty(tariffCode, nameof(tariffCode));
			Argument.NotNullOrEmpty(schedule, nameof(schedule));
			Argument.NotNullOrEmpty(countryCode, nameof(countryCode));
			var result = new List<TariffRuleHeaderWrapper>();
			var rules = await safeRepo.Get<RefCusTariffRule>().Expand(x => x.RefCusTariffType).Expand(x => x.RefCusTariffAttributeRules)
				.Expand(x => x.RefCusTariffRelationshipRules).Where(x => tariffCode.StartsWith(x.ZZ1_TariffCode) && x.ZZ1_ZZZ_NKDataGrouping == countryCode).ExecuteAsync();
			foreach (var rule in rules)
			{
				var header = new TariffRuleHeaderWrapper(rule, safeRepo);
				if (header.IsMatch(tariffCode, schedule, checkdigit, relationshipCode, countryCode))
				{
					result.Add(header);
				}
			}
			return result;
		}
	}
}
