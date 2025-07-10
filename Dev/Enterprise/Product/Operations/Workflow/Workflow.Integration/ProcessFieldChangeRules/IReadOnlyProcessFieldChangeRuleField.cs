using CargoWise.Types;

namespace Enterprise.Workflow.Integration
{
	public interface IReadOnlyProcessFieldChangeRuleField
	{
		ZString FieldName { get; }
		ZString FieldDisplayName { get; }
		ZString TableCode { get; }
		ZGuid PFR { get; }
		bool IsBlacklisted { get; }
	}
}
