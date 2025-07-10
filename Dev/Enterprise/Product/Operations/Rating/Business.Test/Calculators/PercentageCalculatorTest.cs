using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class PercentageCalculatorTest : BasePercentageCalculatorTest
	{
		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER));
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN));
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS));
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MAX));
			AssertNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Text.IncludeGST));
			AssertNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Text.GreaterCharge));
			AssertNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Text.PER_PartThereof));
			AssertNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.Rate));
			AssertNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.ValueOrPartThereOf));

			InitialiseTestCalculator();

			AssertNotNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MAX));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Text.IncludeGST));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Text.GreaterCharge));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Text.PER_PartThereof));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.Rate));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.ValueOrPartThereOf));

			AssertEquals(9, Line.RateLineItems.Count);

			AssertEquals(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_RelevantValueInfo, TestCalculator.Decimal1Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN).TM_RelevantValueInfo, TestCalculator.Decimal2Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_RelevantValueInfo, TestCalculator.Decimal3Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MAX).TM_RelevantValueInfo, TestCalculator.Decimal4Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Text.IncludeGST).TM_TextInfo, TestCalculator.Bool1Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Text.GreaterCharge).TM_TextInfo, TestCalculator.Bool2Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Text.PER_PartThereof).TM_TextInfo, TestCalculator.Bool3Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.Rate).TM_RelevantValueInfo, TestCalculator.Decimal5Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.ValueOrPartThereOf).TM_RelevantValueInfo, TestCalculator.Decimal6Info);
		}

		public override void TestMapping()
		{
			TestMapping(CalculatorConstants.Type.PER, "Decimal1");
			TestMapping(Calculator.Items.Operator.MIN, "Decimal2");
			TestMapping(Calculator.Items.Operator.BAS, "Decimal3");
			TestMapping(Calculator.Items.Operator.MAX, "Decimal4");
			TestMapping(CalculatorConstants.Text.IncludeGST, "Bool1");
			TestMapping(CalculatorConstants.Text.GreaterCharge, "Bool2");
			TestMapping(CalculatorConstants.Text.PER_PartThereof, "Bool3");
			TestMapping(CalculatorConstants.Type.Rate, "Decimal5");
			TestMapping(CalculatorConstants.Type.ValueOrPartThereOf, "Decimal6");
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddPercentageRateLine(rateEntry, "FRT", "AUD", min: 10, flat: 12, max: 100, percent: 14, Helper.ChargeCodes["FRT"], Helper.ChargeCodes["BAF"]);
			var rateLine11 = AddPercentageRateLine(rateEntry, "FRT", "AUD", min: 20, flat: 22, max: 200, percent: 24, Helper.ChargeCodes["FRT"], Helper.ChargeCodes["CAF"]);

			var rateLine20 = AddPercentageRateLine(rateEntry, "FRT", "USD", min: 30, flat: 32, max: 300, percent: 34, Helper.ChargeCodes["FRT"], Helper.ChargeCodes["BAF"]);

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"||38.00|% of Freight",
					"||14.00|% of Bunker Adjustment Factor",
					"|AUD|34.00|",
					"Minimum|AUD|10.00|",
					"Maximum|AUD|200.00|",
					"||24.00|% of Currency Adjustment Factor",
					"||34.00|% of Freight",
					"||34.00|% of Bunker Adjustment Factor",
					"|USD|32.00|",
					"Minimum|USD|30.00|",
					"Maximum|USD|300.00|"
				},
				message: "Calculation component with same currency, charge should be grouped/rolledUp"
			);
		}

		static RateLine AddPercentageRateLine(RateEntry rateEntry, ZString chargeCode, string currency, decimal min, decimal flat, decimal max, decimal percent, params AccChargeCode[] charges)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, PercentageCalculator.Code, currencyCode: currency);
			var percentageCalculator = rateLine.GetCalculator<PercentageCalculator>();
			percentageCalculator.Minimum = min;
			percentageCalculator.BaseRate = flat;
			percentageCalculator.Maximum = max;
			percentageCalculator.Percent = percent;

			foreach (var charge in charges)
			{
				var applytoItem101 = percentageCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
				applytoItem101.TM_AC = charge.PK;
			}

			return rateLine;
		}

		public override void TestQuotationLines()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			Line.TL_WeightVolume = "KG";
			Line.TL_RateCalculator = PercentageCalculator.Code;
			Line.TL_RX_NKCurrency = "USD";

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.Minimum = 120m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|120.00|", quotationLines[0].ToString());

			TestCalculator.BaseRate = 140m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|140.00|", quotationLines[0].ToString());

			TestCalculator.Maximum = 700m;
			TestCalculator.Percent = 0.3825m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|140.00|", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Charge|USD|140.00|", quotationLines[0].ToString());

			TestCalculator.Minimum = 0m;
			TestCalculator.BaseRate = 0m;
			TestCalculator.Maximum = 0m;
			var percent1 = Line.RateLineItems.AddNew();
			percent1.TM_Type = CalculatorConstants.Type.ApplyTo;
			percent1.TM_Text = CalculatorConstants.Text.ChargeCode;
			percent1.TM_AC = Env.Registry.FreightChargeCode;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||0.3825|% of Freight", quotationLines[0].ToString());

			TestCalculator.Percent = 3m;
			var percent2 = Line.RateLineItems.AddNew();
			percent2.TM_Type = CalculatorConstants.Type.ApplyTo;
			percent2.TM_Text = Calculator.Items.Value.DisbursementApplyToTypes.Disbursements;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(2, quotationLines.Count);
			AssertEquals("Test Rate||3.00|% of Freight", quotationLines[0].ToString());
			AssertEquals("||3.00|% of disbursements", quotationLines[1].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(2, quotationLines.Count);
			AssertEquals("Test Charge||3.00|% of Freight", quotationLines[0].ToString());
			AssertEquals("||3.00|% of disbursements", quotationLines[1].ToString());

			TestCalculator.Minimum = 50m;
			TestCalculator.BaseRate = 20m;
			TestCalculator.Maximum = 500m;
			TestCalculator.GreaterCharge = true;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate (Greater Charge)||3.00|% of Freight", quotationLines[0].ToString());
			AssertEquals("or||3.00|% of disbursements", quotationLines[1].ToString());
			AssertEquals("Base Rate|USD|20.00|", quotationLines[2].ToString());
			AssertEquals("Minimum|USD|50.00|", quotationLines[3].ToString());
			AssertEquals("Maximum|USD|500.00|", quotationLines[4].ToString());

			TestCalculator.IsPartThereof = true;
			TestCalculator.Rate = 120m;
			TestCalculator.ValueOrPartThereOf = 10000m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate (Greater Charge)|USD|120.00|per $10000.00 Freight", quotationLines[0].ToString());
			AssertEquals("or|USD|120.00|per $10000.00 disbursements", quotationLines[1].ToString());
			AssertEquals("Base Rate|USD|20.00|", quotationLines[2].ToString());
			AssertEquals("Minimum|USD|50.00|", quotationLines[3].ToString());
			AssertEquals("Maximum|USD|500.00|", quotationLines[4].ToString());
		}

		#region GetQuotationLines LOD/OBR/BRK/UNL

		public void TestGetQuotationLines_LOD() => TestGetQuotationLines
		(
			percent: 0.1m,
			applyTo: CalculatorConstants.Text.LoadingCharges,
			expectedDescription: new string[] { "Test Rate||0.10|% of loading charges" }
		);

		public void TestGetQuotationLines_OBR() => TestGetQuotationLines
		(
			percent: 0.1m,
			applyTo: CalculatorConstants.Text.OriginCustomsBrokerageCharges,
			expectedDescription: new string[] { "Test Rate||0.10|% of origin customs brokerage charges" }
		);

		public void TestGetQuotationLines_BRK() => TestGetQuotationLines
		(
			percent: 0.1m,
			applyTo: CalculatorConstants.Text.CustomsBrokerageCharges,
			expectedDescription: new string[] { "Test Rate||0.10|% of customs brokerage charges" }
		);

		public void TestGetQuotationLines_UNL() => TestGetQuotationLines
		(
			percent: 0.1m,
			applyTo: CalculatorConstants.Text.UnloadingCharges,
			expectedDescription: new string[] { "Test Rate||0.10|% of unloading charges" }
		);

		void TestGetQuotationLines(decimal percent, string applyTo, string[] expectedDescription)
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var quoteRateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			Line.TL_WeightVolume = "KG";
			Line.TL_RateCalculator = PercentageCalculator.Code;
			Line.TL_RX_NKCurrency = "USD";

			TestCalculator.Percent = percent;
			var rateLineItem = Line.RateLineItems.AddNew();
			rateLineItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			rateLineItem.TM_Text = applyTo;
			rateLineItem.TM_AC = Env.Registry.FreightChargeCode;
			var quotationLines = TestCalculator.GetQuotationLines(quoteRateEntry);
			AssertContainsExactElementsInAnyOrder(expectedDescription, quotationLines.Select(quotationLine => quotationLine.ToString()));
		}

		#endregion

		#region Calculation ApplyTo Description, Local Description and Multilingual

		public void TestCalculationApplyTo_LocalDescription_LocalClient()
		{
			InitialiseTestCalculator();

			Entry.ParentRatingHeader.Header.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Local Client", Entry.ParentRatingHeader.Header.OH_RL_NKClosestPort, GlbBranch.CurrentBranch.GB_RL_NKHomePort);
				AssertEquals("TestCC1 description", "TESTCC1", TestCC1.AC_Desc);
			});

			TestCC1.AC_LocalLanguageDescription = "Local Description 地方";
			AssertCalculationApplyTo_LocalDescription(TestCC1, expectedDescription: "Local Description 地方");
		}

		public void TestCalculationApplyTo_LocalDescription_NonLocalClient()
		{
			InitialiseTestCalculator();

			CombineAssertions("Precondition", () =>
			{
				AssertNotEquals("NonLocal Client", Entry.ParentRatingHeader.Header.OH_RL_NKClosestPort, GlbBranch.CurrentBranch.GB_RL_NKHomePort);
				AssertEquals("TestCC1 description", "TESTCC1", TestCC1.AC_Desc);
			});

			TestCC1.AC_LocalLanguageDescription = "Local Description 地方";
			AssertCalculationApplyTo_LocalDescription(TestCC1, expectedDescription: "TESTCC1");
		}

		public void AssertCalculationApplyTo_LocalDescription(AccChargeCode applyAccChargeCode, string expectedDescription)
		{
			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var applyToItem = TestCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
				applyToItem.TM_AC = applyAccChargeCode.PK;
				Line.TL_RateDesc = ChargeCode.AC_Desc; // undo overwrite Line description so assertion is always 'Test Charge'
				TestCalculator.Percent = 10m;

				var quotationLines = TestCalculator.GetQuotationLines(Entry);
				AssertContainsExactElementsInAnyOrder(new[] { $"Test Charge||10.00|% of {expectedDescription}" }, quotationLines.Select(x => x.ToString()));
			}
		}

		public void TestCalculationApplyTo_MultilingualDescription()
		{
			InitialiseTestCalculator();

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				Helper.SetChargeCodeMultilingualDescription(TestCC1, mockRes, "Mein Testgebührencode");

				var applyToItem = TestCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
				applyToItem.TM_AC = TestCC1.PK;
				TestCalculator.Percent = 10m;

				var quotationLines = TestCalculator.GetQuotationLines(Entry);
				AssertContainsExactElementsInAnyOrder(new[] { "Test Rate||10.00|% of Mein Testgebührencode" }, quotationLines.Select(x => x.ToString()));
			}
		}

		#endregion

		public void TestCalculation_WhenMonetaryIsHuge()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			Line.TL_RX_NKCurrency = "AUD";

			InitialiseTestCalculator();

			var applyToItem = TestCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria, AutoRateInfos);
			var monetaryValues = new MoneyType();

			TestCalculator.IsPartThereof = true;
			TestCalculator.Rate = 5m;
			TestCalculator.ValueOrPartThereOf = 10m;

			applyToItem.TM_Text = CalculatorConstants.Text.ApplyTo.Value.InsuranceValue;
			monetaryValues.Add(MoneyType.ValueType.InsuranceValue, new Money(922_337_203_685_477m, Line.Currency));
			parameters.Criteria.MonetaryValues = monetaryValues;
			AssertCalculation(parameters, 461168601842740m, "92233720368548 AUD 10.00 @ AUD 5.00/AUD 10.00 (per AUD 10.00 or part thereof for AUD 922337203685477.00 (Insurance Value))");
		}

		public void TestCalculation()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			Line.TL_RX_NKCurrency = "AUD";

			InitialiseTestCalculator();

			var applyToItem = TestCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			TestCalculator.Percent = 10m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria, AutoRateInfos);

			applyToItem.TM_AC = TestCC1.PK;
			AssertCalculation(parameters, 150m, "10.00% of (AUD 1500.00 (TESTCC1))");

			Line.TL_RX_NKCurrency = "USD";
			Criteria.CurrencyConverter = new TestCurrencyConverter(Factory);
			AssertCalculation(parameters, 105m, "10.00% of (USD 1050.00 (TESTCC1))");
			Line.TL_RX_NKCurrency = "AUD";

			applyToItem.TM_AC = TestCC2.PK;
			AssertCalculation(parameters, 564.30m, "10.00% of (AUD 5643.00 (TESTCC2))");

			applyToItem.TM_AC = TestCC3.PK;
			AssertCalculation(parameters, 453.40m, "10.00% of (AUD 4534.00 (TESTCC3))");

			applyToItem.TM_AC = TestCC4.PK;
			AssertCalculation(parameters, 94.40m, "10.00% of (AUD 944.00 (TESTCC4))");

			applyToItem.TM_Text = CalculatorConstants.Text.AllCharges;
			AssertCalculation(parameters, 1262.1m, "10.00% of (AUD 12621.00 (All Charge Codes TESTCC4 944.00 + TESTCC1 1500.00 + TESTCC3 4534.00 + TESTCC2 5643.00))");

			applyToItem.TM_Text = CalculatorConstants.Text.OriginCharges;
			AssertCalculation(parameters, 714.3m, "10.00% of (AUD 7143.00 (Origin Charges TESTCC1 1500.00 + TESTCC2 5643.00))");

			applyToItem.TM_Text = CalculatorConstants.Text.FreightCharges;
			AssertCalculation(parameters, 453.4m, "10.00% of (AUD 4534.00 (Freight Charges TESTCC3))");

			applyToItem.TM_Text = CalculatorConstants.Text.DestinationCharges;
			AssertCalculation(parameters, 94.4m, "10.00% of (AUD 944.00 (Destination Charges TESTCC4))");

			applyToItem.TM_Text = CalculatorConstants.Text.ApplyTo.Value.ValueOfGoods;
			var monetaryValues = new MoneyType();
			monetaryValues.Add(MoneyType.ValueType.GoodsValue, new Money(11000m, Line.Currency));
			parameters.Criteria.MonetaryValues = monetaryValues;
			TestCalculator.Percent = 2.5m;
			AssertCalculation(parameters, 275m, "2.50% of (AUD 11000.00 (Value of Goods))");

			applyToItem.TM_Text = CalculatorConstants.Text.ApplyTo.Value.InsuranceValue;
			monetaryValues.Add(MoneyType.ValueType.InsuranceValue, new Money(1100m, Line.Currency));
			parameters.Criteria.MonetaryValues = monetaryValues;
			AssertCalculation(parameters, 27.5m, "2.50% of (AUD 1100.00 (Insurance Value))");

			applyToItem.TM_Text = Calculator.Items.Value.DisbursementApplyToTypes.Disbursements;
			AssertCalculation(parameters, 150.85m, "2.50% of (AUD 6034.00 (Disbursements TESTCC1 1500.00 + TESTCC3 4534.00))");

			parameters.AddExistingChargeForTest(Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value), new Money(1100M, Line.Currency));
			AssertCalculation(parameters, 178.35m, "2.50% of (AUD 7134.00 (Disbursements CUSDSB* 1100.00 + TESTCC1 1500.00 + TESTCC3 4534.00))");

			applyToItem.TM_Text = Calculator.Items.Value.DisbursementApplyToTypes.CustomsDisbursement;
			AssertCalculation(parameters, 27.5m, "2.50% of (AUD 1100.00 (Customs Disbursement CUSDSB*))");

			var gstRate = Factory.New<AccTaxRate>();
			gstRate.SetRateNumerator_ForTestOnly(10);
			Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value).AC_AT_GSTRate = gstRate.PK;
			TestCalculator.IncludeGST = true;
			AssertCalculation(parameters, 30.25m, "2.50% of (AUD 1210.00 (Customs Disbursement CUSDSB*))");

			var applyToItem2 = TestCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			applyToItem2.TM_AC = TestCC1.PK;
			AssertCalculation(parameters, 67.75m, "2.50% of (AUD 2710.00 (Customs Disbursement + TESTCC1 CUSDSB* 1210.00 + TESTCC1 1500.00))");

			TestCalculator.GreaterCharge = true;
			AssertCalculation(parameters, 37.5m, "2.50% of (AUD 1500.00 (TESTCC1))");

			// Costing

			Line.RateLineItems.RemoveAndDelete(applyToItem2);
			TestCalculator.Percent = 10m;
			Line.Parent.Parent = Factory.New<Costing>();
			applyToItem.TM_Text = CalculatorConstants.Text.ChargeCode;

			applyToItem.TM_AC = TestCC1.PK;
			AssertCalculation(parameters, 150m, "10.00% of (AUD 1500.00 (TESTCC1))");

			applyToItem.TM_AC = TestCC2.PK;
			AssertCalculation(parameters, 564.30m, "10.00% of (AUD 5643.00 (TESTCC2))");

			applyToItem.TM_AC = TestCC3.PK;
			AssertCalculation(parameters, 453.40m, "10.00% of (AUD 4534.00 (TESTCC3))");

			applyToItem.TM_AC = TestCC4.PK;
			AssertCalculation(parameters, 94.40m, "10.00% of (AUD 944.00 (TESTCC4))");

			applyToItem.TM_Text = CalculatorConstants.Text.AllCharges;
			AssertCalculation(parameters, 1262.10m, "10.00% of (AUD 12621.00 (All Charge Codes TESTCC4 944.00 + TESTCC1 1500.00 + TESTCC3 4534.00 + TESTCC2 5643.00))");

			Line.TL_RX_NKCurrency = "EUR";
			AssertCalculation(parameters, 631.05m, "10.00% of (EUR 6310.50 (All Charge Codes TESTCC4 472.00 + TESTCC1 750.00 + TESTCC3 2267.00 + TESTCC2 2821.50))");

			applyToItem.TM_Text = CalculatorConstants.Text.ChargeCode;
			applyToItem.TM_AC = TestCC3.PK;
			AssertCalculation(parameters, 226.70m, "10.00% of (EUR 2267.00 (TESTCC3))");

			applyToItem.TM_Text = CalculatorConstants.Text.ApplyTo.Value.InvoiceValue;
			TestCalculator.IsPartThereof = true;
			TestCalculator.Rate = 5m;
			TestCalculator.ValueOrPartThereOf = 1000m;

			parameters.Criteria.Invoices = CreateInvoices(1000M);
			AssertCalculation(parameters, 5m, "1 EUR 1000.00 @ EUR 5.00/EUR 1000.00 (per EUR 1000.00 or part thereof for EUR 1000.00 (Invoice Value))");

			parameters.Criteria.Invoices = CreateInvoices(12542M);
			AssertCalculation(parameters, 65m, "13 EUR 1000.00 @ EUR 5.00/EUR 1000.00 (per EUR 1000.00 or part thereof for EUR 12542.00 (Invoice Value))");

			parameters.Criteria.Invoices = CreateInvoices(16525.98M);
			AssertCalculation(parameters, 85m, "17 EUR 1000.00 @ EUR 5.00/EUR 1000.00 (per EUR 1000.00 or part thereof for EUR 16525.98 (Invoice Value))");

			parameters.Criteria.Invoices = CreateInvoices(58467.45M);
			AssertCalculation(parameters, 295m, "59 EUR 1000.00 @ EUR 5.00/EUR 1000.00 (per EUR 1000.00 or part thereof for EUR 58467.45 (Invoice Value))");
		}

		public void TestCalculationForBondAmount()
		{
			GlbCompany.CurrentCompany.SetCountry("US");
			Line.TL_RX_NKCurrency = "USD";

			InitialiseTestCalculator();

			TestCalculator.AddApplyToItem(CalculatorConstants.Text.ApplyTo.Value.SingleTransactionBondAmount);
			TestCalculator.Percent = 1m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria, AutoRateInfos);
			Criteria.CurrencyConverter = new TestCurrencyConverter(Factory);
			Criteria.MonetaryValues = new MoneyType();
			Criteria.MonetaryValues.Add(MoneyType.ValueType.SingleTransactionBondAmount, new Money(2500m, GlbCompany.CurrentCompany.LocalCurrency));
			AssertCalculation(parameters, 25m, "1.00% of (USD 2500.00 (Single Transaction Bond Amount))");

			TestCalculator.IsPartThereof = true;
			TestCalculator.Rate = 5m;
			TestCalculator.ValueOrPartThereOf = 1000m;
			AssertCalculation(parameters, 15m, "3 USD 1000.00 @ USD 5.00/USD 1000.00 (per USD 1000.00 or part thereof for USD 2500.00 (Single Transaction Bond Amount))");
		}

		public void TestCalculationWithExistingCharges()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			Line.TL_RX_NKCurrency = "AUD";

			InitialiseTestCalculator();

			var applyToItem = TestCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			TestCalculator.Percent = 10m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);

			parameters.AddExistingChargeForTest(TestCC1, new Money(500M, Line.Currency));
			parameters.AddExistingChargeForTest(TestCC2, new Money(1000M, Line.Currency));
			parameters.AddExistingChargeForTest(TestCC3, new Money(1200M, Line.Currency));

			Factory.Save();

			applyToItem.TM_Text = CalculatorConstants.Text.OriginCharges;
			AssertCalculation(parameters, 150m, "10.00% of (AUD 1500.00 (Origin Charges TESTCC1* 500.00 + TESTCC2* 1000.00))");

			applyToItem.TM_Text = CalculatorConstants.Text.AllCharges;
			AssertCalculation(parameters, 270m, "10.00% of (AUD 2700.00 (All Charge Codes TESTCC1* 500.00 + TESTCC2* 1000.00 + TESTCC3* 1200.00))");

			applyToItem.TM_Text = CalculatorConstants.Text.ChargeCode;
			applyToItem.TM_AC = TestCC1.PK;
			AssertCalculation(parameters, 50m, "10.00% of (AUD 500.00 (TESTCC1*))");

			Line.TL_RX_NKCurrency = "USD";
			Criteria.CurrencyConverter = new TestCurrencyConverter(Factory);
			AssertCalculation(parameters, 35m, "10.00% of (USD 350.00 (TESTCC1*))");
			Line.TL_RX_NKCurrency = "AUD";

			var autoRateInfosWith1Charge = new AutoRateInfoCollection(Factory);
			autoRateInfosWith1Charge.AddNew(TestCC4, "AUD", 944M);
			applyToItem.TM_Text = CalculatorConstants.Text.AllCharges;
			parameters = new AutoRatingCalculatorParametersForTesting(Criteria, autoRateInfosWith1Charge);
			AssertCalculation(parameters, 364.4M, "10.00% of (AUD 3644.00 (All Charge Codes TESTCC1* 500.00 + TESTCC4 944.00 + TESTCC2* 1000.00 + TESTCC3* 1200.00))");
			parameters.RatingContext.PercentageLinesApplied.Clear();
			ClearUsedByRateLines(autoRateInfosWith1Charge);

			autoRateInfosWith1Charge.AddNew(TestCC1, "AUD", 700M);
			applyToItem.TM_Text = CalculatorConstants.Text.AllCharges;
			AssertCalculation(parameters, 434.4M, "10.00% of (AUD 4344.00 (All Charge Codes TESTCC1* 500.00 + TESTCC1 700.00 + TESTCC4 944.00 + TESTCC2* 1000.00 + TESTCC3* 1200.00))");
			parameters.RatingContext.PercentageLinesApplied.Clear();
			ClearUsedByRateLines(autoRateInfosWith1Charge);

			parameters = new AutoRatingCalculatorParametersForTesting(Criteria, autoRateInfosWith1Charge);
			Criteria.SetExistingCharges(new List<IAutoRatingChargeInfo>());
			parameters.AddExistingChargeForTest(TestCC1, new Money(500M, Line.Currency), true);
			parameters.AddExistingChargeForTest(TestCC2, new Money(1000M, Line.Currency));
			parameters.AddExistingChargeForTest(TestCC3, new Money(1200M, Line.Currency));
			AssertCalculation(parameters, 364.4m, "10.00% of (AUD 3644.00 (All Charge Codes TESTCC1* 500.00 + TESTCC4 944.00 + TESTCC2* 1000.00 + TESTCC3* 1200.00))");
			parameters.RatingContext.PercentageLinesApplied.Clear();
			ClearUsedByRateLines(autoRateInfosWith1Charge);

			parameters = new AutoRatingCalculatorParametersForTesting(Criteria, autoRateInfosWith1Charge);
			Criteria.SetExistingCharges(new List<IAutoRatingChargeInfo>());
			parameters.AddExistingChargeForTest(TestCC1, new Money(350, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD")), true);
			parameters.AddExistingChargeForTest(TestCC2, new Money(1000M, Line.Currency));
			parameters.AddExistingChargeForTest(TestCC3, new Money(1200M, Line.Currency));
			AssertCalculation(parameters, 364.4m, "10.00% of (AUD 3644.00 (All Charge Codes TESTCC1* 500.00 + TESTCC4 944.00 + TESTCC2* 1000.00 + TESTCC3* 1200.00))");
			parameters.RatingContext.PercentageLinesApplied.Clear();
			ClearUsedByRateLines(autoRateInfosWith1Charge);
		}

		public void TestCalculationFromMoreThanOneInfo()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			Line.TL_RX_NKCurrency = "AUD";

			InitialiseTestCalculator();

			var applyToItem = TestCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			TestCalculator.Percent = 10m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria, AutoRateInfos);

			applyToItem.TM_AC = TestCC1.PK;
			AssertCalculation(parameters, 150m, "10.00% of (AUD 1500.00 (TESTCC1))");

			AutoRateInfos.AddNew(TestCC1, "AUD", 600M);
			AutoRateInfos.AddNew(TestCC1, "AUD", 400M);
			AssertCalculation(parameters, 250m, "10.00% of (AUD 2500.00 (TESTCC1 400.00 + TESTCC1 600.00 + TESTCC1 1500.00))");
		}

		public void TestCalculate_ContainerIsEmpty_ApplyChangeToChargesWithAnyContainer_ExistingCharges()
		{
			Criteria.SetExistingCharges(new[]
			{
				CreateCostCharge(chargeCode: "FRT", container: "20GP", amount: 1500, currency: "AUD"),
				CreateCostCharge(chargeCode: "FRT", container: "40GP", amount: 2000, currency: "AUD")
			});

			var calc = CreateCalculatorToTest(applyToChargeCode: "FRT", container: "");
			calc.Percent = 10m;

			var result = calc.Calculate(Criteria).results.Single();
			AssertEquals(
				"Result description should match the expected text",
				"10.00% of (AUD 3500.00 (Freight Charges FRT* 1500.00 + FRT* 2000.00))",
				result.Description
			);
			AssertEquals(
				"Calculated payment base amount should be correct",
				350m,
				result.PaymentBases.Calculate().amount
			);
		}

		public void TestCalculate_ContainerIsEmpty_ApplyChangeToChargesWithAnyContainer_CalculatedCharges()
		{
			var autoRateInfos = new AutoRateInfoCollection(Factory);
			autoRateInfos.AddNew(Helper.ChargeCodes["FRT"], container: "20GP", amount: 1500, currency: "AUD");
			autoRateInfos.AddNew(Helper.ChargeCodes["FRT"], container: "40GP", amount: 2000, currency: "AUD");

			var calc = CreateCalculatorToTest(applyToChargeCode: "FRT", container: "");
			calc.Percent = 10m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria, autoRateInfos);
			var result = calc.Calculate(parameters).results.First();

			AssertEquals(
				"The description should match the expected format.",
				"10.00% of (AUD 3500.00 (Freight Charges FRT 1500.00 + FRT 2000.00))",
				result.Description
			);

			AssertEquals(
				"The calculated amount should be accurate.",
				350m,
				result.PaymentBases.Calculate().amount
			);
		}

		public void TestCalculate_ContainerIsNotEmpty_ApplyChangeToChargesWithMatchingContainer_ExistingCharges()
		{
			Criteria.SetExistingCharges(new[]
			{
				CreateCostCharge(chargeCode: "FRT", container: "20GP", amount: 1500, currency: "AUD"),
				CreateCostCharge(chargeCode: "FRT", container: "40GP", amount: 2000, currency: "AUD"),
				CreateCostCharge(chargeCode: "FRT", container: "", amount: 800, currency: "AUD"),
			});

			var calc = CreateCalculatorToTest(applyToChargeCode: "FRT", container: "40GP");
			calc.Percent = 10m;

			var result = calc.Calculate(Criteria).results.Single();

			AssertEquals(
				"Description should correctly summarize the calculation process and values.",
				"10.00% of (AUD 2800.00 (Freight Charges FRT* 800.00 + FRT* 2000.00))",
				result.Description
			);

			AssertEquals(
				"Payment bases should calculate the correct amount.",
				280m,
				result.PaymentBases.Calculate().amount
			);
		}

		public void TestCalculate_ContainerIsNotEmpty_ApplyChangeToChargesWithMatchingContainer_CalculatedCharges()
		{
			var autoRateInfos = new AutoRateInfoCollection(Factory);
			autoRateInfos.AddNew(Helper.ChargeCodes["FRT"], container: "20GP", amount: 1500, currency: "AUD");
			autoRateInfos.AddNew(Helper.ChargeCodes["FRT"], container: "40GP", amount: 2000, currency: "AUD");
			autoRateInfos.AddNew(Helper.ChargeCodes["FRT"], container: "", amount: 800, currency: "AUD");

			var calc = CreateCalculatorToTest(applyToChargeCode: "FRT", container: "40GP");
			calc.Percent = 10m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria, autoRateInfos);
			var result = calc.Calculate(parameters).results.First();

			AssertEquals(
				"The description of the calculation should match the expected result.",
				"10.00% of (AUD 2800.00 (Freight Charges FRT 800.00 + FRT 2000.00))",
				result.Description
			);

			AssertEquals(
				"The calculated payment base amount should be as expected.",
				280m,
				result.PaymentBases.Calculate().amount
			);
		}

		public void TestCalculate_ContainerIsNotEmpty_MatchByContainerClass_ApplyChangeToChargesWithMatchingContainer_ExistingCharges()
		{
			var fr20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR");
			fr20.RC_HandlingRateClass = "SV20";

			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			gp20.RC_HandlingRateClass = "SV20";

			Factory.Save();

			Criteria.SetExistingCharges(new[]
			{
				CreateCostCharge(chargeCode: "FRT", container: "20GP", amount: 1500, currency: "AUD"),
				CreateCostCharge(chargeCode: "FRT", container: "40GP", amount: 2000, currency: "AUD"),
				CreateCostCharge(chargeCode: "FRT", container: "", amount: 800, currency: "AUD"),
			});

			var calc = CreateCalculatorToTest(applyToChargeCode: "FRT", container: "20FR", matchContainerClass: true);
			calc.Percent = 10m;

			var result = calc.Calculate(Criteria).results.Single();

			AssertEquals(
				"Description should match the expected format with the calculated percentages and charges.",
				"10.00% of (AUD 2300.00 (Freight Charges FRT* 800.00 + FRT* 1500.00))",
				result.Description
			);

			AssertEquals(
				"Payment base calculated amount should match the expected value.",
				230m,
				result.PaymentBases.Calculate().amount
			);
		}

		public void TestCalculate_ContainerIsNotEmpty_MatchByContainerClass_ApplyChangeToChargesWithMatchingContainer_CalculatedCharges()
		{
			var fr20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR");
			fr20.RC_HandlingRateClass = "SV20";

			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			gp20.RC_HandlingRateClass = "SV20";

			Factory.Save();

			var autoRateInfos = new AutoRateInfoCollection(Factory);
			autoRateInfos.AddNew(Helper.ChargeCodes["FRT"], container: "20GP", amount: 1500, currency: "AUD");
			autoRateInfos.AddNew(Helper.ChargeCodes["FRT"], container: "40GP", amount: 2000, currency: "AUD");
			autoRateInfos.AddNew(Helper.ChargeCodes["FRT"], container: "", amount: 800, currency: "AUD");

			var calc = CreateCalculatorToTest(applyToChargeCode: "FRT", container: "20FR", matchContainerClass: true);
			calc.Percent = 10m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria, autoRateInfos);
			var result = calc.Calculate(parameters).results.First();

			AssertEquals(
				"The description should match the expected calculation output.",
				"10.00% of (AUD 2300.00 (Freight Charges FRT 800.00 + FRT 1500.00))",
				result.Description
			);

			AssertEquals(
				"The calculated amount should be correct based on 10% of AUD 2300.",
				230m,
				result.PaymentBases.Calculate().amount
			);
		}

		public void TestCalculate_CommodityIsEmpty_ApplyChangeToChargesWithAnyCommodity_ExistingCharges()
		{
			Criteria.SetExistingCharges(new[]
			{
				CreateCostCharge(chargeCode: "FRT", commodity: "BODY", amount: 1500, currency: "AUD"),
				CreateCostCharge(chargeCode: "FRT", commodity: "CARS", amount: 2000, currency: "AUD"),
			});

			var calc = CreateCalculatorToTest(applyToChargeCode: "FRT", commodity: "");
			calc.Percent = 10m;

			var result = calc.Calculate(Criteria).results.Single();
			AssertEquals(
				"The description should match the expected output.",
				"10.00% of (AUD 3500.00 (Freight Charges FRT* 1500.00 + FRT* 2000.00))",
				result.Description
			);
			AssertEquals("The calculated payment amount should match the expected value.", 350m, result.PaymentBases.Calculate().amount);
		}

		public void TestCalculate_CommodityIsEmpty_ApplyChangeToChargesWithAnyCommodity_CalculatedCharges()
		{
			var autoRateInfos = new AutoRateInfoCollection(Factory);
			autoRateInfos.AddNew(Helper.ChargeCodes["FRT"], commodity: "BODY", amount: 1500, currency: "AUD");
			autoRateInfos.AddNew(Helper.ChargeCodes["FRT"], commodity: "CARS", amount: 2000, currency: "AUD");

			var calc = CreateCalculatorToTest(applyToChargeCode: "FRT", commodity: "");
			calc.Percent = 10m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria, autoRateInfos);
			var result = calc.Calculate(parameters).results.First();

			AssertEquals(
				"The description of the calculation result is incorrect.",
				"10.00% of (AUD 3500.00 (Freight Charges FRT 1500.00 + FRT 2000.00))",
				result.Description
			);

			AssertEquals(
				"The calculated payment base amount is incorrect.",
				350m,
				result.PaymentBases.Calculate().amount
			);
		}

		public void TestCalculate_CommodityIsNotEmpty_ApplyChangeToChargesWithMatchingCommodity_ExistingCharges()
		{
			Criteria.SetExistingCharges(new[]
			{
				CreateCostCharge(chargeCode: "FRT", commodity: "BODY", amount: 1500, currency: "AUD"),
				CreateCostCharge(chargeCode: "FRT", commodity: "CARS", amount: 2000, currency: "AUD"),
				CreateCostCharge(chargeCode: "FRT", commodity: "", amount: 800, currency: "AUD")
			});

			var calc = CreateCalculatorToTest(applyToChargeCode: "FRT", commodity: "CARS");
			calc.Percent = 10m;

			var result = calc.Calculate(Criteria).results.Single();
			AssertEquals(
				"The result description should match the expected formatted string.",
				"10.00% of (AUD 2800.00 (Freight Charges FRT* 800.00 + FRT* 2000.00))",
				result.Description
			);
			AssertEquals(
				"The calculated payment base amount should be correct.",
				280m,
				result.PaymentBases.Calculate().amount
			);
		}

		public void TestCalculate_CommodityIsNotEmpty_ApplyChangeToChargesWithMatchingCommodity_CalculatedCharges()
		{
			var autoRateInfos = new AutoRateInfoCollection(Factory);
			autoRateInfos.AddNew(Helper.ChargeCodes["FRT"], commodity: "BODY", amount: 1500, currency: "AUD");
			autoRateInfos.AddNew(Helper.ChargeCodes["FRT"], commodity: "CARS", amount: 2000, currency: "AUD");
			autoRateInfos.AddNew(Helper.ChargeCodes["FRT"], commodity: "", amount: 800, currency: "AUD");

			var calc = CreateCalculatorToTest(applyToChargeCode: "FRT", commodity: "CARS");
			calc.Percent = 10m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria, autoRateInfos);
			var result = calc.Calculate(parameters).results.First();

			AssertEquals(
				"The description doesn't match the expected value.",
				"10.00% of (AUD 2800.00 (Freight Charges FRT 800.00 + FRT 2000.00))",
				result.Description
			);
			AssertEquals(
				"The calculated amount doesn't match the expected value.",
				280m,
				result.PaymentBases.Calculate().amount
			);
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.Line.ViewAgentRates = false;
			TestCalculator.Percent = 5m;
			TestCalculator.BaseRate = 80m;
			TestCalculator.Line.ViewAgentRates = true;
			TestCalculator.Percent = 10m;
			TestCalculator.BaseRate = 100m;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;

			source.PerUnit = 6m;
			source.BaseRate = 60m;
			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.GetCalculator<PercentageCalculator>().Line.ViewAgentRates = false;
			AssertEquals(5m, clonedLine.GetCalculator<PercentageCalculator>().Percent);
			AssertEquals(140m, clonedLine.GetCalculator<PercentageCalculator>().BaseRate);
			AssertEquals(0m, clonedLine.GetCalculator<PercentageCalculator>().Minimum);
			AssertEquals(0m, clonedLine.GetCalculator<PercentageCalculator>().Maximum);
			clonedLine.GetCalculator<PercentageCalculator>().Line.ViewAgentRates = true;
			AssertEquals(10m, clonedLine.GetCalculator<PercentageCalculator>().Percent);
			AssertEquals(160m, clonedLine.GetCalculator<PercentageCalculator>().BaseRate);
			AssertEquals(0m, clonedLine.GetCalculator<PercentageCalculator>().Minimum);
			AssertEquals(0m, clonedLine.GetCalculator<PercentageCalculator>().Maximum);

			TestCalculator.Line.ViewAgentRates = false;
			TestCalculator.Minimum = 100m;
			TestCalculator.Maximum = 1000m;
			TestCalculator.Line.ViewAgentRates = true;
			TestCalculator.Minimum = 120m;
			TestCalculator.Maximum = 1200m;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.GetCalculator<PercentageCalculator>().Line.ViewAgentRates = false;
			AssertEquals(5m, clonedLine.GetCalculator<PercentageCalculator>().Percent);
			AssertEquals(140m, clonedLine.GetCalculator<PercentageCalculator>().BaseRate);
			AssertEquals(160m, clonedLine.GetCalculator<PercentageCalculator>().Minimum);
			AssertEquals(1060m, clonedLine.GetCalculator<PercentageCalculator>().Maximum);
			clonedLine.GetCalculator<PercentageCalculator>().Line.ViewAgentRates = true;
			AssertEquals(10m, clonedLine.GetCalculator<PercentageCalculator>().Percent);
			AssertEquals(160m, clonedLine.GetCalculator<PercentageCalculator>().BaseRate);
			AssertEquals(180m, clonedLine.GetCalculator<PercentageCalculator>().Minimum);
			AssertEquals(1260m, clonedLine.GetCalculator<PercentageCalculator>().Maximum);

			source.BaseRate = 0m;
			source.Minimum = 40m;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.GetCalculator<PercentageCalculator>().Line.ViewAgentRates = false;
			AssertEquals(5m, clonedLine.GetCalculator<PercentageCalculator>().Percent);
			AssertEquals(80m, clonedLine.GetCalculator<PercentageCalculator>().BaseRate);
			AssertEquals(140m, clonedLine.GetCalculator<PercentageCalculator>().Minimum);
			AssertEquals(1000m, clonedLine.GetCalculator<PercentageCalculator>().Maximum);
			clonedLine.GetCalculator<PercentageCalculator>().Line.ViewAgentRates = true;
			AssertEquals(10m, clonedLine.GetCalculator<PercentageCalculator>().Percent);
			AssertEquals(100m, clonedLine.GetCalculator<PercentageCalculator>().BaseRate);
			AssertEquals(160m, clonedLine.GetCalculator<PercentageCalculator>().Minimum);
			AssertEquals(1200m, clonedLine.GetCalculator<PercentageCalculator>().Maximum);

			source.Minimum = 0m;
			source.BaseRate = 50m;
			source.PerUnit = 0m;
			source.Percent = 20m;
			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.PercentFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.GetCalculator<PercentageCalculator>().Line.ViewAgentRates = false;
			AssertEquals(6m, clonedLine.GetCalculator<PercentageCalculator>().Percent);
			AssertEquals(146m, clonedLine.GetCalculator<PercentageCalculator>().BaseRate);
			AssertEquals(170m, clonedLine.GetCalculator<PercentageCalculator>().Minimum);
			AssertEquals(1250m, clonedLine.GetCalculator<PercentageCalculator>().Maximum);
			clonedLine.GetCalculator<PercentageCalculator>().Line.ViewAgentRates = true;
			AssertEquals(12m, clonedLine.GetCalculator<PercentageCalculator>().Percent);
			AssertEquals(170m, clonedLine.GetCalculator<PercentageCalculator>().BaseRate);
			AssertEquals(194m, clonedLine.GetCalculator<PercentageCalculator>().Minimum);
			AssertEquals(1490m, clonedLine.GetCalculator<PercentageCalculator>().Maximum);

			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.GetCalculator<PercentageCalculator>().Line.ViewAgentRates = false;
			AssertEquals(6m, clonedLine.GetCalculator<PercentageCalculator>().Percent);
			AssertEquals(156m, clonedLine.GetCalculator<PercentageCalculator>().BaseRate);
			AssertEquals(180m, clonedLine.GetCalculator<PercentageCalculator>().Minimum);
			AssertEquals(1260m, clonedLine.GetCalculator<PercentageCalculator>().Maximum);
			clonedLine.GetCalculator<PercentageCalculator>().Line.ViewAgentRates = true;
			AssertEquals(12m, clonedLine.GetCalculator<PercentageCalculator>().Percent);
			AssertEquals(180m, clonedLine.GetCalculator<PercentageCalculator>().BaseRate);
			AssertEquals(204m, clonedLine.GetCalculator<PercentageCalculator>().Minimum);
			AssertEquals(1500m, clonedLine.GetCalculator<PercentageCalculator>().Maximum);

			TestCalculator.IsPartThereof = true;
			TestCalculator.Line.ViewAgentRates = false;
			TestCalculator.Percent = 0m;
			TestCalculator.Rate = 15m;
			TestCalculator.ValueOrPartThereOf = 1000m;
			TestCalculator.Line.ViewAgentRates = true;
			TestCalculator.Percent = 5m;
			TestCalculator.Rate = 20m;
			TestCalculator.ValueOrPartThereOf = 1200m;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			Assert(clonedLine.GetCalculator<PercentageCalculator>().IsPartThereof);
			clonedLine.GetCalculator<PercentageCalculator>().Line.ViewAgentRates = false;
			AssertEquals(0m, clonedLine.GetCalculator<PercentageCalculator>().Percent);
			AssertEquals(18m, clonedLine.GetCalculator<PercentageCalculator>().Rate);
			AssertEquals(1000m, clonedLine.GetCalculator<PercentageCalculator>().ValueOrPartThereOf);
			AssertEquals(156m, clonedLine.GetCalculator<PercentageCalculator>().BaseRate);
			AssertEquals(180m, clonedLine.GetCalculator<PercentageCalculator>().Minimum);
			AssertEquals(1260m, clonedLine.GetCalculator<PercentageCalculator>().Maximum);
			clonedLine.GetCalculator<PercentageCalculator>().Line.ViewAgentRates = true;
			AssertEquals(6m, clonedLine.GetCalculator<PercentageCalculator>().Percent);
			AssertEquals(24m, clonedLine.GetCalculator<PercentageCalculator>().Rate);
			AssertEquals(1200m, clonedLine.GetCalculator<PercentageCalculator>().ValueOrPartThereOf);
			AssertEquals(180m, clonedLine.GetCalculator<PercentageCalculator>().BaseRate);
			AssertEquals(204m, clonedLine.GetCalculator<PercentageCalculator>().Minimum);
			AssertEquals(1500m, clonedLine.GetCalculator<PercentageCalculator>().Maximum);
		}

		public override void TestGetCostsComparerChargesSummary()
		{
			var applyToLine = Line.Parent.AddRateLine("ODOC");
			applyToLine.TL_RateCalculator = CombinedCalculator.Code;
			var applyToCalculator = (CombinedCalculator)applyToLine.Calculator;
			applyToCalculator.Minimum = 100m;
			applyToCalculator.BaseRate = 50m;
			applyToCalculator.PerUnit = 5m;

			var lines = new List<RateLine>();
			lines.Add(applyToLine);

			var applyToItem = TestCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			applyToItem.TM_AC = Helper.ChargeCodes["ODOC"].PK;
			TestCalculator.Percent = 10m;

			var items = TestCalculator.GetCostsComparerChargesSummary(lines);
			AssertEquals(3, items.Count);
			AssertChargesSummaryItem(items[0], "MIN", 10m);
			AssertChargesSummaryItem(items[1], "BAS", 5m);
			AssertChargesSummaryItem(items[2], "UNT", 0.5m);

			TestCalculator.Minimum = 4m;
			TestCalculator.BaseRate = 25m;
			items = TestCalculator.GetCostsComparerChargesSummary(lines);
			AssertEquals(3, items.Count);
			AssertChargesSummaryItem(items[0], "MIN", 10m);
			AssertChargesSummaryItem(items[1], "BAS", 30m);
			AssertChargesSummaryItem(items[2], "UNT", 0.5m);

			TestCalculator.Minimum = 14m;
			items = TestCalculator.GetCostsComparerChargesSummary(lines);
			AssertEquals(3, items.Count);
			AssertChargesSummaryItem(items[0], "MIN", 14m);
			AssertChargesSummaryItem(items[1], "BAS", 30m);
			AssertChargesSummaryItem(items[2], "UNT", 0.5m);

			applyToLine.RateLineItems.RemoveAndDelete(applyToLine.RateLineItems.FindByTM_Type("MIN"));
			applyToLine.RateLineItems.RemoveAndDelete(applyToLine.RateLineItems.FindByTM_Type("BAS"));
			items = TestCalculator.GetCostsComparerChargesSummary(lines);
			AssertEquals(3, items.Count);
			AssertChargesSummaryItem(items[0], "UNT", 0.5m);
			AssertChargesSummaryItem(items[1], "MIN", 14m);
			AssertChargesSummaryItem(items[2], "BAS", 25m);
		}

		#region Implementation

		PercentageCalculator CreateCalculatorToTest(string applyToChargeCode, string container = null, string commodity = null, bool matchContainerClass = false)
		{
			if (!string.IsNullOrEmpty(container))
			{
				Entry.TI_RC = Helper.Containers[container].PK;
			}

			Entry.TI_RH_NKCommodityCode = commodity;
			Entry.TI_MatchContainerRateClass = matchContainerClass;

			var line = Entry.RateLines.AddNew();
			line.TL_AC = ChargeCode.PK;
			line.TL_RateDesc = "Test Rate";
			line.TL_RateCalculator = CalculatorCode;
			line.TL_RX_NKCurrency = "AUD";

			var calc = line.Calculator as PercentageCalculator;
			calc.AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_Text = applyToChargeCode;

			return line.Calculator as PercentageCalculator;
		}

		InvoiceInfoCollection CreateInvoices(decimal invoiceValue)
		{
			var collection = new InvoiceInfoCollection();
			collection.AddNew(new Money(invoiceValue, Line.Currency), 0);

			return collection;
		}

		protected override Type CalculatorType
		{
			get { return typeof(PercentageCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return PercentageCalculator.Code; }
		}

		new PercentageCalculator TestCalculator
		{
			get { return (PercentageCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
