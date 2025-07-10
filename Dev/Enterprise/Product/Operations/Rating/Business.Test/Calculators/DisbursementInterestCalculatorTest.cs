using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class DisbursementInterestCalculatorTest : BasePercentageCalculatorTest
	{
		public void TestAddCreditControlsModifiedLog()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();

			var entry = companyTariff.AddRateEntry("AIR", "LSE", "AUSYD", "SGSIN");
			var line = entry.AddRateLine("FRT", DisbursementInterestCalculator.Code);

			line.TL_RX_NKCurrency = "UAH";
			line.TL_Condition = "BRK";

			var calculator = (DisbursementInterestCalculator)line.Calculator;
			AssertNotNull(calculator);

			calculator.Uplift = 20m;
			calculator.IncludeGST = true;
			calculator.AdjustmentDays = 20m;

			var lineItem = line.RateLineItems.AddNew();
			lineItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			lineItem.TM_Text = "ALL";

			lineItem = line.RateLineItems.AddNew();
			lineItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			lineItem.TM_Text = "FRT";

			Factory.Save();

			var logs = companyTariff.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.CreditControlsModified.Code));
			var expected = new[]
			{
				"DIN|APP=ALL;FRT|CON=BRK|CRD=20|CUR=UAH|EF%=20|VAT=Y"
			};
			AssertContainsExactElementsInAnyOrder("Should add one new CDE log", expected, logs.Select(c => c.SL_Reference.ToString()));

			line.TL_Condition = "GTW";

			calculator.Uplift = 50m;
			calculator.IncludeGST = false;
			calculator.OutstandingDays = true;

			Factory.Save();

			logs = companyTariff.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.CreditControlsModified.Code));
			expected = new[]
			{
				"DIN|APP=ALL;FRT|CON=BRK|CRD=20|CUR=UAH|EF%=20|VAT=Y",
				"DIN|CON=GTW|EF%=50|OSD=Y|VAT=N"
			};

			AssertContainsExactElementsInAnyOrder("Should add one new CDE log and only show values which have been changed", expected, logs.Select(c => c.SL_Reference.ToString()));
		}

		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(DisbursementInterestCalculator.Items.Uplift));
			AssertNull(Line.RateLineItems.FindByTM_Type(DisbursementInterestCalculator.Items.AdjustmentDays));
			AssertNull(Line.RateLineItems.FindByTM_Type(DisbursementInterestCalculator.Items.OutstandingDays));
			InitialiseTestCalculator();
			AssertNotNull(Line.RateLineItems.FindByTM_Type(DisbursementInterestCalculator.Items.Uplift));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(DisbursementInterestCalculator.Items.AdjustmentDays));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(DisbursementInterestCalculator.Items.OutstandingDays));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Text.IncludeGST));
			AssertEquals(4, Line.RateLineItems.Count);

			AssertEquals(TestCalculator.CreditTermsInfo, TestCalculator.String1Info);
			AssertEquals(TestCalculator.CurrentPrimeRateInfo, TestCalculator.Decimal1Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(DisbursementInterestCalculator.Items.Uplift).TM_RelevantValueInfo, TestCalculator.Decimal2Info);
			AssertEquals(TestCalculator.EffectiveRateInfo, TestCalculator.Decimal3Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Text.IncludeGST).TM_TextInfo, TestCalculator.Bool2Info);
		}

		public override void TestMapping()
		{
			TestMapping(DisbursementInterestCalculator.Items.Uplift, "Decimal2");
			TestMapping(DisbursementInterestCalculator.Items.AdjustmentDays, "Decimal4");
			TestMapping(DisbursementInterestCalculator.Items.OutstandingDays, "Bool1");
		}

		public void TestString1Mapping()
		{
			AssertEquals("String1", TestCalculator.CreditTerms, TestCalculator.String1);
		}

		public void TestDecimal1Mapping()
		{
			AssertEquals("Decimal1", TestCalculator.CurrentPrimeRate, TestCalculator.Decimal1);
		}

		public void TestCurrentPrimeRateReadOnly()
		{
			AssertEquals(true, TestCalculator.CurrentPrimeRateInfo.ReadOnly);
			AssertEquals(true, TestCalculator.Decimal1Info.ReadOnly);
		}

		public void TestDecimal3Mapping()
		{
			InitialiseTestCalculator();
			TestCalculator.Uplift = 1.5m;
			AssertEquals("Decimal3", TestCalculator.CurrentPrimeRate + 1.5m, TestCalculator.Decimal3);
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddDisbursementInterestRateLine(rateEntry, "FRT", "AUD", uplift: 10m);
			var rateLine11 = AddDisbursementInterestRateLine(rateEntry, "FRT", "AUD", uplift: 11m);

			var rateLine20 = AddDisbursementInterestRateLine(rateEntry, "FRT", "USD", uplift: 20m);

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"- Credit Terms: Cash On Delivery||21.00|% Plus Prime Rate",
					"- Credit Terms: Cash On Delivery||20.00|% Plus Prime Rate"
				}
			);
		}

		static RateLine AddDisbursementInterestRateLine(RateEntry rateEntry, ZString chargeCode, string currency, ZDecimal uplift)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, DisbursementInterestCalculator.Code, currencyCode: currency);
			var calculator = rateLine.GetCalculator<DisbursementInterestCalculator>();
			calculator.Uplift = uplift;

			return rateLine;
		}

		public override void TestQuotationLines()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate - Credit Terms: Cash On Delivery|||At Prime Rate", quotationLines[0].ToString());

			OrganisationsDataRegistry.Instance.UseARSettlementGroupCreditLimit.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var mock = new Mock<IAccounting> { CallBase = true };

			mock.Setup(m => m.CurrentPrimeRate).Returns(5m);
			mock.Setup(m => m.EnableLocalChargeCodeDescriptionDefault).Returns(false);
			mock.Setup(m => m.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors).Returns(false);
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORT).Returns(true);
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy).Returns(false);
			mock.Setup(m => m.APAccountGroup).Returns(Factory.NewWithValidTestData<OrgCreditorGroup>().PK.ToGuid());
			mock.Setup(m => m.ARAccountGroup).Returns(Factory.NewWithValidTestData<OrgDebtorGroup>().PK.ToGuid());

			using (ObjectFactory.Substitute(mock.Object))
			{
				TestCalculator.Uplift = 1.5m;
				quotationLines = TestCalculator.GetQuotationLines(parentEntry);
				AssertEquals(1, quotationLines.Count);
				AssertEquals("Test Rate - Credit Terms: Cash On Delivery||1.50|% + Prime Rate", quotationLines[0].ToString());

				TestCalculator.Uplift = 0m;
				quotationLines = TestCalculator.GetQuotationLines(parentEntry);
				AssertEquals(1, quotationLines.Count);
				AssertEquals("Test Rate - Credit Terms: Cash On Delivery|||At Prime Rate", quotationLines[0].ToString());

				quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
				AssertEquals(1, quotationLines.Count);
				AssertEquals("Test Charge - Credit Terms: Cash On Delivery|||At Prime Rate", quotationLines[0].ToString());

				TestCalculator.Uplift = 1.5m;
				Line.Parent.Parent.TH_OH = NewClient.PK;
				var dsbTerm = NewClient.CompanyData.CreateOrLoadDisbursementARTerm();
				dsbTerm.PY_InvoiceDays = 5;
				dsbTerm.PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromInvoiceDate;
				quotationLines = TestCalculator.GetQuotationLines(parentEntry);
				AssertEquals(1, quotationLines.Count);
				AssertEquals("Test Rate - Credit Terms: 5 Days From Date of Invoice||1.50|% + Prime Rate", quotationLines[0].ToString());

				dsbTerm = NewClient.CompanyData.ARTerms.AddNew();
				dsbTerm.PY_JobType = JobInvoicingConsumerTypes.Shipment.Code;
				dsbTerm.PY_TransportMode = Core.Constants.TransportModes.Sea;
				dsbTerm.PY_Direction = OrgConstants.ServiceDirection.Code.Export;
				dsbTerm.PY_InvoiceClass = "DSB";
				dsbTerm.PY_InvoiceDays = 15;
				dsbTerm.PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromCustomsClearanceDate;

				Line.Parent.TI_OriginLRC = "AUSYD";
				Line.Parent.TI_DestinationLRC = "USCHI";
				Line.Parent.TI_Mode = Core.Constants.RateMode.FCL;
				quotationLines = TestCalculator.GetQuotationLines(parentEntry);
				AssertEquals(1, quotationLines.Count);
				AssertEquals("Test Rate - Credit Terms: 15 Days From Customs Clearance Date||1.50|% + Prime Rate", quotationLines[0].ToString());

				TestCalculator.OutstandingDays = true;
				TestCalculator.AdjustmentDays = 3;
				quotationLines = TestCalculator.GetQuotationLines(parentEntry);
				AssertEquals(1, quotationLines.Count);
				AssertEquals("Test Rate - Credit Terms: 18 Outstanding Days||1.50|% + Prime Rate", quotationLines[0].ToString());
			}
		}

		public void TestQuotationLines_CompanyTariff()
		{
			var companyTariff = Helper.NewCompanyTariff();
			var tariffEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.FCL, "AUSYD", "NZAKL");
			tariffEntry.TI_RH_NKCommodityCode = "GEN";
			var tariffLine = tariffEntry.AddRateLine("FRT", DisbursementInterestCalculator.Code, "KG", "USD");

			var quotationLines = tariffLine.Calculator.GetQuotationLines(tariffEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("International Freight - Credit Terms: Taken From Invoiced Party.|||At Prime Rate", quotationLines[0].ToString());
		}

		public void TestGetQuotationLines_StandardCosting_DINCalculator()
		{
			var standardCosting = Helper.NewCosting(null);
			var rateEntry = standardCosting.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.FCL, "AUSYD", "NZAKL");
			var rateLine = rateEntry.AddRateLine("FRT", DisbursementInterestCalculator.Code, "KG", "USD");

			var quotationLines = rateLine.Calculator.GetQuotationLines(rateEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("International Freight - Credit Terms: Taken From Invoiced Party.|||At Prime Rate", quotationLines[0].ToString());
		}

		[TestDate(2004, 10, 26)]
		public void TestCalculation()
		{
			var creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>().PK.ToGuid();
			var debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>().PK.ToGuid();

			var repository = new MockRepository(MockBehavior.Loose) { DefaultValue = DefaultValue.Mock };
			var accountingMock = repository.Create<IAccounting>();

			accountingMock.Setup(m => m.CurrentPrimeRate).Returns(9.5m);
			accountingMock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORT).Returns(true);
			accountingMock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy).Returns(false);
			accountingMock.Setup(m => m.APAccountGroup).Returns(creditorGroup);
			accountingMock.Setup(m => m.ARAccountGroup).Returns(debtorGroup);

			var mockRegistry = repository.Create<IRegistryItem>();
			mockRegistry.Setup(r => r.Value).Returns(true);
			accountingMock.Setup(m => m.IncludeUnpostedRevenueInCreditLimitCalculation).Returns(mockRegistry.Object);
			accountingMock.Setup(m => m.IncludeUnpostedRevenueInGlobalCreditLimitCalculation).Returns(mockRegistry.Object);

			using (ObjectFactory.Substitute(accountingMock.Object))
			{
				TestCC1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
				TestCC3.AC_ChargeType = Core.Constants.ChargeType.Disbursement;

				Line.TL_RX_NKCurrency = "AUD";
				var applyToItem = TestCalculator.AddApplyToItem(Calculator.Items.Value.DisbursementApplyToTypes.Disbursements);
				TestCalculator.Uplift = 1.5M;

				NewClient.Factory.Save();

				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromMonthEnd, 5, null, 21.8m, "AUD 7234.00 (Disbursements CUSDSB* 1200.00 + TESTCC1 1500.00 + TESTCC3 4534.00) @ 11 % pa - 5 Days From End of Month (10 effective days) For Test Client #1");
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromInvoiceDate, 5, null, 10.9m, "AUD 7234.00 (Disbursements CUSDSB* 1200.00 + TESTCC1 1500.00 + TESTCC3 4534.00) @ 11 % pa - 5 Days From Date of Invoice (5 effective days) For Test Client #1");
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.CashOnDelivery, 5, null, 0m, "AUD 7234.00 (Disbursements CUSDSB* 1200.00 + TESTCC1 1500.00 + TESTCC3 4534.00) @ 11 % pa - 0 Days Cash On Delivery (0 effective days) For Test Client #1");
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromShipmentDate, 5, null, 17.44m, "AUD 7234.00 (Disbursements CUSDSB* 1200.00 + TESTCC1 1500.00 + TESTCC3 4534.00) @ 11 % pa - 5 Days From Date of Shipment (8 effective days) For Test Client #1");

				new AccountingPeriodTestHelper().SetupPeriods();
				var periodCalc = new AccountingPeriodCalculator(Factory);
				var lastDayOfPeriod = periodCalc.GetLastDayForPeriod(ZDateTime.Today);
				var daysLeftInPeriod = lastDayOfPeriod.IsValid ? (lastDayOfPeriod - ZDateTime.Today).Days : 0;

				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromPeriodEnd, 5, null, 56.68m, "AUD 7234.00 (Disbursements CUSDSB* 1200.00 + TESTCC1 1500.00 + TESTCC3 4534.00) @ 11 % pa - 5 Days From End of Period (" + (daysLeftInPeriod + 5) + " effective days) For Test Client #1");

				applyToItem.TM_Text = Calculator.Items.Value.DisbursementApplyToTypes.CustomsDisbursement;
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromInvoiceDate, 5, null, 1.81m, "AUD 1200.00 (Customs Disbursement CUSDSB*) @ 11 % pa - 5 Days From Date of Invoice (5 effective days) For Test Client #1");

				TestCalculator.OutstandingDays = true;
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromInvoiceDate, 5, null, 1.81m, "AUD 1200.00 (Customs Disbursement CUSDSB*) @ 11 % pa - 5 Days (5 effective outstanding days) For Test Client #1");

				TestCalculator.AdjustmentDays = 6;
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromInvoiceDate, 5, null, 3.98m, "AUD 1200.00 (Customs Disbursement CUSDSB*) @ 11 % pa - 5 Days + 6 Adjustment Days (11 effective outstanding days) For Test Client #1");

				applyToItem.TM_Text = CalculatorConstants.Text.ChargeCode;
				applyToItem.TM_AC = TestCC1.PK;
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromInvoiceDate, 5, null, 4.97m, "AUD 1500.00 (TESTCC1) @ 11 % pa - 5 Days + 6 Adjustment Days (11 effective outstanding days) For Test Client #1");

				applyToItem.TM_AC = TestCC2.PK;
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromInvoiceDate, 5, null, 18.71m, "AUD 5643.00 (TESTCC2) @ 11 % pa - 5 Days + 6 Adjustment Days (11 effective outstanding days) For Test Client #1");

				applyToItem.TM_AC = TestCC3.PK;
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromInvoiceDate, 5, null, 15.03m, "AUD 4534.00 (TESTCC3) @ 11 % pa - 5 Days + 6 Adjustment Days (11 effective outstanding days) For Test Client #1");

				applyToItem.TM_AC = TestCC4.PK;
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromInvoiceDate, 5, null, 3.13m, "AUD 944.00 (TESTCC4) @ 11 % pa - 5 Days + 6 Adjustment Days (11 effective outstanding days) For Test Client #1");

				applyToItem.TM_Text = CalculatorConstants.Text.AllCharges;
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromInvoiceDate, 5, null, 45.82m, "AUD 13821.00 (All Charge Codes TESTCC4 944.00 + CUSDSB* 1200.00 + TESTCC1 1500.00 + TESTCC3 4534.00 + TESTCC2 5643.00) @ 11 % pa - 5 Days + 6 Adjustment Days (11 effective outstanding days) For Test Client #1");

				applyToItem.TM_Text = CalculatorConstants.Text.OriginCharges;
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromInvoiceDate, 5, null, 23.68m, "AUD 7143.00 (Origin Charges TESTCC1 1500.00 + TESTCC2 5643.00) @ 11 % pa - 5 Days + 6 Adjustment Days (11 effective outstanding days) For Test Client #1");

				applyToItem.TM_Text = CalculatorConstants.Text.FreightCharges;
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromInvoiceDate, 5, null, 15.03m, "AUD 4534.00 (Freight Charges TESTCC3) @ 11 % pa - 5 Days + 6 Adjustment Days (11 effective outstanding days) For Test Client #1");

				applyToItem.TM_Text = CalculatorConstants.Text.DestinationCharges;
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromInvoiceDate, 5, null, 3.13m, "AUD 944.00 (Destination Charges TESTCC4) @ 11 % pa - 5 Days + 6 Adjustment Days (11 effective outstanding days) For Test Client #1");

				applyToItem.TM_Text = CalculatorConstants.Text.ApplyTo.Value.ValueOfGoods;
				var monetaryValues = new MoneyType();
				monetaryValues.Add(MoneyType.ValueType.GoodsValue, new Money(11000m, Line.Currency));
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromInvoiceDate, 5, monetaryValues, 36.47m, "AUD 11000.00 (Value of Goods) @ 11 % pa - 5 Days + 6 Adjustment Days (11 effective outstanding days) For Test Client #1");

				applyToItem.TM_Text = CalculatorConstants.Text.ApplyTo.Value.InsuranceValue;
				monetaryValues.Add(MoneyType.ValueType.InsuranceValue, new Money(1100m, Line.Currency));
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromInvoiceDate, 5, monetaryValues, 3.65m, "AUD 1100.00 (Insurance Value) @ 11 % pa - 5 Days + 6 Adjustment Days (11 effective outstanding days) For Test Client #1");

				var applyToItem2 = TestCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
				applyToItem2.TM_AC = TestCC1.PK;
				AssertDisbursementInterestCalculations(Core.Constants.InvoiceTerms.FromInvoiceDate, 5, monetaryValues, 8.62m, "AUD 2600.00 (Insurance Value + TESTCC1 Insurance Value 1100.00 + TESTCC1 1500.00) @ 11 % pa - 5 Days + 6 Adjustment Days (11 effective outstanding days) For Test Client #1");
			}
		}

		public void TestCalculationWithExistingCharges()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			Line.TL_RX_NKCurrency = "AUD";

			InitialiseTestCalculator();

			var applyToItem = TestCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			TestCalculator.Uplift = 1.5M;

			var calcParams = new AutoRatingCalculatorParametersForTesting(Criteria);
			calcParams.AddExistingChargeForTest(TestCC1, new Money(500M, Line.Currency));
			calcParams.AddExistingChargeForTest(TestCC2, new Money(1000M, Line.Currency));
			calcParams.AddExistingChargeForTest(TestCC3, new Money(1200M, Line.Currency));

			var dsbTerm = NewClient.CompanyData.CreateOrLoadDisbursementARTerm();
			dsbTerm.PY_InvoiceDays = 5;
			dsbTerm.PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromInvoiceDate;
			calcParams.Criteria.LocalClient = NewClient;

			var parent = Factory.New<DummyJobHeaderParent>();

			var jobHeader = new JobHeader.Loader(parent).TryLoadOrCreate();
			jobHeader.LocalChargesPK = NewClient.PK;

			Criteria.InvoicingSupporter = new JobInvoicingSupporterWithJob(parent);

			Factory.Save();

			applyToItem.TM_Text = CalculatorConstants.Text.OriginCharges;
			AssertCalculation(calcParams, 0.31m, "AUD 1500.00 (Origin Charges TESTCC1* 500.00 + TESTCC2* 1000.00) @ 1.5 % pa - 5 Days From Date of Invoice (5 effective days) For Test Client #1");
			calcParams.RatingContext.PercentageLinesApplied.Clear();

			applyToItem.TM_Text = CalculatorConstants.Text.AllCharges;
			AssertCalculation(calcParams, 0.55m, "AUD 2700.00 (All Charge Codes TESTCC1* 500.00 + TESTCC2* 1000.00 + TESTCC3* 1200.00) @ 1.5 % pa - 5 Days From Date of Invoice (5 effective days) For Test Client #1");
			calcParams.RatingContext.PercentageLinesApplied.Clear();

			applyToItem.TM_Text = CalculatorConstants.Text.ChargeCode;
			applyToItem.TM_AC = TestCC1.PK;
			AssertCalculation(calcParams, 0.10m, "AUD 500.00 (TESTCC1*) @ 1.5 % pa - 5 Days From Date of Invoice (5 effective days) For Test Client #1");
			calcParams.RatingContext.PercentageLinesApplied.Clear();

			Line.TL_RX_NKCurrency = "USD";
			Criteria.CurrencyConverter = new TestCurrencyConverter(Factory);
			AssertCalculation(calcParams, 0.08m, "USD 400.00 (TESTCC1*) @ 1.5 % pa - 5 Days From Date of Invoice (5 effective days) For Test Client #1");
			calcParams.RatingContext.PercentageLinesApplied.Clear();
			Line.TL_RX_NKCurrency = "AUD";

			var autoRateInfosWith1Charge = new AutoRateInfoCollection(Factory);
			autoRateInfosWith1Charge.AddNew(TestCC4, "AUD", 944M);
			applyToItem.TM_Text = CalculatorConstants.Text.AllCharges;
			calcParams = new AutoRatingCalculatorParametersForTesting(Criteria, autoRateInfosWith1Charge);
			AssertCalculation(calcParams, 0.75M, "AUD 3644.00 (All Charge Codes TESTCC1* 500.00 + TESTCC4 944.00 + TESTCC2* 1000.00 + TESTCC3* 1200.00) @ 1.5 % pa - 5 Days From Date of Invoice (5 effective days) For Test Client #1");
			calcParams.RatingContext.PercentageLinesApplied.Clear();
			ClearUsedByRateLines(autoRateInfosWith1Charge);

			autoRateInfosWith1Charge.AddNew(TestCC1, "AUD", 700M);
			applyToItem.TM_Text = CalculatorConstants.Text.AllCharges;
			AssertCalculation(calcParams, 0.89M, "AUD 4344.00 (All Charge Codes TESTCC1* 500.00 + TESTCC1 700.00 + TESTCC4 944.00 + TESTCC2* 1000.00 + TESTCC3* 1200.00) @ 1.5 % pa - 5 Days From Date of Invoice (5 effective days) For Test Client #1");
			calcParams.RatingContext.PercentageLinesApplied.Clear();
			ClearUsedByRateLines(autoRateInfosWith1Charge);

			calcParams = new AutoRatingCalculatorParametersForTesting(Criteria, autoRateInfosWith1Charge);
			Criteria.SetExistingCharges(new List<IAutoRatingChargeInfo>());
			calcParams.AddExistingChargeForTest(TestCC1, new Money(500M, Line.Currency), true);
			calcParams.AddExistingChargeForTest(TestCC2, new Money(1000M, Line.Currency));
			calcParams.AddExistingChargeForTest(TestCC3, new Money(1200M, Line.Currency));
			AssertCalculation(calcParams, 0.75m, "AUD 3644.00 (All Charge Codes TESTCC1* 500.00 + TESTCC4 944.00 + TESTCC2* 1000.00 + TESTCC3* 1200.00) @ 1.5 % pa - 5 Days From Date of Invoice (5 effective days) For Test Client #1");
			calcParams.RatingContext.PercentageLinesApplied.Clear();
			ClearUsedByRateLines(autoRateInfosWith1Charge);

			calcParams = new AutoRatingCalculatorParametersForTesting(Criteria, autoRateInfosWith1Charge);
			Criteria.SetExistingCharges(new List<IAutoRatingChargeInfo>());
			calcParams.AddExistingChargeForTest(TestCC1, new Money(350, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD")), true);
			calcParams.AddExistingChargeForTest(TestCC2, new Money(1000M, Line.Currency));
			calcParams.AddExistingChargeForTest(TestCC3, new Money(1200M, Line.Currency));
			AssertCalculation(calcParams, 0.74m, "AUD 3581.50 (All Charge Codes TESTCC1* 437.50 + TESTCC4 944.00 + TESTCC2* 1000.00 + TESTCC3* 1200.00) @ 1.5 % pa - 5 Days From Date of Invoice (5 effective days) For Test Client #1");
			calcParams.RatingContext.PercentageLinesApplied.Clear();
			ClearUsedByRateLines(autoRateInfosWith1Charge);
		}

		public void TestPaymentTerm()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_APPaymentTerms = Core.Constants.InvoiceTerms.FromMonthEnd;
			org.CompanyData.OB_APPaymentTermDays = 50;

			var arTerm = org.CompanyData.ARTerms.FirstOrDefault();
			arTerm.PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromInvoiceDate;
			arTerm.PY_InvoiceDays = 40;

			var header = Factory.NewWithValidTestData<Costing>();
			header.TH_OH = org.PK;

			var entry = Factory.NewWithValidTestData<RateEntry>();
			entry.TI_TH = header.PK;

			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = DisbursementInterestCalculator.Code;

			var calculator = (DisbursementInterestCalculator)line.Calculator;
			AssertEquals("50 Days From End of Month", calculator.CreditTerms);

			var header2 = Factory.NewWithValidTestData<ClientRate>();
			header2.TH_OH = org.PK;

			var entry2 = Factory.NewWithValidTestData<RateEntry>();
			entry2.TI_TH = header2.PK;

			var line2 = entry2.RateLines.AddNew();
			line2.TL_RateCalculator = DisbursementInterestCalculator.Code;

			var calculator2 = (DisbursementInterestCalculator)line2.Calculator;
			AssertEquals("40 Days From Date of Invoice", calculator2.CreditTerms);
		}

		public void TestCalculateInternal()
		{
			var calcParams = new AutoRatingCalculatorParametersWithoutFilter(Criteria, new FreightAutoRater(new RatingContext()));
			AssertNull(calcParams.Criteria.Job);
			AssertNoExceptionThrown(() => { TestCalculator.Calculate(calcParams); });
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.Uplift = 15m;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;
			source.Percent = 50m;
			source.PerUnitPercent = 50m;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);

			AssertEquals(22.5m, clonedLine.Calculator.Decimal3);
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode("DIN");
		}

		#region Implementation

		void AssertDisbursementInterestCalculations(string creditTermsType, int creditTermDays, MoneyType monetaryValues, ZDecimal expectedAmount, ZString expectedDescription)
		{
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria, AutoRateInfos);
			Criteria.SetExistingCharges(new List<IAutoRatingChargeInfo>());
			Criteria.MonetaryValues = monetaryValues;
			Criteria.CurrencyConverter = new TestCurrencyConverter(Factory);

			var repository = new MockRepository(MockBehavior.Loose) { DefaultValue = DefaultValue.Mock };
			var jobDatesProvider = repository.Create<IJobDatesProvider>(MockBehavior.Loose);

			if (creditTermsType == Core.Constants.InvoiceTerms.FromShipmentDate)
			{
				jobDatesProvider.Setup(m => m.GetJobDateByType(JobDateTypes.Codes.ArrivalDate, "")).Returns(new ZDate(2004, 10, 29));
			}

			Criteria.JobDatesProvider = jobDatesProvider.Object;

			var dsbTerm = NewClient.CompanyData.CreateOrLoadDisbursementARTerm();
			dsbTerm.PY_InvoiceDays = (ZByte)creditTermDays;
			dsbTerm.PY_InvoiceTerm = creditTermsType;
			parameters.Criteria.LocalClient = NewClient;
			NewClient.Factory.Save();

			var jobHeader = new JobHeader.Loader(TestJobHeaderParent).TryLoadOrCreate();
			jobHeader.LocalChargesPK = NewClient.PK;

			Criteria.InvoicingSupporter = new JobInvoicingSupporterWithJob(TestJobHeaderParent);

			Factory.Save();

			parameters.AddExistingChargeForTest(parameters.Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value), new Money(1200M, Line.Currency));
			AssertCalculation(parameters, expectedAmount, expectedDescription);

			repository.VerifyAll();
		}

		protected override Type CalculatorType
		{
			get { return typeof(DisbursementInterestCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return DisbursementInterestCalculator.Code; }
		}

		new DisbursementInterestCalculator TestCalculator
		{
			get { return (DisbursementInterestCalculator)base.TestCalculator; }
		}

		DummyJobHeaderParent TestJobHeaderParent => testJobHeaderParent ?? (testJobHeaderParent = Factory.NewWithValidTestData<DummyJobHeaderParent>());
		DummyJobHeaderParent testJobHeaderParent;

		#endregion
	}
}
