using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core.Writing;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test;

internal class RateOneOffPotentialCarrierDataObjectWriterTest : DataObjectWriterTest
{
	public void TestPotentialCarrierDataExporting()
	{
		var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
		var code = "MAECHISMA";
		var description = "MAERSK (CHINA) SHIPPING CO. LTD";

		var org = Factory.New<OrgHeader>();
		org.OH_Code = code;
		org.OH_FullName = description;
		org.OH_IsShippingProvider = true;

		var oneOffCarrier = quotedBooking.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
		oneOffCarrier.TTC_OH_Carrier = org.PK;

		var carrierData = new RateOneOffPotentialCarrierDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CLI, oneOffCarrier))).GetDataObject(oneOffCarrier);

		CombineAssertions(() =>
		{
			AssertEquals("carrierData.Code", code, carrierData.Code);
			AssertEquals("carrierData.Name", description, carrierData.Name);
		});
	}

	public void TestPotentialCarrierWithCreditorDataExporting()
	{
		var carrierCode = "MAECHISHA";
		var carrierDescription = "MAERSK (CHINA) SHIPPING CO. LTD";
		var creditorCode = "ABC";
		var creditorDescription = "Alphabet Shipping";

		var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);

		var org = Factory.New<OrgHeader>();
		org.OH_Code = carrierCode;
		org.OH_FullName = carrierDescription;
		org.OH_IsShippingProvider = true;

		var creditor = Factory.New<OrgHeader>();
		creditor.OH_Code = creditorCode;
		creditor.OH_FullName = creditorDescription;
		creditor.OH_IsCreditor = true;

		var oneOffCarrier = quotedBooking.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
		oneOffCarrier.TTC_OH_Carrier = org.PK;
		oneOffCarrier.TTC_OH_Creditor = creditor.PK;

		var carrierData = new RateOneOffPotentialCarrierDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CLI, oneOffCarrier))).GetDataObject(oneOffCarrier);
		AssertNotNull("carrierData", carrierData);

		CombineAssertions(() =>
		{
			AssertEquals("carrierData.Code", carrierCode, carrierData.Code);
			AssertEquals("carrierData.Name", carrierDescription, carrierData.Name);
			AssertEquals("creditorData.Creditor.Code", creditorCode, carrierData.Creditor.Code);
			AssertEquals("creditorData.Creditor.Name", creditorDescription, carrierData.Creditor.Name);
		});
	}
}
