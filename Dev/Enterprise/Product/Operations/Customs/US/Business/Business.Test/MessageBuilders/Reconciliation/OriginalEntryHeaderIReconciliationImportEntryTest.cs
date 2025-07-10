using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class OriginalEntryHeaderIReconciliationImportEntryTest : TestCaseWithFactory
	{
		public void TestImportEntryFilerCodeNumber()
		{
			OriginalEntry.CH_OrigEntryReference = "XJ5234790423";
			AssertEquals("XJ5234790423", IOriginalEntry.ImportEntryFilerCodeNumber);
		}

		public void TestPort()
		{
			OriginalEntry.US_SchDEntry = "8888";
			AssertEquals("8888", IOriginalEntry.Port);
		}

		public void TestEstimatedReconCharges()
		{
			OriginalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, 10m);
			AssertEquals(10m, IOriginalEntry.EstimatedReconciliationInterest);

			OriginalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.OtherExcise, 20m);
			OriginalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Wines, 5m);
			AssertEquals(25m, IOriginalEntry.EstimatedReconciliationTax);

			OriginalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 30m);
			AssertEquals(30m, IOriginalEntry.EstimatedReconciliationDuty);
		}

		public void TestOriginalCharges()
		{
			OriginalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.OtherExcise, 20m);
			OriginalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 30m);

			AssertEquals("Duty", 30m, IOriginalEntry.OriginalDuty);
			AssertEquals("Excise", 20m, IOriginalEntry.OriginalTax);
		}

		public void TestFees()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.AccountingClassFeeCode, RefCusCodeListTypes.Codes.AccountingClassFeeCode);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				Core.Constants.USCustoms.FeeCodes.Avocado, "Hass Avocado Fee", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, "Merchandise Processing Fee", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			OriginalEntry.ReconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			OriginalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 1m);
			OriginalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 15m);

			OriginalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 1m);
			OriginalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Avocado, 10m);
			OriginalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Wines, 20m);

			OriginalEntry.RefundedFees.AddNewIfNotExists(Core.Constants.USCustoms.FeeCodes.Mushroom);

			List<IReconciliationImportEntryFee> fees = new List<IReconciliationImportEntryFee>(IOriginalEntry.Fees);
			AssertEquals("Three fees", 3, fees.Count);

			bool hasSeenMPF = false, hasSeenAvocadoFee = false, hasSeenMushroom = false, hasSeenWines = false;

			foreach (IReconciliationImportEntryFee fee in fees)
			{
				if (fee.FeeClass == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing)
				{
					hasSeenMPF = true;
					AssertEquals(15m, fee.OriginalFee);
					AssertEquals(0m, fee.EstimatedReconciliationFee);
				}
				else if (fee.FeeClass == Core.Constants.USCustoms.FeeCodes.Avocado)
				{
					hasSeenAvocadoFee = true;

					AssertEquals(0m, fee.OriginalFee);
					AssertEquals(10m, fee.EstimatedReconciliationFee);
				}
				else if (fee.FeeClass == Core.Constants.USCustoms.FeeCodes.Mushroom)
				{
					hasSeenMushroom = true;
					AssertEquals(0m, fee.OriginalFee);
					AssertEquals(0m, fee.EstimatedReconciliationFee);
				}
				else if (fee.FeeClass == Core.Constants.USCustoms.FeeCodes.Wines)
				{
					hasSeenWines = true;
				}
			}

			Assert(hasSeenMPF);
			Assert(hasSeenAvocadoFee);
			Assert(hasSeenMushroom);
			Assert(!hasSeenWines);
		}

		public void TestACEFees()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.AccountingClassFeeCode, RefCusCodeListTypes.Codes.AccountingClassFeeCode);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				Core.Constants.USCustoms.FeeCodes.Avocado, "Hass Avocado Fee", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, "Merchandise Processing Fee", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				Core.Constants.USCustoms.FeeCodes.Beef, "Beef Fee", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				Core.Constants.USCustoms.FeeCodes.Wines, "Wine Excise Tax", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				ReconDeclaration.Constants.InterestAccountingClassCode, "InterestAccountingClassCode", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			OriginalEntry.ReconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			OriginalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 1m);
			OriginalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 15m);

			OriginalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 1m);
			OriginalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Avocado, 10m);
			OriginalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Wines, 20m);
			OriginalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, 200m);

			OriginalEntry.RefundedFees.AddNewIfNotExists(Core.Constants.USCustoms.FeeCodes.Mushroom);
			var invoiceLine = OriginalEntry.Invoice.InvoiceLines.AddNew();
			invoiceLine.ReconRefundedFees.AddNew(Core.Constants.USCustoms.FeeCodes.Beef);

			var invoiceLine2 = OriginalEntry.Invoice.InvoiceLines.AddNew();
			invoiceLine2.ReconRefundedFees.AddNew(Core.Constants.USCustoms.FeeCodes.Beef);

			List<IReconciliationImportEntryFee> fees = new List<IReconciliationImportEntryFee>(IOriginalEntry.Fees);
			AssertEquals("Six fees", 6, fees.Count);
			AssertEquals(1, (fees.FindAll(x => x.FeeClass == Core.Constants.USCustoms.FeeCodes.Beef)).Count);

			bool hasSeenMPF = false, hasSeenAvocadoFee = false, hasSeenMushroom = false, hasSeenWines = false, hasInterest = false, hasBeef = false;

			foreach (IReconciliationImportEntryFee fee in fees)
			{
				if (fee.FeeClass == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing)
				{
					hasSeenMPF = true;
					AssertEquals(15m, fee.OriginalFee);
					AssertEquals(0m, fee.EstimatedReconciliationFee);
				}
				else if (fee.FeeClass == Core.Constants.USCustoms.FeeCodes.Avocado)
				{
					hasSeenAvocadoFee = true;

					AssertEquals(0m, fee.OriginalFee);
					AssertEquals(10m, fee.EstimatedReconciliationFee);
				}
				else if (fee.FeeClass == Core.Constants.USCustoms.FeeCodes.Mushroom)
				{
					hasSeenMushroom = true;
					AssertEquals(0m, fee.OriginalFee);
					AssertEquals(0m, fee.EstimatedReconciliationFee);
				}
				else if (fee.FeeClass == Core.Constants.USCustoms.FeeCodes.Beef)
				{
					hasBeef = true;
					AssertEquals(0m, fee.OriginalFee);
					AssertEquals(0m, fee.EstimatedReconciliationFee);
				}
				else if (fee.FeeClass == Core.Constants.USCustoms.FeeCodes.Wines)
				{
					hasSeenWines = true;
				}
				else if (fee.FeeClass == ReconDeclaration.Constants.InterestAccountingClassCode)
				{
					hasInterest = true;
					AssertEquals(0m, fee.OriginalFee);
					AssertEquals(200m, fee.EstimatedReconciliationFee);
				}
			}

			Assert(hasSeenMPF);
			Assert(hasSeenAvocadoFee);
			Assert(!hasSeenMushroom);
			Assert(hasSeenWines);
			Assert(hasInterest);
			Assert(hasBeef);
		}

		public void TestGetTariffRequiredFeeCode()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "6205202067";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeClassCode = "056";
			Factory.Save();

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			var invoiceLine = originalEntry.Invoice.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigTariff = "6205202067";
			invoiceLine.US_R_OrigCV = 15m;
			invoiceLine.JI_LinePrice = 16m;
			invoiceLine.US_R_OrigCottonFeeExempt = YesNoDefaultList.Codes.No;

			new ReconChangedLinesMerger(reconDec).DoMerge(false);
			IReconciliationImportEntry originalEntryHeader = new OriginalEntryHeaderIReconciliationImportEntry(originalEntry);
			var cottonFee = originalEntryHeader.Fees.FirstOrDefault(x => x.FeeClass == "056");
			AssertNotNull(cottonFee);
			AssertEquals(0m, cottonFee.OriginalFee);
			AssertEquals(0m, cottonFee.EstimatedReconciliationFee);
		}

		ReconOriginalEntryHeader originalEntry;
		ReconOriginalEntryHeader OriginalEntry => originalEntry ?? (originalEntry = new ReconDeclaration(Factory.New<JobDeclaration>()).OriginalEntries.AddNew());

		IReconciliationImportEntry iOriginalEntry;
		IReconciliationImportEntry IOriginalEntry => iOriginalEntry ?? (iOriginalEntry = new OriginalEntryHeaderIReconciliationImportEntry(OriginalEntry));
	}
}
