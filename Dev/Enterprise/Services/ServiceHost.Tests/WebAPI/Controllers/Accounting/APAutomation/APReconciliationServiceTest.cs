using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;

namespace Enterprise.Services.ServiceHost.Tests
{
	class APReconciliationServiceTest : TestCaseWithFactory
	{
		public void TestGetChargeAccrualsForSelectedJobsAndExcludeNonSelectedJobs()
		{
			var (firstSelectedJobParentPK, firstJobExpectedCharges) = CreateJobWithExpectedChargesAndReturnThem("S1", "C001");
			var (secondSelectedJobParentPK, secondJobExpectedCharges) = CreateJobWithExpectedChargesAndReturnThem("S3", "C003");
			var (nonSelectedJobParentPK, _) = CreateJobWithExpectedChargesAndReturnThem("S2", "C002");

			var jobParentPKs = new[] { firstSelectedJobParentPK, secondSelectedJobParentPK };
			var jobParentsInfo = jobParentPKs
				.Select(pk => new JobParentInfo { ParentId = pk.ToGuid(), ParentTableCode = "JS" })
				.ToList();
			var companyPK = Env.CurrentCompany.PK;
			var chargeAccruals = service.GetChargeAccruals(jobParentsInfo, companyPK);

			var firstSelectedJobActualCharges = chargeAccruals.Single(ca => ca.JobParentId == firstSelectedJobParentPK);
			var secondSelectedJobActualCharges = chargeAccruals.Single(ca => ca.JobParentId == secondSelectedJobParentPK);

			AssertAccrualCharges(firstSelectedJobActualCharges, firstJobExpectedCharges);
			AssertAccrualCharges(secondSelectedJobActualCharges, secondJobExpectedCharges);
			AssertEquals(expected: false, chargeAccruals.Any(ca => ca.JobParentId == nonSelectedJobParentPK));
		}

		public void TestGetConsolCostsForSelectedJobsAndExcludeNonSelectedJobs()
		{
			var firstSelectedConsolJobExpectedCharges = CreateConsolCosts("C001");
			var secondSelectedConsolJobExpectedCharges = CreateConsolCosts("C002");
			var nonSelectedConsolJobCharges = CreateConsolCosts("C003");

			var consolJobParentPKs = new[] { firstSelectedConsolJobExpectedCharges[0].E6_ParentID, secondSelectedConsolJobExpectedCharges[0].E6_ParentID };
			var jobParentsInfo = consolJobParentPKs
				.Select(pk => new JobParentInfo { ParentId = pk.ToGuid(), ParentTableCode = "JK" })
				.ToList();
			var companyPK = Env.CurrentCompany.PK;
			var consolCosts = service.GetConsolCostAccruals(jobParentsInfo, companyPK);

			var firstSelectedConsolJobActualCharges = consolCosts.Single(ca => ca.JobParentId == consolJobParentPKs[0]);
			var secondSelectedConsolJobActualCharges = consolCosts.Single(ca => ca.JobParentId == consolJobParentPKs[1]);

			AssertAccrualCosts(firstSelectedConsolJobActualCharges, firstSelectedConsolJobExpectedCharges);
			AssertAccrualCosts(secondSelectedConsolJobActualCharges, secondSelectedConsolJobExpectedCharges);
			AssertEquals(expected: false, consolCosts.Any(ca => ca.JobParentId == nonSelectedConsolJobCharges[0].E6_ParentID));
		}

		(ZGuid jobHeaderParentPK, IEnumerable<Charge> expectedCharges) CreateJobWithExpectedChargesAndReturnThem(string shipmentNum, string consolNum)
		{
			var shipment = testObjectCreator.CreateShipment(shipmentNum);
			var job = testObjectCreator.CreateJob(shipment);

			var expectedCharges = GenerateAndReturnExpectedJobCharges(job, consolNum);
			Factory.Save();
			return (job.JH_ParentID, expectedCharges);
		}

		Charge[] GenerateAndReturnExpectedJobCharges(Job parentJob, string consolNum)
		{
			var emptyLineCharge = testObjectCreator.CreateCharge(parentJob, testObjectCreator.FRT, "charge 01", costCurrency: testObjectCreator.AUD, osCostAmt: 400M, creditor: testObjectCreator.Creditor1);
			var accrualLineCharge = testObjectCreator.CreateChargeAndAssociatedAccruals(parentJob.PK, 0, testObjectCreator.FRT.AC_Code).Item2;
			var wipLineCharge = testObjectCreator.CreateChargeAndAssociatedWIPs(parentJob.PK, 0, testObjectCreator.FRT.AC_Code).Item2;
			var revLineCharge = testObjectCreator.CreateChargeAndPostSellSideOnly(parentJob.PK, 0, testObjectCreator.FRT.AC_Code).Item2;
			var costLineCharge = testObjectCreator.CreateChargeAndPostCostSideOnly(parentJob.PK, 0, testObjectCreator.FRT.AC_Code).Item2;
			var chargeApportioned = testObjectCreator.CreateCharge(parentJob, testObjectCreator.FRT, "charge 02", costCurrency: testObjectCreator.AUD, osCostAmt: 400M, creditor: testObjectCreator.Creditor1);
			chargeApportioned.JR_E6 = GetConsolCostPK(consolNum);
			return [emptyLineCharge, accrualLineCharge, wipLineCharge, revLineCharge];
		}

