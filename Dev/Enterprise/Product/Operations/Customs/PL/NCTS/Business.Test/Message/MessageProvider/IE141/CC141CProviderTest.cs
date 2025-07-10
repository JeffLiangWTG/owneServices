using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC141CProviderTest : Customs.Business.Testing.DataProviderTestCase<CC141CProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new CC141CProvider(null, null, null));
			AssertExceptionThrown<ArgumentNullException>("Null Message Type", "Value cannot be null.\r\nParameter name: messageType", () => new CC141CProvider(movementHeader, null, null));
			AssertExceptionThrown<ArgumentNullException>("Null MessageSendingObject", "Value cannot be null.\r\nParameter name: messageSendingObject", () => new CC141CProvider(movementHeader, string.Empty, null));
			AssertNoExceptionThrown("Valid constructor args", () => new CC141CProvider(movementHeader, string.Empty, messageSendingObject));
		});
	}

	public void TestTransitOperationMRN()
	{
		CombineAssertions(() =>
		{
			AssertNullOrEmpty("MRN not set", Provider.TransitOperationMRN);

			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "123";
			AssertEquals("MRN set", "123", GetProvider().TransitOperationMRN);
		});
	}

	public void TestCustomsOfficeOfDestinationReferenceNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("ActualOfficeOfDestination is empty", string.Empty, GetProvider().CustomsOfficeOfDestinationReferenceNumber);

			messageSendingObject.ActualOfficeOfDestination = "abc123";
			AssertEquals("ActualOfficeOfDestination is not empty, but Enquiry text is empty", string.Empty, GetProvider().CustomsOfficeOfDestinationReferenceNumber);

			messageSendingObject.AdditionalText = "some Enquiry text";
			AssertEquals("Enquiry text is not empty", "abc123", GetProvider().CustomsOfficeOfDestinationReferenceNumber);
		});
	}

	public void TestCustomsOfficeOfEnquiryReferenceNumber()
	{
		var departureOffice = (NctsPLOfficeCode)movementHeader.DepartureCustomsOffice;
		departureOffice.CY_Data = "PL12345";
		AssertEquals("CustomsOfficeOfEnquiryReferenceNumber", "PL12345", GetProvider().CustomsOfficeOfEnquiryReferenceNumber);
	}

	public void TestHolderOfTheTransitProcedure() => AssertNotNull(Provider.HolderOfTheTransitProcedure);

	public void TestEnquiry() => AssertNotNull(Provider.Enquiry);

	public void TestConsignee()
	{
		CombineAssertions(() =>
		{
			AssertNull(Provider.Consignee);

			messageSendingObject.AdditionalText = "ad";
			AssertNotNull(GetProvider().Consignee);
		});
	}

	public void TestPhaseID()
	{
		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			AssertEquals("PhaseID in Phase5 Transition period", PhaseID.NCTS_5_0, GetProvider().PhaseID);
		});
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			AssertEquals("PhaseID outside Phase5 Transition period", PhaseID.NCTS_5_1, GetProvider().PhaseID);
		});
	}

	protected override CC141CProvider GetProvider()
	{
		return new CC141CProvider(movementHeader, string.Empty, messageSendingObject);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
		messageSendingObject = new MessageSendingObject(nctsHeader);
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;
	MessageSendingObject messageSendingObject;
}
