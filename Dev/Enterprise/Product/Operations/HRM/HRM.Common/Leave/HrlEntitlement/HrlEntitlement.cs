using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class HrlEntitlement : AutoHrlEntitlement
	{
		public HrlEntitlement(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			LPE_Entitlement = 1;
			LPE_EntitlementTimeUnit = "D";
			LPE_MaximumEntitlement = 1;
			LPE_MaximumEntitlementTimeUnit = "D";
		}
#endif
	}
}
