using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccCashAdvanceRequestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusList()
		{
			var requestHeader = Factory.New<AccCashAdvanceRequestHeader>();

			AssertNotNull("StatusList should not be null", requestHeader.Lookups.StatusCodeList);
			AssertEquals("StatusList.Count", 7, requestHeader.Lookups.StatusCodeList.Count);

			AssertEquals("0th Element (Code)", CashAdvanceStatusCodes.RequestHeader.Pending, requestHeader.Lookups.StatusCodeList[0].Code);
			AssertEquals("0th Element (Description)", CashAdvanceStatusCodes.RequestHeader.PendingDescription, requestHeader.Lookups.StatusCodeList[0].Description);

			AssertEquals("1st Element (Code)", CashAdvanceStatusCodes.RequestHeader.Requested, requestHeader.Lookups.StatusCodeList[1].Code);
			AssertEquals("1st Element (Description)", CashAdvanceStatusCodes.RequestHeader.RequestedDescription, requestHeader.Lookups.StatusCodeList[1].Description);

			AssertEquals("2nd Element (Code)", CashAdvanceStatusCodes.RequestHeader.Paid, requestHeader.Lookups.StatusCodeList[2].Code);
			AssertEquals("2nd Element (Description)", CashAdvanceStatusCodes.RequestHeader.PaidDescription, requestHeader.Lookups.StatusCodeList[2].Description);

			AssertEquals("3rd Element (Code)", CashAdvanceStatusCodes.RequestHeader.PartiallyPaid, requestHeader.Lookups.StatusCodeList[3].Code);
			AssertEquals("3rd Element (Description)", CashAdvanceStatusCodes.RequestHeader.PartiallyPaidDescription, requestHeader.Lookups.StatusCodeList[3].Description);

			AssertEquals("4th Element (Code)", CashAdvanceStatusCodes.RequestHeader.Invoiced, requestHeader.Lookups.StatusCodeList[4].Code);
			AssertEquals("4th Element (Description)", CashAdvanceStatusCodes.RequestHeader.InvoicedDescription, requestHeader.Lookups.StatusCodeList[4].Description);

			AssertEquals("5th Element (Code)", CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, requestHeader.Lookups.StatusCodeList[5].Code);
			AssertEquals("5th Element (Description)", CashAdvanceStatusCodes.RequestHeader.PartiallyInvoicedDescription, requestHeader.Lookups.StatusCodeList[5].Description);

			AssertEquals("6th Element (Code)", CashAdvanceStatusCodes.RequestHeader.Cancelled, requestHeader.Lookups.StatusCodeList[6].Code);
			AssertEquals("6th Element (Description)", CashAdvanceStatusCodes.RequestHeader.CancelledDescription, requestHeader.Lookups.StatusCodeList[6].Description);
		}
	}
}
