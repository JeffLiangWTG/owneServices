using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class GroupInvoiceChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			GroupInvoiceCharge parent = Factory.New<GroupInvoiceCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}

		public void TestChargeTypeList()
		{
			Assert(Lookups.ChargeTypeList is ChargeTypeList);
		}

		GroupInvoiceChargeLookups Lookups
		{
			get
			{
				return GroupInvoiceCharge.Lookups;
			}
		}

		#region GroupInvoiceCharge
		GroupInvoiceCharge GroupInvoiceCharge
		{
			get
			{
				return groupInvoiceCharge ?? (groupInvoiceCharge = Factory.New<GroupInvoiceCharge>());
			}
		}

		GroupInvoiceCharge groupInvoiceCharge;
		#endregion
	}
}