		ZGuid GetConsolCostPK(string consolNum)
		{
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", consolNum);
			var consolCost = consol.GetApportionments().CostsCollection.TryAddNew();
			consolCost.E6_OH_Creditor = testObjectCreator.Creditor1.PK;
			consolCost.E6_AC_ChargeCode = testObjectCreator.FRT.PK;
			consolCost.E6_OSCostAmount = 400m;
			return consolCost.PK;
		}

		void AssertAccrualCharges(AccrualsForAPReconciliation<Charge> actualChargeAccruals, IEnumerable<Charge> expectedCharges)
		{
			AssertNotNull(actualChargeAccruals);
			AssertEquals("JS", actualChargeAccruals.JobParentTableCode);
			AssertEquals(expectedCharges.Count(), actualChargeAccruals.Accruals.Count());
			AssertContainsExactElementsInAnyOrder(new ChargeComparer(), expectedCharges, actualChargeAccruals.Accruals);
		}

		JobConsolCost[] CreateConsolCosts(string consolNum)
		{
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", consolNum);
			consol.Shipments.AddNew();
			var consolCost1 = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1, testObjectCreator.AUD, 1.0M, 100.0M, testObjectCreator.Creditor1);
			var consolCost2 = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC2, testObjectCreator.SAR, 2.0M, 200.0M, testObjectCreator.Creditor2);

			Factory.Save();
			return [consolCost1, consolCost2];
		}

		void AssertAccrualCosts(AccrualsForAPReconciliation<JobConsolCost> actualConsolCosts, IEnumerable<JobConsolCost> expectedConsolCosts)
		{
			AssertNotNull(actualConsolCosts);
			AssertEquals("JK", actualConsolCosts.JobParentTableCode);
			AssertEquals(expectedConsolCosts.Count(), actualConsolCosts.Accruals.Count());
			AssertContainsExactElementsInAnyOrder(new JobConsolCostComparer(), expectedConsolCosts, actualConsolCosts.Accruals);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testObjectCreator = new TestObjectCreator(Factory);
			service = new APReconciliationService();
		}

		APReconciliationService service;
		TestObjectCreator testObjectCreator;

		public class ChargeComparer : IEqualityComparer<Charge>
		{
			public bool Equals(Charge x, Charge y)
			{
				return x != null && y != null &&
					   x.JR_GB == y.JR_GB &&
					   x.JR_AC == y.JR_AC &&
					   x.JR_Desc == y.JR_Desc &&
					   x.JR_OH_CostAccount == y.JR_OH_CostAccount &&
					   x.JR_RX_NKCostCurrency == y.JR_RX_NKCostCurrency &&
					   x.JR_OSCostAmt == y.JR_OSCostAmt &&
					   x.JR_APInvoiceNum == y.JR_APInvoiceNum &&
					   x.JR_APInvoiceDate == y.JR_APInvoiceDate &&
					   x.JR_PaymentDate == y.JR_PaymentDate &&
					   x.JR_OH_SellAccount == y.JR_OH_SellAccount &&
					   x.JR_RX_NKSellCurrency == y.JR_RX_NKSellCurrency &&
					   x.JR_OSSellAmt == y.JR_OSSellAmt &&
					   x.JR_GE == y.JR_GE &&
					   x.JR_OSCostAmt == y.JR_OSCostAmt &&
					   x.JR_OSSellExRate == y.JR_OSSellExRate &&
					   x.JR_LocalSellAmt == y.JR_LocalSellAmt &&
					   x.JR_PaymentType == y.JR_PaymentType &&
					   x.JR_AB == y.JR_AB;
			}

			public int GetHashCode(Charge obj) => throw new NotImplementedException();
		}

		public class JobConsolCostComparer : IEqualityComparer<JobConsolCost>
		{
			public bool Equals(JobConsolCost x, JobConsolCost y)
			{
				return x != null && y != null &&
					x.E6_ParentID == y.E6_ParentID &&
					x.E6_ParentTableCode == y.E6_ParentTableCode &&
					x.ChargeCode?.AC_Code == y.ChargeCode?.AC_Code &&
					x.ChargeCodeDescription == y.ChargeCodeDescription &&
					x.E6_RX_NKCurrency == y.E6_RX_NKCurrency &&
					x.E6_OSCostAmount == y.E6_OSCostAmount &&
					x.E6_OSGSTAmount == y.E6_OSGSTAmount &&
					x.E6_GC == y.E6_GC &&
					x.E6_ExchangeRate == y.E6_ExchangeRate &&
					x.Company?.LocalCurrency?.Code == y.Company?.LocalCurrency?.Code &&
					x.E6_LocalCostAmount == y.E6_LocalCostAmount &&
					x.Creditor?.OH_Code == y.Creditor?.OH_Code &&
					x.TaxRate?.AT_Code == y.TaxRate?.AT_Code &&
					x.E6_OSGSTAmount_Calc == y.E6_OSGSTAmount_Calc;
			}

			public int GetHashCode(JobConsolCost obj) => throw new NotImplementedException();
		}
	}
}
