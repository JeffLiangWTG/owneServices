using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccCashAdvanceRequestLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCAL_Status_Cancelled()
		{
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			line1.CAL_LocalAmount = 200M;
			line1.CAL_LocalPaidAmount = 200M;
			line1.CAL_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
			AssertNoErrors(line1.CAL_StatusInfo);

			line1.CAL_Status = CashAdvanceStatusCodes.RequestHeader.Cancelled;
			AssertHasError(line1.CAL_StatusInfo, "Line status is changed to CAN without resetting the paid amount to zero.");
		}

		public void TestCheckCAL_Status_Requested()
		{
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			line1.CAL_LocalAmount = 200M;
			line1.CAL_LocalPaidAmount = 200M;
			line1.CAL_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
			AssertNoErrors(line1.CAL_StatusInfo);

			line1.CAL_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			AssertHasError(line1.CAL_StatusInfo, "Line status is changed to REQ without resetting the paid amount to zero.");
		}

		public void TestCheckCAL_Status_Paid()
		{
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			line1.CAL_LocalAmount = 200M;
			AssertNoErrors(line1.CAL_StatusInfo);

			line1.CAL_LocalPaidAmount = 120M;
			line1.CAL_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
			AssertHasError(line1.CAL_StatusInfo, "Status is updated to paid. But paid amount is not same as the request advance payment amount.");
		}
	}
}
