using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC007ConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<CC007ConsignmentProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsArrivalMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new CC007ConsignmentProvider(null));
			AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", "Value cannot be null.\r\nParameter name: movementHeader.Header", () => new CC007ConsignmentProvider(Factory.New<NctsArrivalMovementHeader>()));
			AssertNoExceptionThrown("NctsArrivalMovementHeader is not null", () => new CC007ConsignmentProvider(movementHeader));
		});
	}

	public void TestLocationOfGoods()
	{
		movementHeader.GoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Arrival;
		AssertNotNull(GetProvider().LocationOfGoods);

		movementHeader.GoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
		AssertNull(GetProvider().LocationOfGoods);
	}

	public void TestIncidents()
	{
		nctsHeader.BH_ExportFlag = EventFlagList.Codes.Yes;
		nctsHeader.EnRouteIncidents.AddNew();
		nctsHeader.EnRouteIncidents.AddNew();

		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			AssertEquals("In Phase5 Transition Period, Should have 2 Incident", 2, Provider.Incidents.Count);
			AssertEquals("Sequence number should start from 1", "1", Provider.Incidents.First().SequenceNumber);
			AssertEquals("Sequence number for 2nd Incident should be 2", "2", Provider.Incidents.Last().SequenceNumber);

			nctsHeader.BH_ExportFlag = EventFlagList.Codes.No;
			AssertEquals("Incident flag is disabled, Should have no incidents", 0, GetProvider().Incidents.Count);
		});

		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			AssertEquals("Outside Phase5 Transition Period, Should Have no incidents", 0, GetProvider().Incidents.Count);
		});
	}

	protected override CC007ConsignmentProvider GetProvider()
	{
		return new CC007ConsignmentProvider(movementHeader);
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
