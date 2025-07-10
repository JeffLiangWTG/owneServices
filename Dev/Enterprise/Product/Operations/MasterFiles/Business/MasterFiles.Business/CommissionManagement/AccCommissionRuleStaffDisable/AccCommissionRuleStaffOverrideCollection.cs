using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccCommissionRuleStaffOverrideCollection : ActiveBusinessObjectCollection<AccCommissionRuleStaffOverride>
	{
		public AccCommissionRuleStaffOverrideCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
