using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconChangedLinesMergerTest : TestCaseWithFactory
	{
		public void TestGetErrors()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			JobComInvoiceHeader invoice = reconDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			ZString errors = new ReconChangedLinesMerger(reconDeclaration).GetErrors();
			AssertEquals("no entry lines have been entered", ReconChangedLinesMerger.NoInvoiceLinesHaveBeenEntered, errors);
		}

		[TestDate(2017, 12, 1)]
		public void TestMergeWithSupTariffs()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.US_IssueCode = ReconIssueCodeList.Codes._9802Recon;
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "99040201";
			tariff1.UE_DateFrom = new ZDateTime(2016, 1, 1);
			tariff1.UE_DateTo = ZDateTime.Today.AddDays(1);
			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0201308010";
			tariff2.UE_DateFrom = new ZDateTime(2016, 2, 1);
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(1);
			ReconOriginalEntryHeader originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ52";
			originalEntry.US_ImportDate = new ZDateTime(2016, 2, 15);
			originalEntry.US_R_DutyRateDate = new ZDateTime(2016, 3, 5);
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_SchDEntry = "1234";
			originalEntry.US_R_CalcOrigDuty = true;
			JobComInvoiceHeader invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_InvoiceNumber = "HAOA780";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99040201";
			invoiceLine.JI_Tariff = "0201.30.8010";
			invoiceLine.JI_CustomsQuantity = 1500m;
			invoiceLine.JI_LinePrice = 1000m;
			SetReconOriginalValues(invoiceLine);
			invoiceLine.JI_LinePrice = 1200m; //changed
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0201.30.8010";
			invoiceLine2.JI_CustomsQuantity = 1500m;
			invoiceLine2.JI_LinePrice = 1000m;
			SetReconOriginalValues(invoiceLine2);
			invoiceLine2.JI_LinePrice = 1200m; //changed
			JobComInvoiceLine invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.US_SupTariff = "99040201";
			invoiceLine3.JI_Tariff = "0201.30.8010";
			invoiceLine3.JI_CustomsQuantity = 1500m;
			invoiceLine3.JI_LinePrice = 1000m;
			SetReconOriginalValues(invoiceLine3);
			invoiceLine3.JI_LinePrice = 1200m; //changed
			invoiceLine3.US_UC_NKCountryOfOrigin = "JP"; //should not be merged with the first invoice line
			reconDec.CalculateDutyFeesForAllEntries();
			new ReconChangedLinesMerger(reconDec).DoMerge(false);
			ReconChangedLineCollection changedLines = reconDec.ChangedLines;
			AssertEquals("Three changed lines", 5, changedLines.Count);
			AssertEquals("1A", changedLines[0].US_ReconLineNumber);
			AssertEquals("1B", changedLines[1].US_ReconLineNumber);
			AssertEquals("2", changedLines[2].US_ReconLineNumber);
			AssertEquals("3A", changedLines[3].US_ReconLineNumber);
			AssertEquals("3B", changedLines[4].US_ReconLineNumber);
			AssertEquals("99040201", changedLines[0].US_Tariff);
			AssertEquals(new ZDateTime(2016, 1, 1), changedLines[0].US_OriginalHTSEffectiveDate);
			AssertEquals("0201308010", changedLines[1].US_Tariff);
			AssertEquals(new ZDateTime(2016, 2, 1), changedLines[1].US_OriginalHTSEffectiveDate);
			AssertEquals("0201308010", changedLines[2].US_Tariff);
			AssertEquals(new ZDateTime(2016, 2, 1), changedLines[2].US_OriginalHTSEffectiveDate);
			AssertEquals(0m, changedLines[0].US_MPF);
			var totalMPF = ZDecimal.Zero;
			foreach (ReconChangedLine changedLine in changedLines)
			{
				totalMPF += changedLine.US_MPF;
			}

			AssertEquals(12.48m, totalMPF);
		}

		public void TestSortWhenMerging()
		{
			var reconDeclaration = GetReconDeclaration();
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			var changedLines = reconDeclaration.ChangedLines;
			AssertEquals("No changed lines", 0, changedLines.Count);
			reconDeclaration.InvoiceLines[0].JI_LinePrice = 4000m;
			reconDeclaration.InvoiceLines[1].JI_LinePrice = 8000m;
			//this is a parent line and its child line should be included as a changed line
			reconDeclaration.Invoices[1].InvoiceLines[0].US_SupTariff = "9802008020";
			reconDeclaration.Invoices[1].InvoiceLines[1].JI_Tariff = "0102904090";
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			changedLines = reconDeclaration.ChangedLines;
			AssertEquals("All lines are changed. invoiceLine and invoiceLine2 are merged", 4, changedLines.Count);
			AssertEquals("First changed line", "9802008020", changedLines[0].US_Tariff);
			AssertEquals("Second changed line", "8421394000", changedLines[1].US_Tariff);
			AssertEquals("Third changed line", "0102904090", changedLines[2].US_Tariff);
			AssertEquals("Forth changed line", "3201901000", changedLines[3].US_Tariff);
		}

		public void TestSortBySPIWhenMerging()
		{
			var reconDeclaration = GetReconDeclaration();
			reconDeclaration.InvoiceLines[0].JI_LinePrice = 4000m;
			reconDeclaration.InvoiceLines[1].JI_LinePrice = 8000m;

			reconDeclaration.Invoices[0].InvoiceLines[0].US_SupTariff = "9802008021";
			reconDeclaration.Invoices[0].InvoiceLines[0].US_R_OrigEntryLineNo = "1";
			reconDeclaration.Invoices[0].InvoiceLines[1].US_R_OrigEntryLineNo = "1";
			reconDeclaration.Invoices[0].InvoiceLines[1].US_R_OrigSPI = "S";

			reconDeclaration.Invoices[1].InvoiceLines[1].JI_Tariff = "0102904090";
			reconDeclaration.Invoices[1].InvoiceLines[0].US_R_OrigEntryLineNo = "1";
			reconDeclaration.Invoices[1].InvoiceLines[1].US_R_OrigEntryLineNo = "1";
			reconDeclaration.Invoices[1].InvoiceLines[1].US_R_OrigSPI = "S";

			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			var changedLines = reconDeclaration.ChangedLines;
			AssertEquals("All lines are changed. invoiceLine and invoiceLine2 are merged", 4, changedLines.Count);
			AssertEquals("First changed line", "0102904090", changedLines[0].US_Tariff);
			AssertEquals("Second changed line", "3201901000", changedLines[1].US_Tariff);
			AssertEquals("Third changed line", "9802008021", changedLines[2].US_Tariff);
			AssertEquals("Forth changed line", "3201901000", changedLines[3].US_Tariff);
		}

		public void TestSortByLineNoWhenMerging()
		{
			var reconDeclaration = GetReconDeclaration();
			reconDeclaration.InvoiceLines[0].JI_LinePrice = 4000m;
			reconDeclaration.InvoiceLines[1].JI_LinePrice = 8000m;

			reconDeclaration.Invoices[0].InvoiceLines[0].US_R_OrigEntryLineNo = "4";
			reconDeclaration.Invoices[0].InvoiceLines[1].US_R_OrigEntryLineNo = "3";

			reconDeclaration.Invoices[1].InvoiceLines[0].US_SupTariff = "9802008020";
			reconDeclaration.Invoices[1].InvoiceLines[1].JI_Tariff = "0102904090";
			reconDeclaration.Invoices[1].InvoiceLines[0].US_R_OrigEntryLineNo = "2";
			reconDeclaration.Invoices[1].InvoiceLines[1].US_R_OrigEntryLineNo = "1";

			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			var changedLines = reconDeclaration.ChangedLines;
			AssertEquals("All lines are changed. invoiceLine and invoiceLine2 are merged", 4, changedLines.Count);
			AssertEquals("First changed line", "0102904090", changedLines[0].US_Tariff);
			AssertEquals("Second changed line", "9802008020", changedLines[1].US_Tariff);
			AssertEquals("Third changed line", "8421394000", changedLines[2].US_Tariff);
			AssertEquals("Forth changed line", "3201901000", changedLines[3].US_Tariff);
		}

		public void TestInitialisedValuesWhenMerging()
		{
			ReconDeclaration reconDeclaration = GetReconDeclaration();
			reconDeclaration.InvoiceLines[0].JI_LinePrice = 4000m;
			reconDeclaration.InvoiceLines[1].JI_LinePrice = 8000m;
			//this is a parent line and its child line should be included as a changed line
			reconDeclaration.Invoices[0].InvoiceLines[0].US_R_OrigTariff = "8421.39.4000";
			reconDeclaration.Invoices[0].InvoiceLines[1].US_R_OrigTariff = "8421.39.4000";
			reconDeclaration.Invoices[1].InvoiceLines[0].US_R_OrigSupTariff = "3201.90.1000";
			reconDeclaration.Invoices[1].InvoiceLines[0].US_R_OrigTariff = "0102.90.4084";
			reconDeclaration.Invoices[1].InvoiceLines[1].US_R_OrigTariff = "8421.39.4000";
			reconDeclaration.Invoices[1].InvoiceLines[0].US_SupTariff = "9802008020";
			reconDeclaration.Invoices[1].InvoiceLines[1].JI_Tariff = "0102904090";
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			ReconChangedLineCollection changedLines = reconDeclaration.ChangedLines;
			AssertEquals("PreCondition", 4, changedLines.Count);
			AssertEquals("LineNumber", "1A", changedLines[0].US_ReconLineNumber);
			AssertEquals("LineNumber", "1B", changedLines[1].US_ReconLineNumber);
			AssertEquals("LineNumber", "2", changedLines[2].US_ReconLineNumber);
			AssertEquals("LineNumber", "3", changedLines[3].US_ReconLineNumber);
			AssertEquals("Year", "2008", changedLines[0].US_Year);
			AssertEquals("Year", "2007", changedLines[3].US_Year);
			AssertEquals("IssueCode", ReconIssueCodeList.Codes._9802Recon, changedLines[0].US_ReconReason);
			AssertEquals("IssueCode", ReconIssueCodeList.Codes._9802Recon, changedLines[3].US_ReconReason);
			AssertEquals("Origin", "KR", changedLines[0].US_UC_NKCountryOfOrigin);
			AssertEquals("Origin", "KR", changedLines[3].US_UC_NKCountryOfOrigin);
			AssertEquals("HTS number", "9802008020", changedLines[0].US_Tariff);
			AssertEquals("Orig HTS number", "3201901000", changedLines[0].US_OrigTariff);
			AssertEquals("HTS number", "8421394000", changedLines[1].US_Tariff);
			AssertEquals("Orig HTS number", "0102904084", changedLines[1].US_OrigTariff);
			AssertEquals("HTS number", "0102904090", changedLines[2].US_Tariff);
			AssertEquals("Orig HTS number", "8421394000", changedLines[2].US_OrigTariff);
			AssertEquals("HTS number", "3201901000", changedLines[3].US_Tariff);
			AssertEquals("Orig HTS number", "8421394000", changedLines[3].US_OrigTariff);
		}

		public void TestAggregatePort()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ52";
			originalEntry.US_ImportDate = new ZDateTime(2007, 12, 31);
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 5);
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_SchDEntry = "1234";
			JobComInvoiceHeader invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_InvoiceNumber = "HAOA780";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;
			SetReconOriginalValues(invoiceLine);
			ReconOriginalEntryHeader originalEntry2 = reconDeclaration.OriginalEntries.AddNew();
			originalEntry2.CH_OrigEntryReference = "XJ52";
			originalEntry2.US_ImportDate = new ZDateTime(2007, 12, 31);
			originalEntry2.US_R_DutyRateDate = new ZDateTime(2008, 1, 5);
			originalEntry2.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry2.US_SchDEntry = "2345"; //different to the first one
			JobComInvoiceHeader invoice2 = originalEntry2.Invoice;
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_InvoiceNumber = "HAOA780";
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.US_UC_NKCountryOfExport = "KR";
			invoice2.US_UC_NKCountryOfOrigin = "KR";
			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3201.90.1000";
			invoiceLine2.JI_CustomsQuantity = 50m;
			invoiceLine2.JI_LinePrice = 3000m;
			SetReconOriginalValues(invoiceLine2);
			Factory.Save();
			invoiceLine.JI_LinePrice = 2000m;
			invoiceLine2.JI_LinePrice = 4000m;
			Factory.Save();
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			ReconChangedLineCollection changedLines = reconDeclaration.ChangedLines;
			AssertEquals("One changed line only", 1, changedLines.Count);
			AssertEquals("Port merged", "All", changedLines[0].US_SchDEntry);
			originalEntry2.US_SchDEntry = "1234";
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			changedLines = reconDeclaration.ChangedLines;
			AssertEquals("One changed line only", 1, changedLines.Count);
			AssertEquals("Port merged", "1234", changedLines[0].US_SchDEntry);
		}

		public void TestAggregateValuesWhenMerging()
		{
			var reconDeclaration = GetReconDeclaration();
			reconDeclaration.OriginalEntries[0].US_R_CalcOrigDuty = true;
			reconDeclaration.OriginalEntries[1].US_R_CalcOrigDuty = true;
			reconDeclaration.InvoiceLines[0].JI_LinePrice = 4000m;
			reconDeclaration.InvoiceLines[1].JI_LinePrice = 8000m;
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			var changedLines = reconDeclaration.ChangedLines;
			AssertEquals("PreCondition", 1, changedLines.Count);
			AssertEquals("CustomsValue", 12000m, changedLines[0].US_CustomsValue);
			AssertEquals("OrigCustomsValue", 10000m, changedLines[0].US_OrigCustomsValue);
			AssertEquals("ReconDuty", 180m, changedLines[0].US_Duty);
			AssertEquals("OriginalDuty", 150m, changedLines[0].US_OrigDuty);
		}

		public void TestAggregateFeesWhenMerging()
		{
			var reconDeclaration = GetReconDeclaration();
			reconDeclaration.InvoiceLines[1].JI_LinePrice = 7001m;
			reconDeclaration.InvoiceLines[3].JI_CustomsQuantity = 320m;
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			Factory.Save();
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			ReconChangedLineCollection changedLines = reconDeclaration.ChangedLines;
			AssertEquals("PreCondition", 2, changedLines.Count);
			AssertEquals("Recon OtherFee", 320m, changedLines[0].US_OtherFee);
			AssertEquals("Orig OtherFee", 100m, changedLines[0].US_OrigOtherFee);
			AssertEquals("Recon HMF", 8.75m, changedLines[1].US_HMF);
			AssertEquals("Orig HMF", 8.75m, changedLines[1].US_OrigHMF);
		}

		public void TestRoundCustomsValue()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = originalEntry.Invoice;
			JobComInvoiceLine invoiceLine = reconDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_LinePrice = 1201.89m;
			invoiceLine.US_R_OrigCV = 1200.52m;
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			Factory.Save();
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			ReconChangedLineCollection changedLines = reconDeclaration.ChangedLines;
			AssertEquals("one change line is expected", 1, changedLines.Count);
			AssertEquals("CustomsValue", 1202m, changedLines[0].US_CustomsValue);
			AssertEquals("OriginalCustomsValue", 1201m, changedLines[0].US_OrigCustomsValue);
		}

		public void TestInitialiseYear()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = originalEntry.Invoice;
			JobComInvoiceLine invoiceLine = reconDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_LinePrice = 1201.89m;
			invoiceLine.US_R_OrigCV = 1200.52m;
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			Factory.Save();
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			ReconChangedLineCollection changedLines = reconDeclaration.ChangedLines;
			AssertEquals("No year is initialised as import date is empty", "", changedLines[0].US_Year);
			originalEntry.US_ImportDate = ZDateTime.BrettsBirthday;
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			changedLines = reconDeclaration.ChangedLines;
			AssertEquals("Still one line", 1, changedLines.Count);
			AssertEquals("No year is initialised as import date is empty", "1971", changedLines[0].US_Year);
		}

		[TestDate(2017, 12, 1)]
		public void TestAggregatingMPF()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_R_CalcOrigDuty = true;
			var invoice = originalEntry.Invoice;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6206.90.0040";
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_R_OrigTariff = "6206.90.0040";
			invoiceLine.US_R_OrigCV = 2000.00m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6206.90.0040";
			invoiceLine2.US_UC_NKCountryOfExport = "NZ";
			invoiceLine2.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine2.JI_LinePrice = 4000m;
			invoiceLine2.US_R_OrigTariff = "6206.90.0040";
			invoiceLine2.US_R_OrigCV = 2000.00m;
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			Factory.Save();
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			AssertEquals(2, reconDeclaration.ChangedLines.Count);
			AssertEquals("Original MPF for line1", 6.93m, reconDeclaration.ChangedLines[0].US_OrigMPF);
			AssertEquals("Original MPF for line2", 6.93m, reconDeclaration.ChangedLines[1].US_OrigMPF);
			AssertEquals("Recon MPF for line1", 3.46m, reconDeclaration.ChangedLines[0].US_MPF);
			AssertEquals("Recon MPF for line2", 13.86m, reconDeclaration.ChangedLines[1].US_MPF);
		}

		[TestDate(2009, 6, 1)]
		public void TestAggregatingMPFWithRoundingIssue()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_R_CalcOrigDuty = true;
			var invoice = originalEntry.Invoice;
			//Three invoice lines with equal values
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6206.90.0040";
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_R_OrigTariff = "6206.90.0040";
			invoiceLine.US_R_OrigCV = 3000m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6206.90.0040";
			invoiceLine2.US_UC_NKCountryOfExport = "NZ";
			invoiceLine2.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine2.JI_LinePrice = 1000m;
			invoiceLine2.US_R_OrigTariff = "6206.90.0040";
			invoiceLine2.US_R_OrigCV = 3000m;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "6206.90.0040";
			invoiceLine3.US_UC_NKCountryOfExport = "KR";
			invoiceLine3.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine3.JI_LinePrice = 1000m;
			invoiceLine3.US_R_OrigTariff = "6206.90.0040";
			invoiceLine3.US_R_OrigCV = 3000m;
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			Factory.Save();
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			AssertEquals(3, reconDeclaration.ChangedLines.Count);
			AssertEquals("Original MPF for line1", 6.30m, reconDeclaration.ChangedLines[0].US_OrigMPF);
			AssertEquals("Original MPF for line2", 6.30m, reconDeclaration.ChangedLines[1].US_OrigMPF);
			AssertEquals("Original MPF for line3", 6.30m, reconDeclaration.ChangedLines[2].US_OrigMPF);
			AssertEquals("Recon MPF for line1", 2.1m, reconDeclaration.ChangedLines[0].US_MPF);
			AssertEquals("Recon MPF for line2", 2.1m, reconDeclaration.ChangedLines[1].US_MPF);
			AssertEquals("Recon MPF for line3", 2.1m, reconDeclaration.ChangedLines[2].US_MPF);
			Assert("Orignal and Recon MPF are its min amount, $25", !originalEntry.HasMPFChanges);
			AssertEquals(0m, reconDeclaration.ChangedLines[0].MPFChange);
			AssertEquals(0m, reconDeclaration.ChangedLines[1].MPFChange);
			AssertEquals(0m, reconDeclaration.ChangedLines[2].MPFChange);
		}

		[TestDate(2009, 6, 1)]
		public void TestAggregateMPFWhenThereIsLineNotSubjectToMPF()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_R_CalcOrigDuty = true;
			var invoice = originalEntry.Invoice;
			//Three invoice lines with equal values
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6206.90.0040";
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "AU"; //MPF exempt
			invoiceLine.US_R_OrigSPI = ""; //MPF not exempt
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_R_OrigTariff = "6206.90.0040";
			invoiceLine.US_R_OrigCV = 3000m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6206.90.0040";
			invoiceLine2.US_UC_NKCountryOfExport = "NZ";
			invoiceLine2.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine2.JI_LinePrice = 1000m;
			invoiceLine2.US_R_OrigTariff = "6206.90.0040";
			invoiceLine2.US_R_OrigCV = 3000m;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "6206.90.0040";
			invoiceLine3.US_UC_NKCountryOfExport = "KR";
			invoiceLine3.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine3.JI_LinePrice = 1000m;
			invoiceLine3.US_R_OrigTariff = "6206.90.0040";
			invoiceLine3.US_R_OrigCV = 3000m;
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			Factory.Save();
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			AssertEquals(3, reconDeclaration.ChangedLines.Count);
			AssertEquals("Original MPF for line1", 6.30m, reconDeclaration.ChangedLines[0].US_OrigMPF);
			AssertEquals("Original MPF for line2", 6.30m, reconDeclaration.ChangedLines[1].US_OrigMPF);
			AssertEquals("Original MPF for line3", 6.30m, reconDeclaration.ChangedLines[2].US_OrigMPF);
			AssertEquals("Recon MPF for line1", 0m, reconDeclaration.ChangedLines[0].US_MPF);
			AssertEquals("Recon MPF for line2", 2.10m, reconDeclaration.ChangedLines[1].US_MPF);
			AssertEquals("Recon MPF for line3", 2.10m, reconDeclaration.ChangedLines[2].US_MPF);
		}

		ReconDeclaration GetReconDeclaration()
		{
			var result = new ReconDeclaration(Factory.New<JobDeclaration>());
			result.US_IssueCode = ReconIssueCodeList.Codes._9802Recon;
			var originalEntry = result.OriginalEntries.AddNew();
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
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;
			SetReconOriginalValues(invoiceLine);
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3201.90.1000";
			invoiceLine2.JI_CustomsQuantity = 100m;
			invoiceLine2.JI_LinePrice = 7000m;
			SetReconOriginalValues(invoiceLine2);
			var originalEntry2 = result.OriginalEntries.AddNew();
			originalEntry2.CH_OrigEntryReference = "XJ51";
			originalEntry2.US_ImportDate = new ZDateTime(2008, 1, 3);
			originalEntry2.US_R_DutyRateDate = new ZDateTime(2008, 1, 5);
			originalEntry2.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry2.US_SchDEntry = "2345";
			originalEntry2.US_R_CalcOrigDuty = true;
			var invoice2 = originalEntry2.Invoice;
			invoice2.JZ_InvoiceAmount = 20000m;
			invoice2.JZ_InvoiceNumber = "UIOA780";
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.US_UC_NKCountryOfExport = "KR";
			invoice2.US_UC_NKCountryOfOrigin = "KR";
			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.US_SupTariff = "9802.00.8015";
			invoiceLine3.US_98GoodsValue = 3000m;
			invoiceLine3.JI_Tariff = "8421.39.4000";
			invoiceLine3.JI_CustomsQuantity = 100m;
			invoiceLine3.JI_LinePrice = 7000m;
			SetReconOriginalValues(invoiceLine3);
			var invoiceLine5 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "0102.90.4084";
			invoiceLine5.JI_CustomsQuantity = 100m;
			invoiceLine5.JI_CustomsSecondQuantity = 33000m;
			invoiceLine5.JI_LinePrice = 10000m;
			SetReconOriginalValues(invoiceLine5);
			result.CalculateDutyFeesForChangedEntries();
			Factory.Save();
			return result;
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

		public void TestMergeOnlyChangedLines()
		{
			var reconDeclaration = new DeclarationTestHelper().GetDutiableReconDeclaration(Factory);
			reconDeclaration.OriginalEntries[0].US_R_CalcOrigDuty = true;
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			var merger = new ReconChangedLinesMerger(reconDeclaration);
			merger.DoMerge(false);
			AssertEquals("No changed lines", 0, reconDeclaration.ChangedLines.Count);
			var invoiceLine = reconDeclaration.InvoiceLines[0];
			invoiceLine.US_OverrideDuty = true;
			invoiceLine.US_Duty = 2500m;
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			merger.DoMerge(false);
			AssertEquals("One changed line", 1, reconDeclaration.ChangedLines.Count);
			invoiceLine.US_OverrideDuty = false;
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			merger.DoMerge(false);
			AssertEquals("No changed lines", 0, reconDeclaration.ChangedLines.Count);
			invoiceLine.US_R_OrigSPI = "AU";
			invoiceLine.US_SPI = "";
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			merger.DoMerge(false);
			AssertEquals("One changed line", 1, reconDeclaration.ChangedLines.Count);
		}

		public void TestChangedLinesWithDecrease()
		{
			var helper = new DeclarationTestHelper();
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("SV9");
			var reconDeclaration = helper.GetDutiableReconDeclaration(Factory);
			var declaration2 = helper.GetMergedDutiableDeclaration(Factory);
			declaration2.US_EntryFilerCode = "SV9";
			declaration2.CustomsEntryHeaders[0].EntryNumber = "70000014";
			Factory.Save();
			var reconEntry2 = reconDeclaration.OriginalEntries.AddNew();
			reconEntry2.CH_OrigEntryReference = "SV970000014";
			new ReconImportEntryRetriever(reconDeclaration).ImportLines();
			reconEntry2.US_R_DutyRateDate = ZDateTime.Today;
			reconDeclaration.OriginalEntries[0].US_R_CalcOrigDuty = true;
			reconDeclaration.OriginalEntries[1].US_R_CalcOrigDuty = true;
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			var merger = new ReconChangedLinesMerger(reconDeclaration);
			merger.DoMerge(true);
			AssertEquals("No changed lines", 0, reconDeclaration.ChangedLines.Count);
			AssertEquals("No changed lines", 0, reconDeclaration.ChangedLinesWithDecrease.Count);
			var invoiceLine = reconDeclaration.InvoiceLines[0];
			invoiceLine.US_OverrideDuty = true;
			invoiceLine.US_Duty = 2500m;
			var invoiceLine2 = reconDeclaration.InvoiceLines[1];
			invoiceLine2.US_R_OrigOverrideDuty = true;
			invoiceLine2.US_R_OrigDuty = 1310m;
			invoiceLine2.US_OverrideDuty = true;
			invoiceLine2.US_Duty = 1200m;
			var invoiceLine3 = reconDeclaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "3201.90.1000";
			invoiceLine3.JI_CustomsQuantity = 10m;
			invoiceLine3.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.Beef, "20");
			invoiceLine3.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Beef, "12");
			var invoiceLine4 = reconDeclaration.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "3301.90.1000";
			invoiceLine4.JI_CustomsQuantity = 20m;
			invoiceLine4.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.Blueberry, "18");
			invoiceLine4.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Blueberry, "19");
			invoiceLine4.JI_ParentID = invoiceLine3.PK;
			var invoiceLine5 = reconDeclaration.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "3401.90.1000";
			invoiceLine5.JI_CustomsQuantity = 1200m;
			invoiceLine5.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.DairyFee, "20");
			invoiceLine5.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.DairyFee, "20");
			var invoiceLine6 = reconDeclaration.InvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = "3501.90.1000";
			invoiceLine6.JI_CustomsQuantity = 11m;
			invoiceLine6.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.Blueberry, "50");
			invoiceLine6.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Blueberry, "50");
			invoiceLine6.JI_ParentID = invoiceLine5.PK;
			Factory.Save();
			reconDeclaration.US_R_Waive = true;
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			merger.DoMerge(true);
			AssertEquals("Three changed lines", 3, reconDeclaration.ChangedLines.Count);
			AssertEquals("Three changed lines with decriase", 3, reconDeclaration.ChangedLinesWithDecrease.Count);
			merger.DoMerge(false);
			AssertEquals("Three changed lines", 6, reconDeclaration.ChangedLines.Count);
			AssertEquals("Three changed lines with decriase", 0, reconDeclaration.ChangedLinesWithDecrease.Count);
			invoiceLine.US_OverrideDuty = false;
			invoiceLine2.US_R_OrigOverrideDuty = false;
			invoiceLine2.US_OverrideDuty = false;
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			merger.DoMerge(true);
			AssertEquals("Four changed lines", 4, reconDeclaration.ChangedLines.Count);
			AssertEquals("No changed lines with decrease", 0, reconDeclaration.ChangedLinesWithDecrease.Count);
			reconDeclaration.US_R_Waive = false;
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			merger.DoMerge(true);
			AssertEquals("Four changed lines", 4, reconDeclaration.ChangedLines.Count);
			AssertEquals("No changed lines with decrease", 0, reconDeclaration.ChangedLinesWithDecrease.Count);
		}

		public void TestMergeWithNA_SPI()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_R_CalcOrigDuty = true;
			var invoice = originalEntry.Invoice;
			//Three invoice lines with equal values
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6206.90.0040";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "";
			invoiceLine.US_R_OrigSPI = "";
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_R_OrigCV = 3000m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6206.90.0040";
			invoiceLine2.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine2.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine2.US_R_OrigSPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine2.JI_LinePrice = 1000m;
			invoiceLine2.US_R_OrigCV = 3000m;
			Factory.Save();
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			AssertEquals(1, reconDeclaration.ChangedLines.Count);
			AssertEquals(ZString.Empty, reconDeclaration.ChangedLines[0].US_SPI);
			AssertEquals(ZString.Empty, reconDeclaration.ChangedLines[0].US_OrigSPI);
			invoiceLine.US_SPI = "AU";
			invoiceLine.US_R_OrigSPI = "AU";
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			AssertEquals(2, reconDeclaration.ChangedLines.Count);
		}

		public void TestMergeWithNA_SPI2()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_R_CalcOrigDuty = true;
			var invoice = originalEntry.Invoice;
			//Three invoice lines with equal values
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6206.90.0040";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "";
			invoiceLine.US_R_OrigSPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_R_OrigCV = 1000m;
			invoiceLine.US_R_OrigTariff = "6206.90.0040";
			Factory.Save();
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			AssertEquals("No changed line", 0, reconDeclaration.ChangedLines.Count);
		}

		public void TestMergeFees()
		{
			ReconDeclaration reconDeclaration = GetReconDeclaration();
			reconDeclaration.InvoiceLines[0].JI_LinePrice = 4000m;
			reconDeclaration.InvoiceLines[1].JI_LinePrice = 8000m;
			reconDeclaration.Invoices[0].InvoiceLines[0].US_R_OrigTariff = "8421.39.4000";
			reconDeclaration.Invoices[0].InvoiceLines[1].US_R_OrigTariff = "8421.39.4000";
			reconDeclaration.Invoices[1].InvoiceLines[0].US_R_OrigSupTariff = "3201.90.1000";
			reconDeclaration.Invoices[1].InvoiceLines[0].US_R_OrigTariff = "0102.90.4084";
			reconDeclaration.Invoices[1].InvoiceLines[1].US_R_OrigTariff = "8421.39.4000";
			var origCharge1 = reconDeclaration.Invoices[0].InvoiceLines[0].ReconOriginalCharges.AddNew();
			origCharge1.CY_Code = Core.Constants.USCustoms.FeeCodes.DairyFee;
			origCharge1.CY_Amount = 100m;
			var origCharge2 = reconDeclaration.Invoices[0].InvoiceLines[0].ReconOriginalCharges.AddNew();
			origCharge2.CY_Code = Core.Constants.USCustoms.FeeCodes.Beef;
			origCharge2.CY_Amount = 200m;
			var origCharge3 = reconDeclaration.Invoices[0].InvoiceLines[0].ReconOriginalCharges.AddNew();
			origCharge3.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			origCharge3.CY_Amount = 200m;
			var origCharge4 = reconDeclaration.Invoices[0].InvoiceLines[0].ReconOriginalCharges.AddNew();
			origCharge4.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			origCharge4.CY_Amount = 200m;
			var origCharge5 = reconDeclaration.Invoices[0].InvoiceLines[0].ReconOriginalCharges.AddNew();
			origCharge5.CY_Code = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			origCharge5.CY_Amount = 200m;
			var reconFee1 = reconDeclaration.Invoices[0].InvoiceLines[0].FeeCusCodes.AddNew();
			reconFee1.CY_Code = Core.Constants.USCustoms.FeeCodes.Beef;
			reconFee1.CY_FeeAmount = 10m;
			var reconFee2 = reconDeclaration.Invoices[0].InvoiceLines[0].FeeCusCodes.AddNew();
			reconFee2.CY_Code = Core.Constants.USCustoms.FeeCodes.Blueberry;
			reconFee2.CY_FeeAmount = 20m;
			var reconFee3 = reconDeclaration.Invoices[0].InvoiceLines[0].FeeCusCodes.AddNew();
			reconFee3.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			reconFee3.CY_FeeAmount = 20m;
			var reconFee4 = reconDeclaration.Invoices[0].InvoiceLines[0].FeeCusCodes.AddNew();
			reconFee4.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			reconFee4.CY_FeeAmount = 20m;
			var reconFee5 = reconDeclaration.Invoices[0].InvoiceLines[0].FeeCusCodes.AddNew();
			reconFee5.CY_Code = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			reconFee5.CY_FeeAmount = 20m;
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			ReconChangedLineCollection changedLines = reconDeclaration.ChangedLines;
			AssertEquals("PreCondition", 4, changedLines.Count);
			AssertEquals("HTS number", "3201901000", changedLines[3].US_Tariff);
			AssertEquals("Orig HTS number", "8421394000", changedLines[3].US_OrigTariff);
			AssertEquals("Merge 2 lines", 2, ((IReconEntryLine)changedLines[3]).OriginalEntryLines.Count());
			var fee1 = ((IReconEntryLine)changedLines[3]).Fees.FirstOrDefault(x => x.FeeClass == Core.Constants.USCustoms.FeeCodes.DairyFee);
			var fee2 = ((IReconEntryLine)changedLines[3]).Fees.FirstOrDefault(x => x.FeeClass == Core.Constants.USCustoms.FeeCodes.Beef);
			var fee3 = ((IReconEntryLine)changedLines[3]).Fees.FirstOrDefault(x => x.FeeClass == Core.Constants.USCustoms.FeeCodes.Blueberry);
			var fee4 = ((IReconEntryLine)changedLines[3]).Fees.FirstOrDefault(x => x.FeeClass == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			var fee5 = ((IReconEntryLine)changedLines[3]).Fees.FirstOrDefault(x => x.FeeClass == Core.Constants.USCustoms.FeeCodes.HMF);
			var fee6 = ((IReconEntryLine)changedLines[3]).Fees.FirstOrDefault(x => x.FeeClass == Core.Constants.USCustoms.FeeCodes.DistilledSpirits);
			AssertEquals("DairyFee original", 100m, fee1.OriginalFee);
			AssertEquals("DairyFee recon", 0m, fee1.EstimatedReconciliationFee);
			AssertEquals("BeefFee original", 200m, fee2.OriginalFee);
			AssertEquals("BeefFee recon", 10m, fee2.EstimatedReconciliationFee);
			AssertEquals("BlueberryFee original", 0m, fee3.OriginalFee);
			AssertEquals("BlueberryFee recon", 20m, fee3.EstimatedReconciliationFee);
			AssertEquals("MPF original", 200m, fee4.OriginalFee);
			AssertEquals("MPF recon", 20m, fee4.EstimatedReconciliationFee);
			AssertEquals("DistilledSpirits original", 200m, fee6.OriginalFee);
			AssertEquals("DistilledSpirits recon", 20m, fee6.EstimatedReconciliationFee);
			AssertEquals("MPF original", 200m, changedLines[3].US_OrigMPF);
			AssertEquals("MPF recon", 20m, changedLines[3].US_MPF);
		}

		public void TestSetTotalsOnChangedLines()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_R_CalcOrigDuty = true;
			var invoice = originalEntry.Invoice;
			//Three invoice lines with equal values
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6206.90.0040";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "";
			invoiceLine.US_R_OrigSPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_R_OrigCV = 100m;
			invoiceLine.US_R_OrigTariff = "6206.90.0040";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "6206.90.0041";
			invoiceLine1.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine1.US_SPI = "";
			invoiceLine1.US_R_OrigSPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine1.JI_LinePrice = 500m;
			invoiceLine1.US_R_OrigCV = 50m;
			invoiceLine1.US_R_OrigTariff = "6206.90.0041";
			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6206.90.0042";
			invoiceLine2.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine2.US_SPI = "";
			invoiceLine2.US_R_OrigSPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine2.JI_LinePrice = 500m;
			invoiceLine2.US_R_OrigCV = 50m;
			invoiceLine2.US_R_OrigTariff = "6206.90.0042";
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			AssertEquals("3 changed lines", 3, reconDeclaration.ChangedLines.Count);
			AssertEquals("Total Original Customs Value", 100m, reconDeclaration.TotalOriginalValue);
			AssertEquals("Total Recon Customs Value", 1000m, reconDeclaration.TotalCustomsValue);
			AssertEquals("Total Value Change", 900m, reconDeclaration.TotalValueChange);
		}

		public void TestSetTotalsOnChangedLinesWithDecrease()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_R_CalcOrigDuty = true;
			var invoice = originalEntry.Invoice;
			//Three invoice lines with equal values
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6206.90.0040";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "";
			invoiceLine.US_R_OrigSPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.US_R_OrigCV = 1000m;
			invoiceLine.US_R_OrigTariff = "6206.90.0040";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "6206.90.0041";
			invoiceLine1.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine1.US_SPI = "";
			invoiceLine1.US_R_OrigSPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine1.JI_LinePrice = 50m;
			invoiceLine1.US_R_OrigCV = 500m;
			invoiceLine1.US_R_OrigTariff = "6206.90.0041";
			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6206.90.0042";
			invoiceLine2.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine2.US_SPI = "";
			invoiceLine2.US_R_OrigSPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine2.JI_LinePrice = 50m;
			invoiceLine2.US_R_OrigCV = 500m;
			invoiceLine2.US_R_OrigTariff = "6206.90.0042";
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			new ReconChangedLinesMerger(reconDeclaration).DoMerge(false);
			AssertEquals("3 changed lines", 3, reconDeclaration.ChangedLines.Count);
			AssertEquals("Total Decrease Original Customs Value", 1000m, reconDeclaration.TotalOriginalValue);
			AssertEquals("Total Decrease Recon Customs Value", 100m, reconDeclaration.TotalCustomsValue);
			AssertEquals("Total Decrease Value Change", -900m, reconDeclaration.TotalValueChange);
		}

		[TestDate(2020, 11, 11)]
		public void TestPayableMPFDiffBackRouding()
		{
			var testHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			testHelper.CreateTaxOrFee("499", 0.3464, "US", 26.79, 519.76, "AVL", new ZDateTime("2019-10-01"), new ZDateTime("2021-09-30"), "Merchandise Processing Fee");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			var reconDeclaration = new ReconDeclaration(declaration);
			declaration.AllocateEntryNumber("12345678");
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ548180583";
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.No;
			originalEntry.US_R_CalcOrigDuty = true;
			var invoiceOne = originalEntry.Invoice;
			var invoiceLineOne = invoiceOne.JobComInvoiceLines.AddNew();
			invoiceLineOne.US_R_OrigTariff = "3004909290";
			invoiceLineOne.JI_Tariff = "3004909290";
			invoiceLineOne.US_R_OrigCV = 57737m;
			invoiceLineOne.JI_LinePrice = 50000m;
			invoiceLineOne.US_UC_NKCountryOfOrigin = "IE";
			var invoiceLineTwo = invoiceOne.JobComInvoiceLines.AddNew();
			invoiceLineTwo.US_R_OrigTariff = "3004909290";
			invoiceLineTwo.JI_Tariff = "3004909290";
			invoiceLineTwo.US_R_OrigCV = 57737m;
			invoiceLineTwo.JI_LinePrice = 50000m;
			invoiceLineTwo.US_UC_NKCountryOfOrigin = "IE";
			var invoiceLineThree = invoiceOne.JobComInvoiceLines.AddNew();
			invoiceLineThree.US_R_OrigTariff = "3004909290";
			invoiceLineThree.JI_Tariff = "3004909290";
			invoiceLineThree.US_R_OrigCV = 57737m;
			invoiceLineThree.JI_LinePrice = 50000m;
			invoiceLineThree.US_UC_NKCountryOfOrigin = "IE";
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			Factory.Save();
			new ReconChangedLinesMerger(reconDeclaration, ReconMergeContext.Documents).DoMerge(false);
			AssertEquals(1, reconDeclaration.ChangedLines.Count);
			AssertEquals(reconDeclaration.TotalFeeDifference, reconDeclaration.ChangedLines[0].MPFChange);
			AssertEquals(-0.16m, reconDeclaration.ChangedLines[0].MPFChange);
			new ReconChangedLinesMerger(reconDeclaration, ReconMergeContext.Messaging).DoMerge(false);
			AssertEquals(1, reconDeclaration.ChangedLines.Count);
			AssertEquals("MPFChange is used on document only", 0m, reconDeclaration.ChangedLines[0].MPFChange);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
		}
	}
}
