using System;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class IE014RootProviderTest : Customs.Business.Testing.DataProviderTestCase<IE014RootProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null MessageSendingObject", "Value cannot be null.\r\nParameter name: messageSendingObject", () => new IE014RootProvider(null));

			var arrivalNctsHeader = Factory.New<NctsHeader>();
			arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMessageSendingObject = new MessageSendingObject(arrivalNctsHeader);
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader", "Value cannot be null.\r\nParameter name: messageSendingObject.NctsHeader.MovementHeader", () => new IE014RootProvider(arrivalMessageSendingObject));

			AssertNoExceptionThrown("Valid constructor args", () => new IE014RootProvider(messageSendingObject));
		});
	}

	public void TestCC014C() => AssertNotNull(Provider.CC014C);

	public void TestCountrySpecificDataPL() => AssertNotNull(Provider.CountrySpecificDataPL);

	protected override IE014RootProvider GetProvider() => new IE014RootProvider(messageSendingObject);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.MovementHeader.CustomsOffices.RemoveAll();

		messageSendingObject = new MessageSendingObject(nctsHeader);
	}

	NctsHeader nctsHeader;
	MessageSendingObject messageSendingObject;
}
