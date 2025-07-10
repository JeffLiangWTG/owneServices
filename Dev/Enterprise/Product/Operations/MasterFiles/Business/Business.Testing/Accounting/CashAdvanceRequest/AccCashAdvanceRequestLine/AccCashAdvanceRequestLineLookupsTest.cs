using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccCashAdvanceRequestLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusList()
		{
			var requestLine = Factory.New<AccCashAdvanceRequestLine>();

			AssertNotNull("StatusList should not be null", requestLine.Lookups.StatusCodeList);
			AssertEquals("StatusList.Count", 4, requestLine.Lookups.StatusCodeList.Count);

			AssertEquals("1st Element (Code)", CashAdvanceStatusCodes.RequestLine.Requested, requestLine.Lookups.StatusCodeList[0].Code);
			AssertEquals("1st Element (Description)", CashAdvanceStatusCodes.RequestLine.RequestedDescription, requestLine.Lookups.StatusCodeList[0].Description);

			AssertEquals("2nd Element (Code)", CashAdvanceStatusCodes.RequestLine.Paid, requestLine.Lookups.StatusCodeList[1].Code);
			AssertEquals("2nd Element (Description)", CashAdvanceStatusCodes.RequestLine.PaidDescription, requestLine.Lookups.StatusCodeList[1].Description);

			AssertEquals("3th Element (Code)", CashAdvanceStatusCodes.RequestLine.Invoiced, requestLine.Lookups.StatusCodeList[2].Code);
			AssertEquals("3th Element (Description)", CashAdvanceStatusCodes.RequestLine.InvoicedDescription, requestLine.Lookups.StatusCodeList[2].Description);

			AssertEquals("4th Element (Code)", CashAdvanceStatusCodes.RequestLine.Cancelled, requestLine.Lookups.StatusCodeList[3].Code);
			AssertEquals("4th Element (Description)", CashAdvanceStatusCodes.RequestLine.CancelledDescription, requestLine.Lookups.StatusCodeList[3].Description);
		}
	}
}
