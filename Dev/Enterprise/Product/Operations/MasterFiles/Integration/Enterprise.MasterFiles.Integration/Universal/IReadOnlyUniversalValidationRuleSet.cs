using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IReadOnlyUniversalValidationRuleSet
	{
		ZBool AlwaysApply { get; }
		ZString Code { get; }
		ZString Criteria { get; }
		ZString DataContext { get; }
		ZString Description { get; }
		ZBool IsActive { get; }
		ZString Name { get; }

		IEnumerable<IReadOnlyUniversalValidationRule> ActiveRules { get; }
	}
}
