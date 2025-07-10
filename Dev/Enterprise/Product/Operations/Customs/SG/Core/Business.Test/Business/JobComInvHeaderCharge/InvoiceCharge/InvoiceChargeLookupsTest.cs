using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class InvoiceChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			InvoiceCharge parent = Factory.New<InvoiceCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}

		public void TestChargeTypeList()
		{
			Assert(Lookups.ChargeTypeList is ChargeTypeList);
		}

		InvoiceChargeLookups Lookups
		{
			get
			{
				return InvoiceCharge.Lookups;
			}
		}

		#region InvoiceCharge
		InvoiceCharge InvoiceCharge
		{
			get
			{
				return invoiceCharge ?? (invoiceCharge = Factory.New<InvoiceCharge>());
			}
		}

		InvoiceCharge invoiceCharge;
		#endregion
	}
}
