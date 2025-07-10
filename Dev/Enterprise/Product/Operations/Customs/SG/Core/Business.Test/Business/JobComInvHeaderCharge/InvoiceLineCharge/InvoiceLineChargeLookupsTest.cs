using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class InvoiceLineChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeTypeList()
		{
			AssertEquals(1, Lookups.ChargeTypeList.Count);
			Assert(Lookups.ChargeTypeList.ContainsCode(InvoiceLineCharge.ChargeTypes.OptionalItemCharges));
		}

		InvoiceLineChargeLookups Lookups
		{
			get
			{
				return InvoiceLineCharge.Lookups;
			}
		}

		#region InvoiceLineCharge
		InvoiceLineCharge InvoiceLineCharge
		{
			get
			{
				return invoiceLineCharge ?? (invoiceLineCharge = Factory.New<InvoiceLineCharge>());
			}
		}

		InvoiceLineCharge invoiceLineCharge;
		#endregion
	}
}
