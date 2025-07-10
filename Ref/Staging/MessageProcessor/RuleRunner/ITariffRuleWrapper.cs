using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.Staging.Rule
{
	public enum RuleType
	{
		INSERT,
		UPDATE
	}

	public interface ITariffRuleWrapper
	{
		Task<IEnumerable<IRuleResult<T>>> SynchroniseAsync<T>(Guid parentPK, T[] data);
	}
}
