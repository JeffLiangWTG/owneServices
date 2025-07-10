using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class InvoiceApportionChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			InvoiceApportionCharge parent = Factory.New<InvoiceApportionCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}

		public void TestChargeTypeList()
		{
			Assert(Lookups.ChargeTypeList is ChargeTypeList);
		}

		InvoiceApportionChargeLookups Lookups
		{
			get
			{
				return InvoiceApportionCharge.Lookups;
			}
		}

		#region InvoiceApportionCharge
		InvoiceApportionCharge InvoiceApportionCharge
		{
			get
			{
				return invoiceApportionCharge ?? (invoiceApportionCharge = Factory.New<InvoiceApportionCharge>());
			}
		}

		InvoiceApportionCharge invoiceApportionCharge;
		#endregion
	}
}
