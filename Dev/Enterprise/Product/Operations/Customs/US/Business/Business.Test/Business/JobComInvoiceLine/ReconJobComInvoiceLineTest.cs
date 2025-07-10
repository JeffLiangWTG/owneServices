using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconJobComInvoiceLineTest : TestCaseWithFactory
	{
		public void TestThereIsNoExceptionWhenTheChargesIsDeleted()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;
			entry.US_R_CalcOrigDuty = true;

			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.US_R_OverrideOriginHMF = true;
			invoiceLine.US_R_OrigHMFAmount = 10;
			invoiceLine.US_R_OverrideOriginMPF = true;
			invoiceLine.US_R_OrigMPFAmount = 10;
			invoiceLine.US_R_OverrideOrigOtherFeeAmount = true;
			invoiceLine.US_R_OrigOtherFeeCode = "056";
			invoiceLine.US_R_OrigOtherFeeAmount = 10;
			invoiceLine.US_R_OverrideReconHMF = true;
			invoiceLine.US_R_ReconHMFAmount = 10;
			invoiceLine.US_R_OverrideReconMPF = true;
			invoiceLine.US_R_ReconMPFAmount = 10;

			invoiceLine.ReconOriginalCharges.RemoveAndDeleteAll();
			invoiceLine.FeeCusCodes.RemoveAndDeleteAll();
			AssertNoExceptionThrown(() =>
			{
				invoiceLine.US_R_OverrideOriginHMF = false;
				invoiceLine.US_R_OverrideOriginMPF = false;
				invoiceLine.US_R_OverrideOrigOtherFeeAmount = false;
				invoiceLine.US_R_OrigOtherFeeCode = "056";
				invoiceLine.US_R_OverrideReconHMF = false;
				invoiceLine.US_R_OverrideReconMPF = false;
			});
		}

		public void TestUS_R_ReconHMFAmount_ReadOnlyAndUS_R_OrigOtherFeeAmount_ReadOnly()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_EntryFilerCode = "XJ5";

			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;
			entry.US_R_CalcOrigDuty = true;
			entry.Invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			Assert(invoiceLine.US_R_ReconHMFAmountInfo.ReadOnly);
			Assert(invoiceLine.US_R_OrigOtherFeeAmountInfo.ReadOnly);
			invoiceLine.US_R_OverrideReconHMF = true;
			invoiceLine.US_R_OverrideOrigOtherFeeAmount = true;
			Assert(!invoiceLine.US_R_ReconHMFAmountInfo.ReadOnly);
			Assert(!invoiceLine.US_R_OrigOtherFeeAmountInfo.ReadOnly);
		}

		public void TestUS_R_OrigTaxQty_ReadOnlyAndUS_R_OrigTaxRate_ReadOnly()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_EntryFilerCode = "XJ5";

			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;
			entry.US_R_CalcOrigDuty = true;
			entry.Invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_R_OrigTaxRateS = AppendixBTaxRateList.Codes.Specify;
			Assert("Tax Rate Value should not be readonly", !invoiceLine.US_R_OrigTaxRateInfo.ReadOnly);
			Assert("Tax Qty should be readonly", invoiceLine.US_R_OrigTaxQtyInfo.ReadOnly);
			invoiceLine.US_R_OrigTaxRateS = AppendixBTaxRateList.Codes.Other_1;
			Assert("Tax Rate Value should be readonly", invoiceLine.US_R_OrigTaxRateInfo.ReadOnly);
			Assert("Tax Qty should not be readonly", !invoiceLine.US_R_OrigTaxQtyInfo.ReadOnly);
		}

		public void TestUS_R_OrigOverrideSupDuty()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigOverrideSupDuty = true;
			AssertEquals(true, invoiceLine.US_OverrideSupDuty);
			invoiceLine.US_R_OrigOverrideSupDuty = false;
			AssertEquals(true, invoiceLine.US_OverrideSupDuty);
		}

		public void TestUS_R_OrigSupDuty()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigOverrideSupDuty = true;
			invoiceLine.US_R_OrigSupDuty = 10m;
			AssertEquals(10m, invoiceLine.US_SupDuty);
			invoiceLine.US_R_OrigSupDuty = 15m;
			AssertEquals(10m, invoiceLine.US_SupDuty);
			invoiceLine.US_OverrideSupDuty = false;
			invoiceLine.US_SupDuty = ZDecimal.Zero;
			invoiceLine.US_R_OrigSupDuty = 10m;
			AssertEquals(ZDecimal.Zero, invoiceLine.US_SupDuty);
		}

		public void TestUS_R_Orig98Value()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_Orig98Value = 10m;
			AssertEquals(10m, invoiceLine.US_98GoodsValue);
			invoiceLine.US_R_Orig98Value = 15m;
			AssertEquals(10m, invoiceLine.US_98GoodsValue);
		}

		public void TesgUS_R_OrigOverrideDuty()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigOverrideDuty = true;
			AssertEquals(true, invoiceLine.US_OverrideDuty);
			invoiceLine.US_R_OrigOverrideDuty = false;
			AssertEquals(true, invoiceLine.US_OverrideDuty);
		}

		public void TestUS_R_OrigDuty()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigOverrideDuty = true;
			invoiceLine.US_OverrideDuty = true;
			invoiceLine.US_R_OrigDuty = 10m;
			AssertEquals(10m, invoiceLine.US_Duty);
			invoiceLine.US_R_OrigDuty = 15m;
			AssertEquals(10m, invoiceLine.US_Duty);
			invoiceLine.US_OverrideDuty = false;
			invoiceLine.US_Duty = ZDecimal.Zero;
			invoiceLine.US_R_OrigDuty = 10m;
			AssertEquals(ZDecimal.Zero, invoiceLine.US_Duty);
		}

		public void TestRefreshEntryWithTotalOriginalDuty()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDec.OriginalEntries.AddNew();
			var invoiceLine0 = originalEntry.Invoice.JobComInvoiceLines.AddNew();
			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 0);

			invoiceLine0.US_R_OrigDuty = 60m;
			AssertEquals(60m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.Duty));

			invoiceLine0.US_R_OrigDuty = 100m;
			AssertEquals(100m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.Duty));
		}

		public void TestUS_R_OrigCottonFeeExempt()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigCottonFeeExempt = YesNoDefaultList.Codes.Yes;
			AssertEquals(YesNoDefaultList.Codes.Yes, invoiceLine.US_CottonFeeExempt);
			invoiceLine.US_R_OrigCottonFeeExempt = YesNoDefaultList.Codes.No;
			AssertEquals(YesNoDefaultList.Codes.Yes, invoiceLine.US_CottonFeeExempt);
		}

		public void TestUS_R_OrigCV()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigCV = 10m;
			AssertEquals(10m, invoiceLine.JI_LinePrice);
			invoiceLine.US_R_OrigCV = 15m;
			AssertEquals(10m, invoiceLine.JI_LinePrice);
		}

		public void TestUS_R_OrigTaxApply()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigTaxApply = YesNoDefaultList.Codes.Yes;
			AssertEquals(YesNoDefaultList.Codes.Yes, invoiceLine.US_TaxApply);
			invoiceLine.US_R_OrigTaxQty = 100m;

			invoiceLine.US_R_OrigTaxApply = YesNoDefaultList.Codes.No;
			AssertEquals(YesNoDefaultList.Codes.Yes, invoiceLine.US_TaxApply);
			AssertEquals(0m, invoiceLine.US_R_OrigTaxQty);
		}

		public void TestUS_R_OrigTaxCode()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigTaxCode = "O";
			invoiceLine.US_R_OrigTaxQty = 100m;
			AssertEquals("O", invoiceLine.US_TaxCode);

			invoiceLine.US_R_OrigTaxCode = "M";
			AssertEquals("O", invoiceLine.US_TaxCode);
			AssertEquals(0m, invoiceLine.US_R_OrigTaxQty);
		}

		public void TestUS_R_OrigTaxRateT()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "20230106";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;
			var invoice = entry.Invoice;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigTariff = "20230106";
			AssertEquals(TaxApplyList.Codes.Yes, invoiceLine.US_R_OrigTaxApply);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Wines, invoiceLine.US_R_OrigTaxCode);
			invoiceLine.US_R_OrigTaxRateT = "O";
			AssertEquals("O", invoiceLine.US_TaxRateT);
			invoiceLine.US_R_OrigTaxRateT = "M";
			AssertEquals("O", invoiceLine.US_TaxRateT);
			invoiceLine.US_TaxRateT = ZString.Empty;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_R_OrigTaxRateT = "O";
			AssertEquals(ZString.Empty, invoiceLine.US_TaxRateT);
		}

		public void TestUS_R_OrigTaxRateS()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			var invoice = entry.Invoice;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_R_OrigTaxCode = "016";
			invoiceLine.US_R_OrigTaxRateS = AppendixBTaxRateList.Codes.Wines_1;
			Assert(invoiceLine.IsOrigTaxRateQuantityRequired);
			AssertEquals("it should be copied to the corresponding recon field", AppendixBTaxRateList.Codes.Wines_1, invoiceLine.US_TaxRateS);

			invoiceLine.US_R_OrigTaxQty = 100m;
			invoiceLine.US_R_OrigFirstUQ = "L";
			invoiceLine.US_R_OrigTaxRateS = AppendixBTaxRateList.Codes.Other_4;
			Assert(!invoiceLine.IsOrigTaxRateQuantityRequired);
			AssertEquals(0m, invoiceLine.US_R_OrigTaxQty);
		}

		public void TestUS_R_OrigTaxRate()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigTaxRate = 10m;
			AssertEquals(10m, invoiceLine.US_TaxRate);
			invoiceLine.US_R_OrigTaxRate = 15m;
			AssertEquals(10m, invoiceLine.US_TaxRate);
		}

		public void TestUS_R_OrigTaxQty()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			var invoice = entry.Invoice;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_R_OrigTaxCode = "016";
			invoiceLine.US_R_OrigTaxRateS = AppendixBTaxRateList.Codes.Wines_1;

			AssertEquals("PreCondition:Copied from Orig", TaxApplyList.Codes.Override, invoiceLine.US_TaxApply);
			AssertEquals("PreCondition:Copied from Orig", "016", invoiceLine.US_TaxCode);
			AssertEquals("PreCondition:Copied from Orig", AppendixBTaxRateList.Codes.Wines_1, invoiceLine.US_TaxRateS);

			invoiceLine.US_R_OrigTaxQty = 456m;
			AssertEquals("PreCondition:Copied from Orig", 456m, invoiceLine.US_TaxQty);
		}

		public void TestUS_R_OrigSPI()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigSPI = "O";
			AssertEquals("O", invoiceLine.US_SPI);
			invoiceLine.US_R_OrigSPI = "M";
			AssertEquals("O", invoiceLine.US_SPI);
		}

		public void TestUS_R_OrigSupQty1()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigSupQty1 = 10m;
			AssertEquals(10m, invoiceLine.US_SupQty1);
			invoiceLine.US_R_OrigSupQty1 = 15m;
			AssertEquals(10m, invoiceLine.US_SupQty1);
		}

		public void TestUS_R_OrigSupUQ1()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigSupUQ1 = "O";
			AssertEquals("O", invoiceLine.US_SupUQ1);
			invoiceLine.US_R_OrigSupUQ1 = "M";
			AssertEquals("O", invoiceLine.US_SupUQ1);
		}

		public void TestUS_R_OrigSupQty2()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigSupQty2 = 20m;
			AssertEquals(20m, invoiceLine.US_SupQty2);
			invoiceLine.US_R_OrigSupQty2 = 25m;
			AssertEquals(20m, invoiceLine.US_SupQty2);
		}

		public void TestUS_R_OrigSupUQ2()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigSupUQ2 = "O";
			AssertEquals("O", invoiceLine.US_SupUQ2);
			invoiceLine.US_R_OrigSupUQ2 = "M";
			AssertEquals("O", invoiceLine.US_SupUQ2);
		}

		public void TestUS_R_OrigSupQty3()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigSupQty3 = 30m;
			AssertEquals(30m, invoiceLine.US_SupQty3);
			invoiceLine.US_R_OrigSupQty3 = 35m;
			AssertEquals(30m, invoiceLine.US_SupQty3);
		}

		public void TestUS_R_OrigSupUQ3()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigSupUQ3 = "O";
			AssertEquals("O", invoiceLine.US_SupUQ3);
			invoiceLine.US_R_OrigSupUQ3 = "M";
			AssertEquals("O", invoiceLine.US_SupUQ3);
		}

		[TestDate(2008, 1, 1)]
		public void TestUS_R_OrigSupTariff()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.OriginalSupTariffFormatted = "9901.00.53";

			AssertEquals("99010053", invoiceLine.US_R_OrigSupTariff);
			AssertEquals("99010053", invoiceLine.US_SupTariff);

			Assert(invoiceLine.ReconOrigSupTariffIsCandidateForUpdateRequest);
			Assert(invoiceLine.ReconOrigSupTariffMarkedForReferenceFileRequest);

			invoiceLine.US_R_OrigSupTariff = "";
			AssertEquals("", invoiceLine.US_R_OrigSupTariff);
			AssertEquals("", invoiceLine.US_R_OrigSupUQ1);
			Assert(!invoiceLine.ReconOrigSupTariffIsCandidateForUpdateRequest);
			AssertEquals("99010053", invoiceLine.US_SupTariff);
		}

		public void TestUS_R_OrigSupTariffForSecondaryInvoiceLine()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconOriginalEntry = reconDec.OriginalEntries.AddNew();
			reconOriginalEntry.US_R_DutyRateDate = ZDateTime.Today;
			var invoice = reconOriginalEntry.Invoice;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.OriginalSupTariffFormatted = "9901.00.53";

			AssertEquals("99010053", invoiceLine.US_R_OrigSupTariff);
			AssertEquals("99010053", invoiceLine.US_SupTariff);

			var childLine = invoiceLine.AddSecondaryInvoiceLine();
			AssertEquals("Sup tariffs defaulted from parent line", "99010053", childLine.US_R_OrigSupTariff);
			AssertEquals("Sup tariffs defaulted from parent line", "99010053", childLine.US_SupTariff);

			childLine.US_R_OrigSupTariff = "99020054";
			childLine.US_SupTariff = "99020054";
			AssertEquals("99020054", childLine.US_R_OrigSupTariff);
			AssertEquals("99020054", childLine.US_SupTariff);

			invoiceLine.US_R_OrigSupTariff = "";
			invoiceLine.US_SupTariff = "";
			AssertEquals("Sup tariffs defaulted from parent line", "", childLine.US_R_OrigSupTariff);
			AssertEquals("Sup tariffs defaulted from parent line", "", childLine.US_SupTariff);
		}

		public void TestUS_R_OrigSupQuantitiesReadOnly()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = reconDec.InvoiceLines.AddNew();

			Assert(invoiceLine.US_R_OrigFirstQtyInfo.ReadOnly);
			Assert(invoiceLine.US_R_OrigSecondQtyInfo.ReadOnly);
			Assert(invoiceLine.US_R_OrigThirdQtyInfo.ReadOnly);

			invoiceLine.US_R_OrigFirstUQ = "L";
			invoiceLine.US_R_OrigSecondUQ = "KG";
			invoiceLine.US_R_OrigThirdUQ = "M3";

			Assert(!invoiceLine.US_R_OrigFirstQtyInfo.ReadOnly);
			Assert(!invoiceLine.US_R_OrigSecondQtyInfo.ReadOnly);
			Assert(!invoiceLine.US_R_OrigThirdQtyInfo.ReadOnly);

			Assert(invoiceLine.US_R_OrigSupUQ1Info.ReadOnly);
			Assert(invoiceLine.US_R_OrigSupUQ2Info.ReadOnly);
			Assert(invoiceLine.US_R_OrigSupUQ3Info.ReadOnly);

			Assert(invoiceLine.US_R_OrigSupQty1Info.ReadOnly);
			Assert(invoiceLine.US_R_OrigSupQty2Info.ReadOnly);
			Assert(invoiceLine.US_R_OrigSupQty3Info.ReadOnly);

			invoiceLine.US_R_OrigSupUQ1 = "L";
			invoiceLine.US_R_OrigSupUQ2 = "KG";
			invoiceLine.US_R_OrigSupUQ3 = "M3";

			Assert(!invoiceLine.US_R_OrigSupQty1Info.ReadOnly);
			Assert(!invoiceLine.US_R_OrigSupQty2Info.ReadOnly);
			Assert(!invoiceLine.US_R_OrigSupQty3Info.ReadOnly);
		}

		[TestDate(2004, 12, 30)]
		public void TestUS_R_OrigSupQuantities()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;

			var reconInvoice = reconDec.Invoices.AddNew();
			reconInvoice.US_CH_ReconEntry = originalEntry.CH_PK;

			var invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigFirstUQ = "L";
			invoiceLine.US_R_OrigSecondUQ = "KG";
			invoiceLine.US_R_OrigThirdUQ = "M3";

			invoiceLine.US_R_OrigFirstQty = 100m;
			invoiceLine.US_R_OrigSecondQty = 110m;
			invoiceLine.US_R_OrigThirdQty = 120m;

			invoiceLine.US_R_OrigSupTariff = "99020328";
			invoiceLine.US_R_OrigSupUQ1 = "L";
			invoiceLine.US_R_OrigSupUQ2 = "KG";
			invoiceLine.US_R_OrigSupUQ3 = "M3";

			AssertEquals(100m, invoiceLine.US_R_OrigSupQty1);
			AssertEquals(110m, invoiceLine.US_R_OrigSupQty2);
			AssertEquals(120m, invoiceLine.US_R_OrigSupQty3);

			invoiceLine.US_R_OrigSupQty1 = 1000m;
			invoiceLine.US_R_OrigSupQty2 = 1100m;
			invoiceLine.US_R_OrigSupQty3 = 1200m;

			AssertEquals(1000m, invoiceLine.US_R_OrigSupQty1);
			AssertEquals(1100m, invoiceLine.US_R_OrigSupQty2);
			AssertEquals(1200m, invoiceLine.US_R_OrigSupQty3);

			invoiceLine.US_R_OrigSupQty1 = 100m;
			invoiceLine.US_R_OrigSupQty2 = 110m;
			invoiceLine.US_R_OrigSupQty3 = 120m;

			AssertEquals(100m, invoiceLine.US_R_OrigSupQty1);
			AssertEquals(110m, invoiceLine.US_R_OrigSupQty2);
			AssertEquals(120m, invoiceLine.US_R_OrigSupQty3);

			invoiceLine.US_R_OrigFirstQty = 101m;
			invoiceLine.US_R_OrigSecondQty = 111m;
			invoiceLine.US_R_OrigThirdQty = 121m;

			AssertEquals(101m, invoiceLine.US_R_OrigSupQty1);
			AssertEquals(111m, invoiceLine.US_R_OrigSupQty2);
			AssertEquals(121m, invoiceLine.US_R_OrigSupQty3);

			invoiceLine.US_R_OrigSupUQ1 = "LL";
			invoiceLine.US_R_OrigSupUQ2 = "KK";
			invoiceLine.US_R_OrigSupUQ3 = "MM";

			AssertEquals(0m, invoiceLine.US_R_OrigSupQty1);
			AssertEquals(0m, invoiceLine.US_R_OrigSupQty2);
			AssertEquals(0m, invoiceLine.US_R_OrigSupQty3);
		}

		public void TestIReconOriginalChargeParent()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "01010101";
			tariff.UE_DateFrom = new ZDateTime(2009, 9, 1);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "01010101";

			AssertEquals("01010101", ((IReconOriginalChargeParent)invoiceLine).Tariff.UE_Tariff);
			AssertEquals(invoiceLine, ((IReconOriginalChargeParent)invoiceLine).ParentAsBusinessObject);
			Assert("MonthlyFiling", !((IReconOriginalChargeParent)invoiceLine).MonthlyFiling);
		}

		public void TestUS_CH_ReconEntry()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			JobComInvoiceHeader invoice = reconDec.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(ZGuid.Empty, invoiceLine.US_CH_ReconEntry);

			ReconOriginalEntryHeader originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5978453";
			invoice.US_CH_ReconEntry = originalEntry.PK;
			AssertEquals(originalEntry.PK, invoiceLine.US_CH_ReconEntry);
		}

		public void TestUS_R_OrigTariff()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			JobComInvoiceHeader invoice = originalEntry.Invoice;
			JobComInvoiceLine invoiceLine = reconDec.InvoiceLines.AddNew();

			invoiceLine.US_R_OrigTariff = "2123123112";
			AssertEquals("2123123112", invoiceLine.JI_Tariff);
			AssertEquals("", invoiceLine.US_R_OrigFirstUQ);
			AssertEquals("", invoiceLine.US_R_OrigSecondUQ);
			AssertEquals("", invoiceLine.US_R_OrigThirdUQ);

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567890";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_Unit1 = "X";
			tariff.UE_Unit2 = "Y";
			tariff.UE_Unit3 = "KG";
			invoiceLine.US_R_OrigTariff = tariff.UE_Tariff;
			AssertEquals("2123123112", invoiceLine.JI_Tariff);
			AssertEquals("X", invoiceLine.US_R_OrigFirstUQ);
			AssertEquals("Y", invoiceLine.US_R_OrigSecondUQ);
			AssertEquals("KG", invoiceLine.US_R_OrigThirdUQ);
		}

		public void TestRateTypeReadonly()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "01010101";
			tariff.UE_DateFrom = new ZDateTime(2008, 9, 11);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSpecific;

			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			JobComInvoiceLine invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigTariff = tariff.UE_Tariff;
			AssertEquals(false, invoiceLine.US_R_OrigRateType_ReadOnly);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			AssertEquals(true, invoiceLine.US_R_OrigRateType_ReadOnly);
		}

		public void TestLinePriceDecimals()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			JobComInvoiceLine invoiceLine = reconDec.InvoiceLines.AddNew();

			AssertEquals(2, invoiceLine.LinePriceDecimals);
		}

		public void TestOriginalTariffFormatted()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();

			invoiceLine.US_R_OrigTariff = "1234567890";
			AssertEquals("Formatted Original Tariff", "1234.56.7890", invoiceLine.OriginalTariffFormatted);
		}

		public void TestOriginalImportTariff()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "00000000";
			tariff1.UE_DateFrom = new ZDateTime(2008, 1, 1);
			tariff1.UE_DateTo = new ZDateTime(2008, 6, 30);

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "00000000";
			tariff2.UE_DateFrom = new ZDateTime(2008, 7, 1);
			tariff2.UE_DateTo = new ZDateTime(2099, 12, 31);

			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 1);

			JobComInvoiceHeader invoice = originalEntry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.US_R_OrigTariff = "00000000";
			AssertEquals("Import tariff with original duty rate date", tariff1, invoiceLine.OriginalImportTariff);
			AssertEquals("Import Tariff", tariff1, invoiceLine.ImportTariff);
		}

		public void TestUpdateTaxRelatedFieldsWhenTariffChanges()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;

			SetUpTariffsForTaxRelatedFields();

			JobComInvoiceLine invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigTariff = "00000000";

			AssertEquals(TaxApplyList.Codes.Yes, invoiceLine.US_R_OrigTaxApply);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Wines, invoiceLine.US_R_OrigTaxCode);
			AssertEquals("50c/KG", invoiceLine.US_R_OrigTaxRateS);
			AssertEquals(ZString.Empty, invoiceLine.US_R_OrigTaxRateT);

			invoiceLine.US_R_OrigTariff = "00000001";
			AssertEquals(TaxApplyList.Codes.Override, invoiceLine.US_R_OrigTaxApply);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.OtherExcise, invoiceLine.US_R_OrigTaxCode);
			AssertEquals(ZString.Empty, invoiceLine.US_R_OrigTaxRateS);
			AssertEquals(ZString.Empty, invoiceLine.US_R_OrigTaxRateT);
		}

		public void TestWhenTaxApplyChanges()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;

			SetUpTariffsForTaxRelatedFields();

			JobComInvoiceLine invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.US_R_OrigTariff = "00000000";

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;

			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
			invoiceLine.US_R_OrigTaxRateS = AppendixBTaxRateList.Codes.Specify;

			invoiceLine.US_TaxRate = 0.45m;
			invoiceLine.US_R_OrigTaxRate = 0.45m;

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Yes;

			AssertEquals("When tax is back to default(Y), it should clear out rate string fields and return a normal tax rate", "50c/KG", invoiceLine.US_TaxRateS);
			AssertEquals("When tax is back to default(Y), it should clear out rate string fields and return a normal tax rate", "50c/KG", invoiceLine.US_R_OrigTaxRateS);

			AssertEquals(0.5m, invoiceLine.US_TaxRate);
			AssertEquals(0.5m, invoiceLine.US_R_OrigTaxRate);
		}

		public void TestWhenTaxCodeChanges()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;

			SetUpTariffsForTaxRelatedFields();

			var invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000002";
			invoiceLine.US_R_OrigTariff = "00000002";

			AssertEquals("PreCondition", ZString.Empty, invoiceLine.US_TaxApply);
			AssertEquals("PreCondition", ZString.Empty, invoiceLine.US_R_OrigTaxApply);
			AssertEquals("PreCondition", ZString.Empty, invoiceLine.US_TaxCode);
			AssertEquals("PreCondition", ZString.Empty, invoiceLine.US_R_OrigTaxCode);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Yes;

			AssertEquals("PreCondition", Core.Constants.USCustoms.FeeCodes.DistilledSpirits, invoiceLine.US_TaxCode);
			AssertEquals("PreCondition", Core.Constants.USCustoms.FeeCodes.DistilledSpirits, invoiceLine.US_R_OrigTaxCode);

			invoiceLine.US_TaxRateT = RateTypeList.Codes.Secondary;
			invoiceLine.US_R_OrigTaxRateT = RateTypeList.Codes.Primary;

			invoiceLine.US_TaxRateS = "$0.8/PFL";
			invoiceLine.US_R_OrigTaxRateS = "$0.7/PFL";

			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			invoiceLine.US_R_OrigTaxCode = Core.Constants.USCustoms.FeeCodes.Wines;

			AssertEquals(ZString.Empty, invoiceLine.US_TaxRateS);
			AssertEquals(ZString.Empty, invoiceLine.US_R_OrigTaxRateS);

			AssertEquals(ZString.Empty, invoiceLine.US_TaxRateT);
			AssertEquals(ZString.Empty, invoiceLine.US_R_OrigTaxRateT);
		}

		public void TestTaxRateTypeAndTaxRateStringReadOnly()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;

			SetUpTariffsForTaxRelatedFields();

			JobComInvoiceLine invoiceLine = reconDec.InvoiceLines.AddNew();

			Assert(invoiceLine.US_TaxRateSInfo.ReadOnly);
			Assert(invoiceLine.US_R_OrigTaxRateSInfo.ReadOnly);
			Assert(invoiceLine.US_TaxRateTInfo.ReadOnly);
			Assert(invoiceLine.US_R_OrigTaxRateTInfo.ReadOnly);

			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.US_R_OrigTariff = "00000000";

			AssertEquals("PreCondition", TaxApplyList.Codes.Yes, invoiceLine.US_TaxApply);
			AssertEquals("PreCondition", TaxApplyList.Codes.Yes, invoiceLine.US_R_OrigTaxApply);

			Assert(!invoiceLine.US_TaxRateSInfo.ReadOnly);
			Assert(!invoiceLine.US_R_OrigTaxRateSInfo.ReadOnly);
			Assert(invoiceLine.US_TaxRateTInfo.ReadOnly);
			Assert(invoiceLine.US_R_OrigTaxRateTInfo.ReadOnly);

			invoiceLine.JI_Tariff = "00000002";
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			Assert(!invoiceLine.US_TaxRateTInfo.ReadOnly);
			Assert(invoiceLine.US_R_OrigTaxRateTInfo.ReadOnly);

			invoiceLine.US_R_OrigTariff = "00000002";
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Yes;
			Assert(!invoiceLine.US_TaxRateTInfo.ReadOnly);
			Assert(!invoiceLine.US_R_OrigTaxRateTInfo.ReadOnly);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			Assert(!invoiceLine.US_TaxRateSInfo.ReadOnly);
			Assert(!invoiceLine.US_R_OrigTaxRateSInfo.ReadOnly);
			Assert(invoiceLine.US_TaxRateTInfo.ReadOnly);

			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			Assert(!invoiceLine.US_TaxRateSInfo.ReadOnly);
			Assert(!invoiceLine.US_R_OrigTaxRateSInfo.ReadOnly);
			Assert(invoiceLine.US_R_OrigTaxRateTInfo.ReadOnly);
		}

		public void TestClearTaxRateTWhenTaxRateTBecomeReadOnly()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "20230106";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;

			var invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "20230106";
			Assert(!invoiceLine.US_TaxRateTInfo.ReadOnly);
			invoiceLine.US_TaxRateT = RateTypeList.Codes.Primary;
			AssertEquals(RateTypeList.Codes.Primary, invoiceLine.US_TaxRateT);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			Assert(invoiceLine.US_TaxRateTInfo.ReadOnly);
			AssertEquals(ZString.Empty, invoiceLine.US_TaxRateT);

			invoiceLine.US_R_OrigTariff = "20230106";
			Assert(!invoiceLine.US_R_OrigTaxRateTInfo.ReadOnly);
			invoiceLine.US_R_OrigTaxRateT = RateTypeList.Codes.Primary;
			AssertEquals(RateTypeList.Codes.Primary, invoiceLine.US_R_OrigTaxRateT);
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			Assert(invoiceLine.US_R_OrigTaxRateTInfo.ReadOnly);
			AssertEquals(ZString.Empty, invoiceLine.US_R_OrigTaxRateT);
		}

		public void TestGetUS_R_ReconOtherFee()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, "5.32000");
			invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, "4.55000");
			AssertEquals("First Recon Fee Code", ZString.Empty, invoiceLine.US_R_ReconOtherFeeCode);
			AssertEquals("First Recon Fee Amount", 0m, invoiceLine.US_R_ReconOtherFeeAmount);

			var otherFee = invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.Coffee);
			otherFee.CY_FeeAmount = 12.57m;
			var cottonFee = invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.Cotton);
			cottonFee.CY_FeeAmount = 2.73m;
			AssertEquals("First Recon Fee Code", Core.Constants.USCustoms.FeeCodes.Cotton, invoiceLine.US_R_ReconOtherFeeCode);
			AssertEquals("First Recon Fee Amount", 2.73m, invoiceLine.US_R_ReconOtherFeeAmount);

			cottonFee.CY_FeeAmount = 0m;
			AssertEquals("First Recon Fee Code", Core.Constants.USCustoms.FeeCodes.Coffee, invoiceLine.US_R_ReconOtherFeeCode);
			AssertEquals("First Recon Fee Amount", 12.57m, invoiceLine.US_R_ReconOtherFeeAmount);

			otherFee.CY_Code = Core.Constants.USCustoms.FeeCodes.Blueberry;
			otherFee.CY_FeeAmount = 2.23m;
			AssertEquals("First Recon Fee Code", Core.Constants.USCustoms.FeeCodes.Blueberry, invoiceLine.US_R_ReconOtherFeeCode);
			AssertEquals("First Recon Fee Amount", 2.23m, invoiceLine.US_R_ReconOtherFeeAmount);
		}

		public void TestSetUS_R_ReconOtherFee()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var invoiceLine = reconDec.InvoiceLines.AddNew();
			AssertEquals(0, invoiceLine.FeeCusCodes.Count);

			invoiceLine.US_R_ReconOtherFeeCode = Core.Constants.USCustoms.FeeCodes.Cotton;
			invoiceLine.US_R_OverrideReconOtherFeeAmount = true;
			invoiceLine.US_R_ReconOtherFeeAmount = 5.3m;
			AssertEquals(1, invoiceLine.FeeCusCodes.Count);

			var fee = invoiceLine.FeeCusCodes.Cast<FeeCusCodeData>().FirstOrDefault(x => x.CY_Code == Core.Constants.USCustoms.FeeCodes.Cotton);
			AssertNotNull("Cotton Fee exists", fee);
			AssertEquals("Overridden is ticked.", ZBool.True, fee.CY_IsOverridden);
			AssertEquals("Recon Fee Amount", 5.3m, fee.CY_FeeAmount);
		}

		public void TestUS_R_ReconOtherFeeAmount_ReadOnly()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var invoiceLine = reconDec.InvoiceLines.AddNew();
			AssertEquals("US_R_ReconOtherFeeAmount is ReadOnly", true, invoiceLine.US_R_ReconOtherFeeAmountInfo.ReadOnly);

			invoiceLine.US_R_OverrideReconOtherFeeAmount = true;
			AssertEquals("US_R_ReconOtherFeeAmount is not ReadOnly", false, invoiceLine.US_R_ReconOtherFeeAmountInfo.ReadOnly);
		}

		public void TestTaxRateTypeSynchronisationWithFeeCusCodes()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;
			SetUpTariffsForTaxRelatedFields();

			var invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000002";
			invoiceLine.US_R_OrigTariff = "00000002";

			AssertEquals("PreCondition", ZString.Empty, invoiceLine.US_TaxApply);
			AssertEquals("PreCondition", ZString.Empty, invoiceLine.US_R_OrigTaxApply);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Yes;
			AssertEquals("PreCondition", Core.Constants.USCustoms.FeeCodes.DistilledSpirits, invoiceLine.US_TaxCode);
			AssertEquals("PreCondition", Core.Constants.USCustoms.FeeCodes.DistilledSpirits, invoiceLine.US_R_OrigTaxCode);

			invoiceLine.US_TaxRateT = RateTypeList.Codes.Primary;
			invoiceLine.US_R_OrigTaxRateT = RateTypeList.Codes.Secondary;

			AssertEquals(RateTypeList.Codes.Primary, invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.DistilledSpirits).CY_SelectedRateType);
			AssertEquals(RateTypeList.Codes.Secondary, invoiceLine.ReconOriginalCharges.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.DistilledSpirits).CY_SelectedRateType);
		}

		public void TestTaxCode()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;

			SetUpTariffsForTaxRelatedFields();

			JobComInvoiceLine invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000002";
			invoiceLine.US_R_OrigTariff = "00000002";

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;

			Assert(!invoiceLine.US_TaxCodeInfo.ReadOnly);
			Assert(!invoiceLine.US_R_OrigTaxCodeInfo.ReadOnly);

			AssertEquals(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, invoiceLine.US_TaxCode);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, invoiceLine.US_R_OrigTaxCode);

			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			invoiceLine.US_R_OrigTaxCode = Core.Constants.USCustoms.FeeCodes.Wines;

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Yes;

			Assert(!invoiceLine.US_TaxCodeInfo.ReadOnly);
			Assert(!invoiceLine.US_R_OrigTaxCodeInfo.ReadOnly);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, invoiceLine.US_TaxCode);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, invoiceLine.US_R_OrigTaxCode);
		}

		public void TestTaxCodeReturnsDefaultFromTariffFile()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;
			entry.US_R_CalcOrigDuty = true;

			SetUpTariffsForTaxRelatedFields();

			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "00000003";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff3.UE_Unit1 = "PFL";

			var dutyRate3 = tariff3.DutyRates.AddNew();
			dutyRate3.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate3.UD_TaxFeeFlag = "2";
			dutyRate3.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate3.UD_TaxFeeSpecificRate = 0.71m;

			var invoiceLine = entry.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.US_R_OrigTariff = "00000000";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.US_R_OrigFirstQty = 10m;

			AssertEquals(Core.Constants.USCustoms.FeeCodes.Wines, invoiceLine.US_TaxCode);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Wines, invoiceLine.US_R_OrigTaxCode);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Wines, invoiceLine.US_TaxCode);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Wines, invoiceLine.US_R_OrigTaxCode);

			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
			invoiceLine.US_R_OrigTaxRateS = AppendixBTaxRateList.Codes.Specify;

			invoiceLine.US_TaxRate = 0.01m;
			invoiceLine.US_R_OrigTaxRate = 0.02m;

			reconDec.CalculateDutyFeesForAllEntries();
			AssertEquals(1m, entry.ReconWines);
			AssertEquals(0.2m, entry.OriginalWines);

			invoiceLine.JI_Tariff = "00000003";
			invoiceLine.US_R_OrigTariff = "00000003";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.US_R_OrigFirstQty = 10m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Yes;
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Wines, invoiceLine.US_TaxCode);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Wines, invoiceLine.US_R_OrigTaxCode);

			AssertEquals(tariff3.GetTaxFeeRateDescription(Core.Constants.USCustoms.FeeCodes.Wines, ZString.Empty), invoiceLine.US_TaxRateS);
			AssertEquals(tariff3.GetTaxFeeRateDescription(Core.Constants.USCustoms.FeeCodes.Wines, ZString.Empty), invoiceLine.US_R_OrigTaxRateS);
			reconDec.CalculateDutyFeesForAllEntries();
			AssertEquals(71m, entry.ReconWines);
			AssertEquals(7.1m, entry.OriginalWines);
		}

		public void TestOriginalTaxesAndFeesEntered()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000002";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "PFL";

			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			dutyRate.UD_TaxFeeSpecificRate = 0.7m;
			dutyRate.UD_TaxFeeAdvalorem = 0.8m;

			var invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigTariff = tariff.UE_Tariff;
			AssertEquals(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, invoiceLine.US_R_OrigTaxCode);
			Assert("Not readonly, because Tax Code entered", !invoiceLine.US_R_OrigTaxAmountInfo.ReadOnly);

			invoiceLine.US_R_OrigTaxAmount = 52.31m;
			AssertEquals("Spirits tax should be added to original charges", 1, invoiceLine.ReconOriginalCharges.Count);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, invoiceLine.ReconOriginalCharges[0].CY_Code);

			invoiceLine.US_R_OrigMPFAmount = 1.56m;
			invoiceLine.FeeCusCodes.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 1.56m);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, invoiceLine.ReconOriginalCharges[1].CY_Code);
			AssertEquals(1.56m, invoiceLine.US_R_ReconMPFAmount);

			invoiceLine.US_R_OrigHMFAmount = 5.20m;
			Assert(!(invoiceLine.US_R_OrigMPFAmountInfo.GetHashCode() == invoiceLine.US_R_OrigHMFAmountInfo.GetHashCode()));
			AssertEquals(Core.Constants.USCustoms.FeeCodes.HMF, invoiceLine.ReconOriginalCharges[2].CY_Code);

			invoiceLine.US_R_OrigOtherFeeCode = Core.Constants.USCustoms.FeeCodes.Pork;
			invoiceLine.US_R_OrigOtherFeeAmount = 8.6m;
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Pork, invoiceLine.ReconOriginalCharges[3].CY_Code);
		}

		public void TestOriginalChargesReadonly()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			AssertEquals("Should not be readonly by default", false, invoiceLine.US_R_OrigHMFAmount_ReadOnly);
			AssertEquals("Should not be readonly by default", false, invoiceLine.US_R_OrigMPFAmount_ReadOnly);
			AssertEquals("Should not be readonly by default", false, invoiceLine.US_R_OrigOtherFeeCode_ReadOnly);

			invoiceLine.US_R_OrigHMFAmount = 1.6m;
			invoiceLine.US_R_OrigMPFAmount = 2.9m;
			invoiceLine.US_R_OrigOtherFeeCode = Core.Constants.USCustoms.FeeCodes.Blueberry;
			invoiceLine.US_R_OrigOtherFeeAmount = 4.6m;
			AssertEquals("Should not be readonly", false, invoiceLine.US_R_OrigHMFAmount_ReadOnly);
			AssertEquals("Should not be readonly", false, invoiceLine.US_R_OrigMPFAmount_ReadOnly);
			AssertEquals("Should not be readonly", false, invoiceLine.US_R_OrigOtherFeeCode_ReadOnly);

			entry.US_R_CalcOrigDuty = true;
			AssertEquals("Should be readonly", true, invoiceLine.US_R_OrigHMFAmount_ReadOnly);
			AssertEquals("Should be readonly", true, invoiceLine.US_R_OrigMPFAmount_ReadOnly);
			AssertEquals("Should be readonly", true, invoiceLine.US_R_OrigOtherFeeCode_ReadOnly);

			invoiceLine.ReconOriginalCharges[0].CY_IsOverridden = true;
			invoiceLine.ReconOriginalCharges[1].CY_IsOverridden = true;
			AssertEquals("Should not be readonly", false, invoiceLine.US_R_OrigHMFAmount_ReadOnly);
			AssertEquals("Should not be readonly", false, invoiceLine.US_R_OrigMPFAmount_ReadOnly);
			AssertEquals("Should be readonly", true, invoiceLine.US_R_OrigOtherFeeCode_ReadOnly);
		}

		public void TestHasDecrease()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.DairyFee, 15m);
			var fee = invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.DairyFee);
			fee.CY_FeeAmount = 20m;
			Assert(!invoiceLine.HasDecrease);

			var childLine1 = invoiceLine.AddSecondaryInvoiceLine();
			Assert(!invoiceLine.HasDecrease);
			Assert(!childLine1.HasDecrease);

			childLine1.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 25m);
			fee = childLine1.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			fee.CY_FeeAmount = 25m;
			Assert(!invoiceLine.HasDecrease);
			Assert(!childLine1.HasDecrease);

			fee.CY_FeeAmount = 20m;
			Assert(invoiceLine.HasDecrease);
			Assert(childLine1.HasDecrease);

			fee.CY_FeeAmount = 25m;
			invoiceLine.FeeCusCodes.OfType<FeeCusCodeData>().FirstOrDefault(x => x.CY_Code == Core.Constants.USCustoms.FeeCodes.DairyFee).CY_FeeAmount = 12m;

			Assert(invoiceLine.HasDecrease);
			Assert(childLine1.HasDecrease);
		}

		public void TestUS_R_OverrideOriginMPF()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.ReconOriginalCharges.RemoveAndDeleteAll();
			invoiceLine.US_R_OverrideOriginMPF = true;
			AssertEquals(1, invoiceLine.ReconOriginalCharges.Count);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, invoiceLine.ReconOriginalCharges[0].CY_Code);
			AssertEquals(true, invoiceLine.ReconOriginalCharges[0].CY_IsOverridden);
		}

		public void TestUS_R_OverrideReconMPF()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.FeeCusCodes.RemoveAndDeleteAll();
			invoiceLine.US_R_OverrideReconMPF = true;
			AssertEquals(1, invoiceLine.FeeCusCodes.Count);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, invoiceLine.FeeCusCodes[0].CY_Code);
			AssertEquals(true, invoiceLine.FeeCusCodes[0].CY_IsOverridden);
		}

		public void TestUS_R_OverrideOriginHMF()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.ReconOriginalCharges.RemoveAndDeleteAll();
			invoiceLine.US_R_OverrideOriginHMF = true;
			AssertEquals(1, invoiceLine.ReconOriginalCharges.Count);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.HMF, invoiceLine.ReconOriginalCharges[0].CY_Code);
			AssertEquals(true, invoiceLine.ReconOriginalCharges[0].CY_IsOverridden);
		}

		public void TestUS_R_OverrideReconHMF()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.FeeCusCodes.RemoveAndDeleteAll();
			invoiceLine.US_R_OverrideReconHMF = true;
			AssertEquals(1, invoiceLine.FeeCusCodes.Count);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.HMF, invoiceLine.FeeCusCodes[0].CY_Code);
			AssertEquals(true, invoiceLine.FeeCusCodes[0].CY_IsOverridden);
		}

		public void TestUS_R_OverrideOrigOtherFeeAmount()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.ReconOriginalCharges.RemoveAndDeleteAll();
			invoiceLine.US_R_OverrideOrigOtherFeeAmount = true;
			AssertEquals(1, invoiceLine.ReconOriginalCharges.Count);
			AssertEquals(true, invoiceLine.ReconOriginalCharges[0].CY_IsOverridden);
		}

		public void TestUS_R_OverrideReconOtherFeeAmount()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			invoiceLine.FeeCusCodes.RemoveAndDeleteAll();
			invoiceLine.US_R_OverrideReconOtherFeeAmount = true;
			AssertEquals(1, invoiceLine.FeeCusCodes.Count);
			AssertEquals(true, invoiceLine.FeeCusCodes[0].CY_IsOverridden);
		}

		public void TestIsOrigTaxRateSpecifiedManually()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			var invoiceLine = entry.Invoice.InvoiceLines.AddNew();
			AssertEquals(false, invoiceLine.IsOrigTaxRateSpecifiedManually);
			AssertEquals(false, invoiceLine.IsOriginalTaxRateReduced);

			invoiceLine.US_R_OrigTaxRateS = AppendixBTaxRateList.CBMAEligible;
			AssertEquals(true, invoiceLine.IsOrigTaxRateSpecifiedManually);
			AssertEquals(true, invoiceLine.IsOriginalTaxRateReduced);
		}

		public void TestCopyReconEntryLine()
		{
			var srcDec = Factory.New<JobDeclaration>();
			srcDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			srcDec.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Recon;
			srcDec.JE_EntryAuthorisationDate = ZDateTime.BrettsBirthday;
			srcDec.US_Comment = "COMMENTS";
			srcDec.US_PreliminaryStatementPrintDate = ZDateTime.BrettsBirthday;
			srcDec.US_EntryFilerCode = "XJ5";
			srcDec.US_ClientBranchDesignation = "DP";

			CusEntryHeader entryRCI = Factory.New<CusEntryHeader>();
			entryRCI.CH_JE = srcDec.PK;
			entryRCI.CH_BGMReference = "XJ5123456";
			entryRCI.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;
			entryRCI.Charges.AddNew("DDD", 45.5);
			entryRCI.Charges.AddNew("PPP", 46.6);

			CusEntryHeader entryREC = Factory.New<CusEntryHeader>();
			entryREC.CH_JE = srcDec.PK;
			entryREC.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;

			var broker = Factory.New<MasterFiles.Business.GlbStaff>();
			broker.GS_Code = "AGT";
			srcDec.JE_GS_NKCusAgent = broker.GS_Code;
			srcDec.ReconDeclaration = srcDec.ReconDeclaration ?? new ReconDeclaration(srcDec);

			var reconDec = srcDec.ReconDeclaration;
			reconDec.US_PreliminaryStatementPrintDate = ZDateTime.BrettsBirthday;
			reconDec.US_Comment = "Recon COMMENTS";
			reconDec.US_EstimatedEntryDate = ZDateTime.BrettsBirthday;

			JobComInvoiceHeader invoice = reconDec.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "XJ5123456";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_CH_ReconEntry = entryRCI.PK;

			JobComInvoiceLine invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_R_OrigSupTariff = "5678.12.5678";
			invoiceLine.JI_FormattedTariff = "1111.11.1110";
			invoiceLine.US_SupTariff = "1212.22.1212";
			invoiceLine.JI_Calc_Invoice = "XJ5123456";

			invoiceLine.US_R_OrigFirstUQ = "KG";
			invoiceLine.US_R_OrigFirstQty = 30m;
			invoiceLine.US_R_ReconHMFAmount = 11.1;

			invoiceLine.US_R_OrigSecondUQ = "M3";
			invoiceLine.US_R_OrigSecondQty = 40m;

			invoiceLine.US_R_OrigThirdUQ = "NO";
			invoiceLine.US_R_OrigThirdQty = 50m;

			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 35m;

			invoiceLine.JI_CustomsSecondUnitQty = "M3";
			invoiceLine.JI_CustomsSecondQuantity = 45m;

			invoiceLine.JI_CustomsThirdUnitQty = "NO";
			invoiceLine.JI_CustomsThirdQuantity = 55m;

			invoiceLine.JI_LinePrice = 30000m;
			invoiceLine.US_R_OrigCV = 35000m;

			invoiceLine.US_CottonFeeExempt = "C";
			invoiceLine.US_R_OrigSPI = "A";
			invoiceLine.US_SPI = "B";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.M;
			invoiceLine.JI_Description = "WHO IS THIS";
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_R_OrigTaxCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			invoiceLine.US_R_OrigTaxRate = 323.23m;
			invoiceLine.US_R_OrigOverrideDuty = ZBool.True;
			invoiceLine.US_R_OrigDuty = 532.54m;
			invoiceLine.US_OverrideDuty = ZBool.True;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			invoiceLine.US_TaxRate = 983.43m;
			invoiceLine.US_TaxQty = 85.43m;

			invoiceLine.US_R_Orig98Value = 54.6m;
			invoiceLine.US_98GoodsValue = 12.3m;
			invoiceLine.US_R_OrigOverrideSupDuty = false;
			invoiceLine.US_R_OrigSupDuty = 66.0m;
			invoiceLine.US_OverrideSupDuty = true;
			invoiceLine.US_SupDuty = 22.3m;
			invoiceLine.US_R_OrigSupQty1 = 55m;
			invoiceLine.US_R_OrigSupQty2 = 44m;
			invoiceLine.US_SupQty1 = 33m;
			invoiceLine.US_SupQty2 = 22m;

			Factory.Save();

			var newReconDec = (JobDeclaration)srcDec.ReconDeclaration.TemplateReconDeclarationCopyCore(Customs.Business.CloneType.TemplateCopy);
			AssertEquals(newReconDec.JE_MessageType, Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Recon);

			AssertEntryLines(newReconDec.ReconDeclaration.InvoiceLines[0], srcDec.ReconDeclaration.InvoiceLines[0]);
		}

		void AssertEntryLines(JobComInvoiceLine reconLine, JobComInvoiceLine srcLine)
		{
			AssertEquals(reconLine.US_R_OrigHMFAmount, srcLine.US_R_ReconHMFAmount);
			AssertEquals(reconLine.US_R_OrigMPFAmount, srcLine.US_R_ReconMPFAmount);
			AssertEquals(true, reconLine.US_OverrideSupDuty);
			AssertEquals(reconLine.US_R_OrigOverrideSupDuty, srcLine.US_OverrideSupDuty);

			AssertEquals(reconLine.US_R_OrigCV, srcLine.JI_LinePrice);
			AssertEquals(reconLine.US_R_OrigDuty, reconLine.US_Duty);
			AssertEquals(reconLine.US_R_OrigFirstQty, srcLine.JI_CustomsQuantity);
			AssertEquals(reconLine.US_R_OrigFirstUQ, srcLine.JI_CustomsUnitQty);
			AssertEquals(reconLine.US_R_OrigOverrideDuty, srcLine.US_OverrideDuty);

			AssertEquals(reconLine.US_R_OrigSPI, srcLine.US_SPI);
			AssertEquals(reconLine.US_R_OrigSecondQty, srcLine.JI_CustomsSecondQuantity);
			AssertEquals(reconLine.US_R_OrigCottonFeeExempt, srcLine.US_CottonFeeExempt);
			AssertEquals(reconLine.US_R_OrigSecondUQ, srcLine.JI_CustomsSecondUnitQty);
			AssertEquals(reconLine.OriginalTariffFormatted, srcLine.JI_FormattedTariff);
			AssertEquals(reconLine.OriginalSupTariffFormatted, srcLine.SupTariffFormatted);

			AssertEquals(reconLine.US_R_OrigTaxApply, srcLine.US_TaxApply);
			AssertEquals(reconLine.US_R_OrigTaxRateS, srcLine.US_TaxRateS);
			AssertEquals(reconLine.US_R_OrigTaxRateT, srcLine.US_TaxRateT);
			AssertEquals(reconLine.US_R_OrigTaxQty, srcLine.US_TaxQty);

			AssertEquals(reconLine.US_R_OrigSupDuty, srcLine.US_SupDuty);
			AssertEquals(reconLine.US_R_Orig98Value, srcLine.US_98GoodsValue);
			AssertEquals(reconLine.US_R_OrigSupQty1, srcLine.US_SupQty1);
			AssertEquals(reconLine.US_R_OrigSupQty2, srcLine.US_SupQty2);

			AssertEquals(reconLine.US_R_OrigTaxCode, srcLine.US_TaxCode);
			AssertEquals(reconLine.US_R_OrigTaxRate, srcLine.US_TaxRate);
		}

		void SetUpTariffsForTaxRelatedFields()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeSpecificRate = 0.5m;

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "00000001";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff2.UE_Unit1 = "L";

			USCTariffDutyRate dutyRate2 = tariff2.DutyRates.AddNew();
			dutyRate2.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			dutyRate2.UD_TaxFeeFlag = "1";
			dutyRate2.UD_TaxFeeComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;

			USCTariff tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "00000002";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff3.UE_Unit1 = "PFL";

			USCTariffDutyRate dutyRate3 = tariff3.DutyRates.AddNew();
			dutyRate3.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			dutyRate3.UD_TaxFeeFlag = "2";
			dutyRate3.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			dutyRate3.UD_TaxFeeSpecificRate = 0.7m;
			dutyRate3.UD_TaxFeeAdvalorem = 0.8m;
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
