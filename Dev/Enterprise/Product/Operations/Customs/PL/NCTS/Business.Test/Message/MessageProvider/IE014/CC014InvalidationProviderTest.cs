using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC014InvalidationProviderTest : Customs.Business.Testing.DataProviderTestCase<CC014InvalidationProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new CC014InvalidationProvider(null, null));
		AssertExceptionThrown<ArgumentNullException>("Null MessageSendingObject", "Value cannot be null.\r\nParameter name: messageSendingObject", () => new CC014InvalidationProvider(movementHeader, null));
		AssertNoExceptionThrown(() => new CC014InvalidationProvider(movementHeader, messageSendingObject));
	}

	public void TestRequestDateAndTime() => AssertNotNull(Provider.RequestDateAndTime);

	public void TestDecisionDateAndTime() => AssertNull(Provider.DecisionDateAndTime);

	public void TestDecision() => AssertNull(Provider.Decision);

	public void TestInitiatedByCustom() => AssertEquals("InitiatedByCistom should be 0.", Provider.InitiatedByCustom, NCTSIndicator.NO);

	public void TestJustification() => AssertNotEquals("Justification should not be null.", Provider.Justification, null);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
		nctsHeader.MovementHeader.CustomsOffices.RemoveAll();

		messageSendingObject = new MessageSendingObject(nctsHeader);
	}

	protected override CC014InvalidationProvider GetProvider() => new CC014InvalidationProvider(movementHeader, messageSendingObject);

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;
	MessageSendingObject messageSendingObject;
}
