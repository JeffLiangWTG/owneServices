using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	[TestedType(typeof(OrderClientAssignedStaffModuleFilter))]
	public class OrderClientAssignedStaffModuleFilterTest : OrgClientAssignedStaffModuleFilterTest
	{
		protected override string GetExpectedClientTypeList()
		{
			return "BUY - Buyer\r\n" +
				"SUP - Supplier\r\n" +
				"CPY - Controlling Customer";
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrderClientAssignedStaffModuleFilter("Test", null);
		}
	}
}
