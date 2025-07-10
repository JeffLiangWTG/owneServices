using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class IE013RootProviderTest : DataProviderTestCase<IE013RootProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null MessageSendingObject", "Value cannot be null.\r\nParameter name: messageSendingObject", () => new IE013RootProvider(null));

			var arrivalNctsHeader = Factory.New<NctsHeader>();
			arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMessageSendingObject = new MessageSendingObject(arrivalNctsHeader);
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader", "Value cannot be null.\r\nParameter name: messageSendingObject.NctsHeader.MovementHeader", () => new IE013RootProvider(arrivalMessageSendingObject));

			AssertNoExceptionThrown("Valid Constructor args", () => new IE013RootProvider(messageSendingObject));
		});
	}

	public void TestCountrySpecificDataPL() => AssertNotNull(Provider.CountrySpecificDataPL);

	public void TestCC013C() => AssertNotNull(Provider.CC013C);

	protected override IE013RootProvider GetProvider() => new IE013RootProvider(messageSendingObject);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		messageSendingObject = new MessageSendingObject(nctsHeader);
	}

	NctsHeader nctsHeader;
	MessageSendingObject messageSendingObject;
}
