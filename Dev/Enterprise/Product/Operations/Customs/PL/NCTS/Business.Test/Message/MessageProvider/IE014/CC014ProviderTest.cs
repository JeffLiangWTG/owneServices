using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC014CProviderTest : Customs.Business.Testing.DataProviderTestCase<CC014CProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new CC014CProvider(null, null, null));
		AssertExceptionThrown<ArgumentNullException>("Null Message Type", "Value cannot be null.\r\nParameter name: messageType", () => new CC014CProvider(movementHeader, null, null));
		AssertExceptionThrown<ArgumentNullException>("Null MessageSendingObject", "Value cannot be null.\r\nParameter name: messageSendingObject", () => new CC014CProvider(movementHeader, string.Empty, null));
		AssertNoExceptionThrown(() => new CC014CProvider(movementHeader, string.Empty, messageSendingObject));
	}

	public void TestTransitOperation() => AssertNotNull(Provider.TransitOperation);

	public void TestCustomsOfficeOfDeparture()
		=> CombineAssertions(() =>
		{
			AssertNull("No NCTSOfficeOfDeparture", GetProvider().CustomsOfficeOfDeparture);
			AddCustomsOfficeOfType(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			AssertNotNull("NCTSOfficeOfDeparture", GetProvider().CustomsOfficeOfDeparture);
		});

	public void TestHolderOfTheTransitProcedure() => AssertNotNull(Provider.HolderOfTheTransitProcedure);

	public void TestPhaseID() => AssertNotNull(Provider.PhaseID);

	public void TestInvalidation() => AssertNotNull(Provider.Invalidation);

	void AddCustomsOfficeOfType(ZString officeCode) => nctsHeader.MovementHeader.CustomsOffices.AddNew(officeCode);

	protected override CC014CProvider GetProvider() => new CC014CProvider(movementHeader, "Type", messageSendingObject);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
		nctsHeader.MovementHeader.CustomsOffices.RemoveAll();

		messageSendingObject = new MessageSendingObject(nctsHeader);
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;
	MessageSendingObject messageSendingObject;
}
