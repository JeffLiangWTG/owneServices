using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntryHeaderENS7501Line))]
	sealed class EntryHeaderENS7501LineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSecondaryLinesForXVVSets()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			SetUpMergedInvoices();
			var invoiceLine = Declaration.InvoiceLines[0];
			invoiceLine.US_UC_NKCountryOfExport = "FR";
			invoiceLine.US_UC_NKCountryOfExport = "DE";
			invoiceLine.US_SPI = "R";
			invoiceLine.US_SecondarySPI = "X";

			var firstVLine = invoiceLine.AddSecondaryInvoiceLine();
			firstVLine.US_SupTariff = "99038814";
			firstVLine.JI_Tariff = "";
			firstVLine.US_SecondarySPI = "V";

			var secondVLine = firstVLine.AddSecondaryInvoiceLine();
			secondVLine.US_SupTariff = "99038815";
			secondVLine.JI_Tariff = "8457100075";
			secondVLine.US_SecondarySPI = "V";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryPrintLine = new ACEEntryHeaderENS7501Line(firstVLine.GetMatchingSupEntryLine(firstVLine.US_SupTariff), false, false);

			AssertEquals(entryPrintLine.SecondaryLine1FormattedTariff.Replace(".", ""), "99038815");
			AssertEquals(entryPrintLine.SecondaryLine2FormattedTariff.Replace(".", ""), "8457100075");
		}

		[TestDate(2019, 02, 25)]
		public void TestPrintDutyAmountFromEntryLineFor9808003000OnLine()
		{
			var invoice1 = Declaration.Invoices.AddNew();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.US_SupTariff = "9808003000";
			line1.JI_Tariff = "8457100075";
			line1.JI_LinePrice = 2000m;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var entryHeader = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var entryLine = entryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "8457100075");

			var entryPrintLine = new ACEEntryHeaderENS7501Line(entryLine, false, false);
			AssertEquals(84m, entryPrintLine.DutyAmount);
			AssertEquals("4.2%", entryPrintLine.DutyPercentAsString);
		}

		public void TestPrintDutyAmountFromEntryLineForTIBChapter98()
		{
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038001Tariff.UE_Tariff;
			testHelper.ChildLine.US_SupTariff = testHelper.Test9813Tariff.UE_Tariff;
			testHelper.ParentLine.JI_LinePrice = 1000m;
			testHelper.Charpter98Job.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			testHelper.Charpter98Job.US_BondType = "9";
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryHeader = testHelper.Charpter98Job.ActiveEntryHeaders.EntrySummaryEntry;
			AssertNotNull(entryHeader);

			var entryLine = entryHeader.EntryLines.FirstOrDefault(x => x.CL_AdValoremTariff == testHelper.Test99038001Tariff.UE_Tariff);
			var entryPrintLine = new ACEEntryHeaderENS7501Line(entryLine, false, false);
			AssertEquals("99038001(1000 * 40% = 400)", 400m, entryPrintLine.DutyAmount);
		}

		public void TestSecondaryLineOrderForChapter98()
		{
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038501Tariff.UE_Tariff;//99038501
			testHelper.ChildLine.US_SupTariff = testHelper.Test9802005060Tariff.UE_Tariff;//9802005060
			testHelper.ParentLine.JI_LinePrice = 1000m;
			testHelper.ChildLine.US_98GoodsValue = 1000m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryHeader = testHelper.Charpter98Job.ActiveEntryHeaders.EntrySummaryEntry;
			AssertNotNull(entryHeader);

			var entryLine = entryHeader.EntryLines.FirstOrDefault(x => x.CL_AdValoremTariff == testHelper.Test99038501Tariff.UE_Tariff);
			AssertEquals(entryLine.ChildSecondaryEntryLines.Count, 3);

			var entryPrintLine = new ACEEntryHeaderENS7501Line(entryLine, false, false);

			var ientryLine = entryLine as ICusEntryLine;
			AssertNotNull(ientryLine);
			var secondaryTariffLines = ientryLine.SecondaryTariffLines.ToList();
			AssertEquals("Remove the empty line", secondaryTariffLines.Count, 2);
			AssertEquals(entryPrintLine.SecondaryLine1FormattedTariff.Replace(".", ""), testHelper.TestCTariff.UE_Tariff);
			AssertEquals(entryPrintLine.SecondaryLine2FormattedTariff.Replace(".", ""), testHelper.Test9802005060Tariff.UE_Tariff);
		}

		public void TestLineDescription()
		{
			SetUpMergedInvoices();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			EntryHeaderENS7501Line ehl = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("Tariff description should be printed for line.", LineTariff.UE_ShortDescription, ehl.Description);
		}

		public void TestCustomsQuantity()
		{
			JobComInvoiceHeader invoice1 = Declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = LineTariff.UE_Tariff;
			line1.JI_CustomsQuantity = 576.28;
			line1.JI_CustomsUnitQty = "KG";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			EntryHeaderENS7501Line entryPrintLine = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("CustomsQuantity should have 0 decimals", 576m, entryPrintLine.CustomsQuantity);

			line1.ImportTariff.UE_Column1RateSpecific = 1.29m;
			Factory.Save();
			entryPrintLine = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("CustomsQuantity should have 2 decimals for quantity PFL", 576.28m, entryPrintLine.CustomsQuantity);
		}

		public void TestLargeCustomsValue()
		{
			var invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 9200000000.25m;
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "9801001075";
			line1.JI_LinePrice = 9200000000.25m;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			EntryHeaderENS7501Line entryPrintLine = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("Print should handle large values", 9200000000m, entryPrintLine.TotalLinePriceInLocalCurrencyRounded);
			AssertEquals("This field should still not show decimals", "9200000000", entryPrintLine.TotalLinePriceInLocalCurrencyRounded.ToString());
		}

		public void TestHasMPF()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6110202078";
			invoiceLine.JI_LinePrice = 0.3m;

			JobComInvoiceLine childLine1 = AddSecondaryLine(invoiceLine, "6110202079", 10m, 10m);
			JobComInvoiceLine childLine2 = AddSecondaryLine(invoiceLine, "6110202079", 11m, 11m);
			JobComInvoiceLine childLine3 = AddSecondaryLine(invoiceLine, "6110202079", 12m, 12m);
			JobComInvoiceLine childLine4 = AddSecondaryLine(invoiceLine, "6110202079", 13m, 13m);
			JobComInvoiceLine childLine5 = AddSecondaryLine(invoiceLine, "6110202079", 14m, 14m);
			JobComInvoiceLine childLine6 = AddSecondaryLine(invoiceLine, "6110202079", 15m, 15m);
			JobComInvoiceLine childLine7 = AddSecondaryLine(invoiceLine, "6110202079", 16m, 16m);

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			EntryHeaderENS7501Line entryPrintLine = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals(true, entryPrintLine.HasMPF);

			invoiceLine.US_SPI = "AU";
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryPrintLine = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals(false, entryPrintLine.HasMPF);

			invoiceLine.US_SPI = "";
			invoiceLine.JI_LinePrice = 0.3m;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertHasMPF(entry, childLine1);
			AssertHasMPF(entry, childLine2);
			AssertHasMPF(entry, childLine3);
			AssertHasMPF(entry, childLine4);
			AssertHasMPF(entry, childLine5);
			AssertHasMPF(entry, childLine6);
			AssertHasMPF(entry, childLine7);
		}

		public void TestMPFPercentAsString_1()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6110202078";
			invoiceLine.JI_LinePrice = 0.3m;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var entryPrintLine = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("MPF Rate as string", "0.3464%", entryPrintLine.MPFPercentAsString);
		}

		public void TestExportDate()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.US_EntryFilerCode = "XXX";
			dec.US_EnableENS = true;
			dec.US_DateOfExport = ZDateTime.BrettsBirthday;
			dec.Invoices.AddNew();
			var invoiceLine = dec.InvoiceLines.AddNew();
			var invoiceLine2 = dec.InvoiceLines.AddNew();
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = dec.ActiveEntryHeaders.EntrySummaryEntry;

			var printBO = new ACSEntryHeaderENS7501Line(invoiceLine.CusEntryLine, false, false);
			AssertEquals("", printBO.ExportDate);

			invoiceLine2.US_DateOfExport = ZDateTime.BrettsBirthday.AddDays(1);
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			printBO = new ACSEntryHeaderENS7501Line(invoiceLine.CusEntryLine, false, false);
			AssertEquals(ZDateTime.BrettsBirthday.ToString("MMddyy"), printBO.ExportDate);

			printBO = new ACSEntryHeaderENS7501Line(invoiceLine2.CusEntryLine, false, false);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1).ToString("MMddyy"), printBO.ExportDate);
		}

		[TestDate(2009, 12, 12)]
		public void TestMPFPercentAsString_2()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6110202078";
			invoiceLine.JI_LinePrice = 0.3m;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var entryPrintLine = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("MPF Rate as string", "0.21%", entryPrintLine.MPFPercentAsString);
		}

		[TestDate(2009, 12, 12)]
		public void TestSecondaryLinesSecondQtyAndUQ()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6110202078";
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			AddSecondaryLine(invoiceLine, "6110202079", 10m, 10m);
			AddSecondaryLine(invoiceLine, "6110202079", 11m, 11m);
			AddSecondaryLine(invoiceLine, "6110202079", 12m, 12m);
			AddSecondaryLine(invoiceLine, "6110202079", 13m, 13m);
			AddSecondaryLine(invoiceLine, "6110202079", 14m, 14m);
			AddSecondaryLine(invoiceLine, "6110202079", 15m, 15m);
			AddSecondaryLine(invoiceLine, "6110202079", 16m, 16m);

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			EntryHeaderENS7501Line entryPrintLine = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("10 KG", entryPrintLine.SecondaryLine1SecondQtyAndUQ);
			AssertEquals("11 KG", entryPrintLine.SecondaryLine2SecondQtyAndUQ);
			AssertEquals("12 KG", entryPrintLine.SecondaryLine3SecondQtyAndUQ);
			AssertEquals("13 KG", entryPrintLine.SecondaryLine4SecondQtyAndUQ);
			AssertEquals("14 KG", entryPrintLine.SecondaryLine5SecondQtyAndUQ);
			AssertEquals("15 KG", entryPrintLine.SecondaryLine6SecondQtyAndUQ);
			AssertEquals("16 KG", entryPrintLine.SecondaryLine7SecondQtyAndUQ);
		}

		public void TestSorghumAmount()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1007000020";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			entry.MergedLines[0].Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.Sorghum).CF_ChargeAmount = 14m;

			EntryHeaderENS7501Line entryPrintLine = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals(14m, entryPrintLine.LineFeeAmount);
		}

		[TestDate(2009, 6, 1)]
		public void TestSecondaryLineFeesAreIncluded()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			CreateWatchDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Entry should have pro-rated summary", true, ehp.EntryPrintLines[0].ProRatedCalculation);
			AssertEquals("Pro-rated summary line 1", "9802.00.8068 (Free) 3066 / 9426 (Total Value) = 32.527%", ehp.EntryPrintLines[0].ProRatedLine1);
			AssertEquals("Pro-rated summary line 2", "32.527% x $796.25 (Total Duty Column 34) = $259.00", ehp.EntryPrintLines[0].ProRatedLine2);
			AssertEquals("Pro-rated summary line 3", "$796.25 - $259.00 = $537.25 (Total Duty Due)", ehp.EntryPrintLines[0].ProRatedLine3);
			AssertEquals("Box 37 Total Duty should remain as actual Duty", 537.25m, ehp.TotalDutyAmt);

			AssertEquals("Secondary Line 1 Actual Duty", 296.88m, ehp.EntryPrintLines[0].secondaryTariffLine1.DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 440m, ehp.EntryPrintLines[0].SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "44c/NO", ehp.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);

			AssertEquals("Secondary Line 3 Actual Duty", 106.03m, ehp.EntryPrintLines[0].secondaryTariffLine3.DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 157.14m, ehp.EntryPrintLines[0].SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty Rate to print on 7501", "7.09%", ehp.EntryPrintLines[0].SecondaryLine3DutyPercentAsString);

			AssertEquals("Secondary Line 5 Actual Duty", 127.05m, ehp.EntryPrintLines[0].secondaryTariffLine5.DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 188.3m, ehp.EntryPrintLines[0].SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty Rate to print on 7501", "7.09%", ehp.EntryPrintLines[0].SecondaryLine5DutyPercentAsString);

			AssertEquals("Secondary Line 7 Actual Duty", 7.29m, ehp.EntryPrintLines[0].secondaryTariffLine7.DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 10.81m, ehp.EntryPrintLines[0].SecondaryLine7DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty Rate to print on 7501", "7.09%", ehp.EntryPrintLines[0].SecondaryLine7DutyPercentAsString);

			AssertEquals("MPF amount should be calculated", false, ehp.EntryPrintLines[0].MPFPercentAsString.IsEmpty);
			AssertEquals("MPF amount should be calculated", 13.35m, ehp.EntryPrintLines[0].MPFAmount);
			AssertEquals(true, ehp.EntryPrintLines[0].HasMPF);
		}

		[TestDate(2009, 12, 02)]
		public void TestSecondaryLineDutyForNonWatchOSAssembledGoods()
		{
			CreateOSAssembledGoodsDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Box 37 Total Duty should be actual Duty", 687.74m, ehp.TotalDutyAmt);

			AssertEquals("Line 1 Actual Duty", 0m, ehp.EntryPrintLines[0].line.DutyAmount);
			AssertEquals("Line 1 Assembled Component Duty to print on 7501 should be same", 0m, ehp.EntryPrintLines[0].DutyAmount);
			AssertEquals("Line 1 Assembled Component Duty Rate to print on 7501", "Free", ehp.EntryPrintLines[0].DutyPercentAsString);

			AssertEquals("Secondary Line 1 Actual Duty", 687.74m, ehp.EntryPrintLines[0].secondaryTariffLine1.DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501 should be same", 687.74m, ehp.EntryPrintLines[0].SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "27.90%", ehp.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);
		}

		[TestDate(2013, 08, 13)]
		public void TestSecondaryLineCompoundDutyRateForWI00046587()
		{
			CreateDeclarationForWI00046587();
			var entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Box 37 Total Duty should be actual Duty", 2752.50m, ehp.TotalDutyAmt);

			AssertEquals("Line 1 Actual Duty", 0m, ehp.EntryPrintLines[0].line.DutyAmount);
			AssertEquals("Line 1 Duty to print on 7501", 0m, ehp.EntryPrintLines[0].DutyAmount);
			AssertEquals("Line 1 Duty Rate to print on 7501", "Free", ehp.EntryPrintLines[0].DutyPercentAsString);

			AssertEquals("Secondary Line 1 Actual Duty", 2752.50m, ehp.EntryPrintLines[0].secondaryTariffLine1.DutyAmount);
			AssertEquals("Secondary Line 1 Compound Duty to print on 7501 should be total duty", 2752.50m, ehp.EntryPrintLines[0].SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Compound Duty Rate to print on 7501", "18.35%", ehp.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);
		}

		public void TestHideSecondaryLine1SecondQtyLine()
		{
			SetUpMergedInvoices();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			EntryHeaderENS7501Line ehl = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("no secondary line details", true, ehl.HideSecondaryLine1SecondQtyLine);

			JobComInvoiceLine invoiceLine = Declaration.InvoiceLines[0];
			AddSecondaryLineWithADD(invoiceLine);
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			CusEntryLine line = Declaration.CustomsEntryHeaders[0].MergedLines[0];
			ehl = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("has secondary line details with ADD value", false, ehl.HideSecondaryLine1SecondQtyLine);
		}

		public void TestSecondaryLine1ADDDetails()
		{
			SetUpMergedInvoices();
			var entry = Declaration.CustomsEntryHeaders[0];
			var ehl = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("SecondaryLine1ADDNo", "", ehl.SecondaryLine1ADDNo);
			AssertEquals("SecondaryLine1ADDSurety", "", ehl.SecondaryLine1ADDSurety);
			AssertEquals("SecondaryLine1ADDSpecificDepositValueFormatted", "", ehl.SecondaryLine1ADDSpecificDepositValueFormatted);
			AssertEquals("SecondaryLine1ADDRate", "", ehl.SecondaryLine1ADDRate);
			AssertEquals("SecondaryLine1ADDFormatted", "", ehl.SecondaryLine1ADDFormatted);

			var invoiceLine = Declaration.InvoiceLines[0];
			AddSecondaryLineWithADD(invoiceLine);
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var line = Declaration.CustomsEntryHeaders[0].MergedLines[0];
			ehl = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("ADDNo should be blank", "", ehl.ADDNo);
			AssertEquals("SecondaryLine1ADDNo", "A427-818-000", ehl.SecondaryLine1ADDNo);
			AssertEquals("SecondaryLine1ADDSurety", "Surety Code #752", ehl.SecondaryLine1ADDSurety);
			AssertEquals("SecondaryLine1ADDSpecificDepositValueFormatted", "(7500)", ehl.SecondaryLine1ADDSpecificDepositValueFormatted);
			AssertEquals("SecondaryLine1ADDRate", "19.00%", ehl.SecondaryLine1ADDRate);
			AssertEquals("SecondaryLine1ADDFormatted", "1425.00", ehl.SecondaryLine1ADDFormatted);

			invoiceLine.ChildLines.ElementAt(0).US_ADDCaseNo = "A570504000";
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			line = Declaration.CustomsEntryHeaders[0].MergedLines[0];
			ehl = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("SecondaryLine1ADDRate", "108.00%", ehl.SecondaryLine1ADDRate);
		}

		public void TestSecondaryLine1ADDNoWhenSupplemenatryTariffUsed()
		{
			CreateDeclarationWithSupplementaryTariff();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			CusEntryLine line = Declaration.CustomsEntryHeaders[0].MergedLines[0];
			EntryHeaderENS7501Line ehl = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("Formatted tariff", "9817.00.9040", ehl.FormattedTariff);
			AssertEquals("ADDNo should be blank here as ADD relates to Supplementary tariff", "", ehl.ADDNo);
			AssertEquals("SecondaryLine tariff", "8205.20.3000", ehl.SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine1ADDNo", "A570-204-006", ehl.SecondaryLine1ADDNo);
		}

		public void TestADDNoWithSupplemenatryTariffAndADDRelatesToParent()
		{
			CreateDeclarationWithSupplementaryTariffADDRelatesToParent();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			CusEntryLine line = Declaration.CustomsEntryHeaders[0].MergedLines[0];
			EntryHeaderENS7501Line ehl = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("Formatted tariff", "9817.00.9040", ehl.FormattedTariff);
			AssertEquals("ADDNo", "A462-105-000", ehl.ADDNo);
			AssertEquals("SecondaryLine tariff", "8211.10.0000", ehl.SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine1ADDNo should be blank here as ADD relates to Primary tariff", "", ehl.SecondaryLine1ADDNo);
		}

		public void TestSecondaryLine1ADDDetailsWhenTIBEntry()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			SetUpMergedInvoices();
			var invoiceLine = Declaration.InvoiceLines[0];
			AddSecondaryLineWithADD(invoiceLine);
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = Declaration.CustomsEntryHeaders[0];
			var ehl = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("ADDNo should be blank", "", ehl.ADDNo);
			AssertEquals("SecondaryLine1ADDNo", "A427-818-000", ehl.SecondaryLine1ADDNo);
			AssertEquals("SecondaryLine1ADDSurety", "Surety Code #752", ehl.SecondaryLine1ADDSurety);
			AssertEquals("SecondaryLine1ADDRate", "19.00%", ehl.SecondaryLine1ADDRate);
			AssertEquals("SecondaryLine1ADDFormatted", "1425.00", ehl.SecondaryLine1ADDFormatted);

			invoiceLine.ChildLines.ElementAt(0).US_ADDCaseNo = "A570504000";
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			ehl = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("SecondaryLine1ADDRate", "108.00%", ehl.SecondaryLine1ADDRate);
			AssertEquals("SecondaryLine1ADDFormatted", "8100.00", ehl.SecondaryLine1ADDFormatted);
		}

		public void TestSecondaryLine1CVDDetails()
		{
			SetUpMergedInvoices();
			var entry = Declaration.CustomsEntryHeaders[0];
			var ehl = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("SecondaryLine1CVDNo", "", ehl.SecondaryLine1CVDNo);
			AssertEquals("SecondaryLine1CVDSurety", "", ehl.SecondaryLine1CVDSurety);
			AssertEquals("SecondaryLine1CVDRate", "", ehl.SecondaryLine1CVDRate);
			AssertEquals("SecondaryLine1CVDFormatted", "", ehl.SecondaryLine1CVDFormatted);

			var invoiceLine = Declaration.InvoiceLines[0];
			AddSecondaryLineWithCVD(invoiceLine);
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var line = Declaration.CustomsEntryHeaders[0].MergedLines[0];
			ehl = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("SecondaryLine1CVDNo", "C427-819-000", ehl.SecondaryLine1CVDNo);
			AssertEquals("SecondaryLine1CVDSurety", "Surety Code #530", ehl.SecondaryLine1CVDSurety);
			AssertEquals("SecondaryLine1CVDRate", "12.00%", ehl.SecondaryLine1CVDRate);
			AssertEquals("SecondaryLine1CVDFormatted", "1200.00", ehl.SecondaryLine1CVDFormatted);
		}

		public void TestSecondaryLine1ADDDetailsACE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice1 = declaration.Invoices.AddNew();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "2844200020";
			line1.JI_Weight = 10m;
			line1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			line1.JI_LinePrice = 10000m;
			line1.JI_CustomsSecondQuantity = 100m;
			line1.US_ADDCaseNo = "A427818000";
			line1.US_ADDDepositValue = 7500m;
			declaration.US_SuretyCode = "752";
			line1.US_ADDDepositRateIndicator = "A";
			line1.JI_InvoiceQuantity = 100;
			line1.JI_InvoiceUQ = "KG";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.CustomsEntryHeaders[0];
			var ehl = new ACEEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("SecondaryLine1ADDNo", "", ehl.SecondaryLine1ADDNo);
			AssertEquals("SecondaryLine1ADDSurety", "", ehl.SecondaryLine1ADDSurety);
			AssertEquals("SecondaryLine1ADDSpecificDepositValueFormatted", "", ehl.SecondaryLine1ADDSpecificDepositValueFormatted);
			AssertEquals("SecondaryLine1ADDRate", "", ehl.SecondaryLine1ADDRate);
			AssertEquals("SecondaryLine1ADDFormatted", "", ehl.SecondaryLine1ADDFormatted);

			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.US_SupTariff = "2844200020";
			AddSecondaryLineWithADD(invoiceLine);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var line = declaration.CustomsEntryHeaders[0].MergedLines[0];
			line.US_SupLine = true;

			ehl = new ACEEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("ADDNo should be blank", "", ehl.ADDNo);
			AssertEquals("SecondaryLine1ADDNo", "A427-818-000", ehl.SecondaryLine1ADDNo);
			AssertEquals("SecondaryLine1ADDSurety", "Surety Code #752", ehl.SecondaryLine1ADDSurety);
			AssertEquals("SecondaryLine1ADDSpecificDepositValueFormatted", "(7500)", ehl.SecondaryLine1ADDSpecificDepositValueFormatted);
			AssertEquals("SecondaryLine1ADDRate", "19.00%", ehl.SecondaryLine1ADDRate);
			AssertEquals("SecondaryLine1ADDFormatted", "2850.00", ehl.SecondaryLine1ADDFormatted);
		}

		public void TestSecondaryLine1CVDDetailsACE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice1 = declaration.Invoices.AddNew();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "2844200090";
			line1.JI_Weight = 10m;
			line1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			line1.JI_LinePrice = 10000m;
			line1.JI_CustomsSecondQuantity = 100m;
			line1.US_CVDCaseNo = "C427819000";
			declaration.US_SuretyCode = "530";
			line1.US_CVDDepositRateIndicator = "A";
			line1.JI_InvoiceQuantity = 100;
			line1.JI_InvoiceUQ = "KG";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.CustomsEntryHeaders[0];

			var ehl = new ACEEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("SecondaryLine1CVDNo", "", ehl.SecondaryLine1CVDNo);
			AssertEquals("SecondaryLine1CVDRate", "", ehl.SecondaryLine1CVDRate);
			AssertEquals("SecondaryLine1CVDFormatted", "", ehl.SecondaryLine1CVDFormatted);

			var invoiceLine = declaration.InvoiceLines[0];
			AddSecondaryLineWithCVD(invoiceLine);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var line = declaration.CustomsEntryHeaders[0].MergedLines[0];
			line.US_SupLine = true;

			ehl = new ACEEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("SecondaryLine1CVDNo", "C427-819-000", ehl.SecondaryLine1CVDNo);
			AssertEquals("SecondaryLine1CVDRate", "12.00%", ehl.SecondaryLine1CVDRate);
			AssertEquals("SecondaryLine1CVDFormatted", "2400.00", ehl.SecondaryLine1CVDFormatted);
		}

		public void TestSecondaryLine1CVDSpecificDepositValueFormatted()
		{
			SetUpMergedInvoices();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			EntryHeaderENS7501Line ehl = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("SecondaryLine1CVDSpecificDepositValueFormatted", "", ehl.SecondaryLine1CVDSpecificDepositValueFormatted);

			JobComInvoiceLine invoiceLine = Declaration.InvoiceLines[0];
			AddSecondaryLineWithCVD(invoiceLine);
			invoiceLine.ChildLines.ElementAt(0).US_CVDDepositValue = 2350m;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			CusEntryLine line = Declaration.CustomsEntryHeaders[0].MergedLines[0];
			ehl = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("SecondaryLine1CVDSpecificDepositValueFormatted", "(2350)", ehl.SecondaryLine1CVDSpecificDepositValueFormatted);
		}

		public void TestLicenses()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7219320042";
			invoiceLine.US_MiscPermitNo = "S8LPBR129";

			JobComInvoiceLine invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1007000020";
			invoiceLine2.US_AgricultureLicNo = "1-GH-483-8";

			JobComInvoiceLine invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "6110202079";

			JobComInvoiceLine invoiceLine4 = invoice.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "1505009000";
			invoiceLine4.US_WoolLicenceNo = "KT36";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			EntryHeaderENS7501Line entryPrintLine1 = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("Steel Permit should print", "S8LPBR129", entryPrintLine1.LicenseNumber);
			AssertEquals("Licence Text should print", "PMT/LIC#", entryPrintLine1.LicenseText);

			EntryHeaderENS7501Line entryPrintLine2 = new ACSEntryHeaderENS7501Line(entry.MergedLines[1], false, false);
			AssertEquals("Agriculture Licence should still print", "1-GH-483-8", entryPrintLine2.LicenseNumber);
			AssertEquals("Licence Text should print", "PMT/LIC#", entryPrintLine2.LicenseText);

			EntryHeaderENS7501Line entryPrintLine3 = new ACSEntryHeaderENS7501Line(entry.MergedLines[2], false, false);
			AssertEquals("Licence should blank", "", entryPrintLine3.LicenseNumber);
			AssertEquals("Licence Text should be blank", "", entryPrintLine3.LicenseText);

			EntryHeaderENS7501Line entryPrintLine4 = new ACSEntryHeaderENS7501Line(entry.MergedLines[3], false, false);
			AssertEquals("Wool Licence should print", "KT36", entryPrintLine4.LicenseNumber);
			AssertEquals("Licence Text should print", "PMT/LIC#", entryPrintLine4.LicenseText);
		}

		public void TestVisaCertificateNumber()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "5203000500";
			invoiceLine.US_CottonCertificateNo = "Y739485J";

			JobComInvoiceLine invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1007000020";
			invoiceLine2.US_VisaNo = "389595003";

			JobComInvoiceLine invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "6110202079";

			JobComInvoiceLine invoiceLine4 = invoice.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "1702904000";
			invoiceLine4.US_CAExportCertificate = "CA393484";

			JobComInvoiceLine invoiceLine5 = invoice.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "2940006000";
			invoiceLine5.US_CBTPACertificateNo = "YZ838H39";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			EntryHeaderENS7501Line entryPrintLine1 = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("Cotton Certificate should print", "C Y739485J", entryPrintLine1.VisaCertificateNumber);

			EntryHeaderENS7501Line entryPrintLine2 = new ACSEntryHeaderENS7501Line(entry.MergedLines[1], false, false);
			AssertEquals("Visa No should still print", "V 389595003", entryPrintLine2.VisaCertificateNumber);

			EntryHeaderENS7501Line entryPrintLine3 = new ACSEntryHeaderENS7501Line(entry.MergedLines[2], false, false);
			AssertEquals("Visa should blank", "", entryPrintLine3.VisaCertificateNumber);

			EntryHeaderENS7501Line entryPrintLine4 = new ACSEntryHeaderENS7501Line(entry.MergedLines[3], false, false);
			AssertEquals("Canadian Export Certificate should print", "C CA393484", entryPrintLine4.VisaCertificateNumber);

			EntryHeaderENS7501Line entryPrintLine5 = new ACSEntryHeaderENS7501Line(entry.MergedLines[4], false, false);
			AssertEquals("CBTPACertificateNo should print", "C YZ838H39", entryPrintLine5.VisaCertificateNumber);
		}

		[TestDate(2010, 04, 30)]
		public void TestDISPercentAsString()
		{
			CreateDistilledSpiritsEntry();
			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			EntryHeaderENS7501Line entryPrint = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);

			AssertEquals("DISPercentAsString", "$3.566322/PFL", entryPrint.LineFeePercentAsString);
			AssertEquals("SpiritsAmount", 713.26m, entryPrint.LineFeeAmount);

			AssertEquals("LineFeeDescription ", "016 016 DESC FROM DB", entryPrint.LineFeeDescription);
			AssertEquals("$3.566322/PFL", entryPrint.LineFeePercentAsString);
			AssertEquals("LineFeeAmount ", 713.26m, entryPrint.LineFeeAmount);
		}

		[TestDate(2011, 02, 16)]
		public void TestDutyRatePrintsForTIBEntry()
		{
			CreateTIBDeclaration();
			var entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB Duty", 1395.19m, ehp.TIBTotalDuty);
			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "Free", ehp.EntryPrintLines[0].DutyPercentAsString);
			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "5.8%", ehp.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);
			AssertEquals("TIB entry should also print the Duty amount as if not for TIB", 1395.19m, ehp.EntryPrintLines[0].SecondaryLine1DutyAmount);
		}

		[TestDate(2011, 03, 29)]
		public void TestDutyRateAndAmountPrintsForTIBSetEntry()
		{
			CreateTIBSetDeclaration();
			var entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("X line - TIB entry should print normal Duty Rate as if entry was not for TIB - tariff", "Free", ehp.EntryPrintLines[0].DutyPercentAsString);
			AssertEquals("X line - TIB entry should print normal Duty Rate as if entry was not for TIB - tariff", "6.4%", ehp.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);
			AssertEquals("X line - TIB entry should also print the Duty amount as if not for TIB", 320m, ehp.EntryPrintLines[0].SecondaryLine1DutyAmount);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff", "Free", ehp.EntryPrintLines[1].DutyPercentAsString);
			AssertEquals("For 'V' lines, there is no duty rate for this tariff", "", ehp.EntryPrintLines[1].SecondaryLine1DutyPercentAsString);
			AssertEquals("For 'V' lines, there is no duty amount for this tariff", 0m, ehp.EntryPrintLines[1].SecondaryLine1DutyAmount);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff", "Free", ehp.EntryPrintLines[2].DutyPercentAsString);
			AssertEquals("For 'V' lines, there is no duty rate for this tariff", "", ehp.EntryPrintLines[2].SecondaryLine1DutyPercentAsString);
			AssertEquals("For 'V' lines, there is no duty amount for this tariff", 0m, ehp.EntryPrintLines[2].SecondaryLine1DutyAmount);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff", "Free", ehp.EntryPrintLines[3].DutyPercentAsString);
			AssertEquals("For 'V' lines, there is no duty rate for this tariff", "", ehp.EntryPrintLines[3].SecondaryLine1DutyPercentAsString);
			AssertEquals("For 'V' lines, there is no duty amount for this tariff", 0m, ehp.EntryPrintLines[3].SecondaryLine1DutyAmount);

			AssertEquals("TIB Total Duty", 320m, ehp.TIBTotalDuty);
		}

		[TestDate(2011, 03, 29)]
		public void TestDutyRateAndAmountPrintsForTIBWatchEntry()
		{
			CreateTIBWatchDeclaration();
			var entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "Free", ehp.EntryPrintLines[0].DutyPercentAsString);
			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "51c/NO", ehp.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - SecondaryLine2", "6.25%", ehp.EntryPrintLines[0].SecondaryLine2DutyPercentAsString);
			AssertEquals("Customs Value", 100m, ehp.EntryPrintLines[0].secondaryTariffLine2.CL_CustomsValue);
			AssertEquals("TIB entry should also print the Duty amount as if not for TIB", 6.25m, ehp.EntryPrintLines[0].SecondaryLine2DutyAmount);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - SecondaryLine3", "6.25%", ehp.EntryPrintLines[0].SecondaryLine3DutyPercentAsString);
			AssertEquals("Customs Value", 100m, ehp.EntryPrintLines[0].secondaryTariffLine3.CL_CustomsValue);
			AssertEquals("TIB entry should also print the Duty amount as if not for TIB - SecondaryLine3", 6.25m, ehp.EntryPrintLines[0].SecondaryLine3DutyAmount);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - SecondaryLine4", "5.3%", ehp.EntryPrintLines[0].SecondaryLine4DutyPercentAsString);
			AssertEquals("Customs Value", 100m, ehp.EntryPrintLines[0].secondaryTariffLine4.CL_CustomsValue);
			AssertEquals("TIB entry should also print the Duty amount as if not for TIB - SecondaryLine4", 5.30m, ehp.EntryPrintLines[0].SecondaryLine4DutyAmount);
		}

		public void TestSPIAndOrSecondarySPI()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			SetUpMergedInvoices();
			var invoiceLine = Declaration.InvoiceLines[0];
			invoiceLine.US_UC_NKCountryOfExport = "FR";
			invoiceLine.US_UC_NKCountryOfExport = "DE";
			invoiceLine.US_SPI = "R";
			invoiceLine.US_SecondarySPI = "X";
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = Declaration.CustomsEntryHeaders[0];
			var entryPrintLine = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("SPI and Secondary SPI for ACS", "R.X", entryPrintLine.SPIAndOrSecondarySPI);
		}

		public void TestPortOfLadingForLine()
		{
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			var invoice1 = Declaration.Invoices.AddNew();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = LineTariff.UE_Tariff;
			line1.JI_InvoiceQuantity = 100;
			line1.JI_InvoiceUQ = "KG";
			line1.US_SchDLoading = "52000";

			var line2 = invoice1.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = USCTariff.DOTMayBeApplicable;
			line2.JI_InvoiceQuantity = 100;
			line2.JI_InvoiceUQ = "KG";
			line2.US_SchDLoading = "52002";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var ehl = new ACSEntryHeaderENS7501Line(entry.MergedLines.OfType<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == LineTariff.UE_Tariff), false, false);
			AssertEquals("Port Of Loading", "52000", ehl.PortOfLadingForLine);

			ehl = new ACSEntryHeaderENS7501Line(entry.MergedLines.OfType<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == USCTariff.DOTMayBeApplicable), false, false);
			AssertEquals("Port Of Loading", "52002", ehl.PortOfLadingForLine);
		}

		public void TestProductNo()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "Importer";
			OrgCusCode importerEINCode = importer.CustomsCodes.AddNew();
			importerEINCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			importerEINCode.OK_CustomsRegNo = "75-2221134";
			var countryData = importer.CountryData;
			var wrapper = OrgHeaderWrapper.New(importer);
			wrapper.ZO_ENSPrintProduct = true;

			Declaration.JE_OH_Importer = importer.PK;
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "7318160060";
			invoiceLine1.JI_PartNo = "0-423-55-500-2";
			invoiceLine1.JI_InvoiceQuantity = 300;
			invoiceLine1.JI_InvoiceUQ = "L";
			invoiceLine1.JI_LinePrice = 3000m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "7318160060";
			invoiceLine2.JI_PartNo = "0-02-90-124-0";
			invoiceLine2.JI_InvoiceQuantity = 100;
			invoiceLine2.JI_InvoiceUQ = "L";
			invoiceLine2.JI_LinePrice = 5000m;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = Declaration.CustomsEntryHeaders[0];
			AssertEquals("Lines should be merged", 1, entry.MergedLines.Count);
			var entryPrintLine1 = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("Product Number (Block29Element1) should not be printed when merged by tariff", "", entryPrintLine1.Block29Element1);

			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			entry = Declaration.CustomsEntryHeaders[0];
			AssertEquals("Lines should not be merged", 2, entry.MergedLines.Count);
			entryPrintLine1 = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("Product Number (Block29Element1) should print when merged by product - line 1", "0-423-55-500-2", entryPrintLine1.Block29Element1);

			var entryPrintLine2 = new ACSEntryHeaderENS7501Line(entry.MergedLines[1], false, false);
			AssertEquals("Product Number (Block29Element1) should print when merged by product - line 2", "0-02-90-124-0", entryPrintLine2.Block29Element1);

			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			entry = Declaration.CustomsEntryHeaders[0];
			entryPrintLine1 = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("Product Number (Block29Element1) should be printed when not merge", "0-423-55-500-2", entryPrintLine1.Block29Element1);

			entryPrintLine2 = new ACSEntryHeaderENS7501Line(entry.MergedLines[1], false, false);
			AssertEquals("Product Number (Block29Element1) should print when not merge", "0-02-90-124-0", entryPrintLine2.Block29Element1);
		}

		public void TestSecondaryLineCVDNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "99038802";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_Tariff = "8541406025";
			invoiceLine2.US_SupTariff = "99034521";
			invoiceLine2.US_ADDCaseNo = "A570979116";
			invoiceLine2.US_CVDCaseNo = "C570980043";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Lines should be merged", 4, entry.MergedLines.Count);
			var entryPrintLine1 = new ACEEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals(false, entryPrintLine1.HideSecondaryLine2SecondQtyLine);
			AssertEquals("C570-980-043", entryPrintLine1.SecondaryLine2CVDNo);
			var entryPrintLine2 = new ACEEntryHeaderENS7501Line(entry.MergedLines[1], false, false);
			AssertEquals(true, entryPrintLine2.HideSecondaryLine2SecondQtyLine);
			AssertEquals("", entryPrintLine2.SecondaryLine2CVDNo);
			var entryPrintLine3 = new ACEEntryHeaderENS7501Line(entry.MergedLines[2], false, false);
			AssertEquals(true, entryPrintLine3.HideSecondaryLine2SecondQtyLine);
			AssertEquals("", entryPrintLine3.SecondaryLine2CVDNo);
			var entryPrintLine4 = new ACEEntryHeaderENS7501Line(entry.MergedLines[3], false, false);
			AssertEquals(true, entryPrintLine4.HideSecondaryLine2SecondQtyLine);
			AssertEquals("", entryPrintLine4.SecondaryLine2CVDNo);
		}

		public void TestPrintExclusionNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8541406026";
			invoiceLine1.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
			invoiceLine1.US_ExclusionNumber = "STL000111";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8541406025";
			invoiceLine2.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._03;
			invoiceLine2.US_ExclusionNumber = "ALU000333";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Lines should be merged", 2, entry.MergedLines.Count);
			var entryPrintLine1 = new ACEEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("Product Exclusion No: " + invoiceLine1.US_ExclusionNumber, entryPrintLine1.ExclusionNumber);
			var entryPrintLine2 = new ACEEntryHeaderENS7501Line(entry.MergedLines[1], false, false);
			AssertEquals("Product Exclusion No: " + invoiceLine2.US_ExclusionNumber, entryPrintLine2.ExclusionNumber);
		}

		public void TestPrintExclusionNumbersInCombinedLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "99038801";
			invoiceLine1.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
			invoiceLine1.US_ExclusionNumber = "STL000111";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8541406025";
			invoiceLine2.US_SupTariff = "99038801";
			invoiceLine2.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._03;
			invoiceLine2.US_ExclusionNumber = "ALU000333";
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_ParentLine = invoiceLine1.JI_LineNo;
			invoiceLine1.US_IsParent = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Lines should be merged", 4, entry.MergedLines.Count);
			var entryPrintLine1 = new ACEEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("Product Exclusion No: " + invoiceLine1.US_ExclusionNumber + "\r\n" + "Product Exclusion No: " + invoiceLine2.US_ExclusionNumber, entryPrintLine1.ExclusionNumber);
		}

		public void TestPrintTotalLinePriceInLocalCurrencyRoundedForXVVLineWithSupAdditionalTariff()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030124", "7", 0.2m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038815", "7", 0.075m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "4818900080", "7", 0m, "KG");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "2853909090", "7", 0.028m, "KG");

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var xParentLine = invoice.JobComInvoiceLines.AddNew();
			xParentLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			xParentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			xParentLine.JI_FormattedTariff = "4818.90.0080";
			xParentLine.SupTariffFormatted = "9903.88.15";
			xParentLine.SupFormattedAdditionalTariff1 = "9903.01.24";
			xParentLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;

			var vParentLine = xParentLine.AddSecondaryInvoiceLine();
			vParentLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			vParentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			vParentLine.JI_FormattedTariff = "4818.90.0080";
			vParentLine.SupTariffFormatted = "9903.88.15";
			vParentLine.SupFormattedAdditionalTariff1 = "9903.01.24";
			vParentLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			vParentLine.JI_LinePrice = 2000m;

			var vChildLine = xParentLine.AddSecondaryInvoiceLine();
			vChildLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			vChildLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			vChildLine.JI_FormattedTariff = "2853.90.9090";
			vChildLine.SupTariffFormatted = "9903.01.24";
			vChildLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			vChildLine.JI_LinePrice = 3000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryPrintLine1 = new ACEEntryHeaderENS7501Line(xParentLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true, (x) => x.US_SupAdditionalLine), false, false);
			AssertEquals("TotalLinePriceInLocalCurrencyRounded is 0 for tariff 9903.01.24 on X line", 0m, entryPrintLine1.TotalLinePriceInLocalCurrencyRounded);
			AssertEquals("SecondaryLine1TotalLinePriceInLocalCurrencyRounded is 0 for tariff 9903.88.15 on X line", 0m, entryPrintLine1.SecondaryLine1TotalLinePriceInLocalCurrencyRounded);
			AssertEquals("SecondaryLine2TotalLinePriceInLocalCurrencyRounded is 5000 for tariff 4818.90.0080 on X line", 5000m, entryPrintLine1.SecondaryLine2TotalLinePriceInLocalCurrencyRounded);

			var entryPrintLine2 = new ACEEntryHeaderENS7501Line(vParentLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true, (x) => x.US_SupAdditionalLine), false, false);
			AssertEquals("TotalLinePriceInLocalCurrencyRounded is 0 for tariff 9903.01.24 on V line", 0m, entryPrintLine2.TotalLinePriceInLocalCurrencyRounded);
			AssertEquals("SecondaryLine1TotalLinePriceInLocalCurrencyRounded is 0 for tariff 9903.88.15 on V line", 0m, entryPrintLine2.SecondaryLine1TotalLinePriceInLocalCurrencyRounded);
			AssertEquals("SecondaryLine2TotalLinePriceInLocalCurrencyRounded is 5000 for tariff 4818.90.0080 on V line", 2000m, entryPrintLine2.SecondaryLine2TotalLinePriceInLocalCurrencyRounded);

			var entryPrintLine3 = new ACEEntryHeaderENS7501Line(vChildLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true), false, false);
			AssertEquals("TotalLinePriceInLocalCurrencyRounded is 0 for tariff 9903.01.24 on V line", 0m, entryPrintLine3.TotalLinePriceInLocalCurrencyRounded);
			AssertEquals("SecondaryLine1TotalLinePriceInLocalCurrencyRounded is 3000 for tariff 2853.90.9090 on V line", 3000m, entryPrintLine3.SecondaryLine1TotalLinePriceInLocalCurrencyRounded);
		}

		protected override BusinessObject GetNewBusinessObject() => new ACSEntryHeaderENS7501Line(Factory.NewWithValidTestData<CusEntryLine>(), false, false);

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			GlbBranch.CurrentBranch.SetCountry("US");
			DeclarationTestHelper.SetEntryFilerCode("XJ6");
		}

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
					declaration.US_EnableENS = true;
				}
				return declaration;
			}
		}

		void AssertHasMPF(CusEntryHeader entry, JobComInvoiceLine invoiceLine)
		{
			var entryPrintLine = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals(0m, entryPrintLine.MPFAmount);
			AssertEquals(true, entryPrintLine.HasMPF);
		}

		void SetUpMergedInvoices()
		{
			var invoice1 = Declaration.Invoices.AddNew();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = LineTariff.UE_Tariff;
			line1.JI_InvoiceQuantity = 100;
			line1.JI_InvoiceUQ = "KG";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
		}

		USCTariff lineTariff;
		USCTariff LineTariff => lineTariff ?? (lineTariff = Factory.LoadTop1<USCTariff>(new ZQuery()));

		void CreateDistilledSpiritsEntry()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "GB";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2208303030";
			invoiceLine1.JI_InvoiceQuantity = 500m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine1.JI_CustomsQuantity = 200m;
			invoiceLine1.JI_CustomsUnitQty = "PFL";
		}

		void CreateWatchDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "WATCH";
			invoiceHeader.JZ_InvoiceAmount = 9426m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "HK";
			invoiceHeader.JZ_IncoTerm = "FOB";

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "9802008068";
				invoiceLine1.JI_InvoiceQuantity = 0m;
				invoiceLine1.JI_CustomsQuantity = 0m;
				invoiceLine1.US_98GoodsValue = 1852m;
				invoiceLine1.JI_Tariff = "9102111010";
				invoiceLine1.JI_InvoiceQuantity = 1000m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 1000m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 3406m;

				JobComInvoiceLine childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.US_SupTariff = "9802008068";
				childLine2.JI_InvoiceQuantity = 0m;
				childLine2.JI_CustomsQuantity = 0m;
				childLine2.US_98GoodsValue = 1010m;

				childLine2.JI_Tariff = "9102111020";
				childLine2.JI_InvoiceQuantity = 1000m;
				childLine2.JI_InvoiceUQ = "NO";
				childLine2.JI_CustomsQuantity = 1000m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 1609m;

				JobComInvoiceLine childLine4 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine4.US_SupTariff = "9802008068";
				childLine4.JI_InvoiceQuantity = 0m;
				childLine4.JI_CustomsQuantity = 0m;
				childLine4.JI_Tariff = "9102111030";
				childLine4.JI_InvoiceQuantity = 1000m;
				childLine4.JI_InvoiceUQ = "NO";
				childLine4.JI_CustomsQuantity = 1000m;
				childLine4.JI_CustomsUnitQty = "NO";
				childLine4.JI_LinePrice = 1345m;

				JobComInvoiceLine childLine6 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine6.US_SupTariff = "9802008068";
				childLine6.JI_InvoiceQuantity = 0m;
				childLine6.JI_CustomsQuantity = 0m;
				childLine6.US_98GoodsValue = 204m;

				childLine6.JI_Tariff = "9102111040";
				childLine6.JI_InvoiceQuantity = 1000m;
				childLine6.JI_InvoiceUQ = "NO";
				childLine6.JI_CustomsQuantity = 1000m;
				childLine6.JI_CustomsUnitQty = "NO";
				childLine6.JI_LinePrice = 0m;
			}

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateOSAssembledGoodsDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.JE_MessageType = "IMP";
			Declaration.JE_TransportMode = "SEA";

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Declaration.US_CertifyCargoRelease = true;
			Declaration.US_SchDLoading = "60204";
			Declaration.US_SchDArrival = "1101";

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV-0089";
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_InvoiceAmount = 2683.80m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "AU";
			invoiceHeader.US_UC_NKCountryOfExport = "AU";
			invoiceHeader.US_TransactionsRelated = "Y";

			InvoiceCharge freightCharge = invoiceHeader.Charges.AddNew();
			freightCharge.J7_Amount = 59m;
			freightCharge.J7_RX_NKCurrency = "USD";
			freightCharge.J7_ChargeDescription = "OVERSEAS FREIGHT";
			freightCharge.J7_ChargeType = "OFT";
			freightCharge.J7_DistributeBy = "VAL";

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "9802008068";
				invoiceLine1.JI_InvoiceQuantity = 994m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 0m;
				invoiceLine1.JI_LinePrice = 0m;
				invoiceLine1.JI_Weight = 387.66m;
				invoiceLine1.JI_WeightUQ = "KG";
				invoiceLine1.US_98GoodsValue = 2544.64m;
				invoiceLine1.US_UC_NKCountryOfExport = "AU";
				invoiceLine1.US_UC_NKCountryOfOrigin = "HN";
				invoiceLine1.US_SPI = "N/A";
				invoiceLine1.US_DestinationState = "AK";

				AssertEquals("PreCondition", 2544.64m, invoiceLine1.JI_CustomsValue);
				invoiceLine1.JI_Tariff = "6203434030";
				invoiceLine1.JI_InvoiceQuantity = 994m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 83m;
				invoiceLine1.JI_CustomsUnitQty = "DOZ";
				invoiceLine1.JI_LinePrice = 2465.12;
				invoiceLine1.JI_Weight = 49.7m;
				invoiceLine1.JI_WeightUQ = "KG";
				invoiceLine1.JI_CustomsSecondQuantity = 437.36m;
				invoiceLine1.JI_CustomsSecondUnitQty = "KG";
				invoiceLine1.US_UC_NKCountryOfExport = "AU";
				invoiceLine1.US_UC_NKCountryOfOrigin = "HN";
				invoiceLine1.US_SPI = "N/A";
				invoiceLine1.US_DestinationState = "AK";

				JobComInvoiceLine invoiceLine3 = Declaration.InvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "9801001010";
				invoiceLine3.JI_InvoiceQuantity = 994m;
				invoiceLine3.JI_InvoiceUQ = "NO";
				invoiceLine3.JI_CustomsQuantity = 0m;
				invoiceLine3.US_98GoodsValue = 218.68m;
				invoiceLine3.JI_Weight = 19m;
				invoiceLine3.US_UC_NKCountryOfExport = "AU";
				invoiceLine3.US_UC_NKCountryOfOrigin = "US";
				invoiceLine3.US_DestinationState = "AK";
				AssertEquals("PreCondition", 218.68m, invoiceLine3.JI_CustomsValue);
			}

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateDeclarationWithSupplementaryTariff()
		{
			Declaration.US_ADDCVDSuretyCode = "444";

			JobComInvoiceHeader invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceAmount = 123620m;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "8205203000";  // ADD relates to this tariff - child line
			line1.US_SupTariff = "9817009040";
			line1.JI_InvoiceQuantity = 7947;
			line1.JI_InvoiceUQ = "L";
			line1.JI_LinePrice = 123620m;
			line1.US_ADDCaseNo = "A570204006";

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateDeclarationForWI00046587()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.JE_MessageType = "IMP";
			Declaration.JE_TransportMode = "SEA";
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Declaration.US_CertifyCargoRelease = true;
			Declaration.US_SchDLoading = "58201";
			Declaration.US_SchDArrival = "3901";

			var invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV081013";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceAmount = 20000m;
			invoice1.JZ_RN_NKDefaultOrigin = "IR";
			invoice1.US_UC_NKCountryOfExport = "HK";
			invoice1.US_TransactionsRelated = "N";
			invoice1.JZ_IncoTerm = "FOB";

			var freightCharge = invoice1.Charges.AddNew();
			freightCharge.J7_Amount = 1000m;
			freightCharge.J7_RX_NKCurrency = "USD";
			freightCharge.J7_ChargeDescription = "OVERSEAS FREIGHT";
			freightCharge.J7_ChargeType = "OFT";
			freightCharge.J7_DistributeBy = "VAL";

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "6201110010";
			line1.US_SupTariff = "9802008068";
			line1.JI_InvoiceQuantity = 42m;
			line1.JI_InvoiceUQ = "DOZ";
			line1.JI_CustomsQuantity = 42m;
			line1.JI_CustomsUnitQty = "DOZ";
			line1.JI_CustomsSecondQuantity = 1000m;
			line1.JI_CustomsSecondUnitQty = "KG";
			line1.JI_LinePrice = 15000m;
			line1.US_98GoodsValue = 0m;
			line1.US_98ValueInvCurr = 5000m;
			line1.JI_Weight = 1000m;
			line1.JI_WeightUQ = "KG";
			line1.US_DestinationState = "IL";
			AssertEquals("PreCondition", 20000m, line1.JI_CustomsValue);

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateDeclarationWithSupplementaryTariffADDRelatesToParent()
		{
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A462105000";
			addCase.U5_ISOCountryCode = "RU";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.IO;
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "98170090";
			var rate = addCase.CaseRates.AddNew();
			rate.U6_AdValoremRate = 1.02m;
			rate.U6_EffectiveDate = ZDateTime.Today;

			Declaration.US_ADDCVDSuretyCode = "444";

			JobComInvoiceHeader invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceAmount = 123620m;
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "8211100000";
			line1.US_SupTariff = "9817009040";  // ADD relates to this tariff - parent line
			line1.JI_InvoiceQuantity = 7947;
			line1.JI_InvoiceUQ = "L";
			line1.JI_LinePrice = 123620m;
			line1.US_ADDCaseNo = "A462105000";

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateTIBDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_EnableENS = true;

			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_InvoiceAmount = 24055.10m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.US_UC_NKCountryOfExport = "GB";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "GB";

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130050";
			invoiceLine1.JI_Tariff = "3920995000";
			invoiceLine1.JI_CustomsQuantity = 6100m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_LinePrice = 24055m;
			invoiceLine1.US_DestinationState = "PA";

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateTIBSetDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_EnableENS = true;

			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_InvoiceAmount = 5000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.US_UC_NKCountryOfExport = "AU";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "AU";

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130020";
			invoiceLine1.JI_Tariff = "1902194000";
			invoiceLine1.JI_CustomsQuantity = 3m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_LinePrice = 0m;
			invoiceLine1.JI_Weight = 2500;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.US_SecondarySPI = "X";
			invoiceLine1.US_UC_NKCountryOfExport = "CH";
			invoiceLine1.JI_CountryOfOrigin = "CH";
			invoiceLine1.US_DestinationState = "IL";

			var invoiceLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine2.US_SupTariff = "98130020";
			invoiceLine2.JI_Tariff = "0712311000";
			invoiceLine2.JI_CustomsQuantity = 3m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_LinePrice = 1300m;
			invoiceLine2.JI_Weight = 650;
			invoiceLine2.JI_WeightUQ = "KG";
			invoiceLine2.US_SecondarySPI = "V";
			invoiceLine2.US_UC_NKCountryOfExport = "CH";
			invoiceLine2.JI_CountryOfOrigin = "CH";
			invoiceLine2.US_DestinationState = "IL";

			var invoiceLine3 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine3.US_SupTariff = "98130020";
			invoiceLine3.JI_Tariff = "2002908020";
			invoiceLine3.JI_CustomsQuantity = 3m;
			invoiceLine3.JI_CustomsUnitQty = "KG";
			invoiceLine3.JI_LinePrice = 1300m;
			invoiceLine3.JI_Weight = 650;
			invoiceLine3.JI_WeightUQ = "KG";
			invoiceLine3.US_SecondarySPI = "V";
			invoiceLine3.US_UC_NKCountryOfExport = "CH";
			invoiceLine3.JI_CountryOfOrigin = "CH";
			invoiceLine3.US_DestinationState = "IL";

			var invoiceLine4 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine4.US_SupTariff = "98130020";
			invoiceLine4.JI_Tariff = "1902194000";
			invoiceLine4.JI_CustomsQuantity = 3m;
			invoiceLine4.JI_CustomsUnitQty = "KG";
			invoiceLine4.JI_LinePrice = 2400m;
			invoiceLine4.JI_Weight = 1200;
			invoiceLine4.JI_WeightUQ = "KG";
			invoiceLine4.US_SecondarySPI = "V";
			invoiceLine4.US_UC_NKCountryOfExport = "CH";
			invoiceLine4.JI_CountryOfOrigin = "CH";
			invoiceLine4.US_DestinationState = "IL";

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateTIBWatchDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "WATCH";
			invoiceHeader.JZ_InvoiceAmount = 400m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "AU";
			invoiceHeader.JZ_IncoTerm = "FOB";

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "9813000540";
				invoiceLine1.JI_InvoiceQuantity = 1m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 1m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_Tariff = "9101114010";
				invoiceLine1.JI_LinePrice = 100m;
				invoiceLine1.US_SPI = "N/A";

				JobComInvoiceLine childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.JI_Tariff = "9101114020";
				childLine2.JI_InvoiceQuantity = 1m;
				childLine2.JI_InvoiceUQ = "NO";
				childLine2.JI_CustomsQuantity = 1m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 100m;
				childLine2.US_SupTariff = "";
				childLine2.US_SPI = "N/A";

				JobComInvoiceLine childLine3 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine3.JI_Tariff = "9101114030";
				childLine3.JI_InvoiceQuantity = 1m;
				childLine3.JI_InvoiceUQ = "NO";
				childLine3.JI_CustomsQuantity = 1m;
				childLine3.JI_CustomsUnitQty = "NO";
				childLine3.JI_LinePrice = 100m;
				childLine3.US_SupTariff = "";
				childLine3.US_SPI = "N/A";

				JobComInvoiceLine childLine4 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine4.JI_Tariff = "9102111040";
				childLine4.JI_InvoiceQuantity = 1m;
				childLine4.JI_InvoiceUQ = "NO";
				childLine4.JI_CustomsQuantity = 1m;
				childLine4.JI_CustomsUnitQty = "NO";
				childLine4.JI_LinePrice = 100m;
				childLine4.US_SupTariff = "";
				childLine4.US_SPI = "N/A";
			}

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		JobComInvoiceLine AddSecondaryLine(JobComInvoiceLine parentLine, ZString tariff, ZDecimal weight, ZDecimal secondaryQty)
		{
			JobComInvoiceLine result = parentLine.InvoiceHeader.InvoiceLines.AddNew();
			result.JI_ParentID = parentLine.PK;
			result.JI_Tariff = tariff;
			result.JI_Weight = weight;
			result.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			result.JI_CustomsSecondQuantity = secondaryQty;
			return result;
		}

		void AddSecondaryLineWithADD(JobComInvoiceLine parentLine)
		{
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A427818000";
			addCase.U5_ISOCountryCode = "FR";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.IO;
			addCase.U5_CaseStatusDate = ZDateTime.Today;

			var rate = addCase.CaseRates.AddNew();
			rate.U6_AdValoremRate = 0.19m;
			rate.U6_EffectiveDate = ZDateTime.Today;

			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "A570504000";
			uscCase.U5_ISOCountryCode = "CN";
			uscCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			uscCase.U5_CaseStatusDate = ZDateTime.Today;
			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "2844200020";
			var rate2 = uscCase.CaseRates.AddNew();
			rate2.U6_AdValoremRate = 1.08m;
			rate2.U6_EffectiveDate = ZDateTime.Today;

			JobComInvoiceLine childInvoiceLine = parentLine.InvoiceHeader.InvoiceLines.AddNew();
			childInvoiceLine.JI_ParentID = parentLine.PK;
			childInvoiceLine.JI_Tariff = "2844200020";
			childInvoiceLine.JI_Weight = 10m;
			childInvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			childInvoiceLine.JI_LinePrice = 10000m;
			childInvoiceLine.JI_CustomsSecondQuantity = 100m;
			childInvoiceLine.US_ADDCaseNo = "A427818000";
			childInvoiceLine.US_ADDDepositValue = 7500m;
			childInvoiceLine.Declaration.US_ADDCVDSuretyCode = "752";
		}

		void AddSecondaryLineWithCVD(JobComInvoiceLine parentLine)
		{
			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "C427819000";
			uscCase.U5_ISOCountryCode = "FR";
			uscCase.U5_CaseStatus = ACCaseStatusList.Codes.IO;
			uscCase.U5_CaseStatusDate = ZDateTime.Today;
			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "2844200050";
			var rate = uscCase.CaseRates.AddNew();
			rate.U6_AdValoremRate = 0.12m;
			rate.U6_EffectiveDate = ZDateTime.Today;

			JobComInvoiceLine childInvoiceLine = parentLine.InvoiceHeader.InvoiceLines.AddNew();
			childInvoiceLine.JI_ParentID = parentLine.PK;
			childInvoiceLine.JI_Tariff = "2844200050";
			childInvoiceLine.JI_Weight = 10m;
			childInvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			childInvoiceLine.JI_LinePrice = 10000m;
			childInvoiceLine.JI_CustomsSecondQuantity = 100m;
			childInvoiceLine.US_CVDCaseNo = "C427819000";
			childInvoiceLine.Declaration.US_ADDCVDSuretyCode = "530";
		}
	}
}
