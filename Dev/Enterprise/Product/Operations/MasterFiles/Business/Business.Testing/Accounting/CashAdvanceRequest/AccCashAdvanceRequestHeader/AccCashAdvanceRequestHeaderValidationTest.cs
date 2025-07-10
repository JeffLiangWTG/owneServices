using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccCashAdvanceRequestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCAH_Status_Cancelled()
		{
			var header = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			foreach (var line in new[] { line1, line2, line3 })
			{
				line.CAL_CAH_RequestHeader = header.PK;
				header.Lines.Add(line);
			}

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Cancelled;
			AssertHasError(header.CAH_StatusInfo, "Advance Payment request is canceled without canceling all request lines.");

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			AssertNoErrors(header.CAH_StatusInfo);

			line1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Cancelled;
			AssertEquals(ZString.Empty, header.CAH_Status);
			AssertNoErrors(header.CAH_StatusInfo);

			line2.CAL_Status = CashAdvanceStatusCodes.RequestLine.Cancelled;
			AssertEquals(ZString.Empty, header.CAH_Status);
			AssertNoErrors(header.CAH_StatusInfo);

			line3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Cancelled;
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Cancelled, header.CAH_Status);
			AssertNoErrors(header.CAH_StatusInfo);
		}

		public void TestCheckCAH_Status_Invoiced()
		{
			var header = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			foreach (var line in new[] { line1, line2, line3 })
			{
				line.CAL_CAH_RequestHeader = header.PK;
				header.Lines.Add(line);
			}

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Invoiced;
			AssertHasError(header.CAH_StatusInfo, "Advance Payment request status is updated to invoiced. But all request lines are not in invoiced status.");

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			AssertNoErrors(header.CAH_StatusInfo);

			line1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Invoiced;
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, header.CAH_Status);
			AssertNoErrors(header.CAH_StatusInfo);

			line2.CAL_Status = CashAdvanceStatusCodes.RequestLine.Invoiced;
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, header.CAH_Status);
			AssertNoErrors(header.CAH_StatusInfo);

			line3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Invoiced;
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, header.CAH_Status);
			AssertNoErrors(header.CAH_StatusInfo);
		}

		public void TestCheckCAH_Status_PartiallyInvoiced()
		{
			var header = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			foreach (var line in new[] { line1, line2, line3 })
			{
				line.CAL_CAH_RequestHeader = header.PK;
				header.Lines.Add(line);
			}

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced;
			AssertHasError(header.CAH_StatusInfo, "Advance Payment request status is marked as partially invoiced. But there is no request line in invoiced status.");

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			AssertNoErrors(header.CAH_StatusInfo);

			line1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Invoiced;
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, header.CAH_Status);
			AssertNoErrors(header.CAH_StatusInfo);
		}

		public void TestCheckCAH_Status_Paid()
		{
			var header = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			foreach (var line in new[] { line1, line2, line3 })
			{
				line.CAL_CAH_RequestHeader = header.PK;
				header.Lines.Add(line);
			}

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
			AssertHasError(header.CAH_StatusInfo, "Advance Payment request status is updated to paid. But all request lines are not in paid status.");

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			AssertNoErrors(header.CAH_StatusInfo);

			line1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PartiallyPaid, header.CAH_Status);
			AssertNoErrors(header.CAH_StatusInfo);

			line2.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PartiallyPaid, header.CAH_Status);
			AssertNoErrors(header.CAH_StatusInfo);

			line3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, header.CAH_Status);
			AssertNoErrors(header.CAH_StatusInfo);
		}

		public void TestCheckCAH_Status_PartiallyPaid()
		{
			var header = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			foreach (var line in new[] { line1, line2, line3 })
			{
				line.CAL_CAH_RequestHeader = header.PK;
				header.Lines.Add(line);
			}

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.PartiallyPaid;
			AssertHasError(header.CAH_StatusInfo, "Advance Payment request status is marked as partially paid. But there is no request line in paid status.");

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			AssertNoErrors(header.CAH_StatusInfo);

			line1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PartiallyPaid, header.CAH_Status);
			AssertNoErrors(header.CAH_StatusInfo);
		}
	}
}
