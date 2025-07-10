using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.PL.NCTS.Business.Message.MessageProviders.IE054;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

class IE054TransitOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<CC054CTransitOperationProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader",
				"Value cannot be null.\r\nParameter name: sendingObject", () => new CC054CTransitOperationProvider(null));
			var testNctsHeader = Factory.New<NctsHeader>();
			testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var testMessageSendingObject = new MessageSendingObject(testNctsHeader)
			{
				MessageType = DepartureMessageSendingObjectTypeList.Codes.RRL
			};
			AssertNoExceptionThrown(() => new CC054CTransitOperationProvider(testMessageSendingObject));
		});
	}

	[TestDate(2023, 04, 21, 14, 30, 15)]
	public void TestReleaseDateTime()
	{
		AssertEquals(new DateTime(2023, 04, 21, 14, 30, 15), Provider.ReleaseDateTime);
	}

	public void TestReleaseRequested()
	{
		CombineAssertions(() =>
		{
			messageSendingObject.ReleaseRequest = ReleaseRequestedFlagList.Codes.Yes;
			AssertEquals(ReleaseRequestedFlagList.Codes.Yes, expected: NCTSIndicator.YES, Provider.ReleaseRequested);

			messageSendingObject.ReleaseRequest = ReleaseRequestedFlagList.Codes.No;
			AssertEquals(ReleaseRequestedFlagList.Codes.No, expected: NCTSIndicator.NO, new CC054CTransitOperationProvider(messageSendingObject).ReleaseRequested);

			messageSendingObject.ReleaseRequest = "1";
			AssertEquals("1", expected: NCTSIndicator.NO, new CC054CTransitOperationProvider(messageSendingObject).ReleaseRequested);
		});
	}

	public void TestReferenceNumber()
	{
		const string testMovementReferenceEntryNumber = "MRNIERO1234";

		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = testMovementReferenceEntryNumber;
		AssertEquals(testMovementReferenceEntryNumber, Provider.ReferenceNumber);
	}

	protected override CC054CTransitOperationProvider GetProvider() => new CC054CTransitOperationProvider(messageSendingObject);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		messageSendingObject = new MessageSendingObject(nctsHeader)
		{
			MessageType = DepartureMessageSendingObjectTypeList.Codes.RRL,
			ReleaseRequest = "N"
		};
	}

	NctsHeader nctsHeader;
	MessageSendingObject messageSendingObject;
}
