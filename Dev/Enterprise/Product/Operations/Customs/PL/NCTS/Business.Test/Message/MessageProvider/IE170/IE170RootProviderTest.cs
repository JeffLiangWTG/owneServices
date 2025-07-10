using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class IE170RootProviderTest : DataProviderTestCase<IE170RootProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null MessageSendingObject", "Value cannot be null.\r\nParameter name: messageSendingObject", () => new IE170RootProvider(null));

			var arrivalNctsHeader = Factory.New<NctsHeader>();
			arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMessageSendingObject = new MessageSendingObject(arrivalNctsHeader);
			AssertExceptionThrown<ArgumentNullException>("Null MessageSendingObject", "Value cannot be null.\r\nParameter name: messageSendingObject.NctsHeader.MovementHeader", () => new IE170RootProvider(arrivalMessageSendingObject));

			AssertNoExceptionThrown("Valid Constructor args", () => new IE170RootProvider(messageSendingObject));
		});
	}

	public void TestCountrySpecificDataPL() => AssertNotNull(Provider.CountrySpecificDataPL);

	public void TestCC170C() => AssertNotNull(Provider.CC170C);

	protected override IE170RootProvider GetProvider() => new IE170RootProvider(messageSendingObject);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		messageSendingObject = new MessageSendingObjectParent(nctsHeader).SelectedSendingObjects.First() as MessageSendingObject;
	}

	NctsHeader nctsHeader;
	MessageSendingObject messageSendingObject;
}
