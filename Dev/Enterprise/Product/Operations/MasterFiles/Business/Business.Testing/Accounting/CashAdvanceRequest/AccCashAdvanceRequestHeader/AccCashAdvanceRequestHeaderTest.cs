using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccCashAdvanceRequestHeader))]
	public class AccCashAdvanceRequestHeaderTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2022, 6, 13, 23, 55, 30, 253)]
		[TestUtcOffset(10, 0, 0)]
		public void TestCreatedDateTimeLocal()
		{
			var header = Factory.New<AccCashAdvanceRequestHeader>();
			header.CAH_SystemCreateTimeUtc = ZDateTime.UtcNow;
			AssertEquals("13-Jun-22", header.CAH_SystemCreateTimeUtc.ToShortDateString());
			AssertEquals("14-Jun-22", header.CreatedDateTimeLocal.ToShortDateString());

			header.CAH_SystemCreateTimeUtc = ZDateTime.Empty;
			AssertEquals(ZString.Empty, header.CAH_SystemCreateTimeUtc.ToShortDateString());
			AssertEquals(ZString.Empty, header.CreatedDateTimeLocal.ToShortDateString());
		}

		public void TestOrganisationCodeAndName()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ORGAAA";
			orgHeader.OH_FullName = "AAA Transport Corp";
			Factory.Save();

			var header = Factory.New<AccCashAdvanceRequestHeader>();
			AssertNull(header.Organization);
			Assert(header.OrganizationCode.IsEmpty);
			Assert(header.OrganizationName.IsEmpty);

			header.CAH_OH_Organization = orgHeader.PK;
			AssertNotNull(header.Organization);
			AssertEquals("ORGAAA", header.OrganizationCode);
			AssertEquals("AAA Transport Corp", header.OrganizationName);
		}

		public void TestStatusDescription()
		{
			var header = Factory.New<AccCashAdvanceRequestHeader>();
			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			AssertEquals("REQ - Requested", header.StatusDescription);

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.PartiallyPaid;
			AssertEquals("PPA - Partially Paid", header.StatusDescription);

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
			AssertEquals("PAI - Paid in Full", header.StatusDescription);

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced;
			AssertEquals("PIN - Partially Invoiced", header.StatusDescription);

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Invoiced;
			AssertEquals("INV - Invoiced", header.StatusDescription);

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Cancelled;
			AssertEquals("CAN - Canceled", header.StatusDescription);

			header.CAH_Status = "123";
			AssertEquals(ZString.Empty, header.StatusDescription);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesAccCashAdvanceRequestHeader()
		{
			var header = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();

			var osList = new List<string>
				{
					nameof(header.CAH_OSAmount),
					nameof(header.CAH_OSPaidAmount),
					nameof(header.CAH_OSOutstandingAmount)
				};

			var localList = new List<string>
				{
					nameof(header.CAH_LocalAmount),
					nameof(header.CAH_LocalPaidAmount)
				};

			var tester = new DecimalPlacesAttributeTester(header);
			tester.CheckNonLocalCurrency(osList, nameof(header.OSRXDecimals), nameof(header.CAH_RX_NKTransactionCurrency), header);
			tester.CheckLocalCurrency(localList, nameof(header.LocalRXDecimals));
		}

		public void TestHeaderOutstandingAmount()
		{
			var header = GetNewBusinessObject() as AccCashAdvanceRequestHeader;
			header.CAH_OSAmount = 250M;
			header.CAH_LocalAmount = 100M;
			header.CAH_OSPaidAmount = 150M;
			header.CAH_LocalPaidAmount = 50M;

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			AssertEquals(header.CAH_OSAmount, header.CAH_OSOutstandingAmount);
			AssertEquals(header.CAH_LocalAmount, header.CAH_LocalOutstandingAmount);

			foreach (var status in new[] { CashAdvanceStatusCodes.RequestHeader.Cancelled
										, CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced
										, CashAdvanceStatusCodes.RequestHeader.Invoiced })
			{
				header.CAH_Status = status;
				AssertEquals(0M, header.CAH_OSOutstandingAmount);
				AssertEquals(0M, header.CAH_LocalOutstandingAmount);
			}

			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.PartiallyPaid;
			AssertEquals(100M, header.CAH_OSOutstandingAmount);
			AssertEquals(50M, header.CAH_LocalOutstandingAmount);

			header.CAH_Status = "";
			AssertEquals(100M, header.CAH_OSOutstandingAmount);
			AssertEquals(50M, header.CAH_LocalOutstandingAmount);

			header.CAH_OSPaidAmount = 250M;
			header.CAH_LocalPaidAmount = 100M;
			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
			AssertEquals(0M, header.CAH_OSOutstandingAmount);
			AssertEquals(0M, header.CAH_LocalOutstandingAmount);

			header.CAH_Status = "ZZZ";
			AssertExceptionThrown<InvalidOperationException>("Invalid status: ZZZ", () => _ = header.CAH_LocalOutstandingAmount);
		}

		public void TestEvaluateStatusFromLine_ValidStateChange()
		{
			var header = GetNewBusinessObject() as AccCashAdvanceRequestHeader;
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			foreach (var line in new[] { line1, line2, line3 })
			{
				line.CAL_CAH_RequestHeader = header.PK;
			}

			header.UpdateStatusFromLine();
			AssertEquals(header.CAH_Status, CashAdvanceStatusCodes.RequestHeader.Requested);
			AssertEquals(line1.CAL_Status, CashAdvanceStatusCodes.RequestLine.Requested);
			AssertEquals(line2.CAL_Status, CashAdvanceStatusCodes.RequestLine.Requested);
			AssertEquals(line3.CAL_Status, CashAdvanceStatusCodes.RequestLine.Requested);

			line1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			header.UpdateStatusFromLine();
			AssertEquals(header.CAH_Status, CashAdvanceStatusCodes.RequestHeader.PartiallyPaid);
			AssertEquals(line1.CAL_Status, CashAdvanceStatusCodes.RequestLine.Paid);
			AssertEquals(line2.CAL_Status, CashAdvanceStatusCodes.RequestLine.Requested);
			AssertEquals(line3.CAL_Status, CashAdvanceStatusCodes.RequestLine.Requested);

			line2.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			line3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			header.UpdateStatusFromLine();
			AssertEquals(header.CAH_Status, CashAdvanceStatusCodes.RequestHeader.Paid);
			AssertEquals(line1.CAL_Status, CashAdvanceStatusCodes.RequestLine.Paid);
			AssertEquals(line2.CAL_Status, CashAdvanceStatusCodes.RequestLine.Paid);
			AssertEquals(line3.CAL_Status, CashAdvanceStatusCodes.RequestLine.Paid);

			line1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Invoiced;
			header.UpdateStatusFromLine();
			AssertEquals(header.CAH_Status, CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced);
			AssertEquals(line1.CAL_Status, CashAdvanceStatusCodes.RequestLine.Invoiced);
			AssertEquals(line2.CAL_Status, CashAdvanceStatusCodes.RequestLine.Paid);
			AssertEquals(line3.CAL_Status, CashAdvanceStatusCodes.RequestLine.Paid);
		}

		public void TestEvaluateStatusFromLine_InvalidStateChange()
		{
			var header = GetNewBusinessObject() as AccCashAdvanceRequestHeader;
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			foreach (var line in new[] { line1, line2, line3 })
			{
				header.Lines.Add(line);
				line.CAL_CAH_RequestHeader = header.PK;
			}

			AssertEquals(CashAdvanceStatusCodes.RequestLine.Requested, line1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Requested, line2.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Requested, line3.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, header.CAH_Status);

			AssertExceptionThrown(typeof(InvalidOperationException)
								, "There are Paid/Invoiced Advance Payment request lines. Therefore, Advance Payment request should not be allowed to be cancelled."
								, () =>
								{
									line1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
									line2.CAL_Status = CashAdvanceStatusCodes.RequestLine.Cancelled;
									line3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Invoiced;
								});

			line1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Cancelled;
			line2.CAL_Status = CashAdvanceStatusCodes.RequestLine.Cancelled;
			line3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Cancelled;

			AssertEquals(header.CAH_Status, CashAdvanceStatusCodes.RequestHeader.Cancelled);
			AssertEquals(line1.CAL_Status, CashAdvanceStatusCodes.RequestLine.Cancelled);
			AssertEquals(line2.CAL_Status, CashAdvanceStatusCodes.RequestLine.Cancelled);
			AssertEquals(line3.CAL_Status, CashAdvanceStatusCodes.RequestLine.Cancelled);
		}

		public void TestJobNumber()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_JobNum = "TEST0001";
			Factory.Save();

			var header = Factory.New<AccCashAdvanceRequestHeader>();
			AssertNull(header.Job);

			header.CAH_JH_Job = jobHeader.PK;
			AssertNotNull(header.Job);
			AssertEquals(jobHeader.JH_JobNum, header.JobNumber);
		}

		#region Cancel Cash Advance Request

		public void TestCancelCashAdvanceRequestWithREQStatus()
		{
			CancelCashAdvanceRequestCore(CashAdvanceStatusCodes.RequestHeader.Requested, CashAdvanceStatusCodes.RequestLine.Requested, CashAdvanceStatusCodes.RequestLine.Requested);
		}

		public void TestCancelCashAdvanceRequestWithPPAStatus()
		{
			CancelCashAdvanceRequestCore(CashAdvanceStatusCodes.RequestHeader.PartiallyPaid, CashAdvanceStatusCodes.RequestLine.Paid, CashAdvanceStatusCodes.RequestLine.Requested);
		}

		public void TestCancelCashAdvanceRequestWithPAIStatus()
		{
			CancelCashAdvanceRequestCore(CashAdvanceStatusCodes.RequestHeader.Paid, CashAdvanceStatusCodes.RequestLine.Paid, CashAdvanceStatusCodes.RequestLine.Paid);
		}

		public void TestCancelCashAdvanceRequestWithPINStatus()
		{
			CancelCashAdvanceRequestCore(CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, CashAdvanceStatusCodes.RequestLine.Invoiced, CashAdvanceStatusCodes.RequestLine.Paid);
		}

		public void TestCancelCashAdvanceRequestWithINVStatus()
		{
			CancelCashAdvanceRequestCore(CashAdvanceStatusCodes.RequestHeader.Invoiced, CashAdvanceStatusCodes.RequestLine.Invoiced, CashAdvanceStatusCodes.RequestLine.Invoiced);
		}

		public void TestCancelCashAdvanceRequestWithCANStatus()
		{
			CancelCashAdvanceRequestCore(CashAdvanceStatusCodes.RequestHeader.Cancelled, CashAdvanceStatusCodes.RequestLine.Cancelled, CashAdvanceStatusCodes.RequestLine.Cancelled);
		}

		void CancelCashAdvanceRequestCore(string initialHeaderStatus, string initialLine1Status, string initialLine2Status)
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORGAA1";

			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000011";

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";
			Factory.Save();

			var cashAdvanceHeader = CreateCashAdvanceHeader(job.PK, org1.PK, 300m, initialHeaderStatus);
			var cashAdvanceLine1 = CreateCashAdvanceLine(cashAdvanceHeader, 100m, initialLine1Status);
			var cashAdvanceLine2 = CreateCashAdvanceLine(cashAdvanceHeader, 200m, initialLine2Status);
			if (initialHeaderStatus == CashAdvanceStatusCodes.RequestHeader.Invoiced || initialHeaderStatus == CashAdvanceStatusCodes.RequestHeader.Paid)
			{
				cashAdvanceHeader.CAH_OSPaidAmount = cashAdvanceHeader.CAH_LocalPaidAmount = 300m;
			}
			else if (initialHeaderStatus == CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced || initialHeaderStatus == CashAdvanceStatusCodes.RequestHeader.PartiallyPaid)
			{
				cashAdvanceHeader.CAH_OSPaidAmount = cashAdvanceHeader.CAH_LocalPaidAmount = 100m;
			}
			Factory.Save();

			AssertEquals("Precondition", initialHeaderStatus, cashAdvanceHeader.CAH_Status);

			var result = cashAdvanceHeader.CancelRequest();
			if (initialHeaderStatus == CashAdvanceStatusCodes.RequestHeader.Requested)
			{
				AssertEquals(ZString.Empty, result);
				AssertEquals("Postcondition", CashAdvanceStatusCodes.RequestHeader.Cancelled, cashAdvanceHeader.CAH_Status);
				AssertEquals("Postcondition", CashAdvanceStatusCodes.RequestLine.Cancelled, cashAdvanceLine1.CAL_Status);
				AssertEquals("Postcondition", CashAdvanceStatusCodes.RequestLine.Cancelled, cashAdvanceLine2.CAL_Status);
			}
			else if (initialHeaderStatus == CashAdvanceStatusCodes.RequestHeader.Invoiced || initialHeaderStatus == CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced)
			{
				AssertEquals("The Advance Payment Request has been invoiced. Before canceling a Advance Payment Request, the invoice and Advance Payment payment must be reversed.", result);
				AssertEquals("Postcondition", initialHeaderStatus, cashAdvanceHeader.CAH_Status);
				AssertEquals("Postcondition", initialLine1Status, cashAdvanceLine1.CAL_Status);
				AssertEquals("Postcondition", initialLine2Status, cashAdvanceLine2.CAL_Status);
			}
			else if (initialHeaderStatus == CashAdvanceStatusCodes.RequestHeader.Paid || initialHeaderStatus == CashAdvanceStatusCodes.RequestHeader.PartiallyPaid)
			{
				AssertEquals("The Advance Payment Request has been paid. Before canceling a Advance Payment Request, the Advance Payment payment must be reversed.", result);
				AssertEquals("Postcondition", initialHeaderStatus, cashAdvanceHeader.CAH_Status);
				AssertEquals("Postcondition", initialLine1Status, cashAdvanceLine1.CAL_Status);
				AssertEquals("Postcondition", initialLine2Status, cashAdvanceLine2.CAL_Status);
			}
			else
			{
				AssertEquals("Only Advance Payment Requests in the REQ - Requested status can be canceled.", result);
				AssertEquals("Postcondition", initialHeaderStatus, cashAdvanceHeader.CAH_Status);
				AssertEquals("Postcondition", initialLine1Status, cashAdvanceLine1.CAL_Status);
				AssertEquals("Postcondition", initialLine2Status, cashAdvanceLine2.CAL_Status);
			}
		}

		#endregion

		AccCashAdvanceRequestHeader CreateCashAdvanceHeader(ZGuid jobPk, ZGuid orgPk, ZDecimal amount, ZString status)
		{
			var cashAdvanceHeader = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			cashAdvanceHeader.CAH_GC_Company = GlbCompany.CurrentCompany.PK;
			cashAdvanceHeader.CAH_JH_Job = jobPk;
			cashAdvanceHeader.CAH_Ledger = LedgerTypes.AccountsReceivable;
			cashAdvanceHeader.CAH_OH_Organization = orgPk;
			cashAdvanceHeader.CAH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			cashAdvanceHeader.CAH_OSAmount = cashAdvanceHeader.CAH_LocalAmount = amount;
			cashAdvanceHeader.CAH_Status = status;
			return cashAdvanceHeader;
		}

		AccCashAdvanceRequestLine CreateCashAdvanceLine(AccCashAdvanceRequestHeader header, ZDecimal amount, ZString status)
		{
			var cashAdvanceLine = Factory.New<AccCashAdvanceRequestLine>();
			cashAdvanceLine.CAL_GC_Company = GlbCompany.CurrentCompany.PK;
			cashAdvanceLine.CAL_CAH_RequestHeader = header.PK;
			cashAdvanceLine.CAL_LocalAmount = cashAdvanceLine.CAL_OSAmount = amount;
			cashAdvanceLine.CAL_Status = status;
			if (status == CashAdvanceStatusCodes.RequestLine.Invoiced || status == CashAdvanceStatusCodes.RequestLine.Paid)
			{
				cashAdvanceLine.CAL_OSPaidAmount = cashAdvanceLine.CAL_LocalPaidAmount = amount;
			}
			header.Lines.Add(cashAdvanceLine);
			return cashAdvanceLine;
		}

		public void TestMarkAsPaid_SuccessfulCase()
		{
			var cashAdvanceFunctionalityChecker = new Mock<IAccCashAdvanceFunctionalityChecker>();
			cashAdvanceFunctionalityChecker.Setup(c => c.IsReceivablesCashAdvanceFunctionalityEnabled).Returns(true);
			cashAdvanceFunctionalityChecker.Setup(c => c.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed).Returns(true);
			ObjectFactory.Substitute(cashAdvanceFunctionalityChecker.Object);

			var header = GetNewBusinessObject() as AccCashAdvanceRequestHeader;
			header.CAH_Ledger = LedgerTypes.AccountsReceivable;
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			foreach (var line in new[] { line1, line2, line3 })
			{
				line.CAL_CAH_RequestHeader = header.PK;
				line.CAL_Status = CashAdvanceStatusCodes.RequestLine.Requested;
			}
			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;

			header.MarkAsPaid();

			foreach (var line in new[] { line1, line2, line3 })
			{
				AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, line.CAL_Status);
				AssertEquals(line.CAL_LocalAmount, line.CAL_LocalPaidAmount);
				AssertEquals(line.CAL_OSAmount, line.CAL_OSPaidAmount);
			}

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, header.CAH_Status);
			AssertEquals(header.CAH_LocalAmount, header.CAH_LocalPaidAmount);
			AssertEquals(header.CAH_OSAmount, header.CAH_OSPaidAmount);
		}

		public void TestMarkAsPaid_UnsuccessfulCase_NotInRequestedState()
		{
			var cashAdvanceFunctionalityChecker = new Mock<IAccCashAdvanceFunctionalityChecker>();
			cashAdvanceFunctionalityChecker.Setup(c => c.IsReceivablesCashAdvanceFunctionalityEnabled).Returns(true);
			cashAdvanceFunctionalityChecker.Setup(c => c.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed).Returns(true);
			ObjectFactory.Substitute(cashAdvanceFunctionalityChecker.Object);

			var header = GetNewBusinessObject() as AccCashAdvanceRequestHeader;
			header.CAH_Ledger = LedgerTypes.AccountsReceivable;
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			foreach (var line in new[] { line1, line2, line3 })
			{
				line.CAL_CAH_RequestHeader = header.PK;
				line.CAL_Status = CashAdvanceStatusCodes.RequestLine.Invoiced;
			}

			var result = header.MarkAsPaid();

			AssertEquals(false, result.IsSuccessful);
			AssertEquals("Advance Payment is not in requested status.", result.ErrorMessage);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, header.CAH_Status);
			foreach (var line in new[] { line1, line2, line3 })
			{
				AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, line.CAL_Status);
			}
		}

		public void TestMarkAsPaid_UnsuccessfulCase_FunctionalityDisabled()
		{
			var cashAdvanceFunctionalityChecker = new Mock<IAccCashAdvanceFunctionalityChecker>();
			cashAdvanceFunctionalityChecker.Setup(c => c.IsReceivablesCashAdvanceFunctionalityEnabled).Returns(false);
			cashAdvanceFunctionalityChecker.Setup(c => c.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed).Returns(true);
			ObjectFactory.Substitute(cashAdvanceFunctionalityChecker.Object);

			var header = GetNewBusinessObject() as AccCashAdvanceRequestHeader;
			header.CAH_Ledger = LedgerTypes.AccountsReceivable;
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			foreach (var line in new[] { line1, line2, line3 })
			{
				line.CAL_CAH_RequestHeader = header.PK;
				line.CAL_Status = CashAdvanceStatusCodes.RequestLine.Requested;
			}
			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;

			var result = header.MarkAsPaid();

			AssertEquals(false, result.IsSuccessful);
			AssertEquals("Advance Payment cannot be marked as paid, because Advance Payment functionality is disabled.", result.ErrorMessage);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, header.CAH_Status);
			foreach (var line in new[] { line1, line2, line3 })
			{
				AssertEquals(CashAdvanceStatusCodes.RequestLine.Requested, line.CAL_Status);
			}
		}

		public void TestMarkAsPaid_UnsuccessfulCase_ManualMarkingAsPaidIsDisabled()
		{
			var cashAdvanceFunctionalityChecker = new Mock<IAccCashAdvanceFunctionalityChecker>();
			cashAdvanceFunctionalityChecker.Setup(c => c.IsReceivablesCashAdvanceFunctionalityEnabled).Returns(true);
			cashAdvanceFunctionalityChecker.Setup(c => c.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed).Returns(false);
			ObjectFactory.Substitute(cashAdvanceFunctionalityChecker.Object);

			var header = GetNewBusinessObject() as AccCashAdvanceRequestHeader;
			header.CAH_Ledger = LedgerTypes.AccountsReceivable;
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			foreach (var line in new[] { line1, line2, line3 })
			{
				line.CAL_CAH_RequestHeader = header.PK;
				line.CAL_Status = CashAdvanceStatusCodes.RequestLine.Requested;
			}
			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;

			var result = header.MarkAsPaid();

			AssertEquals(false, result.IsSuccessful);
			AssertEquals("Advance Payment cannot be marked as paid, because manual updating of Advance Payment request to paid is not allowed.", result.ErrorMessage);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, header.CAH_Status);
			foreach (var line in new[] { line1, line2, line3 })
			{
				AssertEquals(CashAdvanceStatusCodes.RequestLine.Requested, line.CAL_Status);
			}
		}

		public void TestMarkAsUnpaid_SuccessfulCase()
		{
			var cashAdvanceFunctionalityChecker = new Mock<IAccCashAdvanceFunctionalityChecker>();
			cashAdvanceFunctionalityChecker.Setup(c => c.IsReceivablesCashAdvanceFunctionalityEnabled).Returns(true);
			cashAdvanceFunctionalityChecker.Setup(c => c.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed).Returns(true);
			ObjectFactory.Substitute(cashAdvanceFunctionalityChecker.Object);

			var header = GetNewBusinessObject() as AccCashAdvanceRequestHeader;
			header.CAH_Ledger = LedgerTypes.AccountsReceivable;
			header.Lines.Add(Factory.NewWithValidTestData<AccCashAdvanceRequestLine>());
			header.Lines.Add(Factory.NewWithValidTestData<AccCashAdvanceRequestLine>());
			header.Lines.Add(Factory.NewWithValidTestData<AccCashAdvanceRequestLine>());
			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			foreach (AccCashAdvanceRequestLine line in header.Lines)
			{
				line.CAL_CAH_RequestHeader = header.PK;
				line.CAL_LocalAmount = 200M;
				line.CAL_OSAmount = 100M;
			}

			header.MarkAsPaid();
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, header.CAH_Status);
			AssertEquals(600M, header.CAH_LocalPaidAmount);
			AssertEquals(300M, header.CAH_OSPaidAmount);

			header.UndoPaidStatus();

			foreach (AccCashAdvanceRequestLine line in header.Lines)
			{
				AssertEquals(CashAdvanceStatusCodes.RequestLine.Requested, line.CAL_Status);
				AssertEquals(0M, line.CAL_LocalPaidAmount);
				AssertEquals(0M, line.CAL_OSPaidAmount);
			}

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, header.CAH_Status);
			AssertEquals(0M, header.CAH_LocalPaidAmount);
			AssertEquals(0M, header.CAH_OSPaidAmount);
		}

		public void TestMarkAsUnpaid_UnsuccessfulCase_NotInRequestedState()
		{
			var cashAdvanceFunctionalityChecker = new Mock<IAccCashAdvanceFunctionalityChecker>();
			cashAdvanceFunctionalityChecker.Setup(c => c.IsReceivablesCashAdvanceFunctionalityEnabled).Returns(true);
			cashAdvanceFunctionalityChecker.Setup(c => c.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed).Returns(true);
			ObjectFactory.Substitute(cashAdvanceFunctionalityChecker.Object);

			var header = GetNewBusinessObject() as AccCashAdvanceRequestHeader;
			header.CAH_Ledger = LedgerTypes.AccountsReceivable;
			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			foreach (var line in new[] { line1, line2, line3 })
			{
				line.CAL_CAH_RequestHeader = header.PK;
				line.CAL_LocalAmount = 200M;
				line.CAL_OSAmount = 100M;
			}

			var result = header.UndoPaidStatus();
			AssertEquals(false, result.IsSuccessful);
			AssertEquals("Advance Payment is not in paid status.", result.ErrorMessage);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, header.CAH_Status);
			foreach (var line in new[] { line1, line2, line3 })
			{
				AssertEquals(CashAdvanceStatusCodes.RequestLine.Requested, line.CAL_Status);
			}
		}

		public void TestMarkAsUnpaid_UnsuccessfulCase_FunctionalityDisabled()
		{
			var cashAdvanceFunctionalityChecker = new Mock<IAccCashAdvanceFunctionalityChecker>();
			cashAdvanceFunctionalityChecker.Setup(c => c.IsReceivablesCashAdvanceFunctionalityEnabled).Returns(false);
			cashAdvanceFunctionalityChecker.Setup(c => c.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed).Returns(true);
			ObjectFactory.Substitute(cashAdvanceFunctionalityChecker.Object);

			var header = GetNewBusinessObject() as AccCashAdvanceRequestHeader;
			header.CAH_Ledger = LedgerTypes.AccountsReceivable;
			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			foreach (var line in new[] { line1, line2, line3 })
			{
				line.CAL_CAH_RequestHeader = header.PK;
				line.CAL_LocalAmount = 200M;
				line.CAL_OSAmount = 100M;
			}

			var result = header.UndoPaidStatus();
			AssertEquals(false, result.IsSuccessful);
			AssertEquals("Advance Payment cannot be marked as unpaid, because Advance Payment functionality is disabled.", result.ErrorMessage);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, header.CAH_Status);
			foreach (var line in new[] { line1, line2, line3 })
			{
				AssertEquals(CashAdvanceStatusCodes.RequestLine.Requested, line.CAL_Status);
			}
		}

		public void TestMarkAsUnpaid_UnsuccessfulCase_ManualMarkingAsPaidIsDisabled()
		{
			var cashAdvanceFunctionalityChecker = new Mock<IAccCashAdvanceFunctionalityChecker>();
			cashAdvanceFunctionalityChecker.Setup(c => c.IsReceivablesCashAdvanceFunctionalityEnabled).Returns(true);
			cashAdvanceFunctionalityChecker.Setup(c => c.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed).Returns(false);
			ObjectFactory.Substitute(cashAdvanceFunctionalityChecker.Object);

			var header = GetNewBusinessObject() as AccCashAdvanceRequestHeader;
			header.CAH_Ledger = LedgerTypes.AccountsReceivable;
			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var line3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			foreach (var line in new[] { line1, line2, line3 })
			{
				line.CAL_CAH_RequestHeader = header.PK;
				line.CAL_LocalAmount = 200M;
				line.CAL_OSAmount = 100M;
			}

			var result = header.UndoPaidStatus();
			AssertEquals(false, result.IsSuccessful);
			AssertEquals("Advance Payment cannot be marked as unpaid, because manual updating of Advance Payment request to paid is not allowed.", result.ErrorMessage);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, header.CAH_Status);
			foreach (var line in new[] { line1, line2, line3 })
			{
				AssertEquals(CashAdvanceStatusCodes.RequestLine.Requested, line.CAL_Status);
			}
		}

		public void TestPaidAmountFieldConcurrency()
		{
			var cal1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var cah1 = cal1.RequestHeader;
			cah1.CAH_Ledger = "AR";
			cah1.Lines.Add(cal1);
			Factory.Save();

			AssertConcurrency(cah1
							, AccCashAdvanceRequestHeaderSchema.CAH_LocalPaidAmount
							, (cah) =>
							{
								cah.CAH_LocalAmount =
								cah.CAH_OSAmount =
								cah.CAH_LocalPaidAmount =
								cah.CAH_OSPaidAmount = 200M;
							}
							, (cah) =>
							{
								cah.CAH_LocalAmount =
								cah.CAH_OSAmount =
								cah.CAH_LocalPaidAmount =
								cah.CAH_OSPaidAmount = 210M;
							});
		}

		public void TestCAH_StatusConcurrency()
		{
			var cal1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var cah1 = cal1.RequestHeader;
			cah1.CAH_Ledger = "AR";
			cah1.Lines.Add(cal1);
			cah1.CAH_LocalAmount =
				cah1.CAH_OSAmount =
				cah1.CAH_LocalPaidAmount =
				cah1.CAH_OSPaidAmount = 200M;
			Factory.Save();

			AssertConcurrency(cah1
							, AccCashAdvanceRequestHeaderSchema.CAH_LocalPaidAmount
							, (cah) =>
							{
								cah.CAH_Status = "PAI";
							}
							, (cah) =>
							{
								cah.CAH_Status = "INV";
							});
		}

		void AssertConcurrency(AccCashAdvanceRequestHeader cah, SchemaColumn column, Action<AccCashAdvanceRequestHeader> setupBizOInFactory1, Action<AccCashAdvanceRequestHeader> setupBizOInFactory2)
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var cahInFactory1 = factory1.Load<AccCashAdvanceRequestHeader>(cah.PK);
			var cahInFactory2 = factory2.Load<AccCashAdvanceRequestHeader>(cah.PK);

			setupBizOInFactory1(cahInFactory1);
			setupBizOInFactory2(cahInFactory2);

			factory1.Save();
			try
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				factory2.Save();
				Fail("Expected exception: The system cannot automatically merge your changes because there are conflicts with critical fields.");
			}
			catch (ZSaveConcurrencyException e)
			{
				ZExceptionReporting.HandleSaveException(e);
				AssertContains(column.Name, e.Message);
			}
			AssertContains("The system cannot automatically merge your changes because there are conflicts with critical fields.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestEnterpriseBusinessObjectIsAudited()
		{
			var columns = new SchemaColumn[]
			{
				AccCashAdvanceRequestHeaderSchema.PK,
				AccCashAdvanceRequestHeaderSchema.CAH_OSAmount,
				AccCashAdvanceRequestHeaderSchema.CAH_Status
			};
			AuditLogsHelperForTesting.AssertColumnsExistsInAuditDb(Factory, AccCashAdvanceRequestHeaderSchema.PK.TableSchema.SqlSchemaName, AccCashAdvanceRequestHeaderSchema.PK.TableName, columns);
		}

		public void TestLogsAddedOnMarkAsPaidAndMarkAsUnPaid()
		{
			var cashAdvanceFunctionalityChecker = new Mock<IAccCashAdvanceFunctionalityChecker>();
			cashAdvanceFunctionalityChecker.Setup(c => c.IsReceivablesCashAdvanceFunctionalityEnabled).Returns(true);
			cashAdvanceFunctionalityChecker.Setup(c => c.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed).Returns(true);
			ObjectFactory.Substitute(cashAdvanceFunctionalityChecker.Object);

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			var cashAdvanceHeader = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			cashAdvanceHeader.CAH_Ledger = LedgerTypes.AccountsReceivable;
			cashAdvanceHeader.CAH_GC_Company = GlbCompany.CurrentCompany.PK;
			cashAdvanceHeader.CAH_JH_Job = job.PK;
			cashAdvanceHeader.CAH_OSAmount = cashAdvanceHeader.CAH_LocalAmount = 100m;
			cashAdvanceHeader.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;

			var line1 = cashAdvanceHeader.Lines.AddNew();
			var line2 = cashAdvanceHeader.Lines.AddNew();

			line1.CAL_CAH_RequestHeader = cashAdvanceHeader.PK;
			line2.CAL_CAH_RequestHeader = cashAdvanceHeader.PK;
			AssertNotNull(line1.RequestHeader);
			AssertNotNull(line2.RequestHeader);

			AssertEquals(false, job.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference.Contains(cashAdvanceHeader.CAH_RequestReferenceNumber)));
			cashAdvanceHeader.MarkAsPaid();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, cashAdvanceHeader.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, line1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, line2.CAL_Status);
			AssertEquals(1, job.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference.Equals($"Advance Payment {cashAdvanceHeader.CAH_RequestReferenceNumber} has been paid in full.")));

			job.Logs.RemoveAndDeleteAll();
			AssertEquals(false, job.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference.Contains(cashAdvanceHeader.CAH_RequestReferenceNumber)));

			cashAdvanceHeader.UndoPaidStatus();
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, cashAdvanceHeader.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Requested, line1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Requested, line2.CAL_Status);
			AssertEquals(1, job.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference.Equals($"Advance Payment {cashAdvanceHeader.CAH_RequestReferenceNumber} Payment Status has been reset.")));
		}

		public void TestCanBeMarkedAsPaid()
		{
			var statusCodes = new List<string>() { CashAdvanceStatusCodes.RequestHeader.Cancelled, CashAdvanceStatusCodes.RequestHeader.Invoiced, CashAdvanceStatusCodes.RequestHeader.Paid, CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, CashAdvanceStatusCodes.RequestHeader.PartiallyPaid, CashAdvanceStatusCodes.RequestHeader.Pending, CashAdvanceStatusCodes.RequestHeader.Requested };
			var cashAdvanceHeader = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			foreach (var status in statusCodes)
			{
				cashAdvanceHeader.CAH_Status = status;

				if (cashAdvanceHeader.CAH_Status == CashAdvanceStatusCodes.RequestHeader.Requested || cashAdvanceHeader.CAH_Status == CashAdvanceStatusCodes.RequestHeader.PartiallyPaid)
				{
					AssertEquals(true, cashAdvanceHeader.CanBeMarkedAsPaid);
				}
				else
				{
					AssertEquals(false, cashAdvanceHeader.CanBeMarkedAsPaid);
				}
			}
		}

		public void TestBusinessContext()
		{
			AccCashAdvanceRequestHeader testCashAdvanceRequest = Factory.New<AccCashAdvanceRequestHeader>();
			AssertEquals(BusinessContext.CashAdvanceRequest, testCashAdvanceRequest.CashAdvanceRequestDocumentSupporter.BusinessContext);
		}

		public void TestCodeAndDescriptionProperty_ForCodeFindBox()
		{
			var cashAdvanceHeader = Factory.New<AccCashAdvanceRequestHeader>();
			cashAdvanceHeader.CAH_RequestReferenceNumber = "100001";

			var codeProperty = CodePropertyAttribute.CodePropertyNameFromType(GetExpectedBusinessObjectType());
			var descriptionProperty = DescriptionPropertyAttribute.DescriptionPropertyNameFromType(GetExpectedBusinessObjectType());

			AssertEquals(nameof(cashAdvanceHeader.CAH_RequestReferenceNumber), codeProperty);
			AssertEquals(nameof(cashAdvanceHeader.HumanReadableName), descriptionProperty);
			AssertEquals("100001", CodePropertyAttribute.CodeFromBusinessObject(cashAdvanceHeader));
			AssertEquals("AccCashAdvanceRequestHeader", DescriptionPropertyAttribute.DescriptionFromBusinessObject(cashAdvanceHeader));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var testJobHeader = Factory.NewJobForTesting<JobHeader>();
			var result = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			result.CAH_JH_Job = testJobHeader.PK;
			return result;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		#endregion
	}
}
