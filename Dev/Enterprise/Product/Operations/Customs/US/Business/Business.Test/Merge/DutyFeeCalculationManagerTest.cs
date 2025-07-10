using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DutyFeeCalculationManagerTest : TestCaseWithFactory
	{
		[TestDate(2016, 7, 15)]
		public void TestCottonFeeForCottonFeeCancelThresholdDate()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6206900040"; // Cotton
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsSecondQuantity = 1000m;
			AssertEquals(2.06m, new CottonFeeCalculator().CalculateFee(invoiceLine).Amount);

			invoiceLine.JI_CustomsSecondQuantity = 500m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(1.03m, invoiceLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("no Cotton on entry line level as the job is ACS job.", 0m, invoiceLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
		}

		public void TestCottonFeeDeminimusForACSAndACE()
		{
			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			declaration.US_EntryFilerCode = "XJ5";

			var originalEntry = declaration.OriginalEntries.AddNew();
			DecorateEntryForCottoDeminimus(originalEntry, "6206900040");// Cotton
			originalEntry.US_R_MsgMode = JobApplicationCodeList.Codes.ACE;
			originalEntry.US_R_CalcOrigDuty = true;

			var originalEntry2 = declaration.OriginalEntries.AddNew();
			DecorateEntryForCottoDeminimus(originalEntry2, "6206900040");// Cotton
			originalEntry2.US_R_MsgMode = JobApplicationCodeList.Codes.ACS;
			originalEntry2.US_R_CalcOrigDuty = true;

			declaration.CalculateDutyFeesForChangedEntries();

			AssertEquals("Cotton DeMinimus rule exists in ACE if original has cotton fee", 1.03m, originalEntry.OriginalCotton);
			AssertEquals("Cotton DeMinimus rule exists in ACE if original has cotton fee", 1.03m, originalEntry.ReconCotton);

			AssertEquals("Cotton DeMinimus rule exists in ACS", 0m, originalEntry2.OriginalCotton);
			AssertEquals("Cotton DeMinimus rule exists in ACS", 0m, originalEntry2.ReconCotton);

			var invoiceLine = originalEntry.Invoice.JobComInvoiceLines[0];
			AssertEquals("Cott amount for ACE", 1.03m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertEquals("Cott amount for ACE", 1.03m, invoiceLine.ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton));

			invoiceLine = originalEntry2.Invoice.JobComInvoiceLines[0];
			AssertEquals("Cotton Fee is cleared in invoice line too for ACS", 0m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertEquals("Cotton Fee is cleared in invoice line too for ACS", 0m, invoiceLine.ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton));

			var originalEntry3 = declaration.OriginalEntries.AddNew();
			originalEntry3.US_R_MsgMode = JobApplicationCodeList.Codes.ACE;
			originalEntry3.US_R_CalcOrigDuty = true;
			DecorateEntryForCottoDeminimus(originalEntry3, "2204216000"); // Not Cotton
			var line3 = originalEntry3.Invoice.JobComInvoiceLines[0];
			var cottonFee = line3.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.Cotton);
			cottonFee.CY_IsOverridden = true;
			cottonFee.CY_Data = "1.25";
			DecorateEntryForCottoDeminimus(originalEntry3, "2204216000"); // Not Cotton
			invoiceLine = originalEntry3.Invoice.JobComInvoiceLines[0];
			Assert(invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton) > 0);
		}

		void DecorateEntryForCottoDeminimus(ReconOriginalEntryHeader originalEntry, string tariff)
		{
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 2000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff;
			invoiceLine.JI_CustomsQuantity = 500m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsSecondQuantity = 500m;

			invoiceLine.US_R_OrigCV = invoiceLine.JI_LinePrice;
			invoiceLine.US_R_OrigTariff = invoiceLine.JI_Tariff;
			invoiceLine.US_R_OrigSPI = invoiceLine.US_SPI;
			invoiceLine.US_R_OrigFirstQty = 500m;
			invoiceLine.US_R_OrigSecondQty = 500m;
		}

		[TestDate(2009, 6, 1)]
		public void TestMPFLessThanZeroPointZeroOne()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 1.74m;

			invoiceLine2.AddSecondaryInvoiceLine().JI_LinePrice = 0.50m;
			invoiceLine2.AddSecondaryInvoiceLine().JI_LinePrice = 0.50m;
			invoiceLine2.AddSecondaryInvoiceLine().JI_LinePrice = 0.15m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(31.5m, declaration.ActiveEntryHeaders.EntrySummaryEntry.MPFAmountForEntry);
			AssertEquals(0m, invoiceLine2.CusEntryLine.MPFAmount);
			AssertEquals(true, invoiceLine2.CusEntryLine.US_HasMPF);
		}

		[TestDate(2017, 12, 1)]
		public void TestMPFMinMaxRemovedForConsolidatedMonthlyFiling()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.US_MonthlyFiling = true;
			declaration.US_PayableMPF = 12400m;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10159311;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Monthly filing has a 400$ cap per day and brokers enter the total given by importers", 12400m, declaration.ActiveEntryHeaders.EntrySummaryEntry.MPFAmountForEntry);

			declaration.US_MonthlyFiling = false;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("No longer monthly, so it should be governed by a normal max amount", 485m, declaration.ActiveEntryHeaders.EntrySummaryEntry.MPFAmountForEntry);
		}

		public void TestMPFSurchargeWhenNotCertified()
		{
			var entryFiler = new EntryFiler();
			entryFiler.EntryFilerCode = "SV9";
			entryFiler.IsABICertified = false;
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, entryFiler);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(new FeeCalculationHelper(Factory, entry.DateForMPFCalculation).ManualSurchargeAmount, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge));

			entryFiler = new EntryFiler();
			entryFiler.EntryFilerCode = "SV9";
			entryFiler.IsABICertified = true;
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, entryFiler);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge));
		}

		[TestDate(2017, 12, 1)]
		public void TestMPFLessThanZeroPointZeroOne2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 0.38m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(25m, declaration.ActiveEntryHeaders.EntrySummaryEntry.MPFAmountForEntry);
			AssertEquals(true, invoiceLine.CusEntryLine.US_HasMPF);
			AssertEquals(25m, invoiceLine.US_PayableMPF);
		}

		public void TestDontDeclareDutyValueWith9999Point99()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "6104622006";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now;
			tariff.UE_SPICode = "MA";

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_AdValoremSpecialRate = 9999.99990000m;//this actually exists in the reference file
			dutyRate.UD_ISOCountryCode = "MA";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 251.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104622006";
			invoiceLine.JI_LinePrice = 250m;
			invoiceLine.US_UC_NKCountryOfOrigin = "MA";
			invoiceLine.US_SPI = "MA";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals(ZDecimal.Zero, invoiceLine.CusEntryLine.CL_DutyPercent);
		}

		[TestDate(2017, 12, 1)]
		public void TestAdjustHMFWhenThereIsNoOtherFee()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 251.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1211.90.9180";
			invoiceLine.JI_LinePrice = 250m;

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "4823.20.9000";
			invoiceLine2.JI_LinePrice = 1m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			IFees feeAndCharges = entry.Charges;
			AssertEquals("Total Fee includes HMF less than 3.00$", 25.31m, feeAndCharges.GetGrandTotalFee());

			invoiceLine.US_SPI = "AU";
			invoiceLine2.US_SPI = "AU";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			feeAndCharges = entry.Charges;
			AssertEquals("MPF exempt due to SPI country AU and HMF is less than 3.00$ (becomes exempt)", 0m, feeAndCharges.GetGrandTotalFee());
		}

		[TestDate(2009, 2, 1)]
		public void TestForWI00022024NotAdjustHMFWhenThereIsNoOtherFeeButDuty()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_DestinationState = "NJ";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 250.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2001.10.0000";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_LinePrice = 250m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			IFees feeAndCharges = entry.Charges;
			AssertNotEquals("Has Duty", ZDecimal.Zero, entry.TotalDutyAmount);
			AssertEquals("Has No Tax", ZDecimal.Zero, entry.TotalEstimatedTax);
			AssertEquals("Total HMF less than $3.00", 0.31m, feeAndCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));

			invoiceLine.US_SPI = "AU";
			entry.MergedLines[0].Fees.RemoveAndDeleteAll();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			feeAndCharges = entry.Charges;
			AssertEquals("Has No Duty", ZDecimal.Zero, entry.TotalDutyAmount);
			AssertEquals("Has No Tax", ZDecimal.Zero, entry.TotalEstimatedTax);
			AssertEquals("Total HMF less than $3.00 should be zero if there is no duty or tax", 0m, feeAndCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));

			invoiceLine.JI_Tariff = "2204.21.2000";
			invoiceLine.JI_CustomsQuantity = 1500m;
			invoiceLine.US_UC_NKCountryOfOrigin = "XA";
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			invoiceLine.US_SPI = "CA";
			FeeCusCodeData wineFee = invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.Wines);
			wineFee.CY_IsOverridden = true;
			wineFee.CY_FeeAmount = 10m;
			entry.MergedLines[0].Fees.RemoveAndDeleteAll();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			feeAndCharges = entry.Charges;
			AssertEquals("Has No Duty", ZDecimal.Zero, entry.TotalDutyAmount);
			AssertNotEquals("Has Tax", ZDecimal.Zero, entry.TotalEstimatedTax);
			AssertEquals("Total HMF less than $3.00", 0.31m, feeAndCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));
		}

		[TestDate(2009, 6, 1)]
		public void TestZero62RecordForMPFShouldBeSentIfMPFIsPayableForEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 0m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.JI_Tariff = "0804.40.0010";
			invoiceLine.JI_CustomsQuantity = 5000m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 0m;
			invoiceLine2.JI_Tariff = "0804.40.0010";

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 0m;
			invoiceLine3.JI_Tariff = "0804.40.0010";
			invoiceLine3.US_SPI = "AU";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Assert(invoiceLine.CusEntryLine.US_HasMPF);
			Assert(invoiceLine2.CusEntryLine.US_HasMPF);
			Assert(!invoiceLine3.CusEntryLine.US_HasMPF);
			AssertEquals("MPF still applies", 25m, declaration.ActiveEntryHeaders.EntrySummaryEntry.MPFAmountForEntry);
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
			invoice.JZ_InvoiceAmount = 0m;
			invoice.JZ_InvoiceNumber = "HAOA780";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99040201";
			invoiceLine.JI_Tariff = "0201.30.8010";
			invoiceLine.JI_CustomsQuantity = 1500m;
			invoiceLine.JI_LinePrice = 0m;
			SetReconOriginalValues(invoiceLine);

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0201.30.8010";
			invoiceLine2.JI_CustomsQuantity = 1500m;
			invoiceLine2.JI_LinePrice = 0m;
			SetReconOriginalValues(invoiceLine2);

			reconDec.CalculateDutyFeesForChangedEntries();

			AssertEquals(25m, originalEntry.ReconMPF);
			AssertEquals(25m, originalEntry.OriginalMPF);

			Assert(invoiceLine.US_HasMPF);
			Assert(invoiceLine.US_R_OrigHasMPF);
			Assert(invoiceLine2.US_HasMPF);
			Assert(invoiceLine2.US_R_OrigHasMPF);
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

		public void TestInformalFeeCalculation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "01010000";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_LinePrice = 1000m;
			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();
			CusEntryHeaderCharges charge = declaration.CustomsEntryHeaders[0].Charges[Core.Constants.USCustoms.FeeCodes.MerchandiseInformal];
			AssertEquals(new FeeCalculationHelper(Factory, declaration.DateForFeeCalculation).InformalFeeAmount, charge.C1_ChargeAmount);
		}

		//Null Exception happened during duty calculation
		public void TestForWI00008478()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802008014";
			AssertNull(invoiceLine.ImportSupTariff);
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_LinePrice = 1000m;

			AssertNoExceptionThrown(() => declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
		}

		[TestDate(2017, 12, 1)]
		public void TestCalculateReconMPFWithMonthlyFiling()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.US_MonthlyFiling = true;
			declaration.US_PayableMPF = 30000m;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10159311;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 10159311;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.US_IssueCode = ReconIssueCodeList.Codes._9802Recon;

			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ500000014";
			originalEntry.US_ImportDate = new ZDateTime(2007, 12, 31);
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 5);
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_SchDEntry = "1234";
			originalEntry.US_R_MonthlyFiling = true;

			new ReconImportEntryRetriever(reconDec).ImportLines();
			reconDec.CalculateDutyFeesForChangedEntries();

			AssertEquals(1, reconDec.Invoices.Count);
			AssertEquals(2, reconDec.Invoices[0].InvoiceLines.Count);

			AssertEquals(30000m, originalEntry.ReconMPF);
			AssertEquals(30000m, originalEntry.OriginalMPF);
			AssertEquals(35191.85m, reconDec.Invoices[0].InvoiceLines[0].FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(35191.85m, reconDec.Invoices[0].InvoiceLines[0].ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(35191.85m, reconDec.Invoices[0].InvoiceLines[1].FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(35191.85m, reconDec.Invoices[0].InvoiceLines[1].ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2021, 7, 13)]
		public void TestSetMPFMinimumIfThereIsAtLeastOneEntryLineSubjectToMPF()
		{
			var currentMPFRate = new FeeCalculationHelper(Factory, ZDateTime.Today).GetCurrentRate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());

			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;

			var invoice = reconDec.Invoices.AddNew();
			invoice.US_CH_ReconEntry = originalEntry.CH_PK;

			var originalEntryLine1 = invoice.JobComInvoiceLines.AddNew();
			originalEntryLine1.JI_Tariff = "7326908587";
			originalEntryLine1.JI_LinePrice = 1000;
			reconDec.CalculateDutyFeesForAllEntries();
			AssertEquals("Only one data and it is not overriden. Should be the minimum", currentMPFRate.ZZF_Minimum, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			var mpf1 = originalEntryLine1.FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			mpf1.CY_IsOverridden = true;
			reconDec.CalculateDutyFeesForAllEntries();
			AssertEquals("The data is overriden but the value is not 0. Should be the minimum", currentMPFRate.ZZF_Minimum, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			mpf1.CY_FeeAmount = 0;
			reconDec.CalculateDutyFeesForAllEntries();
			AssertEquals("The only one data is overriden and the value is 0. Should be 0", 0m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			var originalEntryLine2 = reconDec.FilteredInvoiceLines.AddNew();
			originalEntryLine2.JI_Tariff = "7326908587";
			originalEntryLine2.JI_LinePrice = 1000;
			reconDec.CalculateDutyFeesForAllEntries();
			AssertEquals("The first data is overriden to 0, the sencond data is not overriden. Should be the minimum", currentMPFRate.ZZF_Minimum, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			var mpf2 = originalEntryLine2.FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			mpf2.CY_IsOverridden = true;
			reconDec.CalculateDutyFeesForAllEntries();
			AssertEquals("The first data is overriden to 0, the sencond data is overriden but the value is not 0. Should be the minimum", currentMPFRate.ZZF_Minimum, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			mpf2.CY_FeeAmount = 0;
			reconDec.CalculateDutyFeesForAllEntries();
			AssertEquals("Both of them are overriden to 0. Should be 0", 0m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestDoNotSetHasMPFOnEntryLineWhenMPFIsOverridden()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8512300030";
			var mpfCharge = invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			mpfCharge.CY_IsOverridden = true;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entryLine = invoiceLine.CusEntryLine;
			AssertEquals("US_HasMPF should be false becasue override is ticked", false, entryLine.US_HasMPF);

			mpfCharge.Delete();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			entryLine = invoiceLine.CusEntryLine;
			AssertEquals("US_HasMPF should be true", true, entryLine.US_HasMPF);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
		}
	}
}
