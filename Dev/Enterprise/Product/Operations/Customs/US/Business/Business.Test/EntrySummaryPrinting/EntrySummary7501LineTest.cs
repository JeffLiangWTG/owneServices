using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntrySummary7501Line))]
	sealed class EntrySummary7501LineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHideCountryOfExportLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.JI_Tariff = "2204216000";
			AssertNotNull("PreCondition", invoiceLine.ImportTariff);
			invoiceLine.JI_CustomsQuantity = 150m;
			invoiceLine.US_TransactionsRelated = "N";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 5000m;
			invoiceLine2.JI_Tariff = "6104.22.0010";
			invoiceLine2.JI_CustomsQuantity = 200m;
			invoiceLine2.US_TransactionsRelated = "N";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];
			AssertEquals("HideCountryOfExportLine", true, entryLine.HideCountryOfExportLine);

			invoiceLine.US_TransactionsRelated = "Y";
			AssertEquals("HideCountryOfExportLine will be false when Related Transaction has to print at line level", false, entryLine.HideCountryOfExportLine);
		}

		[TestDate(2010, 09, 28)]
		public void TestCottonFeeRateOnEnsemble()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104.22.0010";
			invoiceLine.JI_CustomsQuantity = 840.00000m;
			invoiceLine.JI_CustomsSecondQuantity = 3285.59m;

			var secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.JI_Tariff = "6102.20.0020";
			secondaryLine.JI_CustomsQuantity = 840.00000m;
			secondaryLine.JI_CustomsSecondQuantity = 3285.59m;
			secondaryLine.JI_LinePrice = 5000m;

			Assert("PreCondition", !invoiceLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));
			Assert("PreCondition", secondaryLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Entry print lines count", 1, ehp.EntryPrintLines.Count);
			var entryLine = ehp.EntryPrintLines[0];
			AssertEquals("Parent Tariff Number", "6104.22.0010", entryLine.FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "6102.20.0020", entryLine.SecondaryLine1FormattedTariff);
			AssertEquals("Line Fee Desc", "056 056 DESC FROM DB", entryLine.LineFeeDescription);
			AssertEquals("Line Fee", 27.42m, entryLine.LineFeeAmount);
			AssertEquals("Line Fee Rate", "0.8345c/KG", entryLine.LineFeePercentAsString);
		}

		public void TestHidePortOfLadingLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.JI_Tariff = "2204216000";
			AssertNotNull("PreCondition", invoiceLine.ImportTariff);
			invoiceLine.JI_CustomsQuantity = 150m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			var entryLine = ehp.EntryPrintLines[0];
			AssertEquals("HidePortOfLadingLine", true, entryLine.HidePortOfLadingLine);

			invoiceLine.US_MiscPermitNo = "JP03947X";
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			entryLine = ehp.EntryPrintLines[0];
			AssertEquals("HidePortOfLadingLine", false, entryLine.HidePortOfLadingLine);
		}

		public void TestEntrySummary7501Line()
		{
			CusEntryLine cusLine = Factory.New<CusEntryLine>();
			EntryHeaderENS7501Line line = new ACSEntryHeaderENS7501Line(cusLine, false, false);

			AssertEquals("Gross Weight", 0m, line.GrossWeightInKilograms);
		}

		[TestDate(2010, 04, 30)]
		public void TestOETPercentAsString()
		{
			CreateOtherExciseEntry();
			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];
			AssertEquals("OETPercentAsString", "15.3389c/L", entryLine.LineFeePercentAsString);
			AssertEquals("OETAmount", 50.38m, entryLine.LineFeeAmount);

			AssertEquals("LineFeeDescription ", "022 022 DESC FROM DB", entryLine.LineFeeDescription);
			AssertEquals("15.3389c/L", entryLine.LineFeePercentAsString);
			AssertEquals("LineFeeAmount ", 50.38m, entryLine.LineFeeAmount);
		}

		[TestDate(2010, 04, 30)]
		public void TestOETPercentAsStringWhenTaxIsDeferred()
		{
			CreateOtherExciseEntry();
			Declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTaxWithEFT;
			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];
			AssertEquals("OETPercentAsString", "DEF 15.3389c/L", entryLine.LineFeePercentAsString);
			AssertEquals("OETAmount", 50.38m, entryLine.LineFeeAmount);

			AssertEquals("LineFeeDescription ", "022 022 DESC FROM DB", entryLine.LineFeeDescription);
			AssertEquals("DEF 15.3389c/L", entryLine.LineFeePercentAsString);
			AssertEquals("LineFeeAmount ", 50.38m, entryLine.LineFeeAmount);
		}

		public void TestQuantityAndUnitQty()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "GB";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2208901000";
			invoiceLine1.ImportTariff.UE_Column1RateSpecific = 1.13m;
			invoiceLine1.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine1.JI_CustomsQuantity = 328.46m;
			invoiceLine1.JI_Weight = 1000m;
			invoiceLine1.US_UC_NKCountryOfExport = "MX";
			invoiceLine1.US_UC_NKCountryOfOrigin = "MX";
			invoiceLine1.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine1.US_TaxApply = TaxApplyList.Codes.Yes;

			Declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTaxWithEFT;
			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];
			AssertEquals("QuantityAndUnitQty", "328.46 PFL", entryLine.QuantityAndUnitQty);
		}

		[TestDate(2010, 04, 30)]
		public void TestDISPercentAsString()
		{
			CreateDistilledSpiritsEntry();
			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];
			AssertEquals("DISPercentAsString", "$3.566322/PFL", entryLine.LineFeePercentAsString);
			AssertEquals("SpiritsAmount", 713.26m, entryLine.LineFeeAmount);

			AssertEquals("LineFeeDescription ", "016 016 DESC FROM DB", entryLine.LineFeeDescription);
			AssertEquals("$3.566322/PFL", entryLine.LineFeePercentAsString);
			AssertEquals("LineFeeAmount ", 713.26m, entryLine.LineFeeAmount);
		}

		[TestDate(2010, 04, 30)]
		public void TestDISPercentAsStringWhenTaxIsDeferred()
		{
			CreateDistilledSpiritsEntry();
			Declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;
			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];
			AssertEquals("DISPercentAsString", "DEF $3.566322/PFL", entryLine.LineFeePercentAsString);
			AssertEquals("SpiritsAmount", 713.26m, entryLine.LineFeeAmount);

			AssertEquals("LineFeeDescription ", "016 016 DESC FROM DB", entryLine.LineFeeDescription);
			AssertEquals("DEF $3.566322/PFL", entryLine.LineFeePercentAsString);
			AssertEquals("LineFeeAmount ", 713.26m, entryLine.LineFeeAmount);
		}

		[TestDate(2010, 04, 30)]
		public void TestTOBPercentAsString()
		{
			CreateTobaccoEntry();
			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];
			AssertEquals("TOBPercentAsString", "$1.828/K", entryLine.LineFeePercentAsString);
			AssertEquals("TobaccoAmount", 1828m, entryLine.LineFeeAmount);

			AssertEquals("LineFeeDescription ", "018 018 DESC FROM DB", entryLine.LineFeeDescription);
			AssertEquals("$1.828/K", entryLine.LineFeePercentAsString);
			AssertEquals("LineFeeAmount ", 1828m, entryLine.LineFeeAmount);
		}

		[TestDate(2010, 04, 30)]
		public void TestTOBPercentAsStringWhenTaxIsDeferred()
		{
			CreateTobaccoEntry();
			Declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;
			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];
			AssertEquals("TOBPercentAsString", "DEF $1.828/K", entryLine.LineFeePercentAsString);
			AssertEquals("TobaccoAmount", 1828m, entryLine.LineFeeAmount);

			AssertEquals("LineFeeDescription ", "018 018 DESC FROM DB", entryLine.LineFeeDescription);
			AssertEquals("DEF $1.828/K", entryLine.LineFeePercentAsString);
			AssertEquals("LineFeeAmount ", 1828m, entryLine.LineFeeAmount);
		}

		[TestDate(2009, 04, 08)]
		public void TestWatchEntryReturnedToUSAfterRepairs()
		{
			CreateWatchWithRepairsDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];
			AssertEquals("Entry should have Ad Valorem Calculation summary", true, entryLine.AdValoremConversionCalculation);
			AssertEquals("1) A: Total Watches", "1000 x $0.44 NO", entryLine.AVWatches);
			AssertEquals("1) A: Total Watches Duty", 440m, entryLine.AVWatchesDuty);
			AssertEquals("1) B: Cases", "$2619 x 6%", entryLine.AVCases);
			AssertEquals("1) B: Cases Duty", 157.14m, entryLine.AVCasesDuty);
			AssertEquals("1) C: Bracelets", "$1345 x 14%", entryLine.AVBracelets);
			AssertEquals("1) C: Bracelets Duty", 188.30m, entryLine.AVBraceletsDuty);
			AssertEquals("1) D: Batteries", "$204 x 5.3%", entryLine.AVBatteries);
			AssertEquals("1) C: Batteries Duty", 10.81m, entryLine.AVBatteriesDuty);
			AssertEquals("Ad Valorem Calculation summary line 2", "$796.25/$9426.00 (Total Entered Value) = 8.447%", entryLine.AVLine2);
		}

		[TestDate(2009, 04, 08)]
		public void TestWatchEntryAssembledWithUSComponents()
		{
			CreateWatchDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];
			AssertEquals("Entry should have pro-rated summary", true, entryLine.ProRatedCalculation);
			AssertEquals("Pro-rated summary line 1", "9802.00.8068 (Free) 3066 / 9426 (Total Value) = 32.527%", entryLine.ProRatedLine1);
			AssertEquals("Pro-rated summary line 2", "32.527% x $796.25 (Total Duty Column 34) = $259.00", entryLine.ProRatedLine2);
			AssertEquals("Pro-rated summary line 3", "$796.25 - $259.00 = $537.25 (Total Duty Due)", entryLine.ProRatedLine3);
			AssertEquals("Box 37 Total Duty should remain as actual Duty", 537.25m, ehp.TotalDutyAmt);

			AssertEquals("Secondary Line 1 Actual Duty", 296.88m, entryLine.secondaryTariffLine1.DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 440m, entryLine.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "44c/NO", entryLine.SecondaryLine1DutyPercentAsString);

			AssertEquals("Secondary Line 3 Actual Duty", 106.03m, entryLine.secondaryTariffLine3.DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 157.14m, entryLine.SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty Rate to print on 7501", "7.09%", entryLine.SecondaryLine3DutyPercentAsString);

			AssertEquals("Secondary Line 5 Actual Duty", 127.05m, entryLine.secondaryTariffLine5.DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 188.3m, entryLine.SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty Rate to print on 7501", "7.09%", entryLine.SecondaryLine5DutyPercentAsString);

			AssertEquals("Secondary Line 7 Actual Duty", 7.29m, entryLine.secondaryTariffLine7.DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 10.81m, entryLine.SecondaryLine7DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty Rate to print on 7501", "7.09%", entryLine.SecondaryLine7DutyPercentAsString);
		}

		public void TestMultiLineWatchEntry()
		{
			CreateMultiLineWatchDeclaration();

			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Entry Lines", 2, ehp.EntryPrintLines.Count);
			AssertEquals("Box 37 Total Duty", 1074.47m, ehp.TotalDutyAmt);

			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];
			AssertEquals("Entry should have pro-rated summary", true, entryLine.ProRatedCalculation);
			AssertEquals("Pro-rated summary line 1", "9802.00.8068 (Free) 3066 / 9426 (Total Value) = 32.527%", entryLine.ProRatedLine1);
			AssertEquals("Pro-rated summary line 2", "32.527% x $796.25 (Total Duty Column 34) = $259.00", entryLine.ProRatedLine2);
			AssertEquals("Pro-rated summary line 3", "$796.25 - $259.00 = $537.25 (Total Duty Due)", entryLine.ProRatedLine3);

			AssertEquals("Secondary Line 1 Actual Duty", 296.88m, entryLine.secondaryTariffLine1.DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 440m, entryLine.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "44c/NO", entryLine.SecondaryLine1DutyPercentAsString);

			AssertEquals("Secondary Line 3 Actual Duty", 106.03m, entryLine.secondaryTariffLine3.DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 157.14m, entryLine.SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty Rate to print on 7501", "7.09%", entryLine.SecondaryLine3DutyPercentAsString);

			AssertEquals("Secondary Line 5 Actual Duty", 127.05m, entryLine.secondaryTariffLine5.DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 188.3m, entryLine.SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty Rate to print on 7501", "7.09%", entryLine.SecondaryLine5DutyPercentAsString);

			AssertEquals("Secondary Line 7 Actual Duty", 7.29m, entryLine.secondaryTariffLine7.DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 10.81m, entryLine.SecondaryLine7DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty Rate to print on 7501", "7.09%", entryLine.SecondaryLine7DutyPercentAsString);

			entryLine = ehp.EntryPrintLines[1];
			AssertEquals("Entry should have Ad Valorem Calculation summary", true, entryLine.AdValoremConversionCalculation);
			AssertEquals("1) A: Total Watches", "1000 x $0.44 NO", entryLine.AVWatches);
			AssertEquals("1) A: Total Watches Duty", 440m, entryLine.AVWatchesDuty);
			AssertEquals("1) B: Cases", "$2619 x 6%", entryLine.AVCases);
			AssertEquals("1) B: Cases Duty", 157.14m, entryLine.AVCasesDuty);
			AssertEquals("1) C: Bracelets", "$1345 x 14%", entryLine.AVBracelets);
			AssertEquals("1) C: Bracelets Duty", 188.30m, entryLine.AVBraceletsDuty);
			AssertEquals("1) D: Batteries", "$204 x 5.3%", entryLine.AVBatteries);
			AssertEquals("1) C: Batteries Duty", 10.81m, entryLine.AVBatteriesDuty);
			AssertEquals("Ad Valorem Calculation summary line 2", "$796.25/$9426.00 (Total Entered Value) = 8.447%", entryLine.AVLine2);
		}

		public void TestPrintWhenTaxRateIsOverriden()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.JI_Tariff = "2204216000";
			AssertNotNull("PreCondition", invoiceLine.ImportTariff);
			invoiceLine.JI_CustomsQuantity = 150m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];

			AssertEquals("41.47469c/L", entryLine.LineFeePercentAsString);
			AssertEquals("WinesAmount", 62.21m, entryLine.LineFeeAmount);

			AssertEquals("LineFeeDescription ", "017 017 DESC FROM DB", entryLine.LineFeeDescription);
			AssertEquals("41.47469c/L", entryLine.LineFeePercentAsString);
			AssertEquals("LineFeeAmount ", 62.21m, entryLine.LineFeeAmount);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxRate = 0.5m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			entryLine = ehp.EntryPrintLines[0];
			AssertEquals("50c/L", entryLine.LineFeePercentAsString);
			AssertEquals("WinesAmount", 75m, entryLine.LineFeeAmount);

			AssertEquals("LineFeeDescription ", "017 017 DESC FROM DB", entryLine.LineFeeDescription);
			AssertEquals("50c/L", entryLine.LineFeePercentAsString);
			AssertEquals("LineFeeAmount ", 75m, entryLine.LineFeeAmount);
		}

		public void TestWineExciseRate()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(+10);
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			tariff.UE_Unit1 = "L";

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeSpecificRate = 0.89817800m;
			dutyRate.UD_TaxFeeAdvalorem = 0.87176100m;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1010101010";
			AssertNotNull("Pre-Condition - tariff must exist", invoiceLine.ImportTariff);

			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.JI_CustomsQuantity = 150m;
			invoiceLine.JI_CustomsUnitQty = "L";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.US_TaxRateT = RateTypeList.Codes.Primary;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];

			AssertEquals("89.8178c/L", entryLine.LineFeePercentAsString);
			AssertEquals("WinesAmount", 134.73m, entryLine.LineFeeAmount);

			AssertEquals("LineFeeDescription ", "017 017 DESC FROM DB (P)", entryLine.LineFeeDescription);
			AssertEquals("89.8178c/L", entryLine.LineFeePercentAsString);
			AssertEquals("LineFeeAmount ", 134.73m, entryLine.LineFeeAmount);
		}

		public void TestPrintSPIOnSecondLine()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1010101010";
			tariff1.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff1.UE_DateTo = ZDateTime.Today.AddDays(+10);
			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			tariff1.UE_Unit1 = "L";

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "2010101010";
			tariff2.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(+10);
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			tariff2.UE_Unit1 = "L";

			var tariffRule1 = Factory.New<USCTariffRule>();
			tariffRule1.U1_RuleCode = "A99";
			tariffRule1.U1_Tariff = "101010";
			tariffRule1.U1_DateFrom = ZDateTime.Today;

			var tariffRule2 = Factory.New<USCTariffRule>();
			tariffRule2.U1_RuleCode = "I99";
			tariffRule2.U1_Tariff = "201010";
			tariffRule2.U1_DateFrom = ZDateTime.Today;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "1010101010";
			invoiceLine1.JI_Tariff = "9802008068";
			invoiceLine1.JI_LinePrice = 5000m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "2010101010";
			invoiceLine2.JI_Tariff = "9802008068";
			invoiceLine2.JI_LinePrice = 6000m;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "9802008068";
			invoiceLine3.JI_LinePrice = 7000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			var entryLine1 = ehp.EntryPrintLines[0];
			Assert(entryLine1.PrintSPIOnSecondLine);

			var entryLine2 = ehp.EntryPrintLines[1];
			Assert(!entryLine2.PrintSPIOnSecondLine);

			var entryLine3 = ehp.EntryPrintLines[2];
			Assert(!entryLine3.PrintSPIOnSecondLine);
		}

		public void TestPrintSPIOnSecondLineWithSupAdditionalTariffLines()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8471704065", "7", 0m, "NO");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030104", "0", 0m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030127", "0", 0m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030130", "0", 0m, "");

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Mexico;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.JI_FormattedTariff = "8471.70.4065";
			invoiceLine.SupTariffFormatted = "9903.01.04";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.01.27";
			invoiceLine.SupFormattedAdditionalTariff2 = "9903.01.30";
			invoiceLine.US_SPI = SpecialProgramList.Codes.S;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>(entry, (a, b, c) => new ACEEntryHeaderENS7501Line(a, b, c));
			AssertEquals(1, ehp.EntryPrintLines.Count);

			var entryLine1 = ehp.EntryPrintLines[0];
			AssertEquals("PrintSPIOnSecondLine", true, entryLine1.PrintSPIOnSecondLine);
			AssertEquals("PrintSPIOnSecondLine1", false, entryLine1.PrintSPIOnSecondLine1);
			AssertEquals("PrintSPIOnSecondLine2", false, entryLine1.PrintSPIOnSecondLine2);
			AssertEquals("PrintSPIOnSecondLine3", true, entryLine1.PrintSPIOnSecondLine3);
			AssertEquals("PrintSPIOnSecondLine4", false, entryLine1.PrintSPIOnSecondLine4);
			AssertEquals("PrintSPIOnSecondLine5", false, entryLine1.PrintSPIOnSecondLine5);
			AssertEquals("PrintSPIOnSecondLine6", false, entryLine1.PrintSPIOnSecondLine6);
			AssertEquals("PrintSPIOnSecondLine7", false, entryLine1.PrintSPIOnSecondLine7);
		}

		public void TestPrintSecondaryLine1DutyPercentAsStringWhenTariffAppliesAAURule()
		{
			#region Setup tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8544300000", "7", 0.05m, "NO");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99039406", "0", 0m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9802008069", "X", 0m, "");

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.VietNam;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.VietNam;
			invoiceLine.JI_FormattedTariff = "8544.30.0000";
			invoiceLine.SupTariffFormatted = "9903.94.06";
			invoiceLine.SupFormattedAdditionalTariff1 = "9802.00.8069";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(3, entry.MergedLines.Count);
			var printLine = new ACEEntryHeaderENS7501Line((CusEntryLine)entry.MergedLines.Find(x => x.CL_AdValoremTariff == "9802008069").FirstOrDefault(), false, false);
			AssertEquals("DutyPercentAsString is FREE for tariff 9802008069", "Free", printLine.DutyPercentAsString);
			AssertEquals("DutyPercentAsString is FREE for tariff 99039406", "Free", printLine.SecondaryLine1DutyPercentAsString);
			AssertEquals("DutyPercentAsString is FREE for tariff 8544300000", "5%", printLine.SecondaryLine2DutyPercentAsString);
		}

		public void TestPrintSecondaryLineDutyPercentAndAmountForTIBEntryWithAdditionalTariffs()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "7304191060", "7", 0m, "KG");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038001", "7", 0.25m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9803000540", "0", 0m, "");

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Japan;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Japan;
			invoiceLine.JI_FormattedTariff = "7304.19.1060";
			invoiceLine.SupTariffFormatted = "9903.80.01";
			invoiceLine.SupFormattedAdditionalTariff1 = "9813.00.0540";
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(3, entry.MergedLines.Count);
			var printLine = new ACEEntryHeaderENS7501Line((CusEntryLine)entry.MergedLines.Find(x => x.CL_AdValoremTariff == "9813000540").FirstOrDefault(), false, false);
			AssertEquals("DutyPercentAsString is FREE for tariff 9813000540", "Free", printLine.DutyPercentAsString);
			AssertEquals("DutyAmount is zero for tariff 9813000540", 0m, printLine.DutyAmount);
			AssertEquals("DutyPercentAsString is 25% for tariff 99038001", "25%", printLine.SecondaryLine1DutyPercentAsString);
			AssertEquals("DutyPercentAsString is 2500 for tariff 99038001", 2500m, printLine.SecondaryLine1DutyAmount);
			AssertEquals("DutyPercentAsString is empty for tariff 8544300000", "", printLine.SecondaryLine2DutyPercentAsString);
			AssertEquals("DutyPercentAsString is zero for tariff 99038001", 0m, printLine.SecondaryLine2DutyAmount);
		}

		public void TestNonWatchComponentEntryReturnedToUSAfterRepairs()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1234";
			invoiceHeader.JZ_InvoiceAmount = 452.98m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "HK";
			invoiceHeader.JZ_IncoTerm = "EXW";

			JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "9802005060";
			invoiceLine1.JI_LinePrice = 350m;
			invoiceLine1.JI_Tariff = "9031905800";
			invoiceLine1.JI_InvoiceQuantity = 3m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_CustomsQuantity = 3m;
			invoiceLine1.JI_LinePrice = 103m;

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];
			AssertEquals("Entry should NOT have Ad Valorem Calculation summary", false, entryLine.AdValoremConversionCalculation);
		}

		public void TestWatchRepairsWithAdditionalTariff()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9802004040", "X", 0, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030125", "7", 0.1m, ZString.Empty);

			var tariff = USCTariffTest.CreateNewTariffIfNotExist(Factory, "9102296010", "1", 0m, "NO");
			tariff.UE_Column1RateSpecific = 1.75m;

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9102296020", "7", 0.048m, "NO");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9102296030", "7", 0.022m, "NO");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99030125 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030125", new ZDateTime(2025, 04, 05), new ZDateTime(2079, 06, 06), "ARTICLES THE PRODUCT OF ANY COUNTRY", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030125);

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.UnitedKingdom;
			parentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
			parentLine.JI_FormattedTariff = "9102.29.6010";
			parentLine.SupTariffFormatted = "9903.01.25";
			parentLine.SupFormattedAdditionalTariff1 = "9802.00.4040";
			parentLine.JI_LinePrice = 5000m;
			parentLine.JI_CustomsQuantity = 100m;
			parentLine.US_98GoodsValue = 3000m;

			var childLine1 = parentLine.ChildLines.ElementAt(0);
			childLine1.JI_FormattedTariff = "9102.29.6020";
			childLine1.JI_LinePrice = 2000m;
			childLine1.SupTariffFormatted = ZString.Empty;
			var childLine2 = parentLine.ChildLines.ElementAt(1);
			childLine2.JI_FormattedTariff = "9102.29.6030";
			childLine2.JI_LinePrice = 1000m;
			childLine2.SupTariffFormatted = ZString.Empty;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.FormalEntry;
			var ehp = new EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>(entry, (a, b, c) => new ACEEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Entry should have Ad Valorem Calculation", true, ehp.EntryHasAdValoremConversionCalculation);

			var entryLine = ehp.EntryPrintLines[0];
			AssertEquals("Entry line should have Ad Valorem Calculation", true, entryLine.AdValoremConversionCalculation);
			AssertEquals("1) A: Total Watches", "100 x $1.75 NO", entryLine.AVWatches);
			AssertEquals("1) A: Total Watches Duty", 175m, entryLine.AVWatchesDuty);
			AssertEquals("1) B: Cases", "$5000 x 4.8%", entryLine.AVCases);
			AssertEquals("1) B: Cases Duty", 240m, entryLine.AVCasesDuty);
			AssertEquals("1) C: Bracelets", "$4000 x 2.2%", entryLine.AVBracelets);
			AssertEquals("1) C: Bracelets Duty", 88m, entryLine.AVBraceletsDuty);
			AssertEquals("1) D: Batteries", ZString.Empty, entryLine.AVBatteries);
			AssertEquals("1) C: Batteries Duty", 0m, entryLine.AVBatteriesDuty);
			AssertEquals("Ad Valorem Calculation summary line 2", "$503.00/$11000.00 (Total Entered Value) = 4.572%", entryLine.AVLine2);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}

		protected override BusinessObject GetNewBusinessObject() => new ACSEntryHeaderENS7501Line(Factory.New<CusEntryLine>(), false, false);

		void CreateWatchWithRepairsDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceNumber = "REPAIRED WATCH";
				invoiceHeader.JZ_InvoiceAmount = 9426m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
				invoiceHeader.JZ_RN_NKDefaultOrigin = "HK";
				invoiceHeader.JZ_IncoTerm = "FOB";

				JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "9802004040";
				invoiceLine1.US_98GoodsValue = 1852m;
				invoiceLine1.JI_Tariff = "9102111010";
				invoiceLine1.JI_InvoiceQuantity = 1000m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 1000m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 3406m;

				JobComInvoiceLine childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.US_SupTariff = "9802004040";
				childLine2.US_98GoodsValue = 1010m;
				childLine2.JI_Tariff = "9102111020";
				childLine2.JI_InvoiceQuantity = 1000m;
				childLine2.JI_InvoiceUQ = "NO";
				childLine2.JI_CustomsQuantity = 1000m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 1609m;

				JobComInvoiceLine childLine4 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine4.US_SupTariff = "9802004040";
				childLine4.JI_Tariff = "9102111030";
				childLine4.JI_InvoiceQuantity = 1000m;
				childLine4.JI_InvoiceUQ = "NO";
				childLine4.JI_CustomsQuantity = 1000m;
				childLine4.JI_CustomsUnitQty = "NO";
				childLine4.JI_LinePrice = 1345m;

				JobComInvoiceLine childLine6 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine6.US_SupTariff = "9802004040";
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
				invoiceLine1.US_98GoodsValue = 1852m;
				invoiceLine1.JI_Tariff = "9102111010";
				invoiceLine1.JI_InvoiceQuantity = 1000m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 1000m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 3406m;

				JobComInvoiceLine childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.US_SupTariff = "9802008068";
				childLine2.US_98GoodsValue = 1010m;
				childLine2.JI_Tariff = "9102111020";
				childLine2.JI_InvoiceQuantity = 1000m;
				childLine2.JI_InvoiceUQ = "NO";
				childLine2.JI_CustomsQuantity = 1000m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 1609m;

				JobComInvoiceLine childLine4 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine4.US_SupTariff = "9802008068";
				childLine4.JI_Tariff = "9102111030";
				childLine4.JI_InvoiceQuantity = 1000m;
				childLine4.JI_InvoiceUQ = "NO";
				childLine4.JI_CustomsQuantity = 1000m;
				childLine4.JI_CustomsUnitQty = "NO";
				childLine4.JI_LinePrice = 1345m;

				JobComInvoiceLine childLine6 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine6.US_SupTariff = "9802008068";
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

		void CreateMultiLineWatchDeclaration()
		{
			CreateWatchDeclaration();
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "WATCH";
			invoiceHeader.JZ_InvoiceAmount = 18852m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "HK";
			invoiceHeader.JZ_IncoTerm = "FOB";

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew(); // Watch Assembled With USComponents
				invoiceLine1.US_SupTariff = "9802008068";
				invoiceLine1.US_98GoodsValue = 1852m;
				invoiceLine1.JI_Tariff = "9102111010";
				invoiceLine1.JI_InvoiceQuantity = 1000m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 1000m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 3406m;

				JobComInvoiceLine childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.US_SupTariff = "9802008068";
				childLine2.US_98GoodsValue = 1010m;
				childLine2.JI_Tariff = "9102111020";
				childLine2.JI_InvoiceQuantity = 1000m;
				childLine2.JI_InvoiceUQ = "NO";
				childLine2.JI_CustomsQuantity = 1000m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 1609m;

				JobComInvoiceLine childLine4 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine4.US_SupTariff = "9802008068";
				childLine4.JI_Tariff = "9102111030";
				childLine4.JI_InvoiceQuantity = 1000m;
				childLine4.JI_InvoiceUQ = "NO";
				childLine4.JI_CustomsQuantity = 1000m;
				childLine4.JI_CustomsUnitQty = "NO";
				childLine4.JI_LinePrice = 1345m;

				JobComInvoiceLine childLine6 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine6.US_SupTariff = "9802008068";
				childLine6.US_98GoodsValue = 204m;
				childLine6.JI_Tariff = "9102111040";
				childLine6.JI_InvoiceQuantity = 1000m;
				childLine6.JI_InvoiceUQ = "NO";
				childLine6.JI_CustomsQuantity = 1000m;
				childLine6.JI_CustomsUnitQty = "NO";

				JobComInvoiceLine invoiceLine2 = Declaration.InvoiceLines.AddNew(); // Watch imported after repairs
				invoiceLine2.US_SupTariff = "9802004040";
				invoiceLine2.US_98GoodsValue = 1852m;
				invoiceLine2.JI_Tariff = "9102111010";
				invoiceLine2.JI_InvoiceQuantity = 1000m;
				invoiceLine2.JI_InvoiceUQ = "NO";
				invoiceLine2.JI_CustomsQuantity = 1000m;
				invoiceLine2.JI_CustomsUnitQty = "NO";
				invoiceLine2.JI_LinePrice = 3406m;

				JobComInvoiceLine line2childLine2 = invoiceLine2.AddSecondaryInvoiceLine();
				line2childLine2.US_SupTariff = "9802004040";
				line2childLine2.US_98GoodsValue = 1010m;
				line2childLine2.JI_Tariff = "9102111020";
				line2childLine2.JI_InvoiceQuantity = 1000m;
				line2childLine2.JI_InvoiceUQ = "NO";
				line2childLine2.JI_CustomsQuantity = 1000m;
				line2childLine2.JI_CustomsUnitQty = "NO";
				line2childLine2.JI_LinePrice = 1609m;

				JobComInvoiceLine line2childLine4 = invoiceLine2.AddSecondaryInvoiceLine();
				line2childLine4.US_SupTariff = "9802004040";
				line2childLine4.JI_Tariff = "9102111030";
				line2childLine4.JI_InvoiceQuantity = 1000m;
				line2childLine4.JI_InvoiceUQ = "NO";
				line2childLine4.JI_CustomsQuantity = 1000m;
				line2childLine4.JI_CustomsUnitQty = "NO";
				line2childLine4.JI_LinePrice = 1345m;

				JobComInvoiceLine line2childLine6 = invoiceLine2.AddSecondaryInvoiceLine();
				line2childLine6.US_SupTariff = "9802004040";
				line2childLine6.US_98GoodsValue = 204m;
				line2childLine6.JI_Tariff = "9102111040";
				line2childLine6.JI_InvoiceQuantity = 1000m;
				line2childLine6.JI_InvoiceUQ = "NO";
				line2childLine6.JI_CustomsQuantity = 1000m;
				line2childLine6.JI_CustomsUnitQty = "NO";
			}

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

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

		void CreateOtherExciseEntry()
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
			invoiceLine1.JI_Tariff = "2203.00.00 30";
			invoiceLine1.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine1.JI_CustomsQuantity = 328.46m;
			invoiceLine1.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.Liters;
			invoiceLine1.JI_Weight = 1000m;
			invoiceLine1.US_UC_NKCountryOfExport = "MX";
			invoiceLine1.US_UC_NKCountryOfOrigin = "MX";
			invoiceLine1.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
		}

		void CreateTobaccoEntry()
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
			invoiceLine1.JI_Tariff = "2402103030";
			invoiceLine1.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine1.US_SPI = "";
			invoiceLine1.JI_CustomsQuantity = 1000m;
			invoiceLine1.JI_CustomsSecondQuantity = 1360m;
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
				}
				return declaration;
			}
		}
	}
}
