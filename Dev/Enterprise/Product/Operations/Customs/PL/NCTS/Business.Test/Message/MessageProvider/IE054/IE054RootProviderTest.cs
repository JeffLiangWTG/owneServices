using System;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class IE054RootProviderTest : Customs.Business.Testing.DataProviderTestCase<IE054RootProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader",
				"Value cannot be null.\r\nParameter name: messageSendingObject", () => new IE054RootProvider(null));
			var testNctsHeader = Factory.New<NctsHeader>();
			testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var testMessageSendingObject = new MessageSendingObject(testNctsHeader)
			{
				MessageType = DepartureMessageSendingObjectTypeList.Codes.RRL
			};

			AssertNoExceptionThrown(() => new IE054RootProvider(testMessageSendingObject));

			testNctsHeader = Factory.New<NctsHeader>();
			testNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			testMessageSendingObject = new MessageSendingObject(testNctsHeader)
			{
				MessageType = DepartureMessageSendingObjectTypeList.Codes.RRL
			};
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader.NctsHeader.MovementHeader",
				"Value cannot be null.\r\nParameter name: messageSendingObject.NctsHeader.MovementHeader", () => new IE054RootProvider(testMessageSendingObject));
		});
	}

	public void TestCC054C() => AssertNotNull(Provider.CC054C);

	public void TestCountrySpecificDataPL() { AssertNotNull(Provider.CountrySpecificDataPL); }

	protected override IE054RootProvider GetProvider() => new IE054RootProvider(messageSendingObject);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		messageSendingObject = new MessageSendingObject(nctsHeader)
		{
			MessageType = DepartureMessageSendingObjectTypeList.Codes.RRL
		};
	}

	NctsHeader nctsHeader;
	MessageSendingObject messageSendingObject;
}
