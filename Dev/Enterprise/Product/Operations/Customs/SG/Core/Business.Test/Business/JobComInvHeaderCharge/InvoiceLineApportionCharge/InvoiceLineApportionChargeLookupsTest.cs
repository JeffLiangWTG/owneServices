using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class InvoiceLineApportionChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeTypeList()
		{
			Assert(Lookups.ChargeTypeList is ChargeTypeList);
		}

		InvoiceLineApportionChargeLookups Lookups
		{
			get
			{
				return InvoiceLineApportionCharge.Lookups;
			}
		}

		#region InvoiceLineApportionCharge
		InvoiceLineApportionCharge InvoiceLineApportionCharge
		{
			get
			{
				return invoiceLineApportionCharge ?? (invoiceLineApportionCharge = Factory.New<InvoiceLineApportionCharge>());
			}
		}

		InvoiceLineApportionCharge invoiceLineApportionCharge;
		#endregion
	}
}
