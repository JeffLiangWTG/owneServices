using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class TransportChargesProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportChargesProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null bill", "Value cannot be null.\r\nParameter name: bill", () => new TransportChargesProvider(null));
	}

	public void TestMethodOfPayment() => AssertEquals("ABC", Provider.MethodOfPayment);

	protected override TransportChargesProvider GetProvider()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var bill = header.Bills.AddNew();

		bill.B0_TransportPaymentMethod = "ABC";
		return new TransportChargesProvider(bill);
	}
}
