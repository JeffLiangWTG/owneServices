using System;
using System.Linq;
using System.Reflection;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Moq;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class IE141RootProviderTest : DataProviderTestCase<IE141RootProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null MessageSendingObject", "Value cannot be null.\r\nParameter name: messageSendingObject", () => new IE141RootProvider(null));

			var arrivalNctsHeader = Factory.New<NctsHeader>();
			arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMessageSendingObject = new MessageSendingObject(arrivalNctsHeader);
			AssertExceptionThrown<ArgumentNullException>("Null MessageSendingObject", "Value cannot be null.\r\nParameter name: messageSendingObject.NctsHeader.MovementHeader", () => new IE141RootProvider(arrivalMessageSendingObject));

			AssertNoExceptionThrown("Valid Constructor args", () => new IE141RootProvider(messageSendingObject));
		});
	}

	public void TestCountrySpecificDataPL() => AssertNotNull(Provider.CountrySpecificDataPL);

	public void TestCC141C() => AssertNotNull(Provider.CC141C);

	public void TestFillSharedFields()
	{
		var sharedFields = new NCTSPrettierSharedFields();
		var cc141Mock = new Mock<ICC141C> { CallBase = true };
		cc141Mock.Setup(x => x.TransitOperationMRN).Returns("TESTMRN");
		typeof(IE141RootProvider).GetField("cc141C", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Provider, cc141Mock.Object);
		((INCTSPrettierData)Provider).FillSharedFields(sharedFields);
		AssertContainsExactElementsInAnyOrder(
			expected: new[] {
				("Message Type", "IE141"),
				("MRN (Movement Reference Number)", "TESTMRN"),
			},
			sharedFields.ToArray());
	}

	public void TestAdditionalBlocks()
		=> AssertEquals(Array.Empty<INCTSPrettierAdditionalBlock>(), ((INCTSPrettierData)Provider).AdditionalBlocks);

	protected override IE141RootProvider GetProvider() => new IE141RootProvider(messageSendingObject);

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		messageSendingObject = new MessageSendingObject(nctsHeader);
	}
	MessageSendingObject messageSendingObject;
}
