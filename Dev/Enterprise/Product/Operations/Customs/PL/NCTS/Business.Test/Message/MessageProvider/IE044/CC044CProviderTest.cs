using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC044CProviderTest : DataProviderTestCase<CC044CProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsArrivalMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new CC044CProvider(null, null));
			AssertExceptionThrown<ArgumentNullException>("Null Message Type", "Value cannot be null.\r\nParameter name: messageType", () => new CC044CProvider(movementHeader, null));
			AssertNoExceptionThrown("MovementHeader and MessageType is not null", () => new CC044CProvider(movementHeader, string.Empty));
		});
	}

	public void TestTransitOperation()
	{
		AssertNotNull(Provider.TransitOperation);
	}

	public void TestCustomsOfficeOfDestinationActual()
	{
		CombineAssertions(() =>
		{
			var office = nctsHeader.ArrivalMovementHeader.CustomsOffices.AddNew();
			office.CY_Code = EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			AssertNull("DSA office does not exist", Provider.CustomsOfficeOfDestinationActual);

			office = nctsHeader.ArrivalMovementHeader.CustomsOffices.AddNew();
			office.CY_Code = EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
			AssertNotNull("DSA office exists", GetProvider().CustomsOfficeOfDestinationActual);

			nctsHeader.ArrivalMovementHeader.CustomsOffices.RemoveAll();
			AssertNull("Offices is empty", GetProvider().CustomsOfficeOfDestinationActual);
		});
	}

	public void TestTraderAtDestination()
	{
		CombineAssertions(() =>
		{
			AssertNull("Trader is empty", Provider.TraderAtDestination);

			var destinationTrader = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.DestinationTrader.E2_OA_Address = destinationTrader.MainAddress.PK;
			AssertType<TraderAtDestinationProvider>("Trader is not empty", GetProvider().TraderAtDestination);
		});
	}

	public void TestUnloadingRemark()
	{
		AssertNotNull(Provider.UnloadingRemark);
	}

	public void TestConsignment()
	{
		AssertNotNull(Provider.Consignment);
	}

	public void TestPhaseID()
	{
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			AssertEquals("PhaseID outside Transition Period", CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.PhaseID.NCTS_5_1, GetProvider().PhaseID);
		});

		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			AssertEquals("PhaseID in Transition Period", CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.PhaseID.NCTS_5_0, GetProvider().PhaseID);
		});
	}

	protected override CC044CProvider GetProvider()
	{
		return new CC044CProvider(movementHeader, string.Empty);
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
