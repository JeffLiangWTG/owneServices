using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC054CProviderTest : Customs.Business.Testing.DataProviderTestCase<CC054CProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader",
				"Value cannot be null.\r\nParameter name: sendingObject?.NctsHeader?.MovementHeader", () => new CC054CProvider(null, null));
			var testNctsHeader = Factory.New<NctsHeader>();
			testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var testMessageSendingObject = new MessageSendingObject(testNctsHeader)
			{
				MessageType = DepartureMessageSendingObjectTypeList.Codes.RRL
			};
			AssertExceptionThrown<ArgumentNullException>("Null Message Type",
				"Value cannot be null.\r\nParameter name: messageType",
				() => new CC054CProvider(testMessageSendingObject, null));

			AssertNoExceptionThrown(() => new CC054CProvider(testMessageSendingObject, string.Empty));

			testNctsHeader = Factory.New<NctsHeader>();
			testNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			testMessageSendingObject = new MessageSendingObject(testNctsHeader)
			{
				MessageType = DepartureMessageSendingObjectTypeList.Codes.RRL
			};
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader.NctsHeader.MovementHeader",
				"Value cannot be null.\r\nParameter name: sendingObject?.NctsHeader?.MovementHeader", () => new CC054CProvider(testMessageSendingObject, string.Empty));
		});
	}

	public void TestTransitOperation() => AssertNotNull(Provider.TransitOperation);

	public void TestCustomsOfficeOfDeparture()
	{
		CombineAssertions(() =>
		{
			AssertNull("No NCTSOfficeOfDeparture", GetProvider().CustomsOfficeOfDeparture);

			AddCustomsOfficeOfType(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			AssertNotNull("NCTSOfficeOfDeparture", GetProvider().CustomsOfficeOfDeparture);
		});
	}

	public void TestHolderOfTheTransitProcedure() => AssertNotNull(Provider.HolderOfTheTransitProcedure);

	public void TestPhaseID() { AssertNull(Provider.PhaseID); }

	void AddCustomsOfficeOfType(ZString officeCode) => nctsHeader.MovementHeader.CustomsOffices.AddNew(officeCode);

	protected override CC054CProvider GetProvider() => new CC054CProvider(messageSendingObject, "Type");

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		messageSendingObject = new MessageSendingObject(nctsHeader)
		{
			MessageType = DepartureMessageSendingObjectTypeList.Codes.RRL
		};
		nctsHeader.MovementHeader.CustomsOffices.RemoveAll();
	}

	NctsHeader nctsHeader;
	MessageSendingObject messageSendingObject;
}
