using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Workflow.Integration
{
	public interface IReadOnlyProcessFieldChangeRule
	{
		ZString Description { get; }

		ZString SE_NKEvent { get; }

		ZString GroupName { get; }

		ZBool IsActive { get; }

		ZString ProcessType { get; }

		ZString Reference { get; }

		ZGuid PK { get; }

		IEnumerable<IReadOnlyProcessFieldChangeRuleField> Fields { get; }
	}
}
