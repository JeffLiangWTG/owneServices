using CargoWise.Types;

namespace Enterprise.Workflow.Integration
{
	public interface IProcessFieldChangeRuleField
	{
		ZGuid PK { get; }
		ZString PFL_FieldName { get; set; }
		ZString FieldDisplayName { get; }
		ZString PFL_TableCode { get; }
		ZGuid PFL_PFR { get; set; }
		bool IsBlacklisted { get; }
	}
}
