using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	class ReadOnlyProcessFieldChangeRule : IReadOnlyProcessFieldChangeRule
	{
		public ReadOnlyProcessFieldChangeRule(ProcessFieldChangeRule pfr, IEnumerable<ProcessFieldChangeRuleField> pfrFields)
		{
			Argument.NotNull(pfr, nameof(pfr));
			Argument.NotNull(pfrFields, nameof(pfrFields));

			Description = pfr.PFR_Description;
			GroupName = pfr.PFR_GroupName;
			IsActive = pfr.PFR_IsActive;
			ProcessType = pfr.PFR_ProcessType;
			Reference = pfr.PFR_Reference;
			SE_NKEvent = pfr.PFR_SE_NKEvent;
			PK = pfr.PK;

			Fields = pfrFields
				.Select(pfrField => new ReadOnlyProcessFieldChangeRuleField(pfrField))
				.ToList();
		}

		public ZGuid PK { get; }

		public ZString SE_NKEvent { get; }

		public ZString Description { get; }

		public ZString GroupName { get; }

		public ZBool IsActive { get; }

		public ZString ProcessType { get; }

		public ZString Reference { get; }

		public IEnumerable<IReadOnlyProcessFieldChangeRuleField> Fields { get; }
	}
}
