using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconDutyFeeCalculationManagerTest : TestCaseWithFactory
	{
		[TestDate(2012, 10, 1)]
		public void TestWhenTotalChangedFrom25To26()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.US_PaymentDate = new ZDateTime(2012, 1, 2);
			entry.US_R_DateForMPFCalc = new ZDateTime(2012, 1, 2);
			entry.US_R_CalcOrigDuty = true;

			var line = entry.Invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 1620m;
			line.US_R_OrigCV = 1620m;

			var line2 = entry.Invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 159.37m;
			line2.US_R_OrigCV = 139.80m;
			line2.US_UC_NKCountryOfOrigin = "MX";
			line2.US_R_OrigSPI = "MX";
			line2.US_SPI = "MX";

			var line3 = entry.Invoice.JobComInvoiceLines.AddNew();
			line3.JI_LinePrice = 5885.80m;
			line3.US_R_OrigCV = 5500.00m;
			line3.US_UC_NKCountryOfOrigin = "CN";

			reconDeclaration.CalculateDutyFeesForAllEntries();

			AssertEquals("Original side is apportioned from 25. Payable amount at Recon side is calculated based on value", line.US_R_OrigMPFAmount, line.FeeCusCodes.GetFeeOrChargeAmount("499"));

			AssertEquals("MX - no MPF changes despite value changes", 0m, line2.FeeCusCodes.GetFeeOrChargeAmount("499"));
			AssertEquals("MX - no MPF changes despite value changes", 0m, line2.US_R_OrigMPFAmount);

			AssertNotEquals("MPF changes from $25 to $26 and this line should bear all the MPF change difference", line3.US_R_OrigMPFAmount, line3.FeeCusCodes.GetFeeOrChargeAmount("499"));

			AssertEquals("Total Original MPF", 24.66m, entry.Invoice.JobComInvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.US_R_OrigMPFAmount));
			AssertEquals("Total Recon MPF", 26m, entry.Invoice.JobComInvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.FeeCusCodes.GetFeeOrChargeAmount("499")));
			Assert(entry.HasMPFChanges);
		}

		public void TestCalculateInterestWhenEstimatedReconDateChanges()
		{
			DeclarationTestHelper.SetReconInterestInRegistry(new ZDate(2019, 1, 1), new ZDate(2019, 03, 31), 7m);//7%
			DeclarationTestHelper.SetReconInterestInRegistry(new ZDate(2019, 4, 1), new ZDate(2019, 07, 31), 6m);//6%

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDateTime(2019, 3, 31);

			var entry = GetEntry(reconDeclaration, 5000m, 6000m);//Has to pay for interest
			entry.US_PaymentDate = new ZDateTime(2019, 1, 1);
			entry.US_R_CalcOrigDuty = true;

			reconDeclaration.CalculateDutyFeesForChangedEntries();
			AssertEquals("PreCondition", 2.32m, entry.ReconInterest);
			Factory.Save();

			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDateTime(2019, 4, 1);
			//calculate only changed ones
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			AssertEquals(2.34m, entry.ReconInterest);
		}

		[TestDate(2009, 1, 1)]
		public void TestCalculateDairyFeeForRecon()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.US_IssueCode = ReconIssueCodeList.Codes._9802Recon;

			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ52";
			originalEntry.US_ImportDate = new ZDateTime(2007, 12, 31);
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 5);
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_SchDEntry = "1234";
			originalEntry.US_R_CalcOrigDuty = true;

			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_InvoiceNumber = "HAOA780";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99040201";
			invoiceLine.JI_Tariff = USCTariff.DairyFeeApplicable;
			invoiceLine.JI_CustomsQuantity = 1500m;
			invoiceLine.JI_LinePrice = 1000m;

			SetReconOriginalValues(invoiceLine);

			invoiceLine.ImportTariff.SetUpTestDataForDairyFeeWithXComputationCode();

			invoiceLine.JI_CustomsThirdUnitQty = "CKG";
			invoiceLine.US_R_OrigThirdUQ = "CKG";

			invoiceLine.JI_CustomsThirdQuantity = 1000m;
			invoiceLine.US_R_OrigThirdQty = 2000m;

			reconDec.CalculateDutyFeesForAllEntries();

			AssertEquals("Dairy Fee calculated for Original", 26.54m, invoiceLine.ReconOriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DairyFee));
			AssertEquals("Dairy Fee calculated for Recon", 13.27m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.DairyFee));
		}

		[TestDate(2017, 12, 1)]
		public void TestCalculatePayableMPFForRecon()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.US_IssueCode = ReconIssueCodeList.Codes._9802Recon;
			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ52";
			originalEntry.US_ImportDate = new ZDateTime(2007, 12, 31);
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 5);
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_SchDEntry = "1234";
			originalEntry.US_R_CalcOrigDuty = true;
			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_InvoiceNumber = "HAOA780";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99040201";
			invoiceLine.JI_Tariff = "0201.30.8010";
			invoiceLine.JI_CustomsQuantity = 1500m;
			invoiceLine.JI_LinePrice = 1000m;
			SetReconOriginalValues(invoiceLine);

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0201.30.8010";
			invoiceLine2.JI_CustomsQuantity = 1500m;
			invoiceLine2.JI_LinePrice = 1000m;
			SetReconOriginalValues(invoiceLine2);

			reconDec.CalculateDutyFeesForChangedEntries();

			AssertEquals(25m, originalEntry.ReconMPF);
			AssertEquals(25m, originalEntry.OriginalMPF);

			AssertEquals(3.46m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(3.46m, invoiceLine.ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(3.46m, invoiceLine2.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(3.46m, invoiceLine2.ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[ExpectNoExceptions()]
		public void TestCalculatePayableMPFForRecon_WhereNoInvoiceLines()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.US_IssueCode = ReconIssueCodeList.Codes._9802Recon;

			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ52";
			originalEntry.US_ImportDate = new ZDateTime(2007, 12, 31);
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 5);
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_SchDEntry = "1234";
			originalEntry.US_R_CalcOrigDuty = true;

			originalEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100);
			reconDec.CalculateDutyFeesForChangedEntries();
		}

		public void TestReconOriginalMPFCalculation()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.US_R_CalcOrigDuty = true;

			var invoiceLine1 = entry.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 358.99m;
			invoiceLine1.US_R_OrigCV = 358.99m;

			var invoiceLine2 = entry.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 707.08m;
			invoiceLine2.US_R_OrigCV = 707.08m;

			var invoiceLine3 = entry.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 894.54m;
			invoiceLine3.US_R_OrigCV = 894.54m;

			var invoiceLine4 = entry.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 7603.78m;
			invoiceLine4.US_R_OrigCV = 7603.78m;

			var invoiceLine5 = entry.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_LinePrice = 92.63m;
			invoiceLine5.US_R_OrigCV = 92.63m;

			var invoiceLine6 = entry.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_LinePrice = 1612.66m;
			invoiceLine6.US_R_OrigCV = 1612.66m;

			var invoiceLine7 = entry.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_LinePrice = 2460.13m;
			invoiceLine7.US_R_OrigCV = 2460.13m;

			var invoiceLine8 = entry.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine8.JI_LinePrice = 358.99m;
			invoiceLine8.US_R_OrigCV = 358.99m;

			reconDeclaration.CalculateDutyFeesForAllEntries();

			AssertEquals("As there are no changes in values, Recon MPF = Original MPF", entry.ReconMPF, entry.OriginalMPF);
		}

		public void TestCalculateChangedLinesOnlyMPFMinVal1()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.US_R_DateForMPFCalc = new ZDateTime(2019, 10, 31);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 25.67m);
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigCV = 5000m;
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 17.32m);
			var lineFee = invoiceLine.FeeCusCodes.AddNew();
			lineFee.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			lineFee.CY_FeeAmount = 17.32m;
			Factory.Save();

			entry.US_R_ChangedLinesOnly = true;
			entry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MPC, 20.78m);
			invoiceLine.US_SPI = SpecialProgramList.Codes.KR;
			reconDeclaration.CalculateDutyFeesForAllEntries();

			AssertEquals("ReconHMF", 25.67m, entry.ReconMPF);
		}

		public void TestCalculateChangedLinesOnlyMPFMinVal2()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.US_R_DateForMPFCalc = new ZDateTime(2019, 10, 31);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 25.67m);
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigCV = 5000m;
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 17.32m);
			var lineFee = invoiceLine.FeeCusCodes.AddNew();
			lineFee.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			lineFee.CY_FeeAmount = 17.32m;
			Factory.Save();

			entry.US_R_ChangedLinesOnly = true;
			entry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MPC, 17.32m);
			invoiceLine.US_SPI = SpecialProgramList.Codes.KR;
			reconDeclaration.CalculateDutyFeesForAllEntries();

			AssertEquals("ReconHMF", 0m, entry.ReconMPF);
		}

		public void TestCalculateChangedLinesOnlyMPFMaxVal1()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.US_R_DateForMPFCalc = new ZDateTime(2019, 10, 31);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 497.99m);
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigCV = 5000m;
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.US_R_OrigSPI = SpecialProgramList.Codes.KR;
			Factory.Save();

			entry.US_R_ChangedLinesOnly = true;
			entry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MPC, 502.28m);
			invoiceLine.US_SPI = "";
			reconDeclaration.CalculateDutyFeesForAllEntries();

			AssertEquals("ReconHMF", 497.99m, entry.ReconMPF);
		}

		public void TestCalculateChangedLinesOnlyMPFMaxVal2()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.US_R_DateForMPFCalc = new ZDateTime(2019, 10, 31);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 497.99m);
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigCV = 20000m;
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 69.28m);
			var lineFee = invoiceLine.FeeCusCodes.AddNew();
			lineFee.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			lineFee.CY_FeeAmount = 69.28m;
			Factory.Save();

			entry.US_R_ChangedLinesOnly = true;
			entry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MPC, 519.6m);
			invoiceLine.JI_LinePrice = 10000m;
			reconDeclaration.CalculateDutyFeesForAllEntries();

			AssertEquals("ReconHMF", 484.96m, entry.ReconMPF);
		}

		public void TestCalculateChangedLinesOnlyHMF1()
		{
			SetupTaxOrFee();
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.US_R_DateForMPFCalc = new ZDateTime(2019, 10, 31);
			entry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			entry.US_R_DutyRateDate = new ZDateTime(2019, 10, 31);
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigCV = 1000m;
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_R_OrigSPI = SpecialProgramList.Codes.KR;
			invoiceLine.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 1.25m);
			var lineFee = invoiceLine.FeeCusCodes.AddNew();
			lineFee.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			lineFee.CY_FeeAmount = 1.25m;
			Factory.Save();

			entry.US_R_ChangedLinesOnly = true;
			entry.US_R_OrigCV = 2000m;
			invoiceLine.JI_LinePrice = 1400m;
			reconDeclaration.CalculateDutyFeesForAllEntries();
			AssertEquals("ReconHMF", 0m, entry.ReconHMF);
		}

		public void TestCalculateChangedLinesOnlyHMF2()
		{
			SetupTaxOrFee();
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.US_R_DateForMPFCalc = new ZDateTime(2019, 10, 31);
			entry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			entry.US_R_DutyRateDate = new ZDateTime(2019, 10, 31);
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigCV = 1000m;
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_R_OrigSPI = SpecialProgramList.Codes.KR;
			invoiceLine.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 1.25m);
			Factory.Save();

			entry.US_R_ChangedLinesOnly = true;
			entry.US_R_OrigCV = 2000m;
			invoiceLine.JI_LinePrice = 1400m;
			invoiceLine.US_SPI = "";
			reconDeclaration.CalculateDutyFeesForAllEntries();
			AssertEquals("ReconHMF", 3m, entry.ReconHMF);
		}

		public void TestCalculateChangedLinesOnlyHMF3()
		{
			SetupTaxOrFee();
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.US_R_DateForMPFCalc = new ZDateTime(2019, 10, 31);
			entry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			entry.US_R_DutyRateDate = new ZDateTime(2019, 10, 31);
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigCV = 1000m;
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 1.25m);
			var lineFee = invoiceLine.FeeCusCodes.AddNew();
			lineFee.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			lineFee.CY_FeeAmount = 1.25m;
			Factory.Save();

			entry.US_R_ChangedLinesOnly = true;
			entry.US_R_OrigCV = 2000m;
			invoiceLine.JI_LinePrice = 1410m;
			reconDeclaration.CalculateDutyFeesForAllEntries();
			AssertEquals("ReconHMF", 3.01m, entry.ReconHMF);
		}

		public void TestCalculateChangedLinesOnlyHMF4()
		{
			SetupTaxOrFee();
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.US_R_DateForMPFCalc = new ZDateTime(2019, 10, 31);
			entry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			entry.US_R_DutyRateDate = new ZDateTime(2019, 10, 31);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 3.01m);
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigCV = 1410m;
			invoiceLine.JI_LinePrice = 1410m;
			invoiceLine.US_R_OrigSPI = SpecialProgramList.Codes.KR;
			invoiceLine.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 1.76m);
			var lineFee = invoiceLine.FeeCusCodes.AddNew();
			lineFee.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			lineFee.CY_FeeAmount = 1.76m;
			Factory.Save();

			entry.US_R_ChangedLinesOnly = true;
			entry.US_R_OrigCV = 2410m;
			invoiceLine.JI_LinePrice = 1400m;
			reconDeclaration.CalculateDutyFeesForAllEntries();
			AssertEquals("ReconHMF", 0m, entry.ReconHMF);
		}

		public void TestCalculateChangedLinesOnlyHMF5()
		{
			SetupTaxOrFee();
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.US_R_DateForMPFCalc = new ZDateTime(2019, 10, 31);
			entry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			entry.US_R_DutyRateDate = new ZDateTime(2019, 10, 31);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 3m);
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigCV = 2400m;
			invoiceLine.JI_LinePrice = 2400m;
			invoiceLine.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 8.31m);
			invoiceLine.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 3m);
			var mpfLineFee = invoiceLine.FeeCusCodes.AddNew();
			mpfLineFee.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			mpfLineFee.CY_FeeAmount = 0m;
			Factory.Save();

			entry.US_R_ChangedLinesOnly = true;
			entry.US_R_OrigCV = 2400m;
			entry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MPC, 8.31m);
			invoiceLine.US_SPI = SpecialProgramList.Codes.KR;
			reconDeclaration.CalculateDutyFeesForAllEntries();
			AssertEquals("ReconHMF", 0m, entry.ReconHMF);
		}

		public void TestCalculateAll()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_EstimatedEntryDate = new ZDateTime(2009, 3, 31);

			var entry = GetEntry(reconDeclaration, 5000m, 4000m);
			entry.US_R_CalcOrigDuty = true;
			entry.US_R_DutyRateDate = new ZDateTime(2017, 11, 17);
			var entry2 = GetEntry(reconDeclaration, 5000m, 4500m);
			entry2.US_R_CalcOrigDuty = true;
			entry2.US_R_DutyRateDate = new ZDateTime(2017, 11, 17);
			reconDeclaration.CalculateDutyFeesForChangedEntries();

			AssertEquals("PreCondition", 6.25m, entry.OriginalHMF);
			AssertEquals("PreCondition", 5m, entry.ReconHMF);

			AssertEquals("PreCondition", 6.25m, entry2.OriginalHMF);
			AssertEquals("PreCondition", 5.63m, entry2.ReconHMF);

			Factory.Save();

			entry.Invoice.JobComInvoiceLines[0].JI_LinePrice = 4200m;
			new ReconDutyFeeCalculationManager(reconDeclaration).CalculateAll();

			AssertEquals("Should have been calculated for entry", 6.25m, entry.OriginalHMF);
			AssertEquals("Should have been calculated for entry", 5.25m, entry.ReconHMF);

			AssertEquals("Stays the same before", 6.25m, entry2.OriginalHMF);
			AssertEquals("Stays the same before", 5.63m, entry2.ReconHMF);

			AssertEquals("CalculateAll is selected and causes HasChanges for both entries", true, entry.HasChanges);
			AssertEquals("CalculateAll is selected and causes HasChanges for both entries", true, entry2.HasChanges);

			entry2.US_R_NoLineDetails = true;

			new ReconDutyFeeCalculationManager(reconDeclaration).CalculateAll();
			AssertEquals("Stays the same before", 6.25m, entry2.OriginalHMF);
			AssertEquals("Stays the same before", 5.63m, entry2.ReconHMF);

			var reconDeclaration2 = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration2.US_EstimatedEntryDate = new ZDateTime(2009, 3, 31);
			reconDeclaration2.US_IsAggregate = true;
			reconDeclaration2.US_R_IsNoChangeAgg = true;

			var entry_reconDeclaration2 = GetEntry(reconDeclaration2, 5000m, 4000m);
			entry_reconDeclaration2.US_R_CalcOrigDuty = true;
			reconDeclaration2.CalculateDutyFeesForChangedEntries();

			AssertEquals("PreCondition: NO calculation should occur", 0m, entry_reconDeclaration2.OriginalHMF);
			AssertEquals("PreCondition: NO calculation should occur", 0m, entry_reconDeclaration2.ReconHMF);

			Factory.Save();

			entry_reconDeclaration2.Invoice.JobComInvoiceLines[0].JI_LinePrice = 4200m;
			new ReconDutyFeeCalculationManager(reconDeclaration2).CalculateAll();

			AssertEquals("No calculation for aggreated recon with no fees input", 0m, entry_reconDeclaration2.OriginalHMF);
			AssertEquals("No calculation for aggreated recon with no fees input", 0m, entry_reconDeclaration2.ReconHMF);
		}

		public void TestCalculateOnlyChangedEntriesWhenAddingInvoice()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_EstimatedEntryDate = new ZDateTime(2009, 3, 31);

			var entry = GetEntry(reconDeclaration, 5000m, 4000m);
			entry.US_R_CalcOrigDuty = true;
			entry.US_R_DutyRateDate = new ZDateTime(2017, 11, 17);
			var entry2 = GetEntry(reconDeclaration, 5000m, 4000m);
			entry2.US_R_CalcOrigDuty = true;
			entry2.US_R_DutyRateDate = new ZDateTime(2017, 11, 17);
			reconDeclaration.CalculateDutyFeesForChangedEntries();

			AssertEquals("PreCondition", 6.25m, entry.OriginalHMF);
			AssertEquals("PreCondition", 5m, entry.ReconHMF);

			Factory.Save();

			AddInvoice(entry, 1500m, 1000m);
			reconDeclaration.CalculateDutyFeesForChangedEntries();

			AssertEquals("Original HMF", 8.13m, entry.OriginalHMF);
			AssertEquals("Recon HMF", 6.25m, entry.ReconHMF);

			AssertEquals("No changes should have been caused by the merge", false, entry2.HasChanges);
		}

		public void TestCalculateWhenInvoiceLineChanges()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_EstimatedEntryDate = new ZDateTime(2009, 3, 31);

			var entry = GetEntry(reconDeclaration, 5000m, 4000m);
			entry.US_R_CalcOrigDuty = true;
			entry.US_R_DutyRateDate = new ZDateTime(2017, 11, 17);
			var entry2 = GetEntry(reconDeclaration, 5000m, 4500m);
			entry2.US_R_CalcOrigDuty = true;
			entry2.US_R_DutyRateDate = new ZDateTime(2017, 11, 17);
			reconDeclaration.CalculateDutyFeesForChangedEntries();

			AssertEquals("PreCondition", 6.25m, entry.OriginalHMF);
			AssertEquals("PreCondition", 5m, entry.ReconHMF);

			AssertEquals("PreCondition", 6.25m, entry2.OriginalHMF);
			AssertEquals("PreCondition", 5.63m, entry2.ReconHMF);

			Factory.Save();

			entry.Invoice.JobComInvoiceLines[0].JI_LinePrice = 4200m;
			reconDeclaration.CalculateDutyFeesForChangedEntries();

			AssertEquals("Should have been calculated for entry", 6.25m, entry.OriginalHMF);
			AssertEquals("Should have been calculated for entry", 5.25m, entry.ReconHMF);

			AssertEquals("Stays the same before", 6.25m, entry2.OriginalHMF);
			AssertEquals("Stays the same before", 5.63m, entry2.ReconHMF);
			AssertEquals("No changes should have been caused by the merge", false, entry2.HasChanges);
		}

		public void TestCalculateWhenEntryValueChanges()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_EstimatedEntryDate = new ZDateTime(2009, 3, 31);

			var entry = GetEntry(reconDeclaration, 5000m, 4000m);
			entry.US_R_CalcOrigDuty = true;
			entry.US_R_DutyRateDate = new ZDateTime(2017, 11, 17);
			var entry2 = GetEntry(reconDeclaration, 5000m, 4500m);
			entry2.US_R_CalcOrigDuty = true;
			entry2.US_R_DutyRateDate = new ZDateTime(2017, 11, 17);
			reconDeclaration.CalculateDutyFeesForChangedEntries();

			AssertEquals("PreCondition", 6.25m, entry.OriginalHMF);
			AssertEquals("PreCondition", 5m, entry.ReconHMF);

			AssertEquals("PreCondition", 6.25m, entry2.OriginalHMF);
			AssertEquals("PreCondition", 5.63m, entry2.ReconHMF);

			Factory.Save();

			entry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.No;
			reconDeclaration.CalculateDutyFeesForChangedEntries();

			AssertEquals("Should have been calculated for entry", 0m, entry.OriginalHMF);
			AssertEquals("Should have been calculated for entry", 0m, entry.ReconHMF);

			AssertEquals("Stays the same before", 6.25m, entry2.OriginalHMF);
			AssertEquals("Stays the same before", 5.63m, entry2.ReconHMF);
			AssertEquals("No changes should have been caused by the merge", false, entry2.HasChanges);
		}

		public void TestAddingLogDoesNotCauseRemerge()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_EstimatedEntryDate = new ZDateTime(2009, 3, 31);

			ReconOriginalEntryHeader entry = GetEntry(reconDeclaration, 5000m, 4000m);
			ReconOriginalEntryHeader entry2 = GetEntry(reconDeclaration, 5000m, 4500m);
			reconDeclaration.CalculateDutyFeesForChangedEntries();

			Factory.Save();

			entry.Logs.AddNew();
			entry2.Logs.AddNew();
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			AssertEquals("No changes should have been caused by the merge", false, entry.ReconCharges.HasChanges);
			AssertEquals("No changes should have been caused by the merge", false, entry.OriginalCharges.HasChanges);
			AssertEquals("No changes should have been caused by the merge", false, entry2.ReconCharges.HasChanges);
			AssertEquals("No changes should have been caused by the merge", false, entry2.OriginalCharges.HasChanges);
		}

		public void TestDoNotCalculateOriginalFees()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_EstimatedEntryDate = new ZDateTime(2009, 3, 31);

			var entry = GetEntry(reconDeclaration, 5000m, 4000m);
			entry.US_R_DutyRateDate = new ZDateTime(2017, 11, 17);
			var entry2 = GetEntry(reconDeclaration, 5000m, 4500m);
			entry2.US_R_DutyRateDate = new ZDateTime(2017, 11, 17);
			reconDeclaration.CalculateDutyFeesForChangedEntries();

			AssertEquals("PreCondition - should be zero", 0m, entry.OriginalHMF);
			AssertEquals("PreCondition", 5m, entry.ReconHMF);

			AssertEquals("PreCondition - should be zero", 0m, entry2.OriginalHMF);
			AssertEquals("PreCondition", 5.63m, entry2.ReconHMF);

			Factory.Save();

			entry.Invoice.JobComInvoiceLines[0].JI_LinePrice = 4200m;
			new ReconDutyFeeCalculationManager(reconDeclaration).CalculateAll();

			AssertEquals("Original HMF should have not been calculated", 0m, entry.OriginalHMF);
			AssertEquals("Should have been calculated for entry", 5.25m, entry.ReconHMF);

			AssertEquals("Original HMF should have not been calculated", 0m, entry2.OriginalHMF);
			AssertEquals("Stays the same before", 5.63m, entry2.ReconHMF);

			entry2.US_R_NoLineDetails = true;
			new ReconDutyFeeCalculationManager(reconDeclaration).CalculateAll();
			AssertEquals("Stays the same before", 0m, entry2.OriginalHMF);
			AssertEquals("Stays the same before", 5.63m, entry2.ReconHMF);

			entry.Invoice.JobComInvoiceLines[0].ReconOriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.HMF, 10m);
			new ReconDutyFeeCalculationManager(reconDeclaration).CalculateAll();
			AssertEquals("Stays the same before", 10m, entry.OriginalHMF);
			AssertEquals("Stays the same before", 5.25m, entry.ReconHMF);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
		}

		void SetReconOriginalValues(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_R_OrigTariff = invoiceLine.JI_Tariff;
			invoiceLine.US_R_OrigCV = invoiceLine.JI_CustomsValue >= invoiceLine.US_98GoodsValue ? (ZDecimal)(invoiceLine.JI_CustomsValue - invoiceLine.US_98GoodsValue) : invoiceLine.JI_CustomsValue;
			invoiceLine.US_R_OrigFirstQty = invoiceLine.JI_CustomsQuantity;
			invoiceLine.US_R_OrigSPI = invoiceLine.US_SPI;
			invoiceLine.US_R_OrigSecondQty = invoiceLine.JI_CustomsSecondQuantity;

			invoiceLine.US_R_OrigSupTariff = invoiceLine.US_SupTariff;
			invoiceLine.US_R_Orig98Value = invoiceLine.US_98GoodsValue;
			invoiceLine.US_R_OrigSupQty1 = invoiceLine.US_SupQty1;
			invoiceLine.US_R_OrigSupQty2 = invoiceLine.US_SupQty2;
			invoiceLine.US_R_OrigSupQty3 = invoiceLine.US_SupQty3;
		}

		void SetupTaxOrFee()
		{
			var testHelper = new UniversalReferenceTestDataHelper(Factory);
			testHelper.CreateRefCusTaxOrFeeType("AVL", "ad valorem");
			testHelper.CreateTaxOrFee("501", 0.1250, "US", 3.00, 0, "AVL", new ZDateTime("1995-01-01"), new ZDateTime("2079-06-06"), "Harbor Maintenance Fee");
		}

		//Changes will be in Customs Value and HMF will change as a result
		ReconOriginalEntryHeader GetEntry(ReconDeclaration reconDeclaration, ZDecimal originalCV, ZDecimal reconCV)
		{
			ReconOriginalEntryHeader result = reconDeclaration.OriginalEntries.AddNew();
			result.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			AddInvoice(result, originalCV, reconCV);

			return result;
		}

		JobComInvoiceHeader AddInvoice(ReconOriginalEntryHeader entry, ZDecimal originalCV, ZDecimal reconCV)
		{
			JobComInvoiceHeader result = entry.Invoice;
			result.JZ_InvoiceAmount = reconCV;
			result.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = result.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "5804.30.0010";
			invoiceLine.JI_CustomsQuantity = 5656.00000m;

			invoiceLine.US_R_OrigTariff = "5804.30.0010";
			invoiceLine.US_R_OrigFirstQty = 5656.00000m;

			invoiceLine.JI_LinePrice = reconCV;
			invoiceLine.US_R_OrigCV = originalCV;

			return result;
		}
	}
}
