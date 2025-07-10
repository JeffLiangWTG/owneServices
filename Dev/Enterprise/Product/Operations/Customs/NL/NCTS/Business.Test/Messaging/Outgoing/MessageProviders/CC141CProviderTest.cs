using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC141CProvider))]
sealed class CC141CProviderTest : MessageHeaderProviderAbstractTest<CC141CProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC141CProvider(null));

	public void TestMRN()
	{
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN123";
		AssertEquals("MRN123", Provider.MRN);
	}

	public void TestCustomsOfficeOfDestination()
	{
		CombineAssertions(() =>
		{
			action.ActualOfficeOfDestination = "NL000001";
			AssertEquals("NL000001", provider.CustomsOfficeOfDestination);
			action.ActualOfficeOfDestination = string.Empty;
			AssertNullOrEmpty(provider.CustomsOfficeOfDestination);
		});
	}

	public void TestCustomsOfficeOfEnquiryAtDeparture()
	{
		CombineAssertions(() =>
		{
			AssertNullOrEmpty(provider.CustomsOfficeOfEnquiryAtDeparture);

			var enquiryCustomsOffice = nctsHeader.MovementHeader.CustomsOffices.AddNew();
			enquiryCustomsOffice.CY_Code = EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry;
			enquiryCustomsOffice.CY_Data = "NL000002";
			AssertEquals("NL000002", provider.CustomsOfficeOfEnquiryAtDeparture);
		});
	}

	public void TestHolderOfTheTransitProcedure()
	{
		CombineAssertions(() =>
		{
			AssertType<HolderOfTheTransitProcedureProvider>(Provider.HolderOfTheTransitProcedure);
			AssertNotNull(Provider.HolderOfTheTransitProcedure);
		});
	}

	public void TestEnquiryTC11DeliveryDate()
	{
		CombineAssertions(() =>
		{
			action.TCI11 = new ZDateTime(2024, 05, 10, 15, 20, 08, 123);
			AssertEquals(new DateTime(2024, 05, 10, 15, 20, 08, 123), provider.EnquiryTC11DeliveryDate);
			action.TCI11 = ZDateTime.Empty;
			AssertNull(provider.EnquiryTC11DeliveryDate);
		});
	}

	public void TestEnquiryText()
	{
		action.QueryInformation = "Enquiry Text";
		AssertEquals("Enquiry Text", Provider.EnquiryText);
	}

	public void TestConsignmentConsignee() => AssertType<PartyProvider>(Provider.ConsignmentConsignee);

	protected override void SetUp()
	{
		base.SetUp();
		CreateProvider();
	}
	protected override string MessageType => "CC141C";

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override bool HasSendingActionParameter => true;
}
