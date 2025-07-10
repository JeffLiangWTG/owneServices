using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Reports.Testing
{
	sealed class InvoiceLineProductMatchTableProviderTest : TestCaseWithFactory
	{
		public void TestFilters()
		{
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();

			var dec1 = CreateDeclaration("B10001", importer1, supplier1);
			var dec2 = CreateDeclaration("B10002", importer2, supplier2);
			var dec3 = CreateDeclaration("B10003", importer3, supplier3);

			// changing tariff, so that declarations would be eligible for the report
			AddInvoiceLine(AddInvoice(dec1, "INV1001"), product1).JI_FormattedTariff = "2000.00.001";
			AddInvoiceLine(AddInvoice(dec2, "INV2001"), product2).JI_FormattedTariff = "2000.00.002";
			AddInvoiceLine(AddInvoice(dec3, "INV3001"), product3).JI_FormattedTariff = "2000.00.003";

			dec2.Invoices[0].InvoiceLines[0].JI_OA_ManufacturerAddress = supplier3.MainAddress.PK;

			var today = ZDateTime.Today;
			dec1.JE_DateOfArrival = today.AddDays(-9);
			dec2.JE_DateOfArrival = today.AddDays(-8);
			dec3.JE_DateOfArrival = today.AddDays(-7);

			dec2.JE_GB = branch2.PK;
			dec3.JE_GB = branch3.PK;
			dec3.IOROrgPK = importer1.PK;

			Factory.Save();

			var provider = new InvoiceLineProductMatchTableProvider();
			using (var report = new Report(null, null, Guid.Empty, Constants.DataContext.None))
			{
				AddEmptyFilters(report);

				report.FilterCollection.ClearValues();
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}",
					"INV1001",
					"INV2001",
					"INV3001"
				);

				report.FilterCollection.ClearValues();
				((TextField)report.FilterCollection["Declaration Branch"]).Value = $"{branch2.GB_Code}, {branch3.GB_Code}";
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}",
					"INV2001",
					"INV3001"
				);

				report.FilterCollection.ClearValues();
				((TextField)report.FilterCollection["Declarations"]).Value = "B10001, B10002";
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}",
					"INV1001",
					"INV2001"
				);

				report.FilterCollection.ClearValues();
				((LookupField)report.FilterCollection["Importer"]).Value = importer1.PK.ToGuid();
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}",
					"INV1001"
				);

				var yesterdayOrTodayUtc = ZDateTime.UtcNow.AddMinutes(-10).Date;
				var todayOrTomorrowUtc = ZDateTime.UtcNow.AddMinutes(10).Date;

				report.FilterCollection.ClearValues();
				((DateRangeField)report.FilterCollection["Job Registered On"]).ValueLow = yesterdayOrTodayUtc.AddMinutes(30);
				((DateRangeField)report.FilterCollection["Job Registered On"]).ValueHigh = todayOrTomorrowUtc.AddMinutes(31);
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}",
					"INV1001",
					"INV2001",
					"INV3001"
				);

				report.FilterCollection.ClearValues();
				((DateRangeField)report.FilterCollection["Job Registered On"]).ValueLow = yesterdayOrTodayUtc.AddMinutes(30).AddDays(-1);
				((DateRangeField)report.FilterCollection["Job Registered On"]).ValueHigh = yesterdayOrTodayUtc.AddMinutes(31).AddDays(-1);
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}");

				report.FilterCollection.ClearValues();
				((DateRangeField)report.FilterCollection["Job Registered On"]).ValueLow = todayOrTomorrowUtc.AddMinutes(30).AddDays(1);
				((DateRangeField)report.FilterCollection["Job Registered On"]).ValueHigh = todayOrTomorrowUtc.AddMinutes(31).AddDays(1);
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}");

				report.FilterCollection.ClearValues();
				((DateRangeField)report.FilterCollection["Import Date"]).ValueLow = today.AddDays(-8).AddHours(5);
				((DateRangeField)report.FilterCollection["Import Date"]).ValueHigh = today.AddDays(-8).AddHours(15);
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}",
					"INV2001"
				);

				report.FilterCollection.ClearValues();
				((LookupField)report.FilterCollection["Supplier"]).Value = supplier2.PK.ToGuid();
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}",
					"INV2001"
				);

				report.FilterCollection.ClearValues();
				((LookupField)report.FilterCollection["Importer of Record"]).Value = importer1.PK.ToGuid();
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}",
					"INV3001"
				);

				report.FilterCollection.ClearValues();
				((LookupField)report.FilterCollection["Manufacturer"]).Value = supplier3.PK.ToGuid();
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}",
					"INV2001"
				);

				report.FilterCollection.ClearValues();
				((TextField)report.FilterCollection["Product Code Starts With 1"]).Value = "TESTPROD";
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}",
					"INV1001",
					"INV2001",
					"INV3001"
				);

				report.FilterCollection.ClearValues();
				((TextField)report.FilterCollection["Product Code Starts With 1"]).Value = "TESTPROD1";
				((TextField)report.FilterCollection["Product Code Starts With 2"]).Value = "TESTPROD2";
				((TextField)report.FilterCollection["Product Code Starts With 3"]).Value = "TESTPROD3";
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}",
					"INV1001",
					"INV2001",
					"INV3001"
				);

				report.FilterCollection.ClearValues();
				((TextField)report.FilterCollection["Product Code Starts With 3"]).Value = "1";
				((TextField)report.FilterCollection["Product Code Starts With 4"]).Value = "TESTPROD2";
				((TextField)report.FilterCollection["Product Code Starts With 5"]).Value = "TESTPROD3";
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}",
					"INV2001",
					"INV3001"
				);
			}
		}

		public void TestAuditOptions()
		{
			var declaration1 = CreateDeclaration("B10001", importer1, supplier1);
			var declaration2 = CreateDeclaration("B10002", importer2, supplier2);

			var invoice1 = AddInvoice(declaration1, "1");
			AddInvoiceLine(invoice1, product1).US_CVD_NA = ZBool.False;
			AddInvoiceLine(invoice1, product1).US_CVDCaseNo = "100000002";
			AddInvoiceLine(invoice1, product1).US_CVDDepositRateIndicator = "C";
			AddInvoiceLine(invoice1, product1).US_IsBondedCVD = ZBool.False;

			AddInvoiceLine(invoice1, product1).US_ADD_NA = ZBool.False;
			AddInvoiceLine(invoice1, product1).US_ADDCaseNo = "200000002";
			AddInvoiceLine(invoice1, product1).US_ADDDepositRateIndicator = "D";
			AddInvoiceLine(invoice1, product1).US_IsBondedADD = ZBool.False;
			AddInvoiceLine(invoice1, product1).US_ADDDecID = "300000002";

			var invoice2 = AddInvoice(declaration1, "2");
			AddInvoiceLine(invoice2, product1).US_SPI = "B";

			var invoice3 = AddInvoice(declaration1, "3");
			AddInvoiceLine(invoice3, product1).US_LaceyIndicator = "D";
			AddInvoiceLine(invoice3, product1).US_LaceyDisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_FDAIndicator = "D";
			AddInvoiceLine(invoice3, product1).US_FDADisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_NHTSAIndicator = "D";
			AddInvoiceLine(invoice3, product1).US_NHTDisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_ATFInd = "D";
			AddInvoiceLine(invoice3, product1).US_ODSInd = "D";
			AddInvoiceLine(invoice3, product1).US_ODSDisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_TSCAInd = "D";
			AddInvoiceLine(invoice3, product1).US_TSCADisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_PSTIndicator = "D";
			AddInvoiceLine(invoice3, product1).US_PSTDisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_OMCInd = "D";
			AddInvoiceLine(invoice3, product1).US_OMCDisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_VNEInd = "D";
			AddInvoiceLine(invoice3, product1).US_VNEDisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_AMSInd = "D";
			AddInvoiceLine(invoice3, product1).US_AMSDisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_NOPInd = "D";
			AddInvoiceLine(invoice3, product1).US_NOPDisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_TTBInd = "D";
			AddInvoiceLine(invoice3, product1).US_TTBDisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_CPSCInd = "D";
			AddInvoiceLine(invoice3, product1).US_CPSCDisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_DEAInd = "D";
			AddInvoiceLine(invoice3, product1).US_DEADisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_APHISInd = "D";
			AddInvoiceLine(invoice3, product1).US_APHISDisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_DDTCInd = "D";
			AddInvoiceLine(invoice3, product1).US_NMFS370Ind = "D";
			AddInvoiceLine(invoice3, product1).US_NMFS370DisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_NMFSAMRInd = "D";
			AddInvoiceLine(invoice3, product1).US_NMFSAMRDisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_NMFSHMSInd = "D";
			AddInvoiceLine(invoice3, product1).US_NMFSHMSDisclaimReason = "B";
			AddInvoiceLine(invoice3, product1).US_NMFSSIMPInd = "D";
			AddInvoiceLine(invoice3, product1).US_FWSInd = "D";
			AddInvoiceLine(invoice3, product1).US_FWSDisclaimReason = "B";

			var invoice4 = AddInvoice(declaration2, "4");
			AddInvoiceLine(invoice4, product2).JI_FormattedTariff = "2000.00.002";
			AddInvoiceLine(invoice4, product2).SupTariffFormatted = "2001.00.002";

			Factory.Save();

			var provider = new InvoiceLineProductMatchTableProvider();
			using (var report = new Report(null, null, Guid.Empty, Constants.DataContext.None))
			{
				AddEmptyFilters(report);

				report.FilterCollection.ClearValues();
				((MultipleChoice)report.FilterCollection["Audit Options"]).Value = "";
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}-{r.JI_LineNo:D2}",
					"1-01", "1-02", "1-03", "1-04", "1-05", "1-06", "1-07", "1-08", "1-09",
					"2-01",
					"3-01", "3-02", "3-03", "3-04", "3-05", "3-06", "3-07", "3-08", "3-09", "3-10",
					"3-11", "3-12", "3-13", "3-14", "3-15", "3-16", "3-17", "3-18", "3-19", "3-20",
					"3-21", "3-22", "3-23", "3-24", "3-25", "3-26", "3-27", "3-28", "3-29", "3-30",
					"3-31", "3-32", "3-33", "3-34", "3-35", "3-36", "3-37", "3-38", "3-39",
					"4-01", "4-02"
				);

				report.FilterCollection.ClearValues();
				((MultipleChoice)report.FilterCollection["Audit Options"]).Value = "ADD/CVD";
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}-{r.JI_LineNo:D2}",
					"1-01", "1-02", "1-03", "1-04", "1-05", "1-06", "1-07", "1-08", "1-09"
				);

				report.FilterCollection.ClearValues();
				((MultipleChoice)report.FilterCollection["Audit Options"]).Value = "SPI";
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}-{r.JI_LineNo:D2}",
					"2-01"
				);

				report.FilterCollection.ClearValues();
				((MultipleChoice)report.FilterCollection["Audit Options"]).Value = "PGA";
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}-{r.JI_LineNo:D2}",
					"3-01", "3-02", "3-03", "3-04", "3-05", "3-06", "3-07", "3-08", "3-09", "3-10",
					"3-11", "3-12", "3-13", "3-14", "3-15", "3-16", "3-17", "3-18", "3-19", "3-20",
					"3-21", "3-22", "3-23", "3-24", "3-25", "3-26", "3-27", "3-28", "3-29", "3-30",
					"3-31", "3-32", "3-33", "3-34", "3-35", "3-36", "3-37", "3-38", "3-39"
				);

				report.FilterCollection.ClearValues();
				((MultipleChoice)report.FilterCollection["Audit Options"]).Value = "Tariff";
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}-{r.JI_LineNo:D2}",
					"4-01", "4-02"
				);

				report.FilterCollection.ClearValues();
				((MultipleChoice)report.FilterCollection["Audit Options"]).Value = "SPI, Tariff";
				AssertReport(provider, report, r => $"{r.JZ_InvoiceNumber}-{r.JI_LineNo:D2}",
					"2-01",
					"4-01", "4-02"
				);
			}
		}

		public void TestMatchingFlags()
		{
			var declaration = CreateDeclaration("B10001", importer2, supplier2);
			var invoice = AddInvoice(declaration, "1001");
			var invoiceLine1 = AddInvoiceLine(invoice, product2);
			var invoiceLine2 = AddInvoiceLine(invoice, product2);
			var invoiceLine3 = AddInvoiceLine(invoice, product2);
			var invoiceLine4 = AddInvoiceLine(invoice, product2);

			invoiceLine1.US_CVDCaseNo = "999999999";
			invoiceLine2.US_SPI = "B";
			invoiceLine3.US_FDAIndicator = "D";
			invoiceLine4.JI_FormattedTariff = "9999.99.999";

			Factory.Save();

			var provider = new InvoiceLineProductMatchTableProvider();
			using (var report = new Report(null, null, Guid.Empty, Constants.DataContext.None))
			{
				AddEmptyFilters(report);
				AssertReport(provider, report, r => $"{r.JI_LineNo}, {r.AddCvdMatches}, {r.SPIMatches}, {r.PGAMatches}, {r.TariffMatches}",
					"1, False, True, True, True",
					"2, True, False, True, True",
					"3, True, True, False, True",
					"4, True, True, True, False"
				);
			}
		}

		public void TestWithClassificationLookup()
		{
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "TEST-CC";
			classification.CC_FormattedTariffNum = "5000.00.001";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;

			product1.PivotsForBinding[0].CI_CC = classification.PK;
			product1.PivotsForBinding[0].CI_FormattedTariffNum = ZString.Empty;

			var declaration = CreateDeclaration("B10001", importer1, supplier1);
			var invoiceLine1 = AddInvoiceLine(AddInvoice(declaration, "1001"), product1);
			var invoiceLine2 = AddInvoiceLine(AddInvoice(declaration, "1002"), product1);

			AssertEquals("(pre-condition) product tariff is still empty", ZString.Empty, product1.PivotsForBinding[0].CI_FormattedTariffNum);
			AssertEquals("(pre-condition) tariff should be copied from product classification lookup to invoice line", "5000.00.001", invoiceLine1.JI_FormattedTariff);
			AssertEquals("(pre-condition) tariff should be copied from product classification lookup to invoice line", "5000.00.001", invoiceLine2.JI_FormattedTariff);
			invoiceLine1.JI_FormattedTariff = ZString.Empty;
			invoiceLine2.JI_FormattedTariff = "6000.00.001";

			Factory.Save();

			var provider = new InvoiceLineProductMatchTableProvider();
			using (var report = new Report(null, null, Guid.Empty, Constants.DataContext.None))
			{
				AddEmptyFilters(report);
				AssertReport(provider, report, r => $"{r.PartNo}, {r.JZ_InvoiceNumber}, {r.ProductTariff}, {r.InvoiceLineTariff}",
					"TESTPROD1, 1001, 5000.00.001, ",
					"TESTPROD1, 1002, 5000.00.001, 6000.00.001"
				);
			}
		}

		public void TestWithNonMatchingPartOrPivot()
		{
			product1.PivotsForBinding.RemoveAndDeleteAll();

			var declaration = CreateDeclaration("B10001", importer1, supplier1);
			var invoice = AddInvoice(declaration, "1001");
			var invoiceLine1 = AddInvoiceLine(invoice, null);
			var invoiceLine2 = AddInvoiceLine(invoice, null);
			var invoiceLine3 = AddInvoiceLine(invoice, null);

			invoiceLine1.JI_PartNo = "TESTPROD1";
			invoiceLine1.JI_FormattedTariff = "9999.99.002";
			invoiceLine2.JI_PartNo = "TESTPRODZ";
			invoiceLine2.JI_FormattedTariff = "9999.99.001";
			invoiceLine3.JI_PartNo = ZString.Empty;
			invoiceLine3.JI_FormattedTariff = "9999.99.003";

			Factory.Save();

			AssertEquals("(pre-condition) line1 has matching part", product1.PK, invoiceLine1.JI_OP);
			AssertEquals("(pre-condition) line1 has no matching pivot", null, invoiceLine1.Pivot);

			AssertEquals("(pre-condition) line2 has no matching part", ZGuid.Empty, invoiceLine2.JI_OP);
			AssertEquals("(pre-condition) line2 has no matching pivot", null, invoiceLine2.Pivot);

			AssertEquals("(pre-condition) line3 has no matching part", ZGuid.Empty, invoiceLine3.JI_OP);
			AssertEquals("(pre-condition) line3 has no matching pivot", null, invoiceLine3.Pivot);

			var provider = new InvoiceLineProductMatchTableProvider();
			using (var report = new Report(null, null, Guid.Empty, Constants.DataContext.None))
			{
				AddEmptyFilters(report);
				AssertReport(provider, report, r => $"{r.PartNo}, {r.JI_LineNo}, {r.ProductTariff}, {r.InvoiceLineTariff}",
					"TESTPROD1, 1, , 9999.99.002",
					"TESTPRODZ, 2, , 9999.99.001"
				);
			}
		}

		public void TestWithChildLines()
		{
			product1.PivotsForBinding[0].Children.AddNew().CI_FormattedTariffNum = "1000.01.001";
			product1.PivotsForBinding[0].Children.AddNew().CI_FormattedTariffNum = "1000.02.001";

			var declaration = CreateDeclaration("B10001", importer1, supplier1);
			var invoiceLine = AddInvoiceLine(AddInvoice(declaration, "1001"), product1);

			var invoiceChildLines = invoiceLine.ChildLines.OfType<JobComInvoiceLine>().OrderBy(l => l.JI_LineNo).ToArray();
			AssertEquals("(pre-condition)", 2, invoiceChildLines.Length);

			invoiceLine.JI_FormattedTariff = "1000.00.002";
			invoiceChildLines[0].JI_FormattedTariff = "1000.01.002";
			invoiceChildLines[1].JI_FormattedTariff = "1000.02.002";

			Factory.Save();

			var provider = new InvoiceLineProductMatchTableProvider();
			using (var report = new Report(null, null, Guid.Empty, Constants.DataContext.None))
			{
				AddEmptyFilters(report);
				AssertReport(provider, report, r => $"{r.ProductTariff}, {r.InvoiceLineTariff}",
					"1000.00.001, 1000.00.002",
					"1000.01.001, 1000.01.002",
					"1000.02.001, 1000.02.002"
				);
			}
		}

		public void TestReportLimits()
		{
			var declaration1 = CreateDeclaration("B10001", importer1, supplier1);
			var invoice1 = AddInvoice(declaration1, "1001");
			AddInvoiceLine(invoice1, product1).JI_FormattedTariff = "9999.99.999";
			AddInvoiceLine(invoice1, product1).JI_FormattedTariff = "9999.99.999";

			var declaration2 = CreateDeclaration("B10002", importer2, supplier2);
			var invoice2 = AddInvoice(declaration2, "1002");
			AddInvoiceLine(invoice2, product2).JI_FormattedTariff = "9999.99.999";

			Factory.Save();

			var provider = new InvoiceLineProductMatchTableProvider();
			using (var report = new Report(null, null, Guid.Empty, Constants.DataContext.None))
			{
				AddEmptyFilters(report);

				AssertEquals("Default MaxDeclarationCount", 100000, provider.MaxDeclarationCount);
				AssertEquals("Default MaxInvoiceLineCount", 1000000, provider.MaxInvoiceLineCount);

				provider.MaxDeclarationCount = 1;
				provider.MaxInvoiceLineCount = 3;
				AssertEquals(
					"Estimated number of declarations is 2. That is greater than allowed maximum of 1.",
					AssertExceptionThrown<DataProviderException>(() => GetDataTable(provider, report)).Message
				);

				provider.MaxDeclarationCount = 2;
				provider.MaxInvoiceLineCount = 2;
				AssertEquals(
					"Found at least 3 invoice lines. That is greater than allowed maximum of 2.",
					AssertExceptionThrown<DataProviderException>(() => GetDataTable(provider, report)).Message
				);

				provider.MaxDeclarationCount = 2;
				provider.MaxInvoiceLineCount = 3;
				GetDataTable(provider, report);
			}
		}

		public void TestPerformance_NoRedundantDbHits()
		{
			using (USCustomsDataRegistry.Instance.EntryDeclarant.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true))
			{
				product1.PivotsForBinding[0].Children.AddNew().CI_FormattedTariffNum = "1000.01.001";
				product1.PivotsForBinding[0].Children.AddNew().CI_FormattedTariffNum = "1000.02.001";

				var product1a = CreateProduct("TESTPROD1A", importer1, "1001.01.001", "1002.01.001");
				var product1b = CreateProduct("TESTPROD1B", importer1, "1001.02.001", "1002.02.001");

				var declaration1 = CreateDeclaration("B10001", importer1, supplier1);
				var invoiceLine11 = AddInvoiceLine(AddInvoice(declaration1, "1001"), product1);
				var invoiceLine12 = AddInvoiceLine(AddInvoice(declaration1, "1002"), product1);
				var invoiceLine13 = AddInvoiceLine(AddInvoice(declaration1, "1003"), product1a);
				var invoiceLine14 = AddInvoiceLine(AddInvoice(declaration1, "1004"), product1b);
				var invoiceLine15 = AddInvoiceLine(AddInvoice(declaration1, "1005"), null);
				var invoiceLine16 = AddInvoiceLine(AddInvoice(declaration1, "1006"), null);

				var invoiceChildLines1 = invoiceLine11.ChildLines.OfType<JobComInvoiceLine>().OrderBy(l => l.JI_LineNo).ToArray();
				var invoiceChildLines2 = invoiceLine12.ChildLines.OfType<JobComInvoiceLine>().OrderBy(l => l.JI_LineNo).ToArray();
				AssertEquals("(pre-condition)", 2, invoiceChildLines1.Length);
				AssertEquals("(pre-condition)", 2, invoiceChildLines2.Length);

				invoiceLine11.JI_FormattedTariff = "1000.00.002";
				invoiceChildLines1[0].JI_FormattedTariff = "1000.01.002";
				invoiceChildLines1[1].JI_FormattedTariff = "1000.02.002";

				invoiceLine12.JI_FormattedTariff = "1000.00.002";
				invoiceChildLines2[0].JI_FormattedTariff = "1000.01.002";
				invoiceChildLines2[1].JI_FormattedTariff = "1000.02.002";

				invoiceLine13.JI_FormattedTariff = "1001.01.002";
				invoiceLine14.JI_FormattedTariff = "1001.02.002";

				invoiceLine15.JI_PartNo = "UNKNOWN PART1";
				invoiceLine16.JI_PartNo = "UNKNOWN PART2";
				invoiceLine15.JI_FormattedTariff = "9999.01.002";
				invoiceLine16.JI_FormattedTariff = "9999.02.002";

				var declaration2 = CreateDeclaration("B10002", importer2, supplier2);
				var invoice21 = AddInvoice(declaration2, "2001");
				var invoiceLine21 = AddInvoiceLine(invoice21, product2);
				var invoiceLine22 = AddInvoiceLine(invoice21, product2);
				var invoiceLine23 = AddInvoiceLine(invoice21, product2);
				invoiceLine21.JI_FormattedTariff = "9000.00.002";
				invoiceLine22.JI_FormattedTariff = "9000.00.002";
				invoiceLine23.JI_FormattedTariff = "9000.00.002";

				declaration2.US_EnableENS = true;
				declaration2.US_EntryFilerCode = "ABC";
				declaration2.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
				declaration2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
				declaration2.DoMerge();
				// We need several entry lines to test how many times we hit CusEntryLine table.
				AssertNotEquals("(pre-condition) invoiceLine21.JI_CL != ZGuid.Empty", ZGuid.Empty, invoiceLine21.JI_CL);
				AssertNotEquals("(pre-condition) invoiceLine22.JI_CL != ZGuid.Empty", ZGuid.Empty, invoiceLine22.JI_CL);
				AssertNotEquals("(pre-condition) invoiceLine23.JI_CL != ZGuid.Empty", ZGuid.Empty, invoiceLine23.JI_CL);
				AssertNotEquals("(pre-condition) invoiceLine21.JI_CL != invoiceLine22.JI_CL", invoiceLine21.JI_CL, invoiceLine22.JI_CL);
				AssertNotEquals("(pre-condition) invoiceLine21.JI_CL != invoiceLine23.JI_CL", invoiceLine21.JI_CL, invoiceLine23.JI_CL);
				AssertNotEquals("(pre-condition) invoiceLine22.JI_CL != invoiceLine23.JI_CL", invoiceLine22.JI_CL, invoiceLine23.JI_CL);

				var declaration3 = CreateDeclaration("B10003", importer3, supplier3);
				var invoice31 = AddInvoice(declaration3, "3001");
				var invoiceLine31 = AddInvoiceLine(invoice31, product3);
				var invoiceLine32 = AddInvoiceLine(invoice31, product3);
				invoiceLine31.JI_FormattedTariff = "9000.00.003";
				invoiceLine32.JI_FormattedTariff = "9000.00.003";

				var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
				otherOrg.OH_Code = "TESTZZZ";
				product3.RelatedOrganisations.AddOwner(otherOrg);

				Factory.Save();

				using (AssertDbHitsForAllFactories(null, ignoreUnspecified: true, thresholdForUnspecified: 1, acceptableVariance: 1, expectedHitCounts: new Dictionary<string, int>
				{
					// 2 queries for JobDeclaration (one to estimate total count, one to load batch)
					{ JobDeclarationSchema.Constants.TableName, 2 },
					// 2 queries for OrgSupplierPart (one by PK, one by PartNum)
					{ OrgSupplierPartSchema.Constants.TableName, 2 },
					// 2 queries for OrgSupplierPartBarcode (one by PK, one by Barcode)
					{ OrgSupplierPartBarcodeSchema.Constants.TableName, 2 },
					{ CusAddInfoSchema.Constants.TableName, 1 },
					{ JobDocAddressSchema.Constants.TableName, 6 },
					{ CusEntryLineSchema.Constants.TableName, 2 },
					{ StmALogSchema.Constants.TableName, 3 },
				}))
				{
					var provider = new InvoiceLineProductMatchTableProvider();
					using (var report = new Report(null, null, Guid.Empty, Constants.DataContext.None))
					{
						AddEmptyFilters(report);
						AssertReport(provider, report, r => $"{r.PartNo}, {r.ChildLineNo}, {r.JZ_InvoiceNumber}, {r.ProductTariff}, {r.InvoiceLineTariff}",
							"TESTPROD1, 0, 1001, 1000.00.001, 1000.00.002",
							"TESTPROD1, 0, 1002, 1000.00.001, 1000.00.002",
							"TESTPROD1, 1, 1001, 1000.01.001, 1000.01.002",
							"TESTPROD1, 1, 1002, 1000.01.001, 1000.01.002",
							"TESTPROD1, 2, 1001, 1000.02.001, 1000.02.002",
							"TESTPROD1, 2, 1002, 1000.02.001, 1000.02.002",
							"TESTPROD1A, 0, 1003, 1001.01.001, 1001.01.002",
							"TESTPROD1B, 0, 1004, 1001.02.001, 1001.02.002",
							"TESTPROD2, 0, 2001, 1000.00.002, 9000.00.002",
							"TESTPROD2, 0, 2001, 1000.00.002, 9000.00.002",
							"TESTPROD2, 0, 2001, 1000.00.002, 9000.00.002",
							"TESTPROD3, 0, 3001, 1000.00.003, 9000.00.003",
							"TESTPROD3, 0, 3001, 1000.00.003, 9000.00.003",
							"UNKNOWN PART1, 0, 1005, , 9999.01.002",
							"UNKNOWN PART2, 0, 1006, , 9999.02.002"
						);
					}
				}
			}
		}

		public void TestPerformance_DbHits_BarcodeMatch()
		{
			using (USCustomsDataRegistry.Instance.EntryDeclarant.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true))
			{
				var products = new OrgSupplierPart[10];

				var declaration1 = CreateDeclaration("B10001", importer1, supplier1);
				for (int i = 0; i < 10; i++)
				{
					products[i] = CreateProduct($"PEN{i}", importer1, "1000.01.001", "1002.01.001");

					var invoiceLine = AddInvoiceLine(AddInvoice(declaration1, "1001"), null);
					invoiceLine.JI_FormattedTariff = "1000.00.002";
					invoiceLine.JI_PartNo = $"BarCode0{i}";

					CreateProductBarcode(products[i], $"BarCode0{i}");
				}
				Factory.Save();

				using (AssertDbHitsForAllFactories(null, ignoreUnspecified: true, expectedHitCounts: new Dictionary<string, int>
				{
					{ OrgSupplierPartSchema.Constants.TableName, 2 },
					{ OrgSupplierPartBarcodeSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
				}))
				{
					using (var report = new Report(null, null, Guid.Empty, Constants.DataContext.None))
					{
						AddEmptyFilters(report);
						var provider = new InvoiceLineProductMatchTableProvider();
						AssertReport(provider, report, r => $"{r.PartNo}, {r.ChildLineNo}, {r.JZ_InvoiceNumber}, {r.ProductTariff}, {r.InvoiceLineTariff}",
							"PEN0, 0, 1001, 1000.01.001, 1000.00.002",
							"PEN1, 0, 1001, 1000.01.001, 1000.00.002",
							"PEN2, 0, 1001, 1000.01.001, 1000.00.002",
							"PEN3, 0, 1001, 1000.01.001, 1000.00.002",
							"PEN4, 0, 1001, 1000.01.001, 1000.00.002",
							"PEN5, 0, 1001, 1000.01.001, 1000.00.002",
							"PEN6, 0, 1001, 1000.01.001, 1000.00.002",
							"PEN7, 0, 1001, 1000.01.001, 1000.00.002",
							"PEN8, 0, 1001, 1000.01.001, 1000.00.002",
							"PEN9, 0, 1001, 1000.01.001, 1000.00.002"
						);
					}
				}
			}

			void CreateProductBarcode(OrgSupplierPart part, ZString barcode)
			{
				var orgBarcode = part.PartBarcodes.AddNew();
				orgBarcode.PH_F3_NKPackType = "UNT";
				orgBarcode.PH_Barcode = barcode;
			}
		}

		public void TestBatching()
		{
			var declaration1 = CreateDeclaration("B10001", importer1, supplier1);
			AddInvoiceLine(AddInvoice(declaration1, "1001"), product1).JI_FormattedTariff = "9000.00.001";

			var declaration2 = CreateDeclaration("B10002", importer2, supplier2);
			AddInvoiceLine(AddInvoice(declaration2, "2001"), product2).JI_FormattedTariff = "9000.00.002";

			var declaration3 = CreateDeclaration("B10003", importer3, supplier3);
			AddInvoiceLine(AddInvoice(declaration3, "3001"), product3).JI_FormattedTariff = "9000.00.003";

			Factory.Save();

			// Results should be the same when batch size is below and above number of declatarions, but number of DB hits should be different.

			using (AssertDbHitsForAllFactories(ignoreUnspecified: true, thresholdForUnspecified: 100, expectedHitCounts: new Dictionary<string, int>
			{
				// 2 queries for JobDeclaration (one to estimate total count, one to load batch)
				{ JobDeclarationSchema.Constants.TableName, 2 }
			}))
			{
				var provider = new InvoiceLineProductMatchTableProvider();
				AssertEquals(100, provider.BatchSize);
				using (var report = new Report(null, null, Guid.Empty, Constants.DataContext.None))
				{
					AddEmptyFilters(report);
					AssertReport(provider, report, r => $"{r.PartNo}, {r.ChildLineNo}, {r.JZ_InvoiceNumber}, {r.ProductTariff}, {r.InvoiceLineTariff}",
						"TESTPROD1, 0, 1001, 1000.00.001, 9000.00.001",
						"TESTPROD2, 0, 2001, 1000.00.002, 9000.00.002",
						"TESTPROD3, 0, 3001, 1000.00.003, 9000.00.003"
					);
				}
			}

			using (AssertDbHitsForAllFactories(ignoreUnspecified: true, thresholdForUnspecified: 100, expectedHitCounts: new Dictionary<string, int>
			{
				// 3 queries for JobDeclaration (one to estimate total count, 2 to load 2 batches)
				{ JobDeclarationSchema.Constants.TableName, 3 }
			}))
			{
				var provider = new InvoiceLineProductMatchTableProvider();
				provider.BatchSize = 2;
				using (var report = new Report(null, null, Guid.Empty, Constants.DataContext.None))
				{
					AddEmptyFilters(report);
					AssertReport(provider, report, r => $"{r.PartNo}, {r.ChildLineNo}, {r.JZ_InvoiceNumber}, {r.ProductTariff}, {r.InvoiceLineTariff}",
						"TESTPROD1, 0, 1001, 1000.00.001, 9000.00.001",
						"TESTPROD2, 0, 2001, 1000.00.002, 9000.00.002",
						"TESTPROD3, 0, 3001, 1000.00.003, 9000.00.003"
					);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "TESTIMP1";

			importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "TESTIMP2";

			importer3 = Factory.NewWithValidTestData<OrgHeader>();
			importer3.OH_Code = "TESTIMP3";

			supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_Code = "TESTSUP1";

			supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_Code = "TESTSUP2";

			supplier3 = Factory.NewWithValidTestData<OrgHeader>();
			supplier3.OH_Code = "TESTSUP3";

			product1 = CreateProduct("TESTPROD1", importer1, "1000.00.001", "1001.00.001");
			product2 = CreateProduct("TESTPROD2", importer2, "1000.00.002", "1001.00.002");
			product3 = CreateProduct("TESTPROD3", importer3, "1000.00.003", "1001.00.003");

			var product1Class = product1.PivotsForBinding[0];

			product1Class.CD_CVDApplicable = ZBool.True;
			product1Class.CD_CVDCaseNo = "100000001";
			product1Class.CD_CVDDepositRateInd = "A";
			product1Class.CD_CVDBonded = ZBool.True;

			product1Class.CD_ADDApplicable = ZBool.True;
			product1Class.CD_ADDCaseNo = "200000001";
			product1Class.CD_ADDDepositRateInd = "A";
			product1Class.CD_ADDBonded = ZBool.True;
			product1Class.CD_ADDDecID = "300000001";

			product1Class.CD_LaceyActIndicator = "C";
			product1Class.CD_LaceyActDisclaimReason = "A";
			product1Class.CD_ACEFDAIndicator = "C";
			product1Class.CD_ACEFDADisclaimReason = "A";
			product1Class.CD_NHTSAIndicator = "C";
			product1Class.CD_NHTSADisclaimReason = "A";
			product1Class.CD_ATFIndicator = "C";
			product1Class.CD_ODSIndicator = "C";
			product1Class.CD_ODSDisclaimReason = "A";
			product1Class.CD_TSCAClaimIndicator = "C";
			product1Class.CD_TSCADisclaimReason = "A";
			product1Class.CD_PSTIndicator = "C";
			product1Class.CD_PSTDisclaimReason = "A";
			product1Class.CD_OMCIndicator = "C";
			product1Class.CD_OMCDisclaimReason = "A";
			product1Class.CD_VNEIndicator = "C";
			product1Class.CD_VNEDisclaimReason = "A";
			product1Class.CD_AMSIndicator = "C";
			product1Class.CD_AMSDisclaimReason = "A";
			product1Class.CD_TTBIndicator = "C";
			product1Class.CD_TTBDisclaimReason = "A";
			product1Class.CD_CPSCIndicator = "C";
			product1Class.CD_CPSCDisclaimReason = "A";
			product1Class.CD_DEAIndicator = "C";
			product1Class.CD_DEADisclaimReason = "A";
			product1Class.CD_APHISIndicator = "C";
			product1Class.CD_APHISDisclaimReason = "A";
			product1Class.CD_DDTCIndicator = "C";
			product1Class.CD_NMFS370Indicator = "C";
			product1Class.CD_NMFS370DisclaimReason = "A";
			product1Class.CD_NMFSAMRIndicator = "C";
			product1Class.CD_NMFSAMRDisclaimReason = "A";
			product1Class.CD_NMFSHMSIndicator = "C";
			product1Class.CD_NMFSHMSDisclaimReason = "A";
			product1Class.CD_NMFSSIMPIndicator = "C";
			product1Class.CD_FWSIndicator = "C";
			product1Class.CD_FWSDisclaimReason = "A";
			product1Class.CD_NOPIndicator = "C";
			product1Class.CD_NOPDisclaimReason = "A";
		}

		void AssertReport(InvoiceLineProductMatchTableProvider provider, Report report, Func<InvoiceLineProductMatchDataSet.InvoiceLineProductMatchDataSetRow, string> lineFormat, params string[] expectedLines)
		{
			var dataTable = GetDataTable(provider, report);
			AssertMultilineASCIIEquals(
				string.Join("\r\n", expectedLines),
				string.Join("\r\n", dataTable.Rows.OfType<InvoiceLineProductMatchDataSet.InvoiceLineProductMatchDataSetRow>().Select(lineFormat))
			);
		}

		static DataTable GetDataTable(InvoiceLineProductMatchTableProvider provider, Report report)
		{
			return provider.GetDataTable("SomeTable",
				"SomeReport(<Declaration Branch>, <Declarations>, <Job Registered On->FromDate>, <Job Registered On->ToDate>, <Import Date->FromDate>, <Import Date->ToDate>, <Importer>, <Supplier>, <Importer of Record>, <Manufacturer>, <Product Code Starts With 1>, <Product Code Starts With 2>, <Product Code Starts With 3>, <Product Code Starts With 4>, <Product Code Starts With 5>, <Audit Options>)",
				report, false);
		}

		void AddEmptyFilters(Report report)
		{
			report.FilterCollection.Add(new TextField(Factory)
			{
				DisplayName = "Declaration Branch",
				Value = string.Empty
			});
			report.FilterCollection.Add(new TextField(Factory)
			{
				DisplayName = "Declarations",
				Value = string.Empty
			});
			report.FilterCollection.Add(new DateRangeField(Factory)
			{
				DisplayName = "Job Registered On",
				ValueLow = ZDateTime.Empty,
				ValueHigh = ZDateTime.Empty
			});
			report.FilterCollection.Add(new DateRangeField(Factory)
			{
				DisplayName = "Import Date",
				ValueLow = ZDateTime.Empty,
				ValueHigh = ZDateTime.Empty
			});
			report.FilterCollection.Add(new LookupField(Factory)
			{
				DisplayName = "Importer",
				Value = Guid.Empty
			});
			report.FilterCollection.Add(new LookupField(Factory)
			{
				DisplayName = "Supplier",
				Value = Guid.Empty
			});
			report.FilterCollection.Add(new LookupField(Factory)
			{
				DisplayName = "Importer of Record",
				Value = Guid.Empty
			});
			report.FilterCollection.Add(new LookupField(Factory)
			{
				DisplayName = "Manufacturer",
				Value = Guid.Empty
			});
			report.FilterCollection.Add(new TextField(Factory)
			{
				DisplayName = "Product Code Starts With 1",
				Value = ZString.Empty
			});
			report.FilterCollection.Add(new TextField(Factory)
			{
				DisplayName = "Product Code Starts With 2",
				Value = ZString.Empty
			});
			report.FilterCollection.Add(new TextField(Factory)
			{
				DisplayName = "Product Code Starts With 3",
				Value = ZString.Empty
			});
			report.FilterCollection.Add(new TextField(Factory)
			{
				DisplayName = "Product Code Starts With 4",
				Value = ZString.Empty
			});
			report.FilterCollection.Add(new TextField(Factory)
			{
				DisplayName = "Product Code Starts With 5",
				Value = ZString.Empty
			});
			report.FilterCollection.Add(new MultipleChoice(Factory)
			{
				DisplayName = "Audit Options",
				Value = string.Empty
			});
		}

		JobDeclaration CreateDeclaration(string referenceNumber, OrgHeader importer, OrgHeader supplier)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = referenceNumber;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			return declaration;
		}

		JobComInvoiceHeader AddInvoice(JobDeclaration declaration, string invoiceNumber)
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = invoiceNumber;
			return invoice;
		}

		JobComInvoiceLine AddInvoiceLine(JobComInvoiceHeader invoice, OrgSupplierPart product)
		{
			var invoiceLine = invoice.InvoiceLines.AddNew();

			if (product != null)
			{
				invoiceLine.JI_PartNo = product.OP_PartNum;
				AssertEquals("(pre-condition) product should be linked to invoice line", product.PK, invoiceLine.JI_OP);
				AssertEquals("(pre-condition) product pivot should be linked to invoice line", product.PivotsForBinding[0].PK, invoiceLine.Pivot?.PK);
			}

			return invoiceLine;
		}

		OrgSupplierPart CreateProduct(string partNum, OrgHeader importer, string tariff, string supTariff)
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNum;
			product.OP_Desc = partNum;
			product.RelatedOrganisations.AddOwner(importer);

			var clazz = product.PivotsForBinding.AddNew();
			clazz.CI_ChildType = ClassificationTypeList.Codes.HTI;
			clazz.CI_OH = importer.PK;
			clazz.CI_FormattedTariffNum = tariff;
			clazz.CI_FormattedSupplementalTariff = supTariff;
			clazz.CD_SPI = "A";

			return product;
		}

		OrgHeader importer1;
		OrgHeader importer2;
		OrgHeader importer3;
		OrgHeader supplier1;
		OrgHeader supplier2;
		OrgHeader supplier3;
		OrgSupplierPart product1;
		OrgSupplierPart product2;
		OrgSupplierPart product3;
	}
}
