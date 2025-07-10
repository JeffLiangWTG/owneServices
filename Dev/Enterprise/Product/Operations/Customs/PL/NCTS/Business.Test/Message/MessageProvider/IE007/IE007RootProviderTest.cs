using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class IE007RootProviderTest : DataProviderTestCase<IE007RootProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null MessageSendingObject", "Value cannot be null.\r\nParameter name: messageSendingObject", () => new IE007RootProvider(null));

			var departureNctsHeader = Factory.New<NctsHeader>();
			departureNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var departureMessageSendingObject = new MessageSendingObject(departureNctsHeader);
			AssertExceptionThrown<ArgumentNullException>("Null NctsArrivalMovementHeader", "Value cannot be null.\r\nParameter name: messageSendingObject.NctsHeader.ArrivalMovementHeader", () => new IE007RootProvider(departureMessageSendingObject));

			AssertNoExceptionThrown(() => new IE007RootProvider(messageSendingObject));
		});
	}

	public void TestCountrySpecificDataPL() => AssertNotNull(Provider.CountrySpecificDataPL);

	public void TestCC007C() => AssertNotNull(Provider.CC007C);

	protected override IE007RootProvider GetProvider() => new IE007RootProvider(messageSendingObject);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		messageSendingObject = new MessageSendingObjectParent(nctsHeader).SelectedSendingObjects.First() as MessageSendingObject;
	}

	NctsHeader nctsHeader;
	MessageSendingObject messageSendingObject;
}
