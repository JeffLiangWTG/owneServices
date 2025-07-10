using System;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC014CTransitOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<CC014CTransitOperationProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null MessageSendingObject", "Value cannot be null.\r\nParameter name: messageSendingObject", () => new CC014CTransitOperationProvider(null));
			AssertNoExceptionThrown("Valid constructor args", () => new CC014CTransitOperationProvider(messageSendingObject));
		});
	}

	public void TestMRN()
	{
		CombineAssertions(() =>
		{
			const string testNumber = "1234";
			messageSendingObject.MovementReferenceNumber = testNumber;
			AssertEquals("When MRN is present - MRN must not be bull", testNumber, GetProvider().MRN);
			AssertNull("When MRN is present - LRN must be bull", Provider.LRN);
		});
	}

	public void TestLRN()
	{
		CombineAssertions(() =>
		{
			const string testNumber = "1234";
			messageSendingObject.MovementReferenceNumber = testNumber;
			AssertNull("When MRN is present - LRN must be bull", Provider.LRN);

			messageSendingObject.MovementReferenceNumber = null;
			AssertEquals("When MRN is not present - LRN must not be bull", EDIMessage.PL_NCTS_LRN_PlaceHolder, GetProvider().LRN);
			AssertNullOrEmpty("When MRN is not present - MRN must be bull", Provider.MRN);
		});
	}

	protected override CC014CTransitOperationProvider GetProvider()
	{
		return new CC014CTransitOperationProvider(messageSendingObject);
	}

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
