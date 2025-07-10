using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconDutyDataLineHeaderTest : TestCaseWithFactory
	{
		public void TestHMFDeminimusForACSAndACE()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			declaration.US_EntryFilerCode = "XJ5";

			var originalEntry = declaration.OriginalEntries.AddNew();
			DecorateEntryForHMFDeminimus(originalEntry);
			originalEntry.US_R_MsgMode = JobApplicationCodeList.Codes.ACE;
			originalEntry.US_R_CalcOrigDuty = true;

			var originalEntry2 = declaration.OriginalEntries.AddNew();
			DecorateEntryForHMFDeminimus(originalEntry2);
			originalEntry2.US_R_MsgMode = JobApplicationCodeList.Codes.ACS;
			originalEntry2.US_R_CalcOrigDuty = true;

			declaration.CalculateDutyFeesForChangedEntries();

			AssertEquals("HMF DeMinimus rule exists in ACE", 0m, originalEntry.OriginalHMF);
			AssertEquals("HMF DeMinimus rule exists in ACE", 0m, originalEntry.ReconHMF);

			AssertEquals("HMF DeMinimus rule exists in ACS", 0m, originalEntry2.OriginalHMF);
			AssertEquals("HMF DeMinimus rule exists in ACS", 0m, originalEntry2.ReconHMF);

			var invoiceLine = originalEntry.Invoice.JobComInvoiceLines[0];
			AssertEquals("HMF amount is cleared in invoice line too for ACE", 0m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));
			AssertEquals("HMF amount is cleared in invoice line too for ACE", 0m, invoiceLine.ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));

			invoiceLine = originalEntry2.Invoice.JobComInvoiceLines[0];
			AssertEquals("HMF amount for ACS", 2.5m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));
			AssertEquals("HMF amount for ACS", 2.5m, invoiceLine.ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));
		}

		public void TestOverridenTotalMPFPayable()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			entry.Invoice.InvoiceLines.AddNew();
			entry.US_R_MonthlyFiling = true;
			entry.OriginalCharges.AddNew("499", 6.35m);
			entry.ReconCharges.AddNew("499", 7.23m);

			var reconOriginalDutyDataLineHeader = new ReconOriginalDutyDataLineHeader(entry);
			var reconCurrentDutyDataLineHeader = new ReconCurrentDutyDataLineHeader(entry);

			AssertEquals(6.35m, ((IDutyDataLineHeader)reconOriginalDutyDataLineHeader).OverridenTotalMPFPayable);
			Assert(reconOriginalDutyDataLineHeader.IsFeeUserEntered(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(7.23m, ((IDutyDataLineHeader)reconCurrentDutyDataLineHeader).OverridenTotalMPFPayable);
			Assert(reconCurrentDutyDataLineHeader.IsFeeUserEntered(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestOnCalculating()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MPC, 6.35m);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 35.2m);

			var reconCurrentDutyDataLineHeader = new ReconCurrentDutyDataLineHeader(entry);
			reconCurrentDutyDataLineHeader.OnCalculating();
			AssertEquals(0, entry.ReconCharges.Count);

			entry.US_R_ChangedLinesOnly = true;
			reconCurrentDutyDataLineHeader.OnCalculating();
			AssertEquals(1, entry.ReconCharges.Count);
			AssertEquals(6.35m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		void DecorateEntryForHMFDeminimus(ReconOriginalEntryHeader originalEntry)
		{
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 2000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0101.10.0010";//duty free
			invoiceLine.JI_LinePrice = 2000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "AU";

			invoiceLine.US_R_OrigCV = invoiceLine.JI_LinePrice;
			invoiceLine.US_R_OrigTariff = invoiceLine.JI_Tariff;
			invoiceLine.US_R_OrigSPI = invoiceLine.US_SPI;
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
