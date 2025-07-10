using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class HrlEntitlementCollection : ActiveBusinessObjectCollection<HrlEntitlement>
	{
		public HrlEntitlementCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
