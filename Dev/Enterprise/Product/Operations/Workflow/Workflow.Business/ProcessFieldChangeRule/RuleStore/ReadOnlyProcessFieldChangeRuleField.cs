using CargoWise.Types;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	class ReadOnlyProcessFieldChangeRuleField : IReadOnlyProcessFieldChangeRuleField
	{
		public ReadOnlyProcessFieldChangeRuleField(ProcessFieldChangeRuleField pfrRuleField)
		{
			FieldName = pfrRuleField.PFL_FieldName;

			PFR = pfrRuleField.PFL_PFR;

			FieldDisplayName = pfrRuleField.FieldDisplayName;

			TableCode = pfrRuleField.PFL_TableCode;

			IsBlacklisted = pfrRuleField.IsBlacklisted;
		}
		public ZString FieldName { get; }

		public ZGuid PFR { get; }

		public ZString FieldDisplayName { get; }

		public ZString TableCode { get; }

		public bool IsBlacklisted { get; }
	}
}
