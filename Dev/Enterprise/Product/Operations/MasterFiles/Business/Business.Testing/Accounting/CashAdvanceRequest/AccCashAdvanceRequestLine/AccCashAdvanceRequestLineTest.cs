using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccCashAdvanceRequestLine))]
	sealed class AccCashAdvanceRequestLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPropertiesRetrievedFromHeader()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ORGAAA";
			orgHeader.OH_FullName = "AAA Transport Corp";
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			header.CAH_OH_Organization = orgHeader.PK;
			header.CAH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;

			var line = Factory.New<AccCashAdvanceRequestLine>();
			AssertNull(line.RequestHeader);
			Assert(line.OrganizationCode.IsEmpty);
			Assert(line.OrganizationName.IsEmpty);
			Assert(line.Currency.IsEmpty);

			line.CAL_CAH_RequestHeader = header.PK;
			AssertNotNull(line.RequestHeader);
			AssertEquals("ORGAAA", line.OrganizationCode);
			AssertEquals("AAA Transport Corp", line.OrganizationName);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, line.Currency);
		}

		public void TestStatusDescription()
		{
			var line = Factory.New<AccCashAdvanceRequestLine>();
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Requested, line.CAL_Status);
			AssertEquals("REQ - Requested", line.StatusDescription);

			line.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			AssertEquals("PAI - Paid in Full", line.StatusDescription);

			line.CAL_Status = CashAdvanceStatusCodes.RequestLine.Invoiced;
			AssertEquals("INV - Invoiced", line.StatusDescription);

			line.CAL_Status = CashAdvanceStatusCodes.RequestLine.Cancelled;
			AssertEquals("CAN - Canceled", line.StatusDescription);

			line.CAL_Status = "123";
			AssertEquals(ZString.Empty, line.StatusDescription);
		}

		public void TestRelatedJobChargeCode()
		{
			var arHeader = Factory.New<AccCashAdvanceRequestHeader>();
			arHeader.CAH_Ledger = LedgerTypes.AccountsReceivable;
			var apHeader = Factory.New<AccCashAdvanceRequestHeader>();
			apHeader.CAH_Ledger = LedgerTypes.AccountsPayable;

			var arLine = Factory.New<AccCashAdvanceRequestLine>();
			arLine.CAL_CAH_RequestHeader = arHeader.PK;

			var apLine = Factory.New<AccCashAdvanceRequestLine>();
			apLine.CAL_CAH_RequestHeader = apHeader.PK;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "PQR";
			chargeCode.AC_Desc = "PQR Desc";
			var charge = Factory.New<JobCharge>();
			charge.JR_AC = chargeCode.PK;

			Assert(arLine.RelatedChargeCode.IsEmpty);
			Assert(apLine.RelatedChargeCode.IsEmpty);

			charge.JR_CAL_ARLine = arLine.PK;
			charge.JR_CAL_APLine = apLine.PK;

			AssertEquals("PQR", arLine.RelatedChargeCode);
			AssertEquals("PQR", apLine.RelatedChargeCode);
			AssertEquals("PQR Desc", arLine.RelatedChargeDescription);
			AssertEquals("PQR Desc", apLine.RelatedChargeDescription);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesAccCashAdvanceRequestLine()
		{
			var line = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();

			var osList = new List<string>
				{
					nameof(line.CAL_OSAmount),
					nameof(line.CAL_OSPaidAmount)
				};

			var localList = new List<string>
				{
					nameof(line.CAL_LocalAmount),
					nameof(line.CAL_LocalPaidAmount)
				};

			var tester = new DecimalPlacesAttributeTester(line);
			tester.CheckNonLocalCurrency(osList, nameof(line.OSRXDecimals), nameof(line.RequestHeader.CAH_RX_NKTransactionCurrency), line.RequestHeader);
			tester.CheckLocalCurrency(localList, nameof(line.LocalRXDecimals));
		}

		public void TestMarkAsInvoiced()
		{
			var line = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			AssertExceptionThrown<InvalidOperationException>("Status cannot be updated to 'Invoiced', as either cash advance is not paid or outstanding amount is not zero.", () => line.MarkAsInvoiced());

			line.CAL_OSAmount = line.CAL_OSPaidAmount = 200M;
			line.CAL_LocalAmount = line.CAL_LocalPaidAmount = 200M;
			line.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			line.MarkAsInvoiced();
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, line.CAL_Status);
		}

		public void TestCancel()
		{
			var line = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			line.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			AssertExceptionThrown<InvalidOperationException>($"Status cannot be updated to '{CashAdvanceStatusCodes.RequestLine.Cancelled}'. Current status is '{CashAdvanceStatusCodes.RequestLine.Paid}'", () => line.Cancel());

			line.CAL_Status = CashAdvanceStatusCodes.RequestLine.Requested;
			line.Cancel();
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Cancelled, line.CAL_Status);
		}

		public void TestLineOutstandingAmount()
		{
			var line = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			line.CAL_OSAmount = 250M;
			line.CAL_LocalAmount = 100M;
			line.CAL_OSPaidAmount = 150M;
			line.CAL_LocalPaidAmount = 50M;

			line.CAL_Status = CashAdvanceStatusCodes.RequestLine.Requested;
			AssertEquals(line.CAL_OSAmount, line.CAL_OSOutstandingAmount);
			AssertEquals(line.CAL_LocalAmount, line.CAL_LocalOutstandingAmount);

			foreach (var status in new[] { CashAdvanceStatusCodes.RequestLine.Cancelled
										, CashAdvanceStatusCodes.RequestLine.Invoiced })
			{
				line.CAL_Status = status;
				AssertEquals(0M, line.CAL_OSOutstandingAmount);
				AssertEquals(0M, line.CAL_LocalOutstandingAmount);
			}

			line.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			AssertEquals(100M, line.CAL_OSOutstandingAmount);
			AssertEquals(50M, line.CAL_LocalOutstandingAmount);
		}

		public void TestBooleanStatusProperties()
		{
			var line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			line1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Requested;
			AssertEquals(true, line1.IsRequested);

			var line2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			line2.CAL_Status = CashAdvanceStatusCodes.RequestLine.Cancelled;
			AssertEquals(true, line2.IsCancelled);

			var line3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			line3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			AssertEquals(true, line3.IsPaid);

			var line4 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			line4.CAL_Status = CashAdvanceStatusCodes.RequestLine.Invoiced;
			AssertEquals(true, line4.IsInvoiced);
		}

		public void TestOSPaidAmountFieldConcurrency() => AssertPaidAmountFieldConcurrency(AccCashAdvanceRequestLineSchema.CAL_OSPaidAmount);

		public void TestLocalPaidAmountFieldConcurrency() => AssertPaidAmountFieldConcurrency(AccCashAdvanceRequestLineSchema.CAL_LocalPaidAmount);

		void AssertPaidAmountFieldConcurrency(SchemaColumn column)
		{
			var cal1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var cah1 = cal1.RequestHeader;
			cah1.CAH_Ledger = "AR";
			cah1.CAH_Status = "REQ";
			cah1.CAH_LocalAmount = cah1.CAH_OSAmount = 200M;
			cal1.CAL_CAH_RequestHeader = cah1.PK;
			Factory.Save();

			AssertConcurrency(cal1
							, column
							, (cal) =>
							{
								using (cal.HeaderStatusEvaluationSuspender.GetSuspender())
								{
									cal.CAL_LocalAmount =
									cal.CAL_OSAmount =
									cal.CAL_LocalPaidAmount =
									cal.CAL_OSPaidAmount = 200M;
									cal.CAL_Status = "PAI";
								}
							}
							, (cal) =>
							{
								using (cal.HeaderStatusEvaluationSuspender.GetSuspender())
								{
									cal.CAL_LocalAmount =
									cal.CAL_OSAmount =
									cal.CAL_LocalPaidAmount =
									cal.CAL_OSPaidAmount = 200M;
									cal.CAL_Status = "PAI";
								}
							});
		}

		public void TestStatusConcurrency()
		{
			var cal1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var cah1 = cal1.RequestHeader;
			cah1.CAH_Ledger = "AR";
			cah1.CAH_Status = "REQ";
			cah1.CAH_LocalAmount =
				cah1.CAH_OSAmount =
				cal1.CAL_LocalAmount =
				cal1.CAL_OSAmount =
				cal1.CAL_LocalPaidAmount =
				cal1.CAL_OSPaidAmount = 200M;

			cal1.CAL_Status = "PAI";
			cal1.CAL_CAH_RequestHeader = cah1.PK;
			Factory.Save();

			AssertConcurrency(cal1
							, AccCashAdvanceRequestLineSchema.CAL_Status
							, (cal) =>
							{
								using (cal.HeaderStatusEvaluationSuspender.GetSuspender())
								{
									cal.CAL_Status = "INV";
								}
							}
							, (cal) =>
							{
								using (cal.HeaderStatusEvaluationSuspender.GetSuspender())
								{
									cal.CAL_Status = "INV";
								}
							});
		}

		void AssertConcurrency(AccCashAdvanceRequestLine cal, SchemaColumn column, Action<AccCashAdvanceRequestLine> setupBizOInFactory1, Action<AccCashAdvanceRequestLine> setupBizOInFactory2)
		{
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var calInFactory1 = factory1.Load<AccCashAdvanceRequestLine>(cal.PK);
			var calInFactory2 = factory2.Load<AccCashAdvanceRequestLine>(cal.PK);

			setupBizOInFactory1(calInFactory1);
			setupBizOInFactory2(calInFactory2);

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

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		#endregion
	}
}
