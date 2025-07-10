using System;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class IE015RootProviderTest : Customs.Business.Testing.DataProviderTestCase<IE015RootProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null MessageSendingObject", "Value cannot be null.\r\nParameter name: messageSendingObject", () => new IE015RootProvider(null));

			var arrivalNctsHeader = Factory.New<NctsHeader>();
			arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMessageSendingObject = new MessageSendingObject(arrivalNctsHeader);
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader", "Value cannot be null.\r\nParameter name: messageSendingObject.NctsHeader.MovementHeader", () => new IE015RootProvider(arrivalMessageSendingObject));

			AssertNoExceptionThrown("Valid constructor", () => new IE015RootProvider(messageSendingObject));
		});
	}

	public void TestCC015C() => AssertNotNull(Provider.CC015C);

	public void TestCountrySpecificDataPL() { AssertNotNull(Provider.CountrySpecificDataPL); }

	protected override IE015RootProvider GetProvider() => new IE015RootProvider(messageSendingObject);

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
