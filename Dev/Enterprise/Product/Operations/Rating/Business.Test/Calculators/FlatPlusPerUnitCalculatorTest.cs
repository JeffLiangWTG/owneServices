using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class FlatPlusPerUnitCalculatorTest : CalculatorTest
	{
		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS));
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT));

			InitialiseTestCalculator();

			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT));

			AssertEquals(2, Line.RateLineItems.Count);

			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_RelevantValueInfo, TestCalculator.Decimal1Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_RelevantValueInfo, TestCalculator.Decimal2Info);
		}

		public override void TestMapping()
		{
			TestMapping(Calculator.Items.Operator.BAS, "Decimal1");
			TestMapping(Calculator.Items.Operator.UNT, "Decimal2");
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddFirstPlusAdditionalRateLine(rateEntry, "FRT", "AUD", flat: 10, perUnit: 100, unit: "KG");
			var rateLine11 = AddFirstPlusAdditionalRateLine(rateEntry, "FRT", "AUD", flat: 11, perUnit: 110, unit: "KG");
			var rateLine12 = AddFirstPlusAdditionalRateLine(rateEntry, "FRT", "AUD", flat: 12, perUnit: 120, unit: "M3");

			var rateLine20 = AddFirstPlusAdditionalRateLine(rateEntry, "FRT", "USD", flat: 20, perUnit: 200, unit: "KG");

			var rateLine30 = AddFirstPlusAdditionalRateLine(rateEntry, "FRT", "AUD", flat: 30, perUnit: 300, unit: "CTN");
			var rateLine31 = AddFirstPlusAdditionalRateLine(rateEntry, "FRT", "AUD", flat: 0, perUnit: 301, unit: "CTN");
			var rateLine32 = AddFirstPlusAdditionalRateLine(rateEntry, "FRT", "AUD", flat: 32, perUnit: 302, unit: "CTN");

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine12.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount()
				+ rateLine30.Calculator.GetDocLineAmount()
				+ rateLine31.Calculator.GetDocLineAmount()
				+ rateLine32.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"|AUD|210.00|per KG",
					"|AUD|95.00|",
					"|AUD|120.00|per M3",
					"|USD|200.00|per KG",
					"|USD|20.00|",
					"|AUD|903.00|per Carton"
				}
			);
		}

		static RateLine AddFirstPlusAdditionalRateLine(RateEntry rateEntry, ZString chargeCode, string currency, decimal flat, decimal perUnit, string unit)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, FlatPlusPerUnitCalculator.Code, unit, currency);
			var calculator = rateLine.GetCalculator<FlatPlusPerUnitCalculator>();
			calculator.BaseRate = flat;
			calculator.PerUnit = perUnit;

			return rateLine;
		}

		public override void TestQuotationLines()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var parentEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR);

			Line.Parent.TI_Mode = Core.Constants.RateMode.AIR;
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";
			Line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.BaseRate = 120m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|120.00|", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Charge|USD|120.00|", quotationLines[0].ToString());

			TestCalculator.PerUnit = 10m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Base Rate|USD|120.00|", quotationLines[1].ToString());
			AssertEquals("Per Unit|USD|10.00|per KG (1 M3 = 250 KG)", quotationLines[2].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("Base Rate|USD|120.00|", quotationLines[1].ToString());
			AssertEquals("Per Unit|USD|10.00|per KG (1 M3 = 250 KG)", quotationLines[2].ToString());

			TestCalculator.BaseRate = 0m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|10.00|per KG (1 M3 = 250 KG)", quotationLines[0].ToString());
		}

		protected override void TestQuotationLinesWMCore()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var parentEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL);

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";
			Line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.BaseRate = 120m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|120.00|", quotationLines[0].ToString());

			TestCalculator.PerUnit = 10m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Base Rate|USD|120.00|", quotationLines[1].ToString());
			AssertEquals("Per Unit|USD|10.00|per W/M", quotationLines[2].ToString());

			TestCalculator.BaseRate = 0m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|10.00|per W/M", quotationLines[0].ToString());
		}

		public void TestCalculation()
		{
			Line.TL_WeightVolume = QuantityUnit.KG;
			Line.TL_RX_NKCurrency = "AUD";
			TestCalculator.BaseRate = 50m;
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			AssertCalculation(parameters, 50m, "Base Rate AUD 50.00");

			TestCalculator.PerUnit = 5m;
			parameters.ChargeableAmount = new Quantity(15M, QuantityUnit.KG);
			Line.TL_RX_NKCurrency = "USD";
			AssertCalculation(parameters, 125m, "Base Rate USD 50.00 + 15 Kilogram(s) @ USD 5.00/KG");

			TestCalculator.BaseRate = 0m;
			parameters.ChargeableAmount = new Quantity(100M, QuantityUnit.KG);
			AssertCalculation(parameters, 500m, "100 Kilogram(s) @ USD 5.00/KG");
		}

		public override void TestGetCloneCode()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var line = rateEntry.RateLines.AddNew();

			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;

			source.BaseRate = 10m;
			AssertEquals(CalculatorCode, TestCalculator.GetCloneCode(source));
			source.BaseRate = 0m;
			AssertEquals(CalculatorCode, TestCalculator.GetCloneCode(source));
			source["+45"] = (ZDecimal)5m;
			AssertEquals(CombinedCalculator.Code, TestCalculator.GetCloneCode(source));
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.BaseRate = 25m;
			TestCalculator.PerUnit = 5m;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;
			source.PerUnit = 6m;
			source.BaseRate = 80m;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(11m, clonedLine.GetCalculator<FlatPlusPerUnitCalculator>().PerUnit);
			AssertEquals(105m, clonedLine.GetCalculator<FlatPlusPerUnitCalculator>().BaseRate);

			source.PerUnit = 0m;
			source.PerUnitPercent = 20m;
			source.Percent = 20m;
			source.PerUnitPercent = 20m;
			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.PercentFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(6m, clonedLine.GetCalculator<FlatPlusPerUnitCalculator>().PerUnit);
			AssertEquals(110m, clonedLine.GetCalculator<FlatPlusPerUnitCalculator>().BaseRate);

			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(6m, clonedLine.GetCalculator<FlatPlusPerUnitCalculator>().PerUnit);
			AssertEquals(126m, clonedLine.GetCalculator<FlatPlusPerUnitCalculator>().BaseRate);

			source.Percent = 0m;
			source.PerUnitPercent = 0m;
			source["-45"] = (ZDecimal)4m;
			source["+45"] = (ZDecimal)3m;
			source["+100"] = (ZDecimal)2m;
			source["+250"] = (ZDecimal)1m;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(105m, clonedLine.GetCalculator<CombinedCalculator>().BaseRate);
			AssertEquals(9m, clonedLine.Calculator["-45"]);
			AssertEquals(8m, clonedLine.Calculator["+45"]);
			AssertEquals(7m, clonedLine.Calculator["+100"]);
			AssertEquals(6m, clonedLine.Calculator["+250"]);
		}

		public override void TestGetCostsComparerChargesSummary()
		{
			TestCalculator.BaseRate = 50m;
			TestCalculator.PerUnit = 5m;
			var items = TestCalculator.GetCostsComparerChargesSummary(new List<RateLine>());
			AssertEquals(2, items.Count);
			AssertChargesSummaryItem(items[0], "BAS", 50m);
			AssertChargesSummaryItem(items[1], "UNT", 5m);
		}

		public override void TestPricePerSingleChargeable()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateLine = costing.AddRateEntry("AIR", "LSE", "AUSYD", "NZAKL").AddRateLine("FRT", FlatPlusPerUnitCalculator.Code, "KG", "AUD");
			rateLine.GetCalculator<FlatPlusPerUnitCalculator>().BaseRate = 50m;
			rateLine.GetCalculator<FlatPlusPerUnitCalculator>().PerUnit = 5m;

			AssertEquals("AUD 55.0000", rateLine.ParentRateEntry.AllInCost());
			AssertEquals("AUD 5.0000/KG", rateLine.ParentRateEntry.FreightRatePerChargeableUnit());
		}

		public void TestCalculate_EntryHasNoContainerAndTheRateIsPerContainer_DontPopulateContainerTypeChargeAttribute()
		{
			Line.Parent.TI_RC = ZGuid.Empty;
			Line.TL_WeightVolume = QuantityUnit.CN;
			Line.TL_RX_NKCurrency = "AUD";
			TestCalculator.PerUnit = 100m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(2M, QuantityUnit.CN);

			var result = AssertCalculation(parameters, 200m, "2 Container(s) @ AUD 100.00/Container");

			AssertCollectionNotContains(
				"ContainerCode should not be in the result attributes",
				JobChargeAttribTypeList.Codes.ContainerCode,
				result.Attributes.Attributes.Select(a => a.Code)
			);
		}

		#region Implementation

		protected override Type CalculatorType
		{
			get { return typeof(FlatPlusPerUnitCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return FlatPlusPerUnitCalculator.Code; }
		}

		new FlatPlusPerUnitCalculator TestCalculator
		{
			get { return (FlatPlusPerUnitCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
