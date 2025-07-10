using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class LineDutyApportionManagerTest : TestCaseWithFactory
	{
		public void TestShouldNotCalculateDutyForChapter98ChildLine()
		{
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038501Tariff.UE_Tariff;//99038501
			testHelper.ChildLine.US_SupTariff = testHelper.Test9802005060Tariff.UE_Tariff;//9802005060
			testHelper.ParentLine.JI_LinePrice = 1000m;
			testHelper.ChildLine.US_98GoodsValue = 1000m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(700m, testHelper.ParentLine.US_Duty);
			AssertEquals(100m, testHelper.ParentLine.US_SupDuty);
			AssertEquals("For 98 child line has not duty", 0m, testHelper.ChildLine.US_Duty);
			AssertEquals("For 98 child line has not Prov/Prog duty", 0m, testHelper.ChildLine.US_SupDuty);
		}

		public void TestApportionWithThreeEqualValuedInvoiceLines()
		{
			var declaration = GetMergibleDeclaration();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1m;
			invoiceLine1.JI_Tariff = "8466939585";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 1m;
			invoiceLine2.JI_Tariff = "8466939585";

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 1m;
			invoiceLine3.JI_Tariff = "8466939585";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(1, entry.MergedLines.Count);
			AssertEquals(entry.MergedLines[0].DutyAmount, invoiceLine1.US_Duty.Round(2) + invoiceLine2.US_Duty.Round(2) + invoiceLine3.US_Duty.Round(2));

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(entry.MergedLines[0].DutyAmount, invoiceLine1.US_Duty.Round(2) + invoiceLine2.US_Duty.Round(2) + invoiceLine3.US_Duty.Round(2));
		}

		public void TestPrimaryAndSecondaryLinesEnsemble()
		{
			var declaration = GetMergibleDeclaration();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 0m;
			invoiceLine1.JI_Tariff = "6204.23.0020";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 15m;
			invoiceLine2.JI_Tariff = "6206.40.2510";
			invoiceLine2.JI_CustomsQuantity = 1;
			invoiceLine2.JI_CustomsSecondQuantity = 1;

			AssertNotNull(invoiceLine1.ImportTariff);
			AssertNotNull(invoiceLine2.ImportTariff);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(entry.TotalDutyAmount, invoiceLine1.US_Duty + invoiceLine2.US_Duty);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(entry.TotalDutyAmount, invoiceLine1.US_Duty + invoiceLine2.US_Duty);
		}

		public void TestWhenAllTheDutiesAreOverridden()
		{
			var declaration = GetMergibleDeclaration();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1901.90.5400";
			invoiceLine.US_SupTariff = "9904.17.45";
			invoiceLine.JI_LinePrice = 10000m;

			invoiceLine.US_OverrideDuty = true;
			invoiceLine.US_OverrideSupDuty = true;
			invoiceLine.US_Duty = 500m;
			invoiceLine.US_SupDuty = 1000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Duty overridden by users", entry.TotalDutyAmount, 1500m);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Duty overridden by users", entry.TotalDutyAmount, 1500m);
		}

		public void TestApportionWhenFuzzyRoundingIsApplied()
		{
			var declaration = GetMergibleDeclaration();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 2340m;
			invoiceLine1.JI_Tariff = "8466939585";
			AssertNotNull(invoiceLine1.ImportTariff);

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 10404m;
			invoiceLine2.JI_Tariff = "8456301020";
			invoiceLine2.JI_CustomsQuantity = 1m;
			AssertNotNull(invoiceLine2.ImportTariff);

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 2100m;
			invoiceLine3.JI_Tariff = "8466939585";
			AssertNotNull(invoiceLine3.ImportTariff);

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 2566.20m;
			invoiceLine4.JI_Tariff = "8466939585";
			AssertNotNull(invoiceLine4.ImportTariff);

			var invoiceLine5 = declaration.InvoiceLines.AddNew();
			invoiceLine5.JI_LinePrice = 443.20m;
			invoiceLine5.JI_Tariff = "8466939585";
			AssertNotNull(invoiceLine5.ImportTariff);

			var invoiceLine6 = declaration.InvoiceLines.AddNew();
			invoiceLine6.JI_LinePrice = 989.10m;
			invoiceLine6.JI_Tariff = "8466939585";
			AssertNotNull(invoiceLine6.ImportTariff);

			var invoiceLine7 = declaration.InvoiceLines.AddNew();
			invoiceLine7.JI_LinePrice = 260m;
			invoiceLine7.JI_Tariff = "6302322020";
			invoiceLine7.JI_CustomsQuantity = 2000m;
			AssertNotNull(invoiceLine7.ImportTariff);

			var invoiceLine8 = declaration.InvoiceLines.AddNew();
			invoiceLine8.JI_LinePrice = 1440m;
			invoiceLine8.JI_Tariff = "6302322040";
			invoiceLine8.JI_CustomsQuantity = 4000m;
			AssertNotNull(invoiceLine8.ImportTariff);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Duty should be the same", invoiceLine1.CusEntryLine.DutyAmount, invoiceLine1.US_Duty.Round(2));
			AssertEquals("Duty should be the same", invoiceLine2.CusEntryLine.DutyAmount, invoiceLine2.US_Duty.Round(2));
			AssertEquals("Duty should be the same", invoiceLine3.CusEntryLine.DutyAmount, invoiceLine3.US_Duty.Round(2));
			AssertEquals("Duty should be the same", invoiceLine4.CusEntryLine.DutyAmount, invoiceLine4.US_Duty.Round(2));
			AssertEquals("Duty should be the same", invoiceLine5.CusEntryLine.DutyAmount, invoiceLine5.US_Duty.Round(2));
			AssertEquals("Duty should be the same", invoiceLine6.CusEntryLine.DutyAmount, invoiceLine6.US_Duty.Round(2));
			AssertEquals("Duty should be the same", invoiceLine7.CusEntryLine.DutyAmount, invoiceLine7.US_Duty.Round(2));

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Duty should be the same", invoiceLine1.CusEntryLine.DutyAmount, invoiceLine1.US_Duty.Round(2));
			AssertEquals("Duty should be the same", invoiceLine2.CusEntryLine.DutyAmount, invoiceLine2.US_Duty.Round(2));
			AssertEquals("Duty should be the same", invoiceLine3.CusEntryLine.DutyAmount, invoiceLine3.US_Duty.Round(2));
			AssertEquals("Duty should be the same", invoiceLine4.CusEntryLine.DutyAmount, invoiceLine4.US_Duty.Round(2));
			AssertEquals("Duty should be the same", invoiceLine5.CusEntryLine.DutyAmount, invoiceLine5.US_Duty.Round(2));
			AssertEquals("Duty should be the same", invoiceLine6.CusEntryLine.DutyAmount, invoiceLine6.US_Duty.Round(2));
			AssertEquals("Duty should be the same", invoiceLine7.CusEntryLine.DutyAmount, invoiceLine7.US_Duty.Round(2));
		}

		public void TestApportionSupDuty()
		{
			var declaration = GetMergibleDeclaration();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1901.90.5400";
			invoiceLine.US_SupTariff = "9904.17.45";
			invoiceLine.JI_LinePrice = 10000m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1901.90.5400";
			invoiceLine2.US_SupTariff = "9904.17.45";
			invoiceLine2.JI_LinePrice = 10000m;

			AssertNotNull(invoiceLine2.ImportTariff);
			AssertNotNull(invoiceLine2.ImportSupTariff);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var total = invoiceLine.US_Duty + invoiceLine.US_SupDuty + invoiceLine2.US_SupDuty + invoiceLine2.US_Duty;
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount, total);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount, total);
		}

		public void TestApportionAntiDumpingDuty_Manual()
		{
			var uscCase = Factory.New<USCACCase>();
			uscCase.U5_ISOCountryCode = "CN";
			uscCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			uscCase.U5_CaseNumber = "A570904095";

			USCACCaseBondCash bondCash = uscCase.BondCashIndicators.AddNew();
			bondCash.U8_Indicator = BondCashIndicatorList.Codes.Cash;
			bondCash.U8_InactivatedDate = ZDateTime.Empty;
			bondCash.U8_EffectiveDate = ZDateTime.Today;

			var declaration = GetMergibleDeclaration();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99041745";
			invoiceLine.JI_Tariff = "3802100000";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.US_ADDCaseNo = "A570904095";

			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_ADDuty = 250m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "99041745";
			invoiceLine2.JI_Tariff = "3802100000";
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.JI_CustomsQuantity = 1000m;
			invoiceLine2.US_ADDCaseNo = "A570904095";
			invoiceLine2.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine2.US_ADDuty = 350m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(250m, invoiceLine.US_ADDuty);
			AssertEquals(350m, invoiceLine2.US_ADDuty);

			var total = invoiceLine.US_ADDuty + invoiceLine2.US_ADDuty;
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAntidumpingDuty, total);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			total = invoiceLine.US_ADDuty + invoiceLine2.US_ADDuty;
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAntidumpingDuty, total);
		}

		public void TestApportionAntiDumpingDuty_Calculated()
		{
			USCACCase uscCase = Factory.NewWithValidTestData<USCACCase>();
			uscCase.U5_CaseNumber = "A549822023";

			var declaration = GetMergibleDeclaration();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99041745";
			invoiceLine.JI_Tariff = "0306.13.0012";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "TH";
			invoiceLine.US_ADDCaseNo = "A549822023";
			AssertNotNull(invoiceLine.AntidumpingDutyCase);
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAntidumpingDuty, invoiceLine.US_ADDuty);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAntidumpingDuty, invoiceLine.US_ADDuty);
		}

		public void TestApportionADDDuty_CVDuty()
		{
			var acCase1 = Factory.New<USCACCase>();
			acCase1.U5_CaseNumber = "A10";
			var caseRate1 = acCase1.CaseRates.AddNew();
			caseRate1.U6_EffectiveDate = new ZDateTime(2010, 1, 1);
			caseRate1.U6_AdValoremRate = 0.8555m;

			var acCase2 = Factory.New<USCACCase>();
			acCase2.U5_CaseNumber = "C10";
			var caseRate2 = acCase2.CaseRates.AddNew();
			caseRate2.U6_EffectiveDate = new ZDateTime(2010, 1, 1);
			caseRate2.U6_AdValoremRate = 0.3901m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_CVDCaseNo = "C10";
			invoiceLine1.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine1.US_ADDCaseNo = "A10";
			invoiceLine1.US_SupTariff = "99038001";
			invoiceLine1.JI_Tariff = "7306305020";
			invoiceLine1.JI_LinePrice = 25000m;
			invoiceLine1.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine1.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "99038015";
			invoiceLine1.JI_ParentLine = invoiceLine2.JI_LineNo;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(21387.50m, invoiceLine1.US_ADDuty);
			AssertEquals(9752.50m, invoiceLine1.US_CVDuty);

			AssertEquals(0m, invoiceLine2.US_ADDuty);
			AssertEquals(0m, invoiceLine2.US_CVDuty);
		}

		public void TestApportionAntiDumpingDuty_Calculated2()
		{
			USCACCase uscCase = Factory.NewWithValidTestData<USCACCase>();
			uscCase.U5_CaseNumber = "A549822023";

			var declaration = GetMergibleDeclaration();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0306.13.0012";
			invoiceLine.JI_LinePrice = 10m;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "TH";
			invoiceLine.US_ADDCaseNo = "A549822023";
			AssertNotNull(invoiceLine.AntidumpingDutyCase);
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0306.13.0012";
			invoiceLine2.JI_LinePrice = 10m;
			invoiceLine2.JI_CustomsQuantity = 1000m;
			invoiceLine2.US_UC_NKCountryOfOrigin = "TH";
			invoiceLine2.US_ADDCaseNo = "A549822023";
			AssertNotNull(invoiceLine2.AntidumpingDutyCase);
			invoiceLine2.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAntidumpingDuty, invoiceLine.US_ADDuty + invoiceLine2.US_ADDuty);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAntidumpingDuty, invoiceLine.US_ADDuty + invoiceLine2.US_ADDuty);
		}

		public void TestApportionCountervailingDuty()
		{
			USCACCase uscCase = Factory.NewWithValidTestData<USCACCase>();
			uscCase.U5_CaseNumber = "C580208000";

			var declaration = GetMergibleDeclaration();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7210.70.6030";
			invoiceLine.JI_LinePrice = 10m;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_CVDCaseNo = "C580208000";
			AssertNotNull(invoiceLine.CountervailingDutyCase);
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "7210.70.6030";
			invoiceLine2.JI_LinePrice = 10m;
			invoiceLine2.JI_CustomsQuantity = 1000m;
			invoiceLine2.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine2.US_CVDCaseNo = "C580208000";
			AssertNotNull(invoiceLine2.CountervailingDutyCase);
			invoiceLine2.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalCountervailingDuty, invoiceLine.US_CVDuty + invoiceLine2.US_CVDuty);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalCountervailingDuty, invoiceLine.US_CVDuty + invoiceLine2.US_CVDuty);
		}

		public void TestRemoveAntiDumpingCountervailingLineLevelDuty()
		{
			USCACCase uscCase = Factory.NewWithValidTestData<USCACCase>();
			uscCase.U5_CaseNumber = "A549822023";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0306.13.0012";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "TH";
			invoiceLine.US_ADDCaseNo = "A549822023";
			AssertNotNull(invoiceLine.AntidumpingDutyCase);
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("invoice line level AD duty", declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAntidumpingDuty, invoiceLine.US_ADDuty);

			invoiceLine.US_ADDCaseNo = "";
			invoiceLine.US_ADDDepositRateIndicator = "";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("invoice line level AD duty", 0m, invoiceLine.US_ADDuty);

			invoiceLine.US_ADDCaseNo = "A549822023";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_ADDuty = 20m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("invoice line level AD duty", 20m, invoiceLine.US_ADDuty);
		}

		public void TestRemoveDuty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Tariff = "8466939585";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("invoice line level duty", declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount, invoiceLine.US_Duty);

			invoiceLine.JI_Tariff = "";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("invoice line level duty", 0m, invoiceLine.US_Duty);

			invoiceLine.US_OverrideDuty = true;
			invoiceLine.US_Duty = 20m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("invoice line level duty", 20m, invoiceLine.US_Duty);
		}

		public void TestCalculateForFTZ()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Tariff = "8466939585";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("HMF calculated", 12.5m, invoiceLine.HMFAmount);
			AssertEquals("HMF calculated", 12.5m, invoiceLine.CusEntryLine.HMFAmount);
		}

		[TestDate(2020, 10, 9)]
		public void TestApportionmentOfFeesShouldNotProduceNegativeFee_CS00887572()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff.UE_Column1RateAdValorem = 0.32m;
			tariff.UE_Column2RateAdValorem = 0.72m;
			tariff.UE_Unit1 = "DOZ";
			tariff.UE_Unit2 = "KG";

			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Cotton;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateSecondQuantity;
			dutyRate.UD_TaxFeeSpecificRate = 0.00356394m;
			dutyRate.UD_DutyElement = "5";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 600m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.US_SupTariff = "9819.11.12";
			invoiceLine.JI_CustomsQuantity = 2m;
			invoiceLine.JI_CustomsUnitQty = "DOZ";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsSecondQuantity = 4.2m;
			invoiceLine.JI_CustomsSecondUnitQty = "KG";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "00000000";
			invoiceLine2.US_SupTariff = "9819.11.12";
			invoiceLine2.JI_CustomsQuantity = 2m;
			invoiceLine2.JI_CustomsUnitQty = "DOZ";
			invoiceLine2.JI_LinePrice = 100m;
			invoiceLine2.JI_CustomsSecondQuantity = 4.2m;
			invoiceLine2.JI_CustomsSecondUnitQty = "KG";

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "00000000";
			invoiceLine3.US_SupTariff = "9819.11.12";
			invoiceLine3.JI_CustomsQuantity = 2m;
			invoiceLine3.JI_CustomsUnitQty = "DOZ";
			invoiceLine3.JI_LinePrice = 100m;
			invoiceLine3.JI_CustomsSecondQuantity = 4.2m;
			invoiceLine3.JI_CustomsSecondUnitQty = "KG";

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "00000000";
			invoiceLine4.US_SupTariff = "9819.11.12";
			invoiceLine4.JI_CustomsQuantity = 2m;
			invoiceLine4.JI_CustomsUnitQty = "DOZ";
			invoiceLine4.JI_LinePrice = 100m;
			invoiceLine4.JI_CustomsSecondQuantity = 4.2m;
			invoiceLine4.JI_CustomsSecondUnitQty = "KG";

			var invoiceLine5 = declaration.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "00000000";
			invoiceLine5.US_SupTariff = "9819.11.12";
			invoiceLine5.JI_CustomsQuantity = 2m;
			invoiceLine5.JI_CustomsUnitQty = "DOZ";
			invoiceLine5.JI_LinePrice = 100m;
			invoiceLine5.JI_CustomsSecondQuantity = 4.2m;
			invoiceLine5.JI_CustomsSecondUnitQty = "KG";

			var invoiceLine6 = declaration.InvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = "00000000";
			invoiceLine6.US_SupTariff = "9819.11.12";
			invoiceLine6.JI_CustomsQuantity = 2m;
			invoiceLine6.JI_CustomsUnitQty = "DOZ";
			invoiceLine6.JI_LinePrice = 100m;
			invoiceLine6.JI_CustomsSecondQuantity = 4.2m;
			invoiceLine6.JI_CustomsSecondUnitQty = "KG";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var cottonFeeEntryLine = invoiceLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton);
			AssertEquals("EntryLine level CottonFee", 0.09m, cottonFeeEntryLine);

			AssertEquals("There should be no negative fee", true, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton) >= 0m);
			AssertEquals("There should be no negative fee", true, invoiceLine2.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton) >= 0m);
			AssertEquals("There should be no negative fee", true, invoiceLine3.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton) >= 0m);
			AssertEquals("There should be no negative fee", true, invoiceLine4.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton) >= 0m);
			AssertEquals("There should be no negative fee", true, invoiceLine5.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton) >= 0m);
			AssertEquals("There should be no negative fee", true, invoiceLine6.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton) >= 0m);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}

		JobDeclaration GetMergibleDeclaration()
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			result.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			result.US_EntryFilerCode = "XJ5";
			result.US_EnableENS = true;
			return result;
		}
	}
}
