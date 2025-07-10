using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC170CHouseConsignmentProviderTest : ConsignmentProviderBaseTest<CC170CHouseConsignmentProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsBill", "Value cannot be null.\r\nParameter name: transportMeansProvider", () => new CC170CHouseConsignmentProvider(1, null, null));
			AssertExceptionThrown<ArgumentNullException>("Null ICC170C", "Value cannot be null.\r\nParameter name: cc170cProvider", () => new CC170CHouseConsignmentProvider(1, nctsBill, null));
		});
	}

	public void TestSequenceNumber() => AssertEquals("22", GetProvider().SequenceNumber);

	protected override CC170CHouseConsignmentProvider GetProvider() => new CC170CHouseConsignmentProvider(22, nctsBill, GetCC170CProvider());

	ICC170C GetCC170CProvider() => new CC170CProvider(movementHeader, "CC170C");

	protected override bool IsTransportTypeAtDepartureRequired => true;

	protected override void SetUp()
	{
		base.SetUp();

		nctsBill.B0_ReferenceID = "ReferenceId";
		nctsBill.B0_Weight = 123040m;
		nctsBill.B0_WeightUQ = Core.Constants.Weight.Grams;
	}
}
