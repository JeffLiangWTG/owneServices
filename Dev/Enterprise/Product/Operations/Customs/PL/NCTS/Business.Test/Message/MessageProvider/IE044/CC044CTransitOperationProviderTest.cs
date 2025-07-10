using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC044CTransitOperationProviderTest : DataProviderTestCase<CC044CTransitOperationProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsArrivalMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new CC044CTransitOperationProvider(null));
			var movementHeaderWithoutNctsHeader = Factory.New<NctsArrivalMovementHeader>();
			AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", "Value cannot be null.\r\nParameter name: movementHeader.Header", () => new CC044CTransitOperationProvider(movementHeaderWithoutNctsHeader));
			AssertNoExceptionThrown("NctsArrivalMovementHeader is not null", () => new CC044CTransitOperationProvider(movementHeader));
		});
	}

	public void TestMRN()
	{
		CombineAssertions(() =>
		{
			nctsHeader.ArrivalMrnFromUser = string.Empty;
			AssertEquals("MRN is empty", string.Empty, GetProvider().MRN);

			nctsHeader.ArrivalMrnFromUser = "123";
			AssertEquals("MRN equals 123", "123", GetProvider().MRN);
		});
	}

	public void TestOtherThingsToReport()
	{
		CombineAssertions(() =>
		{
			movementHeader.OtherThingsToReport = string.Empty;
			AssertEquals("OtherThingsToReport is empty", string.Empty, GetProvider().OtherThingsToReport);

			movementHeader.OtherThingsToReport = "ABC";
			AssertEquals("OtherThingsToReport equals ABC", "ABC", GetProvider().OtherThingsToReport);
		});
	}

	protected override CC044CTransitOperationProvider GetProvider()
	{
		return new CC044CTransitOperationProvider(movementHeader);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		movementHeader = nctsHeader.ArrivalMovementHeader;
	}

	NctsHeader nctsHeader;
	NctsArrivalMovementHeader movementHeader;
}
