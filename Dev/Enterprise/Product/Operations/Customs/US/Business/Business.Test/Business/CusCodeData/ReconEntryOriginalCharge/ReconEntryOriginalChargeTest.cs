using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconEntryOriginalCharge))]
	sealed class ReconEntryOriginalChargeTest : Customs.Business.Testing.CusCodeDataTest<ReconEntryOriginalCharge>
	{
		public void TestIFee()
		{
			var charge = Factory.New<ReconEntryOriginalCharge>();
			charge.CY_Code = Core.Constants.USCustoms.FeeCodes.Avocado;
			charge.CY_Amount = 34289.90m;
			charge.CY_IsOverridden = true;
			IFee fee = charge;
			AssertEquals("IFee.FeeCode", Core.Constants.USCustoms.FeeCodes.Avocado, fee.Code);
			AssertEquals("IFee.FeeAmount", 34289.90m, fee.Amount);
			AssertEquals("IFee.IsOverridden", true, fee.IsOverridden);
			fee.Delete();
			AssertEquals("IsDeleted", true, charge.IsDeleted);
		}

		public void TestSetDefaultValues()
		{
			var charge = Factory.New<ReconEntryOriginalCharge>();
			AssertEquals(CusCodeDataTypeList.Codes.ReconEntryOriginalCharge, charge.CY_Type);
		}

		public void TestCY_Amount()
		{
			var charge = Factory.New<ReconEntryOriginalCharge>();
			charge.CY_Amount = 1000.01m;
			charge.CY_ParentTableCode = "CH";
			charge.CY_ParentID = ZGuid.NewZGuid();
			AssertEquals(1000.01m, charge.CY_Amount);
			Factory.Save();
			var chargeLoaded = new BusinessObjectFactory().Load<ReconEntryOriginalCharge>(charge.PK);
			AssertEquals(1000.01m, chargeLoaded.CY_Amount);
		}

		public void TestParent()
		{
			var charge = Factory.New<ReconEntryOriginalCharge>();
			var entry = Factory.New<CusEntryHeader>();
			charge.CY_ParentID = entry.PK;
			charge.CY_ParentTableCode = "CH";
			AssertEquals(entry, charge.Parent);
			AssertNoExceptionThrown("Parent can be Entry, InvoiceLine or Aggregate Recon Declaration. For Invoice Line when charge is overidden, proxy field in the grid should be refreshed and not readonly. Refresh Binding should be called.", () => charge.CY_IsOverridden = true);
		}

		public void TestValidationAndLookups()
		{
			var charge = Factory.New<ReconEntryOriginalCharge>();
			AssertEquals(typeof(ReconEntryOriginalChargeLookups), charge.Lookups.GetType());
			AssertEquals(typeof(ReconEntryOriginalChargeValidation), charge.Validation.GetType());
		}

		public void TestDeleteWhenAmountIsEmptyForReconOriginalEntryCharge()
		{
			var charge = Factory.New<ReconEntryOriginalCharge>();
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconEntry = reconDeclaration.OriginalEntries.AddNew();
			charge.CY_ParentID = reconEntry.CH_PK;
			charge.CY_ParentTableCode = "CH";
			Factory.Save();
			AssertEquals("Should have not been deleted, even if amount is zero", false, charge.IsDeleted);
			charge = Factory.New<ReconEntryOriginalCharge>();
			charge.CY_ParentID = reconEntry.CH_PK;
			charge.CY_ParentTableCode = "CH";
			charge.CY_Amount = 0.01m;
			Factory.Save();
			AssertEquals("Should have not been deleted", false, charge.IsDeleted);
		}

		public void TestCY_Amount_ReadOnly()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDeclaration.OriginalEntries.AddNew();
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			var charge = invoiceLine.ReconOriginalCharges.AddNew();
			AssertEquals("Should not be readonly - user input allowed if no 'Calculate Orig Duty' flag", false, charge.CY_Amount_ReadOnly);
			entry.US_R_CalcOrigDuty = true;
			AssertEquals("Should be readonly - duty and charges will be calculated", true, charge.CY_Amount_ReadOnly);
			charge.CY_IsOverridden = true;
			AssertEquals("ReadOnly", false, charge.CY_Amount_ReadOnly);
			charge = Factory.New<ReconEntryOriginalCharge>();
			charge.CY_Amount = 1000.01m;
			AssertEquals("Should be readonly", true, charge.CY_Amount_ReadOnly);
			charge.CY_IsOverridden = true;
			AssertEquals("Should not be readonly", false, charge.CY_Amount_ReadOnly);
		}

		public void TestRefreshEntryWithTotalOriginalCustomsFees()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDec.OriginalEntries.AddNew();
			var invoiceLine0 = originalEntry.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine0.US_R_OrigMPFAmount = 60m;
			invoiceLine0.US_R_OrigHMFAmount = 80m;
			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 0);
			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 0);
			AssertEquals(60m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(80m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.HMF));
			invoiceLine0.US_R_OrigMPFAmount = 65m;
			AssertEquals(65m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(80m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.HMF));
			invoiceLine0.US_R_OrigHMFAmount = 90m;
			AssertEquals(65m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(90m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.HMF));
			invoiceLine0.ReconOriginalCharges[0].Delete();
			AssertEquals(0m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(90m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.HMF));
			invoiceLine0.ReconOriginalCharges[0].Delete();
			AssertEquals(0m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(0m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.HMF));
		}

		public void TestCY_Code_ReadOnly()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDeclaration.OriginalEntries.AddNew();
			var charge1 = entry.OriginalCharges.AddNew();
			entry.US_R_MonthlyFiling = true;
			charge1.CY_Code = "499";
			var charge2 = entry.OriginalCharges.AddNew();
			charge2.CY_Code = "503";
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			entry.UpdateChargesReadOnlyState();
			Assert(!charge1.ReadOnly);
			Assert("Amount of 499 should be editable", !charge1.CY_Amount_ReadOnly);
			Assert("Code of 499 should not be editable", charge1.CY_Code_ReadOnly);
			Assert("Amount of 503 should not be editable", charge2.CY_Amount_ReadOnly);
			Assert("Code of 503 should not be editable", charge2.CY_Code_ReadOnly);
			entry.US_R_NoLineDetails = true;
			Assert("editable", !charge1.CY_Amount_ReadOnly);
			Assert("editable", !charge1.CY_Code_ReadOnly);
			Assert("editable", !charge2.CY_Amount_ReadOnly);
			Assert("editable", !charge2.CY_Code_ReadOnly);
		}

		public void TestCY_SelectedRateType_ReadOnly()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(+10);
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			dutyRate.UD_TaxFeeSpecificRate = 0.89817800m;
			dutyRate.UD_TaxFeeAdvalorem = 0.87176100m;

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			var invoiceLine = originalEntry.Invoice.JobComInvoiceLines.AddNew();
			var fee = invoiceLine.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Wines);
			fee.CY_IsOverridden = true;
			AssertEquals("fee.CY_SelectedRateTypeInfo.ReadOnly", true, fee.CY_SelectedRateTypeInfo.ReadOnly);
			invoiceLine.US_R_OrigTariff = tariff.UE_Tariff;
			AssertEquals("fee.CY_SelectedRateTypeInfo.ReadOnly", false, fee.CY_SelectedRateTypeInfo.ReadOnly);
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			AssertEquals("fee.CY_SelectedRateTypeInfo.ReadOnly", true, fee.CY_SelectedRateTypeInfo.ReadOnly);
		}

		protected override IEnumerable<ReconEntryOriginalCharge> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var reconDeclaration = new ReconDeclaration(factory.New<JobDeclaration>());
			var reconEntry = reconDeclaration.OriginalEntries.AddNew();
			yield return reconEntry.OriginalCharges.AddNew();
			yield return reconDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew().ReconOriginalCharges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.NewWithValidTestData<ReconEntryOriginalCharge>();
	}
}
