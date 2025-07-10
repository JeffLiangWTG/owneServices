using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OutstandingCashAdvanceFilterTest : TestCaseWithFactory
	{
		public void TestHasOutstandingARCashAdvances()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORGAA1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORGBB1";

			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000011";

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";

			Factory.Save();

			var jobNumbers = new[] { "S00000011" };

			AssertEquals("Job has no charges", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvances(jobNumbers));
			AssertEquals("Job has no charges", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org1.PK));
			AssertEquals("Job has no charges", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org2.PK));

			var charge1 = CreateCharge(job, 100m, org1);
			var charge2 = CreateCharge(job, 200m, org1);
			var charge3 = CreateCharge(job, 300m, org2);
			var charge4 = CreateCharge(job, 400m, org2);
			Factory.Save();

			AssertEquals("Charges are not marked as AR Cash Advance Required", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvances(jobNumbers));
			AssertEquals("Charges are not marked as AR Cash Advance Required", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org1.PK));
			AssertEquals("Charges are not marked as AR Cash Advance Required", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org2.PK));

			charge1.JR_IsAPCashAdvance = true;
			charge2.JR_IsAPCashAdvance = true;
			charge3.JR_IsAPCashAdvance = true;
			charge4.JR_IsAPCashAdvance = true;
			Factory.Save();

			AssertEquals("AP Cash Advances should not be considered", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvances(jobNumbers));
			AssertEquals("AP Cash Advances should not be considered", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org1.PK));
			AssertEquals("AP Cash Advances should not be considered", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org2.PK));

			var apCashAdvanceHeaderForOrg1 = CreateCashAdvanceHeader(org1, 300m, false);
			var apCashAdvanceLine1ForOrg1 = CreateCashAdvanceLine(apCashAdvanceHeaderForOrg1, 100m);
			charge1.JR_CAL_APLine = apCashAdvanceLine1ForOrg1.PK;
			var apCashAdvanceLine2ForOrg1 = CreateCashAdvanceLine(apCashAdvanceHeaderForOrg1, 200m);
			charge2.JR_CAL_APLine = apCashAdvanceLine2ForOrg1.PK;
			var apCashAdvanceHeaderForOrg2 = CreateCashAdvanceHeader(org2, 700m, false);
			var apCashAdvanceLine1ForOrg2 = CreateCashAdvanceLine(apCashAdvanceHeaderForOrg2, 300m);
			charge3.JR_CAL_APLine = apCashAdvanceLine1ForOrg2.PK;
			var apCashAdvanceLine2ForOrg2 = CreateCashAdvanceLine(apCashAdvanceHeaderForOrg2, 400m);
			charge4.JR_CAL_APLine = apCashAdvanceLine2ForOrg2.PK;
			Factory.Save();

			AssertEquals("AP Cash Advances should not be considered", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvances(jobNumbers));
			AssertEquals("AP Cash Advances should not be considered", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org1.PK));
			AssertEquals("AP Cash Advances should not be considered", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org2.PK));

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;
			Factory.Save();

			AssertEquals("Charges are marked as AR Cash Advance Required", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvances(jobNumbers));
			AssertEquals("Charges are marked as AR Cash Advance Required", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org1.PK));
			AssertEquals("Charges are marked as AR Cash Advance Required", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org2.PK));

			var arCashAdvanceHeaderForOrg1 = CreateCashAdvanceHeader(org1, 300m, true);
			var arCashAdvanceLine1ForOrg1 = CreateCashAdvanceLine(arCashAdvanceHeaderForOrg1, 100m);
			charge1.JR_CAL_ARLine = arCashAdvanceLine1ForOrg1.PK;
			var arCashAdvanceLine2ForOrg1 = CreateCashAdvanceLine(arCashAdvanceHeaderForOrg1, 200m);
			charge2.JR_CAL_ARLine = arCashAdvanceLine2ForOrg1.PK;
			var arCashAdvanceHeaderForOrg2 = CreateCashAdvanceHeader(org2, 700m, true);
			var arCashAdvanceLine1ForOrg2 = CreateCashAdvanceLine(arCashAdvanceHeaderForOrg2, 300m);
			charge3.JR_CAL_ARLine = arCashAdvanceLine1ForOrg2.PK;
			var arCashAdvanceLine2ForOrg2 = CreateCashAdvanceLine(arCashAdvanceHeaderForOrg2, 400m);
			charge4.JR_CAL_ARLine = arCashAdvanceLine2ForOrg2.PK;
			Factory.Save();

			AssertEquals("AR Cash Advances are in REQ status", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvances(jobNumbers));
			AssertEquals("AR Cash Advances are in REQ status", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org1.PK));
			AssertEquals("AR Cash Advances are in REQ status", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org2.PK));

			arCashAdvanceLine1ForOrg1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			arCashAdvanceLine1ForOrg1.CAL_LocalPaidAmount = arCashAdvanceLine1ForOrg1.CAL_OSPaidAmount = 100m;
			arCashAdvanceHeaderForOrg1.CAH_Status = CashAdvanceStatusCodes.RequestHeader.PartiallyPaid;
			arCashAdvanceHeaderForOrg1.CAH_LocalPaidAmount = arCashAdvanceHeaderForOrg1.CAH_OSPaidAmount = 100m;
			Factory.Save();

			AssertEquals("AR Cash Advances are in REQ and PPA status", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvances(jobNumbers));
			AssertEquals("AR Cash Advance for org1 is in PPA status", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org1.PK));
			AssertEquals("AR Cash Advance for org2 is in REQ status", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org2.PK));

			arCashAdvanceLine2ForOrg1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			arCashAdvanceLine2ForOrg1.CAL_LocalPaidAmount = arCashAdvanceLine2ForOrg1.CAL_OSPaidAmount = 200m;
			arCashAdvanceHeaderForOrg1.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
			arCashAdvanceHeaderForOrg1.CAH_LocalPaidAmount = arCashAdvanceHeaderForOrg1.CAH_OSPaidAmount = 300m;
			Factory.Save();

			AssertEquals("AR Cash Advance for org2 is in REQ status", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvances(jobNumbers));
			AssertEquals("AR Cash Advance for org1 is in PAI status", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org1.PK));
			AssertEquals("AR Cash Advance for org2 is in REQ status", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org2.PK));

			var arInvoice1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			arInvoice1.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice1.AH_TransactionType = TransactionTypes.Invoice;
			var line1 = Factory.New<AccTransactionLines>();
			line1.AL_GB = charge1.JR_GB;
			line1.AL_GE = charge1.JR_GE;
			line1.AL_LineType = TransactionLineTypes.Revenue;
			line1.AL_AH = arInvoice1.PK;
			line1.AL_AC = charge1.JR_AC;
			line1.AL_OSAmount = charge1.JR_OSSellAmt;
			line1.AL_LineAmount = charge1.JR_LocalSellAmt;
			charge1.ARLine.AL_JH = ZGuid.Empty;
			charge1.ARLine.AL_ReverseDate = ZDateTime.Today;
			charge1.JR_AL_ARLine = line1.PK;
			arCashAdvanceHeaderForOrg1.CAH_Status = CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced;
			Factory.Save();

			AssertEquals("AR Cash Advance for org2 is in REQ status", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvances(jobNumbers));
			AssertEquals("AR Cash Advance for org1 is in PIN status", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org1.PK));
			AssertEquals("AR Cash Advance for org2 is in REQ status", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org2.PK));

			var arInvoice2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			arInvoice2.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice2.AH_TransactionType = TransactionTypes.Invoice;
			var line2 = Factory.New<AccTransactionLines>();
			line2.AL_GB = charge2.JR_GB;
			line2.AL_GE = charge2.JR_GE;
			line2.AL_LineType = TransactionLineTypes.Revenue;
			line2.AL_AH = arInvoice2.PK;
			line2.AL_AC = charge2.JR_AC;
			line2.AL_OSAmount = charge2.JR_OSSellAmt;
			line2.AL_LineAmount = charge2.JR_LocalSellAmt;
			charge2.ARLine.AL_JH = ZGuid.Empty;
			charge2.ARLine.AL_ReverseDate = ZDateTime.Today;
			charge2.JR_AL_ARLine = line2.PK;
			arCashAdvanceHeaderForOrg1.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Invoiced;
			Factory.Save();

			AssertEquals("AR Cash Advance for org2 is in REQ status", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvances(jobNumbers));
			AssertEquals("AR Cash Advance for org1 is in INV status", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org1.PK));
			AssertEquals("AR Cash Advance for org2 is in REQ status", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org2.PK));

			arCashAdvanceLine1ForOrg2.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			arCashAdvanceLine1ForOrg2.CAL_LocalPaidAmount = arCashAdvanceLine1ForOrg2.CAL_OSPaidAmount = 300m;
			arCashAdvanceLine2ForOrg2.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			arCashAdvanceLine2ForOrg2.CAL_LocalPaidAmount = arCashAdvanceLine2ForOrg2.CAL_OSPaidAmount = 400m;
			arCashAdvanceHeaderForOrg2.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
			arCashAdvanceHeaderForOrg2.CAH_LocalPaidAmount = arCashAdvanceHeaderForOrg2.CAH_OSPaidAmount = 700m;
			Factory.Save();

			AssertEquals("AR Cash Advances are in INV and PAI status", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvances(jobNumbers));
			AssertEquals("AR Cash Advance for org1 is in INV status", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org1.PK));
			AssertEquals("AR Cash Advance for org2 is in PAI status", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org2.PK));

			AccCashAdvanceRequestHeader CreateCashAdvanceHeader(OrgHeader org, ZDecimal amount, bool isARCashAdvance)
			{
				var cashAdvanceHeader = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
				cashAdvanceHeader.CAH_GC_Company = GlbCompany.CurrentCompany.PK;
				cashAdvanceHeader.CAH_JH_Job = job.PK;
				cashAdvanceHeader.CAH_Ledger = isARCashAdvance ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
				cashAdvanceHeader.CAH_OH_Organization = org.PK;
				cashAdvanceHeader.CAH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
				cashAdvanceHeader.CAH_OSAmount = cashAdvanceHeader.CAH_LocalAmount = amount;
				cashAdvanceHeader.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
				return cashAdvanceHeader;
			}

			AccCashAdvanceRequestLine CreateCashAdvanceLine(AccCashAdvanceRequestHeader header, ZDecimal amount)
			{
				var cashAdvanceLine = Factory.New<AccCashAdvanceRequestLine>();
				cashAdvanceLine.CAL_GC_Company = GlbCompany.CurrentCompany.PK;
				cashAdvanceLine.CAL_CAH_RequestHeader = header.PK;
				cashAdvanceLine.CAL_LocalAmount = cashAdvanceLine.CAL_OSAmount = amount;
				cashAdvanceLine.CAL_Status = CashAdvanceStatusCodes.RequestLine.Requested;
				return cashAdvanceLine;
			}
		}

		public void TestFilterQueryOnlyLookAtCashAdvancesInCurrentCompany()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORGAA1";

			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			otherBranch.GB_GC = otherCompany.PK;

			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000011";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, otherBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job1.JH_ParentID = shipment.PK;
				job1.JH_ParentTableCode = "JS";
				var charge1 = CreateCharge(job1, 100m, org1);
				charge1.JR_IsARCashAdvance = true;
				Factory.Save();
			}

			var jobNumbers = new[] { "S00000011" };

			AssertEquals("Should not look at cash advance in other company", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvances(jobNumbers));
			AssertEquals("Should not look at cash advance in other company", false, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org1.PK));

			var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.JH_ParentID = shipment.PK;
			job2.JH_ParentTableCode = "JS";

			var charge2 = CreateCharge(job2, 100m, org1);
			charge2.JR_IsARCashAdvance = true;
			Factory.Save();

			AssertEquals("Should look at cash advance in current company", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvances(jobNumbers));
			AssertEquals("Should look at cash advance in current company", true, OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(jobNumbers, org1.PK));
		}

		public void TestHasOutstandingARCashAdvances_Chunks()
		{
			const int jobNumberCount = 110;
			var jobNumbers = Enumerable.Range(0, jobNumberCount).Select(x => x.ToString()).ToArray();

			var jobNumberQueryCount = new Dictionary<string, int>();
			using (new DisposableAction(() => OutstandingCashAdvanceFilter.RecordJobNumberInfo_ForTestOnly = RecordJobNumberInfo,
										() => OutstandingCashAdvanceFilter.RecordJobNumberInfo_ForTestOnly = null))
			{
				OutstandingCashAdvanceFilter.HasOutstandingARCashAdvances(jobNumbers);

				for (int i = 0; i < jobNumberCount; i++)
				{
					AssertEquals("Should query the same job number twice only. One for Requested/Partially Paid Cash Advances. One for Pending Cash Advances", 2, jobNumberQueryCount[i.ToString()]);
				}
			}

			void RecordJobNumberInfo(string[] jobNumbers)
			{
				foreach (var jobNumber in jobNumbers)
				{
					if (jobNumberQueryCount.ContainsKey(jobNumber))
					{
						jobNumberQueryCount[jobNumber] = jobNumberQueryCount[jobNumber] + 1;
					}
					else
					{
						jobNumberQueryCount.Add(jobNumber, 1);
					}
				}
			}
		}

		JobCharge CreateCharge(JobHeader job, ZDecimal amount, OrgHeader org)
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = chargeCode.PK;
			charge.JR_OH_SellAccount = org.PK;
			charge.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.Australia;
			charge.JR_OSSellAmt = amount;
			charge.JR_OH_CostAccount = org.PK;
			charge.JR_RX_NKCostCurrency = Core.Constants.CurrencyCodes.Australia;
			charge.JR_OSCostAmt = amount;
			return charge;
		}
	}
}
