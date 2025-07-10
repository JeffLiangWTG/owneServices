using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconImportEntryRetrieverTest : TestCaseWithFactory
	{
		public void TestDeleteAllPGALinesAfterImportOriginalEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.ACE_FDALines.AddNew();
			invoiceLine1.FDAs.AddNew();
			invoiceLine1.DOTs.AddNew();
			invoiceLine1.FCCs.AddNew();
			invoiceLine1.LaceyActLines.AddNew();
			invoiceLine1.AIILines.AddNew();
			invoiceLine1.LineGroupingRanges.AddNew();
			invoiceLine1.FSISLines.AddNew();
			invoiceLine1.VehicleLines.AddNew();
			invoiceLine1.PSTLines.AddNew();
			invoiceLine1.OMCHeaders.AddNew();
			invoiceLine1.ACE_FDALines.AddNew();
			invoiceLine1.NHTSALines.AddNew();
			invoiceLine1.CPSCHeaders.AddNew();
			invoiceLine1.DEAHeaders.AddNew();

			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.ACE_FDALines.AddNew();
			invoiceLine2.FDAs.AddNew();
			invoiceLine2.DOTs.AddNew();
			invoiceLine2.FCCs.AddNew();
			invoiceLine2.LaceyActLines.AddNew();
			invoiceLine2.AIILines.AddNew();
			invoiceLine2.LineGroupingRanges.AddNew();
			invoiceLine2.FSISLines.AddNew();
			invoiceLine2.VehicleLines.AddNew();
			invoiceLine2.PSTLines.AddNew();
			invoiceLine2.OMCHeaders.AddNew();
			invoiceLine2.ACE_FDALines.AddNew();
			invoiceLine2.NHTSALines.AddNew();
			invoiceLine2.CPSCHeaders.AddNew();
			invoiceLine2.DEAHeaders.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;

			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });
			AssertEquals("2 lines", 2, originalEntry.Invoice.InvoiceLines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[0].ACE_FDALines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[0].FDAs.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[0].DOTs.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[0].FCCs.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[0].LaceyActLines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[0].AIILines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[0].LineGroupingRanges.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[0].FSISLines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[0].VehicleLines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[0].PSTLines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[0].OMCHeaders.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[0].ACE_FDALines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[0].NHTSALines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[0].CPSCHeaders.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[0].DEAHeaders.Count);

			AssertEquals(0, originalEntry.Invoice.InvoiceLines[1].ACE_FDALines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[1].FDAs.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[1].DOTs.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[1].FCCs.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[1].LaceyActLines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[1].AIILines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[1].LineGroupingRanges.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[1].FSISLines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[1].VehicleLines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[1].PSTLines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[1].OMCHeaders.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[1].ACE_FDALines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[1].NHTSALines.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[1].CPSCHeaders.Count);
			AssertEquals(0, originalEntry.Invoice.InvoiceLines[1].DEAHeaders.Count);
		}

		public void TestDoNotCopyJI_MatchingKey()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 1000m;
				invoiceLine.JI_MatchingKey = "WTLBAK00000085";

				var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 1000m;
				invoiceLine2.JI_MatchingKey = "WTLBAK00000085";
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });
			AssertEquals("2 lines", 2, reconDeclaration.InvoiceLines.Count);

			AssertEquals("JI_MatchingKey should not be copied", ZString.Empty, reconDeclaration.InvoiceLines[0].JI_MatchingKey);
			AssertEquals("JI_MatchingKey should not be copied", ZString.Empty, reconDeclaration.InvoiceLines[1].JI_MatchingKey);
		}

		public void TestRetrieveHMFFromOriginalEntry()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.Invoices.AddNew();

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.US_SupTariff = "9802.00.8068";
				invoiceLine.JI_Tariff = "9102111010";
				invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
				invoiceLine.US_98GoodsValue = 560m;
				invoiceLine.JI_LinePrice = 13000m;

				var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine2.US_SupTariff = "9802.00.8068";
				invoiceLine2.JI_Tariff = "9102111020";
				invoiceLine2.JI_Description = "COMMERCIAL DESCRIPTION";
				invoiceLine2.US_98GoodsValue = 874m;
				invoiceLine2.JI_LinePrice = 5000m;

				var invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine3.US_SupTariff = "9802.00.8068";
				invoiceLine3.JI_Tariff = "9102111030";
				invoiceLine3.JI_Description = "COMMERCIAL DESCRIPTION";
				invoiceLine3.US_98GoodsValue = 5m;
				invoiceLine3.JI_LinePrice = 510m;

				var invoiceLine4 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine4.US_SupTariff = "9802.00.8068";
				invoiceLine4.JI_Tariff = "9102111040";
				invoiceLine4.JI_Description = "COMMERCIAL DESCRIPTION";
				invoiceLine4.US_98GoodsValue = 1m;
				invoiceLine4.JI_LinePrice = 629m;
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();//addinfo gets serialised

			AssertEquals(25.72m, declaration.ActiveEntryHeaders.EntrySummaryEntry.Charges.GetAmount("501"));

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;

			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });
			AssertEquals("4 lines", 4, originalEntry.Invoice.InvoiceLines.Count);

			AssertEquals("Should have been copied to the first line only", 25.72m, originalEntry.Invoice.InvoiceLines[0].FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));
			AssertEquals("Should have been copied to the first line only", 25.72m, originalEntry.Invoice.InvoiceLines[0].ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));

			AssertEquals("Should have been copied to the first line only", 0m, originalEntry.Invoice.InvoiceLines[1].FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));
			AssertEquals("Should have been copied to the first line only", 0m, originalEntry.Invoice.InvoiceLines[1].ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));

			AssertEquals("Should have been copied to the first line only", 0m, originalEntry.Invoice.InvoiceLines[2].FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));
			AssertEquals("Should have been copied to the first line only", 0m, originalEntry.Invoice.InvoiceLines[2].ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));

			AssertEquals("Should have been copied to the first line only", 0m, originalEntry.Invoice.InvoiceLines[3].FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));
			AssertEquals("Should have been copied to the first line only", 0m, originalEntry.Invoice.InvoiceLines[3].ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF));
		}

		public void TestLinesWith98()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.Invoices.AddNew();

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.US_SupTariff = "9802.00.8068";
				invoiceLine.JI_Tariff = "562589741";
				invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
				invoiceLine.US_98GoodsValue = 560m;
				invoiceLine.JI_LinePrice = 13000m;

				var invoiceLine2 = declaration.InvoiceLines.AddNew();
				invoiceLine2.US_SupTariff = "9802.00.8068";
				invoiceLine2.JI_Tariff = "562589741";
				invoiceLine2.JI_Description = "COMMERCIAL DESCRIPTION";
				invoiceLine2.US_98GoodsValue = 874m;
				invoiceLine2.JI_LinePrice = 5000m;
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();//addinfo gets serialised

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;

			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });
			AssertEquals("1 line", 1, originalEntry.Invoice.InvoiceLines.Count);

			AssertEquals("The total of 98 goods value is copied", 1434m, originalEntry.Invoice.InvoiceLines[0].US_98GoodsValue);
		}

		[TestDate(2008, 9, 11)]
		public void TestRetrieveCottonFeeExemptIndicatorForACEEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104220040";
			invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._12, "007894812");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();//addinfo gets serialised

			AssertEquals(0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.CottonFee);

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });

			reconDeclaration.CalculateDutyFeesForAllEntries();
			AssertEquals(0m, reconDeclaration.TotalReconCotton);
			AssertEquals(0m, reconDeclaration.TotalOriginalCotton);
		}

		[TestDate(2010, 1, 1)]
		public void TestImportHasMPF()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_MonthlyFiling = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99010050";
			invoiceLine.JI_LinePrice = 0.04m;
			invoiceLine.JI_Tariff = "2207106000";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Assert(invoiceLine.GetEntryLineFor("ENS", true).US_HasMPF);

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.US_DutyCalcDate = new ZDateTime(2020, 2, 19);
			entry.US_MPFCalcDate = new ZDateTime(2020, 2, 18);
			Factory.Save();//addinfo gets serialised

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;

			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });

			AssertEquals("US_R_MonthlyFiling", true, originalEntry.US_R_MonthlyFiling);
			AssertEquals("one invoice", 1, reconDeclaration.Invoices.Count);
			AssertEquals("1 invoice line", 1, reconDeclaration.InvoiceLines.Count);
			AssertEquals(1m, reconDeclaration.InvoiceLines[0].JI_LinePrice);

			Assert("parentLine's HasMPF should be aggregated", reconDeclaration.InvoiceLines[0].US_HasMPF);
			AssertEquals(new ZDateTime(2020, 2, 19), originalEntry.US_R_DutyRateDate);
			AssertEquals(new ZDateTime(2020, 2, 18), originalEntry.US_R_DateForMPFCalc);
		}

		public void TestImportStandAlone9801Line()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Tariff = "8466.93.9585";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "9801.00.1090";
			invoiceLine2.JI_LinePrice = 1000m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;

			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });

			AssertEquals("two entry lines. 9801 lines should be imported too if it is not a SUP line", 2, reconDeclaration.InvoiceLines.Count);
		}

		[TestDate(2013, 01, 01)]
		public void TestImportFromOriginalEntryForTaxFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3303003000";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Other_4;
			invoiceLine.US_TaxQty = 10m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(35.66m, entry.OtherExciseTax);
			Factory.Save();

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());

			var reconOriginalEntryHeader = reconDec.OriginalEntries.AddNew();
			reconOriginalEntryHeader.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;
			reconOriginalEntryHeader.US_R_DutyRateDate = ZDateTime.Today;

			reconDec.ImportLines(new JobDeclaration[] { declaration });

			AssertEquals("one invoice", 1, reconDec.Invoices.Count);
			AssertEquals("1 invoice line", 1, reconDec.InvoiceLines.Count);

			var reconLine = reconDec.InvoiceLines[0];

			AssertEquals(TaxApplyList.Codes.Override, reconLine.US_TaxApply);
			AssertEquals(TaxApplyList.Codes.Override, reconLine.US_R_OrigTaxApply);

			AssertEquals(Core.Constants.USCustoms.FeeCodes.OtherExcise, reconLine.US_TaxCode);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.OtherExcise, reconLine.US_R_OrigTaxCode);

			AssertEquals(AppendixBTaxRateList.Codes.Other_4, reconLine.US_TaxRateS);
			AssertEquals(AppendixBTaxRateList.Codes.Other_4, reconLine.US_R_OrigTaxRateS);

			AssertEquals(10m, reconLine.US_TaxQty);
			AssertEquals(10m, reconLine.US_R_OrigTaxQty);
		}

		public void TestImport99InvoiceLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99010050";
			invoiceLine.US_SupQty1 = 10000m;
			invoiceLine.JI_Tariff = "2207106000";
			invoiceLine.JI_CustomsQuantity = 5000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();//addinfo gets serialised

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;

			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });

			AssertEquals("one invoice", 1, reconDeclaration.Invoices.Count);
			AssertEquals("1 invoice line", 1, reconDeclaration.InvoiceLines.Count);

			JobComInvoiceLine reconLine = reconDeclaration.InvoiceLines[0];

			AssertEquals("99010050", reconLine.US_SupTariff);
			AssertEquals(invoiceLine.US_SupUQ1, reconLine.US_SupUQ1);
			AssertEquals(invoiceLine.US_SupQty1, reconLine.US_SupQty1);
			AssertEquals("99010050", reconLine.US_R_OrigSupTariff);
			AssertEquals(invoiceLine.US_SupUQ1, reconLine.US_R_OrigSupUQ1);
			AssertEquals(invoiceLine.US_SupQty1, reconLine.US_R_OrigSupQty1);

			AssertEquals("2207106000", reconLine.JI_Tariff);
			AssertEquals(invoiceLine.JI_CustomsUnitQty, reconLine.JI_CustomsUnitQty);
			AssertEquals(invoiceLine.JI_CustomsQuantity, reconLine.JI_CustomsQuantity);
			AssertEquals("2207106000", reconLine.US_R_OrigTariff);
			AssertEquals(invoiceLine.JI_CustomsUnitQty, reconLine.US_R_OrigFirstUQ);
			AssertEquals(invoiceLine.JI_CustomsQuantity, reconLine.US_R_OrigFirstQty);
		}

		public void TestProvTariffCustomValue()
		{
			CreateOrLoadTariff("99038803", ComputationCodeList.Codes.Derived);

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_EnableENS = true;
			declaration1.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration1.US_EntryFilerCode = "XJ5";

			var dec1Invoice = declaration1.Invoices.AddNew();
			dec1Invoice.JZ_InvoiceNumber = "INV1";

			var dec1InvoiceLine = dec1Invoice.JobComInvoiceLines.AddNew();
			dec1InvoiceLine.JI_Tariff = "3923500000";
			dec1InvoiceLine.US_SupTariff = "99038803";
			dec1InvoiceLine.JI_LinePrice = 5000m;

			declaration1.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EnableENS = true;
			declaration2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration2.US_EntryFilerCode = "XJ5";

			var dec2Invoice = declaration2.Invoices.AddNew();
			dec2Invoice.JZ_InvoiceNumber = "INV1";

			var dec2InvoiceLine1 = dec2Invoice.InvoiceLines.AddNew();
			dec2InvoiceLine1.JI_LinePrice = 2000m;
			dec2InvoiceLine1.JI_Tariff = "6104442010";

			var dec2InvoiceLine2 = dec2Invoice.InvoiceLines.AddNew();
			dec2InvoiceLine2.JI_LinePrice = 5000m;
			dec2InvoiceLine2.JI_Tariff = "6104442010";

			declaration2.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.US_EnableENS = true;
			declaration3.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration3.US_EntryFilerCode = "XJ5";

			CreateOrLoadTariff("9802004020");

			var dec3Invoice = declaration3.Invoices.AddNew();
			dec3Invoice.JZ_InvoiceNumber = "INV1";

			var dec3InvoiceLine = dec3Invoice.JobComInvoiceLines.AddNew();
			dec3InvoiceLine.JI_Tariff = "8457100075";
			dec3InvoiceLine.US_SupTariff = "9802004020";
			dec3InvoiceLine.JI_LinePrice = 2000m;
			dec3InvoiceLine.US_98GoodsValue = 1000m;
			declaration3.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();//addinfo gets serialised

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration1.ImportEntryNumber;

			reconDeclaration.ImportLines(new JobDeclaration[] { declaration1, declaration2, declaration3 });

			var reconLine1 = reconDeclaration.InvoiceLines[0];
			AssertEquals("One invoice line split to two entries, cacluate the customs value from the JI_LinePrice of sup line", 5000m, reconLine1.US_R_OrigCV);
			AssertEquals("One invoice line split to two entries, cacluate the customs value from the JI_LinePrice of sup line", 5000m, reconLine1.JI_LinePrice);

			var reconLine2 = reconDeclaration.InvoiceLines[1];
			AssertEquals("Two invoice lines mreged into one entry, calculate the customs value from both sup line and child line", 7000m, reconLine2.US_R_OrigCV);
			AssertEquals("Two invoice lines mreged into one entry, calculate the customs value from both sup line and child line", 7000m, reconLine2.JI_LinePrice);

			var reconLine3 = reconDeclaration.InvoiceLines[2];
			AssertEquals("For 98 tariff line, cacluate the customs value from the JI_LinePrice of child line", 2000m, reconLine3.US_R_OrigCV);
			AssertEquals("For 98 tariff line, cacluate the customs value from the JI_LinePrice of child line", 2000m, reconLine3.JI_LinePrice);
			AssertEquals("For 98 tariff line, calculate US/Orig. value from US_98GoodsValue of the child line", 1000m, reconLine3.US_R_Orig98Value);
			AssertEquals("For 98 tariff line, calculate US/Orig. value from US_98GoodsValue of the child line", 1000m, reconLine3.US_98GoodsValue);
		}

		public void TestCustomValueRounding()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8537.10.9130";
			invoiceLine1.JI_LinePrice = 10807.20m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8548.90.0100";
			invoiceLine2.JI_LinePrice = 2534.40m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "9026.20.4000";
			invoiceLine3.JI_LinePrice = 2484m;

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "3920.99.2000";
			invoiceLine4.JI_LinePrice = 2787.84m;

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "3801.10.1000";
			invoiceLine5.JI_LinePrice = 10319.40m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;

			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });

			var reconLine1 = reconDeclaration.InvoiceLines[0];
			AssertEquals("cacluate the rounding customs value from the JI_LinePrice", 10807m, reconLine1.US_R_OrigCV);
			AssertEquals("cacluate the rounding customs value from the JI_LinePrice", 10807m, reconLine1.JI_LinePrice);

			var reconLine2 = reconDeclaration.InvoiceLines[1];
			AssertEquals("cacluate the rounding customs value from the JI_LinePrice", 2534m, reconLine2.US_R_OrigCV);
			AssertEquals("cacluate the rounding customs value from the JI_LinePrice", 2534m, reconLine2.JI_LinePrice);

			var reconLine3 = reconDeclaration.InvoiceLines[2];
			AssertEquals("cacluate the rounding customs value from the JI_LinePrice", 2484m, reconLine3.US_R_OrigCV);
			AssertEquals("cacluate the rounding customs value from the JI_LinePrice", 2484m, reconLine3.JI_LinePrice);

			var reconLine4 = reconDeclaration.InvoiceLines[3];
			AssertEquals("cacluate the rounding customs value from the JI_LinePrice", 2788m, reconLine4.US_R_OrigCV);
			AssertEquals("cacluate the rounding customs value from the JI_LinePrice", 2788m, reconLine4.JI_LinePrice);

			var reconLine5 = reconDeclaration.InvoiceLines[4];
			AssertEquals("cacluate the rounding customs value from the JI_LinePrice", 10320m, reconLine5.US_R_OrigCV);
			AssertEquals("cacluate the rounding customs value from the JI_LinePrice", 10320m, reconLine5.JI_LinePrice);
		}

		public void TestTwoInvoiceLinesFromTwoInvoicesDoNotGetMergedAndRetrievedOKToReconJob()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			JobComInvoiceLine invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);

			Factory.Save();//addinfo gets serialised
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;

			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });

			AssertEquals(1, reconDeclaration.Invoices.Count);
			AssertEquals(2, reconDeclaration.Invoices[0].JobComInvoiceLines.Count);
		}

		[TestDate(2009, 1, 1)]
		public void TestRetrieveDairyQty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.DairyFeeApplicable;
			AssertNotNull(invoiceLine.ImportTariff);
			invoiceLine.ImportTariff.SetUpTestDataForDairyFeeWithXComputationCode();
			invoiceLine.JI_LinePrice = 460m;
			invoiceLine.JI_CustomsThirdUnitQty = "CKG";
			invoiceLine.JI_CustomsThirdQuantity = 1000m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = USCTariff.DairyFeeApplicable;
			invoiceLine2.JI_LinePrice = 156m;
			invoiceLine2.JI_CustomsThirdUnitQty = "CKG";
			invoiceLine2.JI_CustomsThirdQuantity = 500m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("PreCondition", invoiceLine.CusEntryLine, invoiceLine2.CusEntryLine);

			Factory.Save();//addinfo gets serialised
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;

			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });

			AssertEquals(1, reconDeclaration.Invoices.Count);
			AssertEquals(1, reconDeclaration.Invoices[0].JobComInvoiceLines.Count);

			var reconLine = reconDeclaration.Invoices[0].JobComInvoiceLines[0];
			AssertEquals(1500m, reconLine.JI_CustomsThirdQuantity);
			AssertEquals(1500m, reconLine.US_R_OrigThirdQty);

			var dairyAmount = reconLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.DairyFee);
			AssertEquals("Dairy", 19.91m, dairyAmount);
		}

		public void TestOverriddenTaxRateCopiedOK()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxRateT = RateTypeList.Codes.Secondary;
			invoiceLine.US_TaxRateS = "$0.999/KG";
			invoiceLine.US_TaxRate = 0.998m;
			invoiceLine.US_TaxQty = 230m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();//addinfo gets serialised
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;

			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });

			AssertEquals("One line", 1, reconDeclaration.InvoiceLines.Count);
			AssertEquals(TaxApplyList.Codes.Override, reconDeclaration.InvoiceLines[0].US_TaxApply);
			AssertEquals(TaxApplyList.Codes.Override, reconDeclaration.InvoiceLines[0].US_R_OrigTaxApply);

			AssertEquals("$0.999/KG", reconDeclaration.InvoiceLines[0].US_TaxRateS);
			AssertEquals("$0.999/KG", reconDeclaration.InvoiceLines[0].US_R_OrigTaxRateS);

			AssertEquals(0.998m, reconDeclaration.InvoiceLines[0].US_TaxRate);
			AssertEquals(0.998m, reconDeclaration.InvoiceLines[0].US_R_OrigTaxRate);

			AssertEquals(RateTypeList.Codes.Secondary, reconDeclaration.InvoiceLines[0].US_TaxRateT);
			AssertEquals(RateTypeList.Codes.Secondary, reconDeclaration.InvoiceLines[0].US_R_OrigTaxRateT);

			AssertEquals(230m, reconDeclaration.InvoiceLines[0].US_TaxQty);
			AssertEquals(230m, reconDeclaration.InvoiceLines[0].US_R_OrigTaxQty);
		}

		[TestDate(2009, 12, 12)]
		public void TestOriginalCottonFeeIsRetainedCorrectly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.JZ_InvoiceAmount = 30000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine1.JI_Tariff = USCTariff.CottonFeeApplicable;
			invoiceLine1.JI_CustomsQuantity = 1000m;
			invoiceLine1.JI_CustomsSecondQuantity = 500m;

			JobComInvoiceLine invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 20000m;
			invoiceLine2.JI_Tariff = USCTariff.CottonFeeApplicable;
			invoiceLine2.JI_CustomsQuantity = 2000m;
			invoiceLine2.JI_CustomsSecondQuantity = 500m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition:InvoiceLine1 and InvoiceLine2 are merged together", invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals("Cotton fee is applicable > 2.00", 2.46m, invoiceLine1.CusEntryLine.CottonAmount);
			Factory.Save();

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;

			reconDeclaration.CalculateDutyFeesForChangedEntries();
			AssertEquals("OriginalCottonFee should be there", 2.46m, originalEntry.OriginalCotton);
		}

		public void TestRetrieveInvoiceValuesJI_OP()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "Part1";
			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "Part2";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = USCTariff.CottonFeeApplicable;
			invoiceLine1.JI_OP = part1.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = USCTariff.FCCMayBeApplicable;
			invoiceLine2.JI_OP = part2.PK;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			part2.OP_IsActive = ZBool.False;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reconDeclaration = new ReconDeclaration(newFactory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });

			AssertEquals("One invoice line is retrieved", 2, reconDeclaration.InvoiceLines.Count);
			AssertEquals(part1.PK, reconDeclaration.InvoiceLines[0].JI_OP);
			reconDeclaration.InvoiceLines[0].Validation.ValidateJI_OP();
			AssertEquals(ZGuid.Empty, reconDeclaration.InvoiceLines[1].JI_OP);
			reconDeclaration.InvoiceLines[1].Validation.ValidateJI_OP();
			Assert(!reconDeclaration.InvoiceLines[1].HasErrors);
		}

		public void TestRetrieveInvoiceValuesCorrectly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.JZ_InvoiceAmount = 30000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.US_UC_NKCountryOfOrigin = "CN";

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine1.JI_Tariff = USCTariff.CottonFeeApplicable;
			invoiceLine1.JI_CustomsQuantity = 1000m;
			invoiceLine1.JI_Weight = 1586.285;
			invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			invoiceLine1.JI_InvoiceQuantity = 250m;
			invoiceLine1.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
			invoiceLine1.JI_Volume = 249m;
			invoiceLine1.JI_VolumeUQ = Core.Constants.Volume.CubicMetres;
			invoiceLine1.JI_CustomsSecondQuantity = 500m;

			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 20000m;
			invoiceLine2.JI_Tariff = USCTariff.CottonFeeApplicable;
			invoiceLine2.JI_CustomsQuantity = 2000m;
			invoiceLine2.JI_Weight = 350m;
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Pounds;
			invoiceLine2.JI_InvoiceQuantity = 350m;
			invoiceLine2.JI_InvoiceUQ = Core.Constants.Weight.Pounds;
			invoiceLine2.JI_Volume = 349m;
			invoiceLine2.JI_VolumeUQ = Core.Constants.Volume.CubicFeet;
			invoiceLine2.JI_CustomsSecondQuantity = 500m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition:InvoiceLine1 and InvoiceLine2 are merged together", invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			invoiceLine2.FDAs.AddNew();
			invoiceLine2.VehicleLines.AddNew();
			invoiceLine2.FSISLines.AddNew();
			invoiceLine2.PSTLines.AddNew();
			invoiceLine2.ACE_FDALines.AddNew();
			invoiceLine2.NHTSALines.AddNew();

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reconDeclaration = new ReconDeclaration(newFactory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });

			AssertEquals("One invoice line is retrieved", 1, reconDeclaration.InvoiceLines.Count);
			AssertEquals("LinePrice", 30000m, reconDeclaration.InvoiceLines[0].JI_LinePrice);
			AssertEquals(0, reconDeclaration.InvoiceLines[0].FDAs.Count);
			AssertEquals(0, reconDeclaration.InvoiceLines[0].FSISLines.Count);
			AssertEquals(0, reconDeclaration.InvoiceLines[0].VehicleLines.Count);
			AssertEquals(0, reconDeclaration.InvoiceLines[0].PSTLines.Count);
			AssertEquals(0, reconDeclaration.InvoiceLines[0].ACE_FDALines.Count);
			AssertEquals(0, reconDeclaration.InvoiceLines[0].NHTSALines.Count);

			AssertEquals("When a line is cloned, same factory should be used as recon dec", reconDeclaration.Invoices[0], reconDeclaration.InvoiceLines[0].InvoiceHeader);
			AssertEquals("CustomsQuantity", 3000m, reconDeclaration.InvoiceLines[0].JI_CustomsQuantity);
			AssertEquals("SecondQuantity", 1000m, reconDeclaration.InvoiceLines[0].JI_CustomsSecondQuantity);

			AssertEquals("Weight is not converted to KG, just cloned as it is", 1586.285m, reconDeclaration.InvoiceLines[0].JI_Weight);
			AssertEquals("Weight UQ", Core.Constants.Weight.Tonnes, reconDeclaration.InvoiceLines[0].JI_WeightUQ);

			AssertEquals("Volume is not converted, just cloned as it is", 249m, reconDeclaration.InvoiceLines[0].JI_Volume);
			AssertEquals("VolumeUQ", Core.Constants.Volume.CubicMetres, reconDeclaration.InvoiceLines[0].JI_VolumeUQ);

			AssertEquals("CountryOfOrigin copied", "CN", reconDeclaration.InvoiceLines[0].US_UC_NKCountryOfOrigin);
		}

		public void TestFindExistingEntryByReferenceNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			OrgHeader importerOfRecord = Factory.New<OrgHeader>();
			importerOfRecord.FillWithValidTestData();
			declaration.IOROrgPK = importerOfRecord.PK;
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			declaration.US_SuretyCode = "891";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();//addinfo gets serialised
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;

			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });

			AssertEquals("OriginalEntry should have been used", 1, reconDeclaration.OriginalEntries.Count);
		}

		[TestDate(2008, 3, 25)]
		public void TestCopyDutiesAndFees()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2203.00.00 60";
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine.US_UC_NKCountryOfExport = "NZ";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 15000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Excise Tax", 2300.84m, declaration.CustomsEntryHeaders[0].TotalEstimatedTax);
			Factory.Save();

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;
			originalEntry.US_R_NoLineDetails = true;

			new ReconImportEntryRetriever(reconDeclaration).ImportEntryDetails(originalEntry);
			AssertEquals("1 original entry created", 1, reconDeclaration.OriginalEntries.Count);
			AssertEquals("Charges should be copied from original entries", 2300.84m, reconDeclaration.OriginalEntries[0].OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise));
			AssertEquals("Charges should be copied to ReconCharges", 2300.84m, reconDeclaration.OriginalEntries[0].ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise));
		}

		public void TestDefaultIssueCodeAndSuretyCodeIfRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var importerOfRecord = Factory.NewWithValidTestData<OrgHeader>();
			declaration.IOROrgPK = importerOfRecord.PK;
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			declaration.US_FixRecon = true;
			declaration.US_SuretyCode = "891";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();//addinfo gets serialised

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });

			AssertEquals("IssueCode defaulted", ReconIssueCodeList.Codes.ClassRecon, reconDeclaration.US_IssueCode);
			AssertEquals("SuretyCode defaulted", "891", reconDeclaration.US_SuretyCode);
			AssertEquals("ImporterOfRecord defaulted", importerOfRecord.PK, reconDeclaration.IOROrgPK);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var importerOfRecord2 = Factory.NewWithValidTestData<OrgHeader>();
			declaration.IOROrgPK = importerOfRecord2.PK;
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.Class9802Recon;
			declaration.US_FixRecon = true;
			declaration.US_SuretyCode = "888";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();//addinfo gets serialised

			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });
			AssertEquals("IssueCode defaulted before stays there", ReconIssueCodeList.Codes.ClassRecon, reconDeclaration.US_IssueCode);
			AssertEquals("SuretyCode defaulted before stays there", "891", reconDeclaration.US_SuretyCode);
			AssertEquals("ImporterOfRecord defaulted before stays there", importerOfRecord.PK, reconDeclaration.IOROrgPK);
		}

		[TestDate(2008, 3, 25)]
		public void TestCopyExciseFees()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2203.00.00 60";
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine.US_UC_NKCountryOfExport = "NZ";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 15000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Excise Tax", 2300.84m, declaration.CustomsEntryHeaders[0].TotalEstimatedTax);
			Factory.Save();

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });
			reconDeclaration.OriginalEntries[0].US_R_DutyRateDate = ZDateTime.Today;

			AssertEquals("1 original entry created", 1, reconDeclaration.OriginalEntries.Count);

			reconDeclaration.CalculateDutyFeesForChangedEntries();

			AssertEquals("OtherExcise in OriginalCharges is calcualted and rolled up", 2300.84m, reconDeclaration.OriginalEntries[0].OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise));
		}

		public void TestImportOnSavedReconDeclaration()
		{
			var declaration1 = GetImportMergedDeclaration();

			var declaration2 = GetImportMergedDeclaration();

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconOriginalEntry = reconDec.OriginalEntries.AddNew();
			reconOriginalEntry.CH_OrigEntryReference = "XJ5" + declaration1.ActiveEntryHeaders[0].EntryNumber;
			reconOriginalEntry.CH_CH_OriginalEntry = declaration1.ActiveEntryHeaders[0].PK;

			reconDec.ImportLines(new JobDeclaration[] { declaration1 });
			Factory.Save();

			//Duplicate Top Group Invoice problem
			var factory = new BusinessObjectFactory();
			var reconDecLoaded = factory.Load<JobDeclaration>(reconDec.JE_PK);
			reconDec = new ReconDeclaration(reconDecLoaded);

			reconOriginalEntry = reconDec.OriginalEntries.AddNew();
			reconOriginalEntry.CH_OrigEntryReference = "XJ5" + declaration2.ActiveEntryHeaders[0].EntryNumber;
			reconOriginalEntry.CH_CH_OriginalEntry = declaration2.ActiveEntryHeaders[0].PK;

			AssertNoExceptionThrown(() => reconDec.ImportLines(new JobDeclaration[] { declaration2 }));
		}

		public void TestCopyFromOriginalDeclarationToReconOriginalEntry()
		{
			JobDeclaration declaration = GetImportMergedDeclaration();
			declaration.US_SchDEntry = "3901";
			declaration.JE_ExportDate = ZDateTime.BrettsBirthday;
			declaration.JE_GoodsDescription = "some goods";

			declaration.CustomsEntryHeaders[0].EntryNumber = "~7854378";
			Factory.Save();

			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());

			reconDec.ImportLines(new JobDeclaration[] { declaration });

			AssertEquals("1 recon entry", 1, reconDec.OriginalEntries.Count);
			AssertEquals("US_SchDEntry", "3901", reconDec.OriginalEntries[0].US_SchDEntry);
			AssertEquals("some goods", reconDec.OriginalEntries[0].US_R_GoodsDescription);
		}

		[TestDate(2008, 3, 25)]
		public void TestImportFromMultipleDeclarations()
		{
			JobDeclaration declaration1 = GetImportMergedDeclaration();
			declaration1.CustomsEntryHeaders[0].EntryNumber = "~7854378";
			declaration1.JE_OwnerRef = "Owner1";
			JobDeclaration declaration2 = GetImportMergedDeclaration();
			declaration2.CustomsEntryHeaders[0].EntryNumber = "~7854379";
			declaration2.JE_OwnerRef = "Owner2";

			Factory.Save();

			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());

			reconDec.ImportLines(new JobDeclaration[] { declaration1, declaration2 });

			ReconOriginalEntryHeader reconEntry = reconDec.OriginalEntries[0];
			ReconOriginalEntryHeader reconEntry2 = reconDec.OriginalEntries[1];

			AssertEquals("Two invoices imported", 2, reconDec.Invoices.Count);
			AssertCollectionContains(reconEntry.Invoice, reconDec.Invoices);
			AssertCollectionContains(reconEntry2.Invoice, reconDec.Invoices);

			AssertEquals("Owner Ref retrieved", "Owner1", reconEntry.US_R_OwnerRef);
			AssertEquals("Owner Ref retrieved", "Owner2", reconEntry2.US_R_OwnerRef);

			AssertForOneReconOriginalEntry(reconEntry.Invoice);

			AssertForOneReconOriginalEntry(reconEntry2.Invoice);
		}

		[TestDate(2008, 3, 25)]
		public void TestRetrieveSimpleInvoices()
		{
			JobDeclaration declaration = GetImportMergedDeclaration();
			declaration.CustomsEntryHeaders[0].EntryNumber = "~7854378";
			declaration.JE_ExportDate = ZDateTime.BrettsBirthday;

			AssertEquals("PreCondition", 45m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
			Factory.Save();

			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration2);
			ReconOriginalEntryHeader reconEntry = reconDec.OriginalEntries.AddNew();
			reconEntry.CH_OrigEntryReference = "XJ5~7854378";
			new ReconImportEntryRetriever(reconDec).ImportLines();

			AssertEquals("Invoice is copied", 1, reconDec.Invoices.Count);

			AssertForOneReconOriginalEntry(declaration2.Invoices[0]);
		}

		public void TestImportWatchRepair()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "~9342838";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				invoiceHeader.JZ_InvoiceAmount = 3406m;
				JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.US_SupTariff = "9802004040";    // repairs
				invoiceLine.US_98ValueInvCurr = 3406m;
				invoiceLine.JI_Tariff = "9102111010";
				invoiceLine.JI_LinePrice = 5258m;
				invoiceLine.JI_CustomsQuantity = 1000m;

				JobComInvoiceLine secondRepairLine = invoiceLine.AddSecondaryInvoiceLine();
				secondRepairLine.US_SupTariff = "9802004040";   // repairs
				secondRepairLine.JI_Tariff = "9102111020";
				secondRepairLine.JI_LinePrice = 2619m;
				secondRepairLine.JI_CustomsQuantity = 1000m;

				JobComInvoiceLine thirdRepairLine = invoiceLine.AddSecondaryInvoiceLine();
				thirdRepairLine.US_SupTariff = "9802004040";    // repairs
				thirdRepairLine.JI_Tariff = "9102111030";
				thirdRepairLine.JI_LinePrice = 1344m;
				thirdRepairLine.JI_CustomsQuantity = 1000m;

				JobComInvoiceLine fourthRepairLine = invoiceLine.AddSecondaryInvoiceLine();
				fourthRepairLine.US_SupTariff = "9802004040";   // repairs
				fourthRepairLine.JI_Tariff = "9102111040";
				fourthRepairLine.JI_LinePrice = 204m;
				fourthRepairLine.JI_CustomsQuantity = 1000m;
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader reconEntry = reconDec.OriginalEntries.AddNew();
			reconEntry.CH_OrigEntryReference = "XJ5~9342838";
			new ReconImportEntryRetriever(reconDec).ImportLines();

			AssertEquals(4, reconDec.InvoiceLines.Count);
			AssertEquals("9102111010", reconDec.InvoiceLines[0].JI_Tariff);
			AssertEquals("9802004040", reconDec.InvoiceLines[0].US_SupTariff);

			AssertEquals(3406m, reconDec.InvoiceLines[0].US_98GoodsValue);
			AssertEquals(3406m, reconDec.InvoiceLines[0].US_R_Orig98Value);

			AssertEquals(ZGuid.Empty, reconDec.InvoiceLines[0].JI_ParentID);
			AssertEquals(reconDec.InvoiceLines[0].PK, reconDec.InvoiceLines[1].JI_ParentID);
			AssertEquals(reconDec.InvoiceLines[0].PK, reconDec.InvoiceLines[2].JI_ParentID);
			AssertEquals(reconDec.InvoiceLines[0].PK, reconDec.InvoiceLines[3].JI_ParentID);
		}

		public void TestParentIDIsCopiedCorrectly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000";

			JobComInvoiceLine secondary1 = invoiceLine.AddSecondaryInvoiceLine();
			secondary1.JI_Tariff = "1111";
			secondary1.JI_CustomsUnitQty = "KG";
			secondary1.JI_CustomsQuantity = 100m;
			secondary1.JI_CustomsSecondQuantity = 200m;
			secondary1.US_SPI = "A";

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2222";

			JobComInvoiceLine secondary2 = invoiceLine2.AddSecondaryInvoiceLine();
			secondary2.JI_Tariff = "3333";
			secondary2.JI_CustomsUnitQty = "KG";
			secondary2.JI_CustomsQuantity = 110m;
			secondary2.JI_CustomsSecondQuantity = 210m;
			secondary2.US_SPI = "B";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.ImportLines(new JobDeclaration[] { declaration });
			JobComInvoiceHeader reconInvoice = reconDec.OriginalEntries[0].Invoice;
			AssertEquals("Four invoice lines should have been cloned", 4, reconInvoice.JobComInvoiceLines.Count);

			AssertEquals("PreCondition", "0000", reconInvoice.JobComInvoiceLines[0].JI_Tariff);
			AssertEquals("HasSecondaryLines", true, reconInvoice.JobComInvoiceLines[0].HasSecondaryTariffLines);
			AssertEquals("ChildLine details should have been cloned", "1111", reconInvoice.JobComInvoiceLines[0].SecondaryTariffLines.ElementAt(0).JI_Tariff);
			AssertEquals("ChildLine details should have been cloned", 100m, reconInvoice.JobComInvoiceLines[0].SecondaryTariffLines.ElementAt(0).JI_CustomsQuantity);
			AssertEquals("ChildLine details should have been cloned", 200m, reconInvoice.JobComInvoiceLines[0].SecondaryTariffLines.ElementAt(0).JI_CustomsSecondQuantity);
			AssertEquals("ChildLine details should have been cloned", "A", reconInvoice.JobComInvoiceLines[0].SecondaryTariffLines.ElementAt(0).US_SPI);

			AssertEquals("PreCondition", "2222", reconInvoice.JobComInvoiceLines[2].JI_Tariff);
			AssertEquals("HasSecondaryLines", true, reconInvoice.JobComInvoiceLines[2].HasSecondaryTariffLines);
			AssertEquals("ChildLine details should have been cloned", "3333", reconInvoice.JobComInvoiceLines[2].SecondaryTariffLines.ElementAt(0).JI_Tariff);
			AssertEquals("ChildLine details should have been cloned", 110m, reconInvoice.JobComInvoiceLines[2].SecondaryTariffLines.ElementAt(0).JI_CustomsQuantity);
			AssertEquals("ChildLine details should have been cloned", 210m, reconInvoice.JobComInvoiceLines[2].SecondaryTariffLines.ElementAt(0).JI_CustomsSecondQuantity);
			AssertEquals("ChildLine details should have been cloned", "B", reconInvoice.JobComInvoiceLines[2].SecondaryTariffLines.ElementAt(0).US_SPI);
		}

		public void TestRetrieveOriginalFields()
		{
			var declaration = GetImportMergedDeclaration();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Containerised;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.CustomsEntryHeaders[0].EntryNumber = "~7854378";
			declaration.InvoiceLines[0].JI_Tariff = "3201901000";
			declaration.InvoiceLines[0].US_SPI = PrimarySpecProgramIndicatorList.Codes.C;
			declaration.InvoiceLines[0].US_SecondarySPI = "G";
			declaration.InvoiceLines[0].US_UC_NKCountryOfOrigin = "AD";
			declaration.InvoiceLines[0].US_SelectedRateType = "P";
			declaration.InvoiceLines[0].JI_CustomsUnitQty = "NO";
			declaration.InvoiceLines[0].JI_CustomsQuantity = 50m;
			declaration.InvoiceLines[0].JI_CustomsSecondUnitQty = "XX";
			declaration.InvoiceLines[0].JI_CustomsSecondQuantity = 70m;
			declaration.InvoiceLines[0].JI_CustomsThirdUnitQty = "NO";
			declaration.InvoiceLines[0].JI_CustomsThirdQuantity = 71m;
			declaration.InvoiceLines[0].US_OverrideDuty = true;
			declaration.InvoiceLines[0].US_Duty = 340m;
			declaration.InvoiceLines[0].US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;

			var invoiceLine2 = declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6401.70.1002";
			invoiceLine2.JI_CustomsQuantity = 50m;
			invoiceLine2.JI_LinePrice = 3000m;
			invoiceLine2.US_SPI = "";
			invoiceLine2.US_SetInd = "X";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.ImportLines(new JobDeclaration[] { declaration });
			JobComInvoiceHeader invoice = reconDec.OriginalEntries[0].Invoice;

			AssertEquals("HMF Applicable is copied", "Y", reconDec.OriginalEntries[0].US_R_IsHMFApplicable);
			AssertEquals("reconDec has one invoiceLine", 2, invoice.JobComInvoiceLines.Count);
			AssertEquals("InvoiceLine is copied", "3201901000", invoice.JobComInvoiceLines[0].JI_Tariff);
			AssertEquals("InvoiceLine is copied", "3201901000", invoice.JobComInvoiceLines[0].US_R_OrigTariff);
			AssertEquals("InvoiceLine is copied", 50m, invoice.JobComInvoiceLines[0].JI_CustomsQuantity);
			AssertEquals("InvoiceLine is copied", 3000m, invoice.JobComInvoiceLines[0].US_R_OrigCV);
			AssertEquals("InvoiceLine is copied", 3000m, invoice.JobComInvoiceLines[0].JI_LinePrice);
			AssertEquals("InvoiceLine is copied", PrimarySpecProgramIndicatorList.Codes.C, invoice.JobComInvoiceLines[0].US_R_OrigSPI);
			AssertEquals("Secondary SPI should be empty, because this is not X or V line", "", invoice.JobComInvoiceLines[0].US_SecondarySPI);
			AssertEquals("InvoiceLine is copied", "P", invoice.JobComInvoiceLines[0].US_R_OrigRateType);
			AssertEquals("InvoiceLine is copied", 50m, invoice.JobComInvoiceLines[0].US_R_OrigFirstQty);
			AssertEquals("InvoiceLine is copied", "NO", invoice.JobComInvoiceLines[0].US_R_OrigFirstUQ);
			AssertEquals("InvoiceLine is copied", 70m, invoice.JobComInvoiceLines[0].US_R_OrigSecondQty);
			AssertEquals("InvoiceLine is copied", "XX", invoice.JobComInvoiceLines[0].US_R_OrigSecondUQ);
			AssertEquals("InvoiceLine is copied", 71m, invoice.JobComInvoiceLines[0].US_R_OrigThirdQty);
			AssertEquals("InvoiceLine is copied", "NO", invoice.JobComInvoiceLines[0].US_R_OrigThirdUQ);
			AssertEquals(YesNoDefaultList.Codes.Yes, invoice.JobComInvoiceLines[0].US_R_OrigCottonFeeExempt);
			AssertEquals(YesNoDefaultList.Codes.Yes, invoice.JobComInvoiceLines[0].US_CottonFeeExempt);

			AssertEquals("invoiceLine is copied", true, invoice.JobComInvoiceLines[0].US_OverrideDuty);
			AssertEquals("invoiceLine is copied", true, invoice.JobComInvoiceLines[0].US_R_OrigOverrideDuty);

			AssertEquals("invoiceLine is copied", 340m, invoice.JobComInvoiceLines[0].US_Duty);
			AssertEquals("invoiceLine is copied", 340m, invoice.JobComInvoiceLines[0].US_R_OrigDuty);

			AssertEquals("For InvoiceLine2 Secondary SPI should be empty, because not an ACE declaration", "", invoice.JobComInvoiceLines[1].US_SecondarySPI);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "SV9";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.ImportLines(new JobDeclaration[] { declaration });
			invoice = reconDec.OriginalEntries[0].Invoice;

			AssertEquals("Secondary SPI should be empty, because this is not X or V line", "", invoice.JobComInvoiceLines[0].US_SecondarySPI);
			AssertEquals("For InvoiceLine2 Secondary SPI should be X", "X", invoice.JobComInvoiceLines[1].US_SecondarySPI);
		}

		public void TestDefaultEntryDatesFromOriginalImportEntry()
		{
			JobDeclaration declaration = GetImportMergedDeclaration();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2008, 4, 1);
			declaration.US_PaymentDueDate = new ZDateTime(2008, 6, 2);
			declaration.US_PaymentDate = new ZDateTime(2008, 6, 1);
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.US_DutyCalcDate = new ZDateTime(2008, 4, 1);
			Factory.Save();

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.ImportLines(new JobDeclaration[] { declaration });

			AssertEquals("Payment date should be defaulted from declaration Payment Date if it is not empty",
				new ZDateTime(2008, 6, 1), reconDec.OriginalEntries[0].US_PaymentDate);
			AssertEquals("one original entry", 1, reconDec.OriginalEntries.Count);
			AssertEquals("Entry Date is defaulted", new ZDateTime(2008, 4, 1), reconDec.OriginalEntries[0].US_R_ReleaseDate);
			AssertEquals("Duty rate date is defaulted", new ZDateTime(2008, 4, 1), reconDec.OriginalEntries[0].US_R_DutyRateDate);
			AssertEquals(JobApplicationCodeList.Codes.ACE, reconDec.OriginalEntries[0].US_R_MsgMode);

			declaration.US_PaymentDate = ZDateTime.Empty;
			Factory.Save();
			reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.ImportLines(new JobDeclaration[] { declaration });
			AssertEquals("Payment date should be defaulted from declaration Payment Due Date if Payment Date is empty",
				new ZDateTime(2008, 6, 2), reconDec.OriginalEntries[0].US_PaymentDate);
		}

		[TestDate(2016, 1, 1)]
		public void TestRetrieveReconOriginalCharges()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_IsHMFApplicable = YesNoList.Codes.Yes;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.JZ_InvoiceAmount = 30000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.US_UC_NKCountryOfOrigin = "CN";

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = USCTariff.CottonFeeApplicable;
			invoiceLine1.JI_LinePrice = 9630m;
			invoiceLine1.JI_CustomsQuantity = 109m;
			invoiceLine1.JI_Weight = 250m;
			invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine1.JI_InvoiceQuantity = 250m;
			invoiceLine1.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;

			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = USCTariff.DairyFeeApplicable;
			invoiceLine2.JI_LinePrice = 12085m;
			invoiceLine2.JI_CustomsQuantity = 205m;
			invoiceLine2.JI_Weight = 350m;
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Pounds;
			invoiceLine2.JI_InvoiceQuantity = 350m;
			invoiceLine2.JI_InvoiceUQ = Core.Constants.Weight.Pounds;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;
			new ReconImportEntryRetriever(reconDeclaration).ImportLines(reconDeclaration.OriginalEntries.ToArray<ReconOriginalEntryHeader>());
			AssertEquals("Should be 2 Invoice Lines", 2, originalEntry.Invoice.InvoiceLines.Count);

			var reconInvoiceLine = originalEntry.Invoice.InvoiceLines[0];
			AssertEquals("Original Charge should be created during the import", 2, reconInvoiceLine.ReconOriginalCharges.Count);
			var hmf = reconInvoiceLine.ReconOriginalCharges.GetCharge(Core.Constants.USCustoms.FeeCodes.HMF);
			AssertEquals("HMF", 12.04m, hmf.CY_Amount);

			var mpf = reconInvoiceLine.ReconOriginalCharges.GetCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			AssertEquals("MPF", 33.36m, mpf.CY_Amount);
		}

		public void TestRetrieveEntryDetailsFromENSEntry_GetLastENSClearedMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "SV9";
			declaration.ImportEntryNumber = "71028110";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "5609004000";
			invoiceLine.US_CottonFeeExempt = "";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			ensEntry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;
			var mockOutgoingMessage1 = Factory.NewMoq<MQEDIMessage>();
			mockOutgoingMessage1.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoingMessage1 = mockOutgoingMessage1.Object;
			outgoingMessage1.EM_MessageNum = "HYEDUSCMT_189201";
			outgoingMessage1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingMessage1.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingMessage1.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_189201     " +
				"10ASV9  71028110 1101B00170020   0110 XA          2060817                       " +
				"1113-14792700013-147927000                     052917       DC                  " +
				"20APLU1101052917    ADMIRAL BULKER                                              " +
				"21324                                                                           " +
				"2200000001PC                                                                    " +
				"23MAPLU324897DSHU                                                               " +
				"318B 037                                                                        " +
				"SE30SE CH 1 IMPORTER/EXPORTER                                                   " +
				"SE3515CHEMIN DU TOURBILLON 8                                                    " +
				"SE36PLAN LES                                    1228           CH               " +
				"40  001 CHCH052217        0000000500602670000000003    N                        " +
				"47MCHHARWIN8PLA                                                                 " +
				"47C13-147927000                                                                 " +
				"47S13-147927000                                                                 " +
				"SE50MF CH 1 IMPORTER/EXPORTER                                                   " +
				"SE5515CHEMIN DU TOURBILLON 8                                                    " +
				"SE56PLAN LES                                    1228           CH               " +
				"505609004000 0000019500 0000005000 000000000200KG                               " +
				"6250100000625                                                                   " +
				"6249900001732                                                                   " +
				"6205600000000                                                                   " +
				"89501000000006254990000000250005600000000000                                    " +
				"9000000019500 00000003125 00000000000 00000000000 00000000000                   " +
				"Y  1101SV9AE";
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_Status = MQEDIMessage.Status.Sent;
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			ensEntry.Messages.Add(outgoingMessage1);

			var incomingMessage1 = Factory.New<MQEDIMessage>();
			incomingMessage1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incomingMessage1.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			incomingMessage1.EM_Status = MQEDIMessage.Status.Received;
			incomingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage1.EM_MessageNum = "HYEDUSCMT_189201";
			incomingMessage1.EM_MessageText = "B001101SV9AX                                               HYEDUSCMT_189201     " +
				"E0 SUMMRY 000001 REF ID: SV9 71028110 B00170020    173                          " +
				"E0 LINITM 000001 REF ID: 001                                                    " +
				"E0 TARIFF 000001 REF ID: 5609004000                                             " +
				"E1 W27D   *CENSUS* OR-HI VAL/QTY (1)              SV9  71028110     B00170020   " +
				"E1AW524   TRANSACTION DATA REJECTED               SV9  7102811000100B00170020   " +
				"Y  1101SV9AX00005";
			ensEntry.Messages.Add(incomingMessage1);

			var mockOutgoingMessage2 = Factory.NewMoq<MQEDIMessage>();
			mockOutgoingMessage2.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoingMessage2 = mockOutgoingMessage2.Object;
			outgoingMessage2.EM_MessageNum = "HYEDUSCMT_189202";
			outgoingMessage2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingMessage2.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingMessage2.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_189202     " +
				"10ASV9  71028110 1101B00170020   0110 XA          2060817                       " +
				"1113-14792700013-147927000                     052917       DC                  " +
				"20APLU1101052917    ADMIRAL BULKER                                              " +
				"21324                                                                           " +
				"2200000001PC                                                                    " +
				"23MAPLU324897DSHU                                                               " +
				"318B 037                                                                        " +
				"SE30SE CH 1 IMPORTER/EXPORTER                                                   " +
				"SE3515CHEMIN DU TOURBILLON 8                                                    " +
				"SE36PLAN LES                                    1228           CH               " +
				"40  001 CHCH052217        0000000500602670000000003    N 1                      " +
				"47MCHHARWIN8PLA                                                                 " +
				"47C13-147927000                                                                 " +
				"47S13-147927000                                                                 " +
				"SE50MF CH 1 IMPORTER/EXPORTER                                                   " +
				"SE5515CHEMIN DU TOURBILLON 8                                                    " +
				"SE56PLAN LES                                    1228           CH               " +
				"505609004000 0000019500 0000005000 000000000200KG                               " +
				"6250100000625                                                                   " +
				"6249900001732                                                                   " +
				"6205600000000                                                                   " +
				"89501000000006254990000000250005600000000000                                    " +
				"9000000019500 00000003125 00000000000 00000000000 00000000000                   " +
				"Y  1101SV9AE";
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_Status = MQEDIMessage.Status.Sent;
			outgoingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-1);
			ensEntry.Messages.Add(outgoingMessage2);

			var incomingMessage2 = Factory.New<MQEDIMessage>();
			incomingMessage2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incomingMessage2.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			incomingMessage2.EM_Status = MQEDIMessage.Status.Received;
			incomingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage2.EM_MessageNum = "HYEDUSCMT_189202";
			incomingMessage2.EM_MessageText = "B001101SV9AX                                               HYEDUSCMT_189202     " +
				"E0 SUMMRY 000001 REF ID: SV9 71028110 B00170020    173                          " +
				"E0 LINITM 000001 REF ID: 001                                                    " +
				"E0 TARIFF 000001 REF ID: 5609004000                                             " +
				"E1 W27D   *CENSUS* OR-HI VAL/QTY (1)              SV9  71028110     B00170020   " +
				"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  7102811000100B00170020   " +
				"Y  1101SV9AX00005";
			ensEntry.Messages.Add(incomingMessage2);
			Factory.Save();

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });
			AssertEquals(1, reconDeclaration.OriginalEntries.Count);
			AssertEquals(false, reconDeclaration.OriginalEntries[0].US_R_CottonFeeMandatory);
		}

		[TestDate(2018, 10, 24)]
		public void TestDefaultCottonFeeMandatoryFromOriginalEntry()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration1.US_EnableENS = true;
			declaration1.US_EntryFilerCode = "SV9";
			declaration1.ImportEntryNumber = "71028110";
			var invoice1 = declaration1.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "5609004000";
			invoiceLine1.US_CottonFeeExempt = "";
			declaration1.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var ensEntry1 = declaration1.ActiveEntryHeaders.EntrySummaryEntry;
			ensEntry1.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;
			var mockIncomingMessage1 = Factory.NewMoq<MQEDIMessage>();
			mockIncomingMessage1.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var incomingMessage1 = mockIncomingMessage1.Object;
			incomingMessage1.EM_MessageNum = "HYEDUSCMT_189201";
			incomingMessage1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			incomingMessage1.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			incomingMessage1.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_189201     10ASV9  71028110 1101B00170020   0110 XA          2060817                       1113-14792700013-147927000                     052917       DC                  20APLU1101052917    ADMIRAL BULKER                                              21324                                                                           2200000001PC                                                                    23MAPLU324897DSHU                                                               318B 037                                                                        SE30SE CH 1 IMPORTER/EXPORTER                                                   SE3515CHEMIN DU TOURBILLON 8                                                    SE36PLAN LES                                    1228           CH               40  001 CHCH052217        0000000500602670000000003    N                        47MCHHARWIN8PLA                                                                 47C13-147927000                                                                 47S13-147927000                                                                 SE50MF CH 1 IMPORTER/EXPORTER                                                   SE5515CHEMIN DU TOURBILLON 8                                                    SE56PLAN LES                                    1228           CH               505609004000 0000019500 0000005000 000000000200KG                               6250100000625                                                                   6249900001732                                                                   6205600000000                                                                   89501000000006254990000000250005600000000000                                    9000000019500 00000003125 00000000000 00000000000 00000000000                   Y  1101SV9AE";
			incomingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			incomingMessage1.EM_Status = MQEDIMessage.Status.Sent;
			ensEntry1.Messages.Add(incomingMessage1);

			var outgoingMessage1 = Factory.New<MQEDIMessage>();
			outgoingMessage1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incomingMessage1.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingMessage1.EM_Status = MQEDIMessage.Status.Received;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			outgoingMessage1.EM_MessageNum = "HYEDUSCMT_189201";
			outgoingMessage1.EM_MessageText = "B001101SV9AX                                               HYEDUSCMT_189201     " +
				"E0 SUMMRY 000001 REF ID: SV9 71028110 B00170020    173                          " +
				"E0 LINITM 000001 REF ID: 001                                                    " +
				"E0 TARIFF 000001 REF ID: 5609004000                                             " +
				"E1 W27D   *CENSUS* OR-HI VAL/QTY (1)              SV9  71028110     B00170020   " +
				"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  7102811000100B00170020   Y  1101SV9AX00005";
			ensEntry1.Messages.Add(outgoingMessage1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration2.US_EnableENS = true;
			declaration2.US_EntryFilerCode = "SV9";
			declaration2.ImportEntryNumber = "70044456";
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "5609004000";
			invoiceLine2.US_CottonFeeExempt = "";
			declaration2.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var ensEntry2 = declaration2.ActiveEntryHeaders.EntrySummaryEntry;
			ensEntry2.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			var mockIncomingMessage2 = Factory.NewMoq<MQEDIMessage>();
			mockIncomingMessage2.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var incomingMessage2 = mockIncomingMessage2.Object;
			incomingMessage2.EM_MessageNum = "HYEDUSCMT_189204";
			incomingMessage2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			incomingMessage2.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			incomingMessage2.EM_MessageText =
				"B  1101SV9AE                                               HYEDUSCMT_189204     " +
				"10ASV9  70044456 1101B00170021   0110 XA          2060817                       " +
				"1113-14792700013-147927000                     052917       DC                  " +
				"20APLU1101052917    ADMIRAL BULKER                                              " +
				"214398                                                                          " +
				"2200000001PC                                                                    " +
				"23MAPLU3240987H                                                                 " +
				"318B 037                                                                        " +
				"SE30SE CH 1 IMPORTER/EXPORTER                                                   " +
				"SE3515CHEMIN DU TOURBILLON 8                                                    " +
				"SE36PLAN LES                                    1228           CH               40  001 CHCH052217        0000000500602670000000250    N                        47MCHHARWIN8PLA                                                                 47C13-147927000                                                                 47S13-147927000                                                                 SE50MF CH 1 IMPORTER/EXPORTER                                                   SE5515CHEMIN DU TOURBILLON 8                                                    SE56PLAN LES                                    1228           CH               505609004000 0000019500 0000005000 000000020000KG                               6205600000046                                                                   6250100000625                                                                   6249900001732                                                                   89056000000000464990000000250050100000000625                                    9000000019500 00000003171 00000000000 00000000000 00000000000                   Y  1101SV9AE";
			incomingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			incomingMessage2.EM_Status = MQEDIMessage.Status.Sent;
			ensEntry2.Messages.Add(incomingMessage2);

			var outgoingMessage2 = Factory.New<MQEDIMessage>();
			outgoingMessage2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			outgoingMessage2.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingMessage2.EM_Status = MQEDIMessage.Status.Received;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			outgoingMessage2.EM_MessageNum = "HYEDUSCMT_189204";
			outgoingMessage2.EM_MessageText =
		"B001101SV9AX                                               HYEDUSCMT_189204     " +
		"E0 SUMMRY 000001 REF ID: SV9 70044456 B00170021    173                          " +
		"E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7004445600100B00170021   " +
		"Y  1101SV9AX00002";
			ensEntry2.Messages.Add(outgoingMessage2);
			Factory.Save();

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration1, declaration2 });
			AssertEquals(2, reconDeclaration.OriginalEntries.Count);
			AssertEquals(true, reconDeclaration.OriginalEntries[0].US_R_CottonFeeMandatory);
			AssertEquals(false, reconDeclaration.OriginalEntries[1].US_R_CottonFeeMandatory);

			reconDeclaration.OriginalEntries[0].US_R_CottonFeeMandatory = false;
			new ReconImportEntryRetriever(reconDeclaration).ImportLinesFromImportEntry(reconDeclaration.OriginalEntries[0]);
			AssertEquals(true, reconDeclaration.OriginalEntries[0].US_R_CottonFeeMandatory);
		}

		void CreateOrLoadTariff(ZString tariffCode, string dutyComputationCode = "")
		{
			var tariff = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, tariffCode));
			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffCode;
			tariff.UE_DateFrom = ZDateTime.Now.AddDays(-100);
			tariff.UE_DateTo = ZDateTime.Now.AddDays(100);
			tariff.UE_DutyComputationCode = dutyComputationCode;
		}

		JobDeclaration GetImportMergedDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "SG";
			invoice.US_UC_NKCountryOfOrigin = "SG";
			invoice.Charges.AddNew("OFT", 50m, "USD");

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.US_SPI = "";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			return declaration;
		}

		void AssertForOneReconOriginalEntry(JobComInvoiceHeader invoice)
		{
			AssertEquals("Invoice is copied", Core.Constants.CurrencyCodes.UnitedStates, invoice.JZ_RX_NKInvoice_Currency);

			AssertEquals("reconDec has one invoiceLine", 1, invoice.JobComInvoiceLines.Count);
			AssertEquals("InvoiceLine is copied", "3201901000", invoice.JobComInvoiceLines[0].JI_Tariff);
			AssertEquals("InvoiceLine is copied", 50m, invoice.JobComInvoiceLines[0].JI_CustomsQuantity);
			AssertEquals("InvoiceLine is copied", 3000m, invoice.JobComInvoiceLines[0].JI_LinePrice);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
