using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Workflow.Integration
{
	public interface IProcessFieldChangeRule : IAuditDetails
	{
		ZGuid PK { get; }
		ZString PFR_Description { get; set; }
		ZString PFR_GroupName { get; set; }
		ZBool PFR_IsActive { get; set; }
		ZString PFR_Reference { get; set; }
		ZString PFR_SE_NKEvent { get; set; }
		ZString PFR_ProcessType { get; set; }
		IActiveBusinessObjectCollection<IProcessFieldChangeRuleField> Fields { get; }
	}
}
