using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EntrySummary98_99MergeStrategyTest : TestCaseWithFactory
	{
		public void TestMergeWith98_99Details()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "5208112040";
			invoiceLine.JI_LinePrice = 500m;

			invoiceLine.US_SupTariff = "9802006000";
			invoiceLine.US_98GoodsValue = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertNotNull(entry);
			AssertMergeResult(entry);

			CusEntryLine entryLine = invoiceLine.CusEntryLine;
			CusEntryLine supEntryLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertMergeResult(entry);
			Assert(entry.MergedLines.Contains(entryLine));
			Assert(entry.MergedLines.Contains(supEntryLine));

			invoiceLine.US_SupTariff = "";
			invoiceLine.US_98GoodsValue = ZDecimal.Zero;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("one entry line", 1, entry.MergedLines.Count);
			AssertEquals(ZGuid.Empty, entry.MergedLines[0].US_CL_ParentLine);
			AssertNull(entry.MergedLines[0].ParentLine);
			Assert(entry.MergedLines.Contains(entryLine));
		}

		public void TestMergeWith98_99DetailsForCargoRelease()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "5208112040";
			invoiceLine.JI_LinePrice = 500m;

			invoiceLine.US_SupTariff = "9802006000";
			invoiceLine.US_98GoodsValue = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			CusEntryHeader entry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			AssertNotNull(entry);
			AssertMergeResult(entry);

			CusEntryLine entryLine = invoiceLine.CusEntryLine;
			CusEntryLine supEntryLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.CargoRelease, true);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertMergeResult(entry);
			Assert(entry.MergedLines.Contains(entryLine));
			Assert(entry.MergedLines.Contains(supEntryLine));

			declaration.US_EnableENS = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(2, declaration.CustomsEntryHeaders.Count);
			entry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			AssertNotNull(entry);
			AssertMergeResult(entry);

			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			CusEntryLine ensEntryLine = invoiceLine.CusEntryLine;
			AssertNotNull(entry);
			AssertMergeResult(entry);

			invoiceLine.US_SupTariff = "";
			invoiceLine.US_98GoodsValue = ZDecimal.Zero;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("one entry line", 1, entry.MergedLines.Count);
			AssertEquals(ZGuid.Empty, entry.MergedLines[0].US_CL_ParentLine);
			AssertNull(entry.MergedLines[0].ParentLine);
			Assert(entry.MergedLines.Contains(ensEntryLine));

			entry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			AssertEquals("one entry line", 1, entry.MergedLines.Count);
			AssertEquals(ZGuid.Empty, entry.MergedLines[0].US_CL_ParentLine);
			AssertNull(entry.MergedLines[0].ParentLine);
			Assert(entry.MergedLines.Contains(entryLine));
		}

		public void TestWatchRepairEntryCalculation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 500m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
				line1.JI_Tariff = "9102111010";
				line1.JI_CustomsQuantity = 1000m;
				line1.JI_LinePrice = 3406m;
				line1.US_SupTariff = "9802004040";

				JobComInvoiceLine line2 = line1.AddSecondaryInvoiceLine();
				line2.JI_Tariff = "9102111020";
				line2.JI_CustomsQuantity = 1000m;
				line2.JI_LinePrice = 1609m;
				line2.US_SupTariff = "9802004040";
				line2.US_98GoodsValue = 500m;

				JobComInvoiceLine line3 = line1.AddSecondaryInvoiceLine();
				line3.JI_Tariff = "9102111030";
				line3.JI_CustomsQuantity = 1000m;
				line3.JI_LinePrice = 1345m;
				line3.US_SupTariff = "9802004040";

				JobComInvoiceLine line4 = line1.AddSecondaryInvoiceLine();
				line4.JI_Tariff = "9102111040";
				line4.JI_CustomsQuantity = 1000m;
				line4.JI_LinePrice = 204m;
				line4.US_SupTariff = "9802004040";

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				AssertEquals("There should be one entry", 1, declaration.CustomsEntryHeaders.Count);
				AssertEquals("There should be eight entry lines", 8, declaration.CustomsEntryHeaders[0].MergedLines.Count);

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				AssertEquals("There should be one entry", 1, declaration.CustomsEntryHeaders.Count);
				AssertEquals("There should be eight entry lines", 8, declaration.CustomsEntryHeaders[0].MergedLines.Count);

				CusEntryLine supParentLine = line1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);

				AssertNull(supParentLine.ParentLine);
				AssertEquals(7, supParentLine.ChildLines.Count);
				AssertEquals(supParentLine, line1.CusEntryLine.ParentLine);
				AssertEquals(supParentLine, line2.CusEntryLine.ParentLine);
				AssertEquals(supParentLine, line3.CusEntryLine.ParentLine);
				AssertEquals(supParentLine, line4.CusEntryLine.ParentLine);
				AssertEquals(supParentLine, line2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true).ParentLine);
				AssertEquals(supParentLine, line3.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true).ParentLine);
				AssertEquals(supParentLine, line4.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true).ParentLine);

				AssertEquals(7, supParentLine.ChildSecondaryEntryLines.Count);
			}
		}

		public void TestWatchRepairEntryCalculationForCargoRelease()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 500m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
				line1.JI_Tariff = "9102111010";
				line1.JI_CustomsQuantity = 1000m;
				line1.JI_LinePrice = 3406m;
				line1.US_SupTariff = "9802004040";

				JobComInvoiceLine line2 = line1.AddSecondaryInvoiceLine();
				line2.JI_Tariff = "9102111020";
				line2.JI_CustomsQuantity = 1000m;
				line2.JI_LinePrice = 1609m;
				line2.US_SupTariff = "9802004040";
				line2.US_98GoodsValue = 500m;

				JobComInvoiceLine line3 = line1.AddSecondaryInvoiceLine();
				line3.JI_Tariff = "9102111030";
				line3.JI_CustomsQuantity = 1000m;
				line3.JI_LinePrice = 1345m;
				line3.US_SupTariff = "9802004040";

				JobComInvoiceLine line4 = line1.AddSecondaryInvoiceLine();
				line4.JI_Tariff = "9102111040";
				line4.JI_CustomsQuantity = 1000m;
				line4.JI_LinePrice = 204m;
				line4.US_SupTariff = "9802004040";
			}
			declaration.US_EnableENS = false;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("There should be one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertWatchRepairMergeResult(declaration.CustomsEntryHeaders[0]);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("There should be one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertWatchRepairMergeResult(declaration.CustomsEntryHeaders[0]);

			declaration.US_EnableENS = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("There should be one entry", 2, declaration.CustomsEntryHeaders.Count);
			AssertWatchRepairMergeResult(declaration.CustomsEntryHeaders[0]);
			AssertWatchRepairMergeResult(declaration.CustomsEntryHeaders[1]);

			declaration.US_EnableCRL = false;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("There should be one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertWatchRepairMergeResult(declaration.CustomsEntryHeaders[0]);

			AssertEquals(1, declaration.InvoiceLines[0].AdditionalEntryLineLinks.Count);
			AssertEquals(1, declaration.InvoiceLines[1].AdditionalEntryLineLinks.Count);
			AssertEquals(1, declaration.InvoiceLines[2].AdditionalEntryLineLinks.Count);
			AssertEquals(1, declaration.InvoiceLines[3].AdditionalEntryLineLinks.Count);
			AssertNotNull(declaration.InvoiceLines[0].CusEntryLine);
			AssertNotNull(declaration.InvoiceLines[1].CusEntryLine);
			AssertNotNull(declaration.InvoiceLines[2].CusEntryLine);
			AssertNotNull(declaration.InvoiceLines[3].CusEntryLine);
		}

		public void TestWatchRepairDutyCalculation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 500m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
				line1.JI_Tariff = "9102111010";
				line1.JI_CustomsQuantity = 1000m;
				line1.JI_LinePrice = 3406m;
				line1.US_SupTariff = "9802004040";

				JobComInvoiceLine line2 = line1.AddSecondaryInvoiceLine();
				line2.JI_Tariff = "9102111020";
				line2.JI_CustomsQuantity = 1000m;
				line2.JI_LinePrice = 1609m;
				line2.US_SupTariff = "9802004040";
				line2.US_98GoodsValue = 500m;

				JobComInvoiceLine line3 = line1.AddSecondaryInvoiceLine();
				line3.JI_Tariff = "9102111030";
				line3.JI_CustomsQuantity = 1000m;
				line3.JI_LinePrice = 1345m;
				line3.US_SupTariff = "9802004040";

				JobComInvoiceLine line4 = line1.AddSecondaryInvoiceLine();
				line4.JI_Tariff = "9102111040";
				line4.JI_CustomsQuantity = 1000m;
				line4.JI_LinePrice = 204m;
				line4.US_SupTariff = "9802004040";

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				AssertEquals("There should be one entry", 1, declaration.CustomsEntryHeaders.Count);
				AssertEquals("There should be eight entry lines", 8, declaration.CustomsEntryHeaders[0].MergedLines.Count);

				string[] tariffsInOrder = { "9802004040", "9102111010", "9802004040", "9102111020", "9802004040", "9102111030", "9802004040", "9102111040" };
				for (int i = 0; i < 8; i++)
				{
					CusEntryLine entryLine = declaration.ActiveEntryHeaders.EntrySummaryEntry.MergedLines[i];
					AssertEquals("Tariff Line : " + (i + 1).ToString(), tariffsInOrder[i], entryLine.CL_AdValoremTariff.ToString());
				}

				AssertEquals("Duty calculated for movement", 369.14m, line1.CusEntryLine.Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
				AssertEquals("Duty calculated for case", 174.38m, line2.CusEntryLine.Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
				AssertEquals("Duty calculated for bracelet", 145.77m, line3.CusEntryLine.Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
				AssertEquals("Duty calculated for battery", 22.11m, line4.CusEntryLine.Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
				AssertEquals("Duty percent", 10.838m, line1.CusEntryLine.CL_DutyPercent);
				AssertEquals("Duty percent", 10.838m, line2.CusEntryLine.CL_DutyPercent);
				AssertEquals("Duty percent", 10.838m, line3.CusEntryLine.CL_DutyPercent);
				AssertEquals("Duty percent", 10.838m, line4.CusEntryLine.CL_DutyPercent);
			}
		}

		public void TestFromWatchRepairToWatch()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 500m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
				line1.JI_Tariff = "9102111010";
				line1.JI_CustomsQuantity = 1000m;
				line1.JI_LinePrice = 3406m;
				line1.US_SupTariff = "9802004040";

				JobComInvoiceLine line2 = line1.AddSecondaryInvoiceLine();
				line2.JI_Tariff = "9102111020";
				line2.JI_CustomsQuantity = 1000m;
				line2.JI_LinePrice = 1609m;
				line2.US_SupTariff = "9802004040";
				line2.US_98GoodsValue = 500m;

				JobComInvoiceLine line3 = line1.AddSecondaryInvoiceLine();
				line3.JI_Tariff = "9102111030";
				line3.JI_CustomsQuantity = 1000m;
				line3.JI_LinePrice = 1345m;
				line3.US_SupTariff = "9802004040";

				JobComInvoiceLine line4 = line1.AddSecondaryInvoiceLine();
				line4.JI_Tariff = "9102111040";
				line4.JI_CustomsQuantity = 1000m;
				line4.JI_LinePrice = 204m;
				line4.US_SupTariff = "9802004040";

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				AssertEquals("There should be one entry", 1, declaration.CustomsEntryHeaders.Count);
				AssertEquals("There should be eight entry lines", 8, declaration.CustomsEntryHeaders[0].MergedLines.Count);

				line1.US_SupTariff = ZString.Empty;
				line2.US_SupTariff = ZString.Empty;
				line2.US_98GoodsValue = ZDecimal.Zero;
				line3.US_SupTariff = ZString.Empty;
				line4.US_SupTariff = ZString.Empty;

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("There should be one entry", 1, declaration.CustomsEntryHeaders.Count);
				AssertEquals("There should be four entry lines", 4, declaration.CustomsEntryHeaders[0].MergedLines.Count);

				AssertNull(line1.CusEntryLine.ParentLine);
				AssertEquals(line1.CusEntryLine, line2.CusEntryLine.ParentLine);
				AssertEquals(line1.CusEntryLine, line3.CusEntryLine.ParentLine);
				AssertEquals(line1.CusEntryLine, line4.CusEntryLine.ParentLine);
			}
		}

		public void TestFromWatchRepairToWatchForBothENSAndCRL()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 500m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
				line1.JI_Tariff = "9102111010";
				line1.JI_CustomsQuantity = 1000m;
				line1.JI_LinePrice = 3406m;
				line1.US_SupTariff = "9802004040";

				JobComInvoiceLine line2 = line1.AddSecondaryInvoiceLine();
				line2.JI_Tariff = "9102111020";
				line2.JI_CustomsQuantity = 1000m;
				line2.JI_LinePrice = 1609m;
				line2.US_SupTariff = "9802004040";
				line2.US_98GoodsValue = 500m;

				JobComInvoiceLine line3 = line1.AddSecondaryInvoiceLine();
				line3.JI_Tariff = "9102111030";
				line3.JI_CustomsQuantity = 1000m;
				line3.JI_LinePrice = 1345m;
				line3.US_SupTariff = "9802004040";

				JobComInvoiceLine line4 = line1.AddSecondaryInvoiceLine();
				line4.JI_Tariff = "9102111040";
				line4.JI_CustomsQuantity = 1000m;
				line4.JI_LinePrice = 204m;
				line4.US_SupTariff = "9802004040";

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				AssertWatchRepairMergeResult(declaration.CustomsEntryHeaders[0]);
				AssertWatchRepairMergeResult(declaration.CustomsEntryHeaders[1]);

				line1.US_SupTariff = ZString.Empty;
				line2.US_SupTariff = ZString.Empty;
				line2.US_98GoodsValue = ZDecimal.Zero;
				line3.US_SupTariff = ZString.Empty;
				line4.US_SupTariff = ZString.Empty;

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("There should be one entry", 2, declaration.CustomsEntryHeaders.Count);
				AssertEquals("There should be four entry lines", 4, declaration.CustomsEntryHeaders[0].MergedLines.Count);
				AssertEquals("There should be four entry lines", 4, declaration.CustomsEntryHeaders[1].MergedLines.Count);

				AssertNull(line1.CusEntryLine.ParentLine);
				AssertEquals(line1.CusEntryLine, line2.CusEntryLine.ParentLine);
				AssertEquals(line1.CusEntryLine, line3.CusEntryLine.ParentLine);
				AssertEquals(line1.CusEntryLine, line4.CusEntryLine.ParentLine);
			}
		}

		void AssertMergeResult(CusEntryHeader entry)
		{
			AssertEquals("Two entry lines", 2, entry.MergedLines.Count);

			List<Customs.Business.CusEntryLine> entryLines = new List<Customs.Business.CusEntryLine>(new TypedEnumerable<Customs.Business.CusEntryLine>(entry.MergedLines));
			entryLines.Sort(new EntrySummaryEntryLineComparerForNumbering());

			if (entry.CH_MessageType == CusEntryHeaderMessageTypeList.Codes.EntrySummary)
			{
				AssertEquals(entryLines[0], ((CusEntryLine)entryLines[1]).ParentLine);
			}

			AssertEquals("9802006000", entryLines[0].CL_AdValoremTariff);
			AssertEquals(10000m, entryLines[0].CL_CustomsValue);
			AssertEquals((short)1, entryLines[0].CL_LineNumber);

			AssertEquals("5208112040", entryLines[1].CL_AdValoremTariff);
			AssertEquals(500m, entryLines[1].CL_CustomsValue);
			AssertEquals((short)1, entryLines[0].CL_LineNumber);
		}

		void AssertWatchRepairMergeResult(CusEntryHeader entry)
		{
			AssertEquals("There should be eight entry lines", 8, entry.MergedLines.Count);

			if (entry.CH_MessageType == CusEntryHeaderMessageTypeList.Codes.EntrySummary)
			{
				CusEntryLine supParentLine = entry.MergedLines[0];
				AssertEquals(7, supParentLine.ChildSecondaryEntryLines.Count);

				AssertNull(supParentLine.ParentLine);
				AssertEquals(supParentLine, entry.MergedLines[1].ParentLine);
				AssertEquals(supParentLine, entry.MergedLines[2].ParentLine);
				AssertEquals(supParentLine, entry.MergedLines[3].ParentLine);
				AssertEquals(supParentLine, entry.MergedLines[4].ParentLine);
				AssertEquals(supParentLine, entry.MergedLines[5].ParentLine);
				AssertEquals(supParentLine, entry.MergedLines[6].ParentLine);
				AssertEquals(supParentLine, entry.MergedLines[7].ParentLine);
			}

			Assert(!entry.MergedLines[1].US_SupLine);
			Assert(entry.MergedLines[2].US_SupLine);
			Assert(!entry.MergedLines[3].US_SupLine);
			Assert(entry.MergedLines[4].US_SupLine);
			Assert(!entry.MergedLines[5].US_SupLine);
			Assert(entry.MergedLines[6].US_SupLine);
			Assert(!entry.MergedLines[7].US_SupLine);
		}
	}

	class SupplementaryTariffLineMergeStrategyTest : TestCaseWithFactory
	{
		public void TestLineIsValidForMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var strategy = new SupplementaryTariffLineMergeStrategy(declaration, ZString.Empty, null);
			AssertEquals(false, strategy.LineIsValidForMerge(invoiceLine));

			invoiceLine.US_SupTariff = TariffViewAsCodeDescription.NotApplicableCode;
			AssertEquals(false, strategy.LineIsValidForMerge(invoiceLine));

			invoiceLine.US_SupTariff = "9999999999";
			AssertEquals(true, strategy.LineIsValidForMerge(invoiceLine));
		}
	}
}
