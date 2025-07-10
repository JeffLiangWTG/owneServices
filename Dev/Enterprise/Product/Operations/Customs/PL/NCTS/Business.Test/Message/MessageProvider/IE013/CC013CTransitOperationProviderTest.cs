using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC013CTransitOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<CC013CTransitOperationProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new CC013CTransitOperationProvider(null, null));
			AssertExceptionThrown<ArgumentNullException>("Null MessageSendingObject", "Value cannot be null.\r\nParameter name: messageSendingObject", () => new CC013CTransitOperationProvider(movementHeader, null));
			AssertNoExceptionThrown("Valid constructor args", () => new CC013CTransitOperationProvider(movementHeader, messageSendingObject));
		});
	}

	public void TestMRN()
	{
		CombineAssertions(() =>
		{
			const string testNumber = "1234";
			messageSendingObject.MovementReferenceNumber = testNumber;
			AssertEquals("When MRN is Present - MRN must not be null", testNumber, GetProvider().MRN);
			AssertNull("When MRN is Present - LRN must be null", Provider.LRN);
		});
	}

	public void TestAmendmentTypeFlag()
	{
		CombineAssertions(() =>
		{
			messageSendingObject.AmendmentType = AmendmentTypeList.Codes._0DeclarationAmendment;
			AssertEquals("Amendment type is set to 0", NCTSIndicator.NO, Provider.AmendmentTypeFlag);

			messageSendingObject.AmendmentType = AmendmentTypeList.Codes._1GuaranteeAmendment;
			AssertEquals("Amendment type is set to 1", NCTSIndicator.YES, Provider.AmendmentTypeFlag);
		});
	}

	public void TestLRN()
	{
		CombineAssertions(() =>
		{
			const string testNumber = "1234";
			messageSendingObject.MovementReferenceNumber = testNumber;
			AssertNull("When MRN is Present - LRN must be null", Provider.LRN);

			messageSendingObject.MovementReferenceNumber = null;
			AssertEquals("When MRN is null - LRN must not be null", EDIMessage.PL_NCTS_LRN_PlaceHolder, GetProvider().LRN);
			AssertNullOrEmpty("When MRN is null - MRN must be null", Provider.MRN);
		});
	}

	public void TestDeclarationType() => CombineAssertions(() =>
	{
		AssertEquals("Declaration type is not set", string.Empty, Provider.DeclarationType);
		movementHeader.BM_InBondEntryType = "ABC";
		AssertEquals("Declaration type is set", "ABC", GetProvider().DeclarationType);
	});

	public void TestAdditionalDeclarationType() => CombineAssertions(() =>
	{
		AssertEquals("Additional declaration type is not set", string.Empty, Provider.AdditionalDeclarationType);
		movementHeader.BM_AdditionalDeclarationType = "A";
		AssertEquals("Additional declaration type is set", "A", GetProvider().AdditionalDeclarationType);
	});

	public void TestTIRCarnetNumber() => AssertNull(Provider.TIRCarnetNumber);

	[TestDate(2023, 6, 28)]
	public void TestPresentationOfTheGoodsDateAndTime()
	{
		CombineAssertions(() =>
		{
			AssertNull("Presentation Date and Time not set", Provider.PresentationOfTheGoodsDateAndTime);

			movementHeader.BM_PresentationDateTime = ZDateTime.Today.ToOffset();
			AssertEquals("Presentation Date and Time is set", new DateTime(2023, 6, 28), Provider.PresentationOfTheGoodsDateAndTime);
		});
	}

	public void TestSecurity() => AssertNull(Provider.Security);

	public void TestReducedDatasetIndicator() => AssertNotNull(Provider.ReducedDatasetIndicator);

	public void TestSpecificCircumstanceIndicator()
	{
		CombineAssertions(() =>
		{
			AssertEquals(GetMessage(), string.Empty, GetProvider().SpecificCircumstanceIndicator);

			movementHeader.BM_SpecificCircumstance = "ABC";
			AssertEquals(GetMessage(), "ABC", GetProvider().SpecificCircumstanceIndicator);
		});

		string GetMessage() => $"{nameof(movementHeader.BM_SpecificCircumstance)} = {movementHeader.BM_SpecificCircumstance}";
	}

	public void TestCommunicationLanguageAtDeparture() => AssertNull(Provider.CommunicationLanguageAtDeparture);

	public void TestBindingItinerary() => AssertEquals("BindingItinerary should always be 0", NCTSIndicator.NO, Provider.BindingItinerary);

	public void TestLimitDate()
	{
		CombineAssertions(() =>
		{
			movementHeader.IsSimplifiedNctsProcedure = false;
			movementHeader.BM_ExportDate = ZDateTime.Empty;
			AssertNull("Should be null when BM_ExportDate is empty", GetProvider().LimitDate);

			var testDateTime = DateTime.Now.AddDays(3);
			movementHeader.BM_ExportDate = testDateTime;
			AssertNull("Should be null when is not simplified procedure", GetProvider().LimitDate);

			movementHeader.IsSimplifiedNctsProcedure = true;
			AssertEquals("The value should be equal to BM_ExportDate and is simplified procedure", testDateTime, GetProvider().LimitDate);
		});
	}

	protected override CC013CTransitOperationProvider GetProvider()
	{
		return new CC013CTransitOperationProvider(movementHeader, messageSendingObject);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
		messageSendingObject = new MessageSendingObject(nctsHeader);
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;
	MessageSendingObject messageSendingObject;
}
