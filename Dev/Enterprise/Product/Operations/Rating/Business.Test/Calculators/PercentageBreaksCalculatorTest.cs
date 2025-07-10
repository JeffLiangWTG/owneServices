using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class PercentageBreaksCalculatorTest : BaseCombinedCalculatorTest<PercentageBreaksCalculator>
	{
		protected override int NumberOfRateLineItemsAfterInitialization { get { return 6; } }

		public void TestRequiresWeightVolume()
		{
			AssertEquals("RequiresWeightVolume", true, TestCalculator.Line.RequiresWeightVolume());

			TestCalculator.UseBreaksBasedOnValues = true;
			AssertEquals("RequiresWeightVolume based on NOT BreaksBasedOnValues", false, TestCalculator.Line.RequiresWeightVolume());
			AssertEquals(false, Line.UseOnlyActualWeightMeasure);
			AssertEquals(true, Line.ConversionFactorForBinding.ReadOnly);
			AssertEquals(true, Line.UseOnlyActualWeightMeasureInfo.ReadOnly);

			TestCalculator.UseBreaksBasedOnValues = false;
			AssertEquals("RequiresWeightVolume based on NOT BreaksBasedOnValues", true, TestCalculator.Line.RequiresWeightVolume());
			AssertEquals(false, Line.UseOnlyActualWeightMeasure);
			AssertEquals(false, Line.ConversionFactorForBinding.ReadOnly);
			AssertEquals(false, Line.UseOnlyActualWeightMeasureInfo.ReadOnly);
		}

		public override void TestCheckOrCreateItems()
		{
			base.TestCheckOrCreateItems();

			AssertNotNull(Line.RateLineItems.FindByTM_Type(PercentageBreaksCalculator.Items.BreaksBasedOnValues));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Text.IncludeGST));

			AssertEquals(Line.RateLineItems.FindByTM_Type(PercentageBreaksCalculator.Items.BreaksBasedOnValues).TM_TextInfo, TestCalculator.Bool4Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Text.IncludeGST).TM_TextInfo, TestCalculator.Bool5Info);

			Assert(!TestCalculator.IsAccumulated);
			Assert(!TestCalculator.UseHigherChargeableLowerRateRule);
			Assert(!TestCalculator.UseInclusiveBreaks);
			Assert(!TestCalculator.UseBreaksBasedOnValues);
			Assert(!TestCalculator.IncludeGST);
		}

		public override void TestMapping()
		{
			base.TestMapping();

			TestMapping(PercentageBreaksCalculator.Items.BreaksBasedOnValues, "Bool4");
			TestMapping(CalculatorConstants.Text.IncludeGST, "Bool5");
		}

		public override void TestQuotationLines()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "AUD";
			Line.ConversionFactor = new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());

			Line.UseOnlyActualWeightMeasure = true;
			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(2, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[1].ToString());

			Line.RateLineItems.RemoveAndDeleteAll();

			TestCalculator.Minimum = 100m;
			TestCalculator["-5"] = (ZDecimal)60m;
			var minus5Item = Line.RateLineItems[Line.RateLineItems.Count - 1];
			TestCalculator["+5"] = (ZDecimal)55m;
			var plus5Item = Line.RateLineItems[Line.RateLineItems.Count - 1];
			TestCalculator["+10"] = (ZDecimal)52m;
			var plus10Item = Line.RateLineItems[Line.RateLineItems.Count - 1];
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(2, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[1].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(2, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[1].ToString());

			TestCalculator.Minimum = 100m;
			minus5Item.TM_BreakMinimum = 1.0;
			plus5Item.TM_BreakMinimum = 1.1;
			plus10Item.TM_BreakMinimum = 1.2;
			var percent1 = Line.RateLineItems.AddNew();
			percent1.TM_Type = CalculatorConstants.Type.ApplyTo;
			percent1.TM_Text = CalculatorConstants.Text.ChargeCode;
			percent1.TM_AC = Env.Registry.FreightChargeCode;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(8, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 5 M3|||", quotationLines[1].ToString());
			AssertEquals("||1.00|% of Freight", quotationLines[2].ToString());
			AssertEquals("5 M3 to less than 10 M3|||", quotationLines[3].ToString());
			AssertEquals("||1.10|% of Freight", quotationLines[4].ToString());
			AssertEquals("10 M3 and above|||", quotationLines[5].ToString());
			AssertEquals("||1.20|% of Freight", quotationLines[6].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[7].ToString());

			minus5Item.TM_FlatAmount = 10;
			plus5Item.TM_FlatAmount = 20;
			TestCalculator["+5"] = (ZDecimal)55m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(10, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 5 M3|||", quotationLines[1].ToString());
			AssertEquals("||1.00|% of Freight", quotationLines[2].ToString());
			AssertEquals("Flat Amount|AUD|10.00|", quotationLines[3].ToString());
			AssertEquals("5 M3 to less than 10 M3|||", quotationLines[4].ToString());
			AssertEquals("||1.10|% of Freight", quotationLines[5].ToString());
			AssertEquals("Flat Amount|AUD|20.00|", quotationLines[6].ToString());
			AssertEquals("10 M3 and above|||", quotationLines[7].ToString());
			AssertEquals("||1.20|% of Freight", quotationLines[8].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[9].ToString());

			var percent2 = Line.RateLineItems.AddNew();
			percent2.TM_Type = CalculatorConstants.Type.ApplyTo;
			percent2.TM_Text = Calculator.Items.Value.DisbursementApplyToTypes.Disbursements;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(13, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 5 M3|||", quotationLines[1].ToString());
			AssertEquals("||1.00|% of Freight", quotationLines[2].ToString());
			AssertEquals("||1.00|% of disbursements", quotationLines[3].ToString());
			AssertEquals("Flat Amount|AUD|10.00|", quotationLines[4].ToString());
			AssertEquals("5 M3 to less than 10 M3|||", quotationLines[5].ToString());
			AssertEquals("||1.10|% of Freight", quotationLines[6].ToString());
			AssertEquals("||1.10|% of disbursements", quotationLines[7].ToString());
			AssertEquals("Flat Amount|AUD|20.00|", quotationLines[8].ToString());
			AssertEquals("10 M3 and above|||", quotationLines[9].ToString());
			AssertEquals("||1.20|% of Freight", quotationLines[10].ToString());
			AssertEquals("||1.20|% of disbursements", quotationLines[11].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[12].ToString());

			TestCalculator.UseBreaksBasedOnValues = true;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(13, quotationLines.Count);
			AssertEquals("Test Rate - Breaks are based on values|||", quotationLines[0].ToString());
			AssertEquals("Less than 5 M3|||", quotationLines[1].ToString());
			AssertEquals("||1.00|% of Freight", quotationLines[2].ToString());
			AssertEquals("||1.00|% of disbursements", quotationLines[3].ToString());
			AssertEquals("Flat Amount|AUD|10.00|", quotationLines[4].ToString());
			AssertEquals("5 M3 to less than 10 M3|||", quotationLines[5].ToString());
			AssertEquals("||1.10|% of Freight", quotationLines[6].ToString());
			AssertEquals("||1.10|% of disbursements", quotationLines[7].ToString());
			AssertEquals("Flat Amount|AUD|20.00|", quotationLines[8].ToString());
			AssertEquals("10 M3 and above|||", quotationLines[9].ToString());
			AssertEquals("||1.20|% of Freight", quotationLines[10].ToString());
			AssertEquals("||1.20|% of disbursements", quotationLines[11].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[12].ToString());
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddPercentageBreaksRateLine(rateEntry, "FRT", "AUD", min: 100, max: 109, flat: 101, perUnit: 102, unit: "KG", applyToList: new[] { CalculatorConstants.Text.OriginCharges, CalculatorConstants.Text.DestinationCharges }, ("-5", 1, 104), ("+5", 2, 106));
			var rateLine11 = AddPercentageBreaksRateLine(rateEntry, "FRT", "AUD", min: 110, max: 119, flat: 111, perUnit: 112, unit: "KG", applyToList: new[] { CalculatorConstants.Text.OriginCharges, CalculatorConstants.Text.LoadingCharges }, ("-5", 3, 114), ("+5", 4, 116), ("+6", 5, 118));
			var rateLine12 = AddPercentageBreaksRateLine(rateEntry, "FRT", "AUD", min: 120, max: 129, flat: 121, perUnit: 122, unit: "M3", applyToList: new[] { CalculatorConstants.Text.OriginCharges, CalculatorConstants.Text.DestinationCharges }, ("-5", 1, 124), ("+5", 2, 126));

			var rateLine20 = AddPercentageBreaksRateLine(rateEntry, "FRT", "USD", min: 200, max: 209, flat: 201, perUnit: 202, unit: "KG", applyToList: new[] { CalculatorConstants.Text.OriginCharges, CalculatorConstants.Text.DestinationCharges }, ("-5", 5, 204), ("+5", 6, 206));

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine12.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"Less than 5 Kilogram(s)|AUD|4.00|% of origin charges",
					"Less than 5 Kilogram(s)|AUD|218.00|",
					"Less than 5 Kilogram(s)|AUD|1.00|% of destination charges",
					"5 Kilogram(s) and above|AUD|2.00|% of origin charges",
					"5 Kilogram(s) and above|AUD|106.00|",
					"5 Kilogram(s) and above|AUD|2.00|% of destination charges",
					"|AUD|333.00|",
					"Minimum|AUD|100.00|",
					"Maximum|AUD|129.00|",
					"Less than 5 Kilogram(s)|AUD|3.00|% of loading charges",
					"5 Kilogram(s) to less than 6 Kilogram(s)|AUD|4.00|% of origin charges",
					"5 Kilogram(s) to less than 6 Kilogram(s)|AUD|116.00|",
					"5 Kilogram(s) to less than 6 Kilogram(s)|AUD|4.00|% of loading charges",
					"6 Kilogram(s) and above|AUD|5.00|% of origin charges",
					"6 Kilogram(s) and above|AUD|118.00|",
					"6 Kilogram(s) and above|AUD|5.00|% of loading charges",
					"Less than 5 Cubic Meter(s)|AUD|1.00|% of origin charges",
					"Less than 5 Cubic Meter(s)|AUD|124.00|",
					"Less than 5 Cubic Meter(s)|AUD|1.00|% of destination charges",
					"5 Cubic Meter(s) and above|AUD|2.00|% of origin charges",
					"5 Cubic Meter(s) and above|AUD|126.00|",
					"5 Cubic Meter(s) and above|AUD|2.00|% of destination charges",
					"Less than 5 Kilogram(s)|USD|5.00|% of origin charges",
					"Less than 5 Kilogram(s)|USD|204.00|",
					"Less than 5 Kilogram(s)|USD|5.00|% of destination charges",
					"5 Kilogram(s) and above|USD|6.00|% of origin charges",
					"5 Kilogram(s) and above|USD|206.00|",
					"5 Kilogram(s) and above|USD|6.00|% of destination charges",
					"|USD|201.00|",
					"Minimum|USD|200.00|",
					"Maximum|USD|209.00|"
				},
				message: "Calculation component with same currency, unit, charge, break should be grouped/rolledUp"
			);
		}

		static RateLine AddPercentageBreaksRateLine(RateEntry rateEntry, ZString chargeCode, string currency, decimal min, decimal max, decimal flat, decimal perUnit, string unit, string[] applyToList, params (string Break, decimal Percentage, decimal Flat)[] breaks)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, PercentageBreaksCalculator.Code, unit, currencyCode: currency);
			var calculator = rateLine.GetCalculator<PercentageBreaksCalculator>();
			calculator.Minimum = min;
			calculator.Maximum = max;
			calculator.BaseRate = flat;
			calculator.PerUnit = perUnit;

			foreach (var currentBreak in breaks)
			{
				calculator[currentBreak.Break] = (ZDecimal)currentBreak.Percentage;
				var rateLineItem = rateLine.RateLineItems[rateLine.RateLineItems.Count - 1];
				rateLineItem.TM_BreakMinimum = currentBreak.Percentage;
				rateLineItem.TM_FlatAmount = currentBreak.Flat;
			}

			foreach (var applyTo in applyToList)
			{
				var applyToRateLineItem = rateLine.RateLineItems.AddNew();
				applyToRateLineItem.TM_Type = CalculatorConstants.Type.ApplyTo;
				applyToRateLineItem.TM_Text = applyTo;
			}

			return rateLine;
		}

		protected override void TestQuotationLinesWMCore()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "AUD";
			Line.ConversionFactor = new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());

			Line.UseOnlyActualWeightMeasure = true;
			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(2, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[1].ToString());

			Line.RateLineItems.RemoveAndDeleteAll();

			TestCalculator.Minimum = 100m;
			TestCalculator["-5"] = (ZDecimal)60m;
			var minus5Item = Line.RateLineItems[Line.RateLineItems.Count - 1];
			TestCalculator["+5"] = (ZDecimal)55m;
			var plus5Item = Line.RateLineItems[Line.RateLineItems.Count - 1];
			TestCalculator["+10"] = (ZDecimal)52m;
			var plus10Item = Line.RateLineItems[Line.RateLineItems.Count - 1];
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(2, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[1].ToString());

			TestCalculator.Minimum = 100m;
			minus5Item.TM_BreakMinimum = 1.0;
			plus5Item.TM_BreakMinimum = 1.1;
			plus10Item.TM_BreakMinimum = 1.2;
			var percent1 = Line.RateLineItems.AddNew();
			percent1.TM_Type = CalculatorConstants.Type.ApplyTo;
			percent1.TM_Text = CalculatorConstants.Text.ChargeCode;
			percent1.TM_AC = Env.Registry.FreightChargeCode;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(8, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 5 W/M|||", quotationLines[1].ToString());
			AssertEquals("||1.00|% of Freight", quotationLines[2].ToString());
			AssertEquals("5 W/M to less than 10 W/M|||", quotationLines[3].ToString());
			AssertEquals("||1.10|% of Freight", quotationLines[4].ToString());
			AssertEquals("10 W/M and above|||", quotationLines[5].ToString());
			AssertEquals("||1.20|% of Freight", quotationLines[6].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[7].ToString());

			minus5Item.TM_FlatAmount = 10;
			plus5Item.TM_FlatAmount = 20;
			TestCalculator["+5"] = (ZDecimal)55m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(10, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 5 W/M|||", quotationLines[1].ToString());
			AssertEquals("||1.00|% of Freight", quotationLines[2].ToString());
			AssertEquals("Flat Amount|AUD|10.00|", quotationLines[3].ToString());
			AssertEquals("5 W/M to less than 10 W/M|||", quotationLines[4].ToString());
			AssertEquals("||1.10|% of Freight", quotationLines[5].ToString());
			AssertEquals("Flat Amount|AUD|20.00|", quotationLines[6].ToString());
			AssertEquals("10 W/M and above|||", quotationLines[7].ToString());
			AssertEquals("||1.20|% of Freight", quotationLines[8].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[9].ToString());

			var percent2 = Line.RateLineItems.AddNew();
			percent2.TM_Type = CalculatorConstants.Type.ApplyTo;
			percent2.TM_Text = Calculator.Items.Value.DisbursementApplyToTypes.Disbursements;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(13, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 5 W/M|||", quotationLines[1].ToString());
			AssertEquals("||1.00|% of Freight", quotationLines[2].ToString());
			AssertEquals("||1.00|% of disbursements", quotationLines[3].ToString());
			AssertEquals("Flat Amount|AUD|10.00|", quotationLines[4].ToString());
			AssertEquals("5 W/M to less than 10 W/M|||", quotationLines[5].ToString());
			AssertEquals("||1.10|% of Freight", quotationLines[6].ToString());
			AssertEquals("||1.10|% of disbursements", quotationLines[7].ToString());
			AssertEquals("Flat Amount|AUD|20.00|", quotationLines[8].ToString());
			AssertEquals("10 W/M and above|||", quotationLines[9].ToString());
			AssertEquals("||1.20|% of Freight", quotationLines[10].ToString());
			AssertEquals("||1.20|% of disbursements", quotationLines[11].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[12].ToString());

			TestCalculator.UseBreaksBasedOnValues = true;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(13, quotationLines.Count);
			AssertEquals("Test Rate - Breaks are based on values|||", quotationLines[0].ToString());
			AssertEquals("Less than 5 W/M|||", quotationLines[1].ToString());
			AssertEquals("||1.00|% of Freight", quotationLines[2].ToString());
			AssertEquals("||1.00|% of disbursements", quotationLines[3].ToString());
			AssertEquals("Flat Amount|AUD|10.00|", quotationLines[4].ToString());
			AssertEquals("5 W/M to less than 10 W/M|||", quotationLines[5].ToString());
			AssertEquals("||1.10|% of Freight", quotationLines[6].ToString());
			AssertEquals("||1.10|% of disbursements", quotationLines[7].ToString());
			AssertEquals("Flat Amount|AUD|20.00|", quotationLines[8].ToString());
			AssertEquals("10 W/M and above|||", quotationLines[9].ToString());
			AssertEquals("||1.20|% of Freight", quotationLines[10].ToString());
			AssertEquals("||1.20|% of disbursements", quotationLines[11].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[12].ToString());
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			Assert(true);
		}

		protected override CalculationResult AssertCalculation(AutoRatingCalculatorParameters @params, ZDecimal expectedAmount, ZString expectedDescription, string message = null)
		{
			var result = base.AssertCalculation(@params, expectedAmount, expectedDescription);
			@params.RatingContext.PercentageLinesApplied.Clear();

			if (AutoRateInfos != null)
			{
				ClearUsedByRateLines(AutoRateInfos);
			}

			return result;
		}

		public void TestCalculation()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			Line.TL_RX_NKCurrency = "AUD";
			Line.TL_WeightVolume = "KG";

			InitialiseTestCalculator();

			var applyToItem = TestCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			TestCalculator["-5"] = (ZDecimal)60m;
			var minus5Item = Line.RateLineItems[Line.RateLineItems.Count - 1];
			TestCalculator["+5"] = (ZDecimal)55m;
			var plus5Item = Line.RateLineItems[Line.RateLineItems.Count - 1];
			minus5Item.TM_BreakMinimum = 10m;
			plus5Item.TM_BreakMinimum = 20m;

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

			Criteria.WeightBreakOverride = 3m;
			Criteria.WeightBreakOverrideUnit = "AUD";
			AssertCalculation(parameters, 94.4m, "10.00% of (AUD 944.00 (Destination Charges TESTCC4))");

			Criteria.WeightBreakOverride = 6m;
			AssertCalculation(parameters, 188.8m, "20.00% of (AUD 944.00 (Destination Charges TESTCC4))");

			TestCalculator.Minimum = 100m;
			Criteria.WeightBreakOverride = 3m;
			applyToItem.TM_Text = CalculatorConstants.Text.FreightCharges;
			AssertCalculation(parameters, 453.4m, "10.00% of (AUD 4534.00 (Freight Charges TESTCC3))");

			applyToItem.TM_Text = CalculatorConstants.Text.DestinationCharges;
			AssertCalculation(parameters, 100m, "Minimum AUD 100.00");

			TestCalculator.Maximum = 300m;
			applyToItem.TM_Text = CalculatorConstants.Text.FreightCharges;
			AssertCalculation(parameters, 300.0m, "Maximum AUD 300.00");

			TestCalculator.Maximum = 0m;
			TestCalculator.BaseRate = 5m;
			applyToItem.TM_Text = CalculatorConstants.Text.FreightCharges;
			AssertCalculation(parameters, 458.4m, "Base Rate AUD 5.00 + 10.00% of (AUD 4534.00 (Freight Charges TESTCC3))");

			TestCalculator.Maximum = 500m;
			TestCalculator.BaseRate = 5m;
			applyToItem.TM_Text = CalculatorConstants.Text.FreightCharges;
			AssertCalculation(parameters, 458.4m, "Base Rate AUD 5.00 + 10.00% of (AUD 4534.00 (Freight Charges TESTCC3))");

			TestCalculator.Minimum = 0m;
			TestCalculator.Maximum = 0m;
			TestCalculator.BaseRate = 0m;
			applyToItem.TM_Text = CalculatorConstants.Text.ApplyTo.Value.ValueOfGoods;
			var monetaryValues = new MoneyType();
			monetaryValues.Add(MoneyType.ValueType.GoodsValue, new Money(11000m, Line.Currency));
			Criteria.MonetaryValues = monetaryValues;
			Criteria.WeightBreakOverride = 3m;
			minus5Item.TM_BreakMinimum = 2.5m;
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

			// Costing

			minus5Item.TM_BreakMinimum = 10m;
			Line.Parent.Parent = Factory.New<Costing>();
			applyToItem.TM_Text = CalculatorConstants.Text.ChargeCode;

			applyToItem.TM_AC = TestCC1.PK;
			AssertCalculation(parameters, 150m, "10.00% of (AUD 1500.00 (TESTCC1))");

			minus5Item.TM_FlatAmount = 100;
			AssertCalculation(parameters, 250m, "Base Rate AUD 100.00 + 10.00% of (AUD 1500.00 (TESTCC1))");

			applyToItem.TM_AC = TestCC2.PK;
			AssertCalculation(parameters, 664.30m, "Base Rate AUD 100.00 + 10.00% of (AUD 5643.00 (TESTCC2))");

			applyToItem.TM_AC = TestCC3.PK;
			AssertCalculation(parameters, 553.40, "Base Rate AUD 100.00 + 10.00% of (AUD 4534.00 (TESTCC3))");

			applyToItem.TM_AC = TestCC4.PK;
			AssertCalculation(parameters, 194.40m, "Base Rate AUD 100.00 + 10.00% of (AUD 944.00 (TESTCC4))");

			applyToItem.TM_Text = CalculatorConstants.Text.AllCharges;
			AssertCalculation(parameters, 1362.10m, "Base Rate AUD 100.00 + 10.00% of (AUD 12621.00 (All Charge Codes TESTCC4 944.00 + TESTCC1 1500.00 + TESTCC3 4534.00 + TESTCC2 5643.00))");

			Line.TL_RX_NKCurrency = "EUR";
			AssertCalculation(parameters, 731.05m, "Base Rate EUR 100.00 + 10.00% of (EUR 6310.50 (All Charge Codes TESTCC4 472.00 + TESTCC1 750.00 + TESTCC3 2267.00 + TESTCC2 2821.50))");

			applyToItem.TM_Text = CalculatorConstants.Text.ChargeCode;
			applyToItem.TM_AC = TestCC3.PK;
			AssertCalculation(parameters, 326.70m, "Base Rate EUR 100.00 + 10.00% of (EUR 2267.00 (TESTCC3))");

			Line.TL_RX_NKCurrency = "AUD";
			applyToItem.TM_Text = CalculatorConstants.Text.ChargeCode;
			applyToItem.TM_AC = TestCC1.PK;

			TestCalculator.UseBreaksBasedOnValues = true;
			AssertCalculation(parameters, 300m, "20.00% of (AUD 1500.00 (TESTCC1 - Breaks are based on values))");

			TestCalculator.UseBreaksBasedOnValues = false;
			AssertCalculation(parameters, 250m, "Base Rate AUD 100.00 + 10.00% of (AUD 1500.00 (TESTCC1))");
		}

		public override void TestCalculationWithCallForPricingFlag()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			Line.TL_RX_NKCurrency = "AUD";

			InitialiseTestCalculator();

			var applyToItem = TestCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			applyToItem.TM_AC = TestCC2.PK;

			TestCalculator["-5"] = (ZDecimal)60m;
			TestCalculator["+5"] = (ZDecimal)55m;
			TestCalculator.UseBreaksBasedOnValues = true;

			var plus5Item = Line.RateLineItems[Line.RateLineItems.Count - 1];
			plus5Item.TM_Text = "Call us for pricing";
			plus5Item.TM_CallForPricing = true;

			var calculatorParams = new AutoRatingCalculatorParametersForTesting(Criteria, AutoRateInfos);
			using (_Rating.Start(new LoggerDecorator()))
			{
				AssertExceptionThrown<AutoRater.CallForPriceException>(() => TestCalculator.Calculate(calculatorParams));
			}
		}

		public void TestPercentageBreaksCalculatorValidationOfDuplicateAllChargesOnDifferentLines()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();

			line.TL_AC = DSBCharge.PK;
			line.TL_RateDesc = "Goose";
			line.TL_RateCalculator = PercentageBreaksCalculator.Code;

			var applyToAllItem = line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.ApplyTo);
			if (applyToAllItem == null)
			{
				applyToAllItem = line.RateLineItems.AddNew();
				applyToAllItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			}

			applyToAllItem.TM_Text = CalculatorConstants.Text.AllCharges;

			line.RunPreSaveValidation();
			AssertEquals("No errors", false, applyToAllItem.TM_TextInfo.HasErrors());

			var line2 = entry.RateLines.AddNew();

			line2.TL_AC = REVCharge.PK;
			line2.TL_RateDesc = "Daphne";
			line2.TL_RateCalculator = PercentageBreaksCalculator.Code;

			var applyToAllItem2 = line2.RateLineItems.AddNew();
			applyToAllItem2.TM_Type = CalculatorConstants.Type.ApplyTo;
			applyToAllItem2.TM_Text = CalculatorConstants.Text.AllCharges;

			entry.RunPreSaveValidation();
			AssertEquals("Errors about % of ALL charge duplicated on the same chare code", true, line.HasErrors);
			AssertEquals("Errors about % of ALL charge duplicated on the same chare code", true, line2.HasErrors);

			applyToAllItem.Delete();
			line2.TL_WeightVolume = "KG";
			entry.RunPreSaveValidation();
			AssertEquals("No more errors", false, line2.HasErrors);
		}

		public void TestTwoPercentsOfDifferentItemsHasNoError()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("WHS");

			var line1 = entry.RateLines.AddNew();
			line1.TL_AC = DSBCharge.PK;
			line1.TL_RateCalculator = PercentageBreaksCalculator.Code;
			var applyToOriginItem = line1.RateLineItems.AddNew();
			applyToOriginItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			applyToOriginItem.TM_Text = CalculatorConstants.Text.OriginCharges;

			rate.RunPreSaveValidation();
			foreach (RateLineItem item in line1.RateLineItems)
			{
				AssertNoRowErrors("No errors", item);
			}

			var line2 = entry.RateLines.AddNew();
			line2.TL_AC = DSBCharge.PK;
			line2.TL_RateCalculator = PercentageBreaksCalculator.Code;
			var applyToDestItem = line2.RateLineItems.AddNew();
			applyToDestItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			applyToDestItem.TM_Text = CalculatorConstants.Text.DestinationCharges;

			rate.RunPreSaveValidation();
			foreach (RateLineItem item in line1.RateLineItems)
			{
				AssertNoRowErrors("No errors - as they are separate charges codes with a percent of different items", item);
			}

			foreach (RateLineItem item in line2.RateLineItems)
			{
				AssertNoRowErrors("No errors - as they are separate charges codes with a percent of different items", item);
			}
		}

		public void TestPercentageBreaksCalculatorValidationOfDisbursements()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();

			line.TL_AC = REVCharge.PK;
			line.TL_RateDesc = "DAPH MOO MOO NOLE JAMO";
			line.TL_RateCalculator = PercentageBreaksCalculator.Code;

			var applyToDisbItem = line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.ApplyTo);
			if (applyToDisbItem == null)
			{
				applyToDisbItem = line.RateLineItems.AddNew();
				applyToDisbItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			}
			applyToDisbItem.TM_Text = Calculator.Items.Value.DisbursementApplyToTypes.Disbursements;

			var applyToOrgItem = line.RateLineItems.AddNew();
			applyToOrgItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			applyToOrgItem.TM_Text = CalculatorConstants.Text.OriginCharges;

			line.RunPreSaveValidation();
			AssertEquals("Errors about % of disbursements and origin charges on the same charge code", true, line.HasErrors);

			applyToOrgItem.Delete();
			line.TL_WeightVolume = "KG";
			line.RunPreSaveValidation();
			AssertEquals("No errors", false, line.HasErrors);
		}

		public override void TestIsAccumulatedAndUseHigherChargeableLowerRateRuleWhenBreaksPerIsNotEmpty()
		{
			var newLine = Line.Parent.RateLines.AddNew();
			newLine.TL_AC = ChargeCode.PK;
			newLine.TL_RateDesc = "New Test Rate 1";
			newLine.TL_WeightVolume = QuantityUnit.KG;
			var testCalc = (PercentageBreaksCalculator)base.TestCalculator;

			Assert("BreaksPerInfo should be readonly", testCalc.BreaksPerInfo.ReadOnly);
		}

		public override void TestDefaultUseHigherRule()
		{
			RatingDataRegistry.Instance.UseHigherWeightOrUnitLowerRateRuleDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var testCal = (PercentageBreaksCalculator)base.TestCalculator;

			AssertEquals("Default Rule should not be taken from registry", false, testCal.UseHigherChargeableLowerRateRule);
		}

		#region Implementation

		AutoRateInfoCollection fAutoRateInfos;
		AutoRateInfoCollection AutoRateInfos
		{
			get
			{
				if (fAutoRateInfos == null)
				{
					fAutoRateInfos = new AutoRateInfoCollection(Factory);
					fAutoRateInfos.AddNew(TestCC1, "AUD", 1500M);
					fAutoRateInfos.AddNew(TestCC2, "AUD", 5643M);
					fAutoRateInfos.AddNew(TestCC3, "AUD", 4534M);
					fAutoRateInfos.AddNew(TestCC4, "AUD", 944M);
				}
				return fAutoRateInfos;
			}
		}

		#region Charge Codes

		AccChargeCode fTestCC1;
		AccChargeCode TestCC1
		{
			get { return fTestCC1 ?? (fTestCC1 = InsertChargeCode("TESTCC1", "Test CC1", CombinedCalculator.Code, ChargeCodeGroupList.Codes.Origin, Core.Constants.ChargeType.Disbursement)); }
		}

		AccChargeCode fTestCC2;
		AccChargeCode TestCC2
		{
			get { return fTestCC2 ?? (fTestCC2 = InsertChargeCode("TESTCC2", "Test CC2", CombinedCalculator.Code, ChargeCodeGroupList.Codes.Origin)); }
		}

		AccChargeCode fTestCC3;
		AccChargeCode TestCC3
		{
			get { return fTestCC3 ?? (fTestCC3 = InsertChargeCode("TESTCC3", "Test CC3", CombinedCalculator.Code, ChargeCodeGroupList.Codes.Freight, Core.Constants.ChargeType.Disbursement)); }
		}

		AccChargeCode fTestCC4;
		AccChargeCode TestCC4
		{
			get { return fTestCC4 ?? (fTestCC4 = InsertChargeCode("TESTCC4", "Test CC4", CombinedCalculator.Code, ChargeCodeGroupList.Codes.Destination)); }
		}

		AccChargeCode DSBCharge
		{
			get { return fDSBCharge ?? (fDSBCharge = InsertChargeCode("TESTDSB", "Test DSB", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, Core.Constants.ChargeType.Disbursement)); }
		}

		AccChargeCode fDSBCharge;

		AccChargeCode REVCharge
		{
			get { return fREVCharge ?? (fREVCharge = InsertChargeCode("TESTREV", "Test REV", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, Core.Constants.ChargeType.Revenue)); }
		}

		AccChargeCode fREVCharge;

		AccChargeCode InsertChargeCode(ZString code, ZString description, ZString calculatorCode, ZString chargeGroup, string chargeType = "")
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_Desc = description;
			chargeCode.AC_RateCalculator = calculatorCode;
			chargeCode.AC_ChargeGroup = chargeGroup;
			if (!string.IsNullOrEmpty(chargeType))
			{
				chargeCode.AC_ChargeType = chargeType;
			}

			return chargeCode;
		}

		#endregion

		protected override Type CalculatorType
		{
			get { return typeof(PercentageBreaksCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return PercentageBreaksCalculator.Code; }
		}

		new PercentageBreaksCalculator TestCalculator
		{
			get { return (PercentageBreaksCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
