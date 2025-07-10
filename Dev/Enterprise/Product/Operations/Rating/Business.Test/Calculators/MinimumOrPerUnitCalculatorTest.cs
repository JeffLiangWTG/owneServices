using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class MinimumOrPerUnitCalculatorTest : CalculatorTest
	{
		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN));
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT));

			InitialiseTestCalculator();

			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT));

			AssertEquals(2, Line.RateLineItems.Count);

			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN).TM_RelevantValueInfo, TestCalculator.Decimal1Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_RelevantValueInfo, TestCalculator.Decimal2Info);
		}

		public override void TestMapping()
		{
			TestMapping(Calculator.Items.Operator.MIN, "Decimal1");
			TestMapping(Calculator.Items.Operator.UNT, "Decimal2");
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddMinOrPerUnitRateLine(rateEntry, "FRT", "AUD", "KG", min: 10, perUnit: 100);
			var rateLine11 = AddMinOrPerUnitRateLine(rateEntry, "FRT", "AUD", "KG", min: 11, perUnit: 101);

			var rateLine20 = AddMinOrPerUnitRateLine(rateEntry, "FRT", "USD", "KG", min: 20, perUnit: 200);

			var rateLine30 = AddMinOrPerUnitRateLine(rateEntry, "FRT", "USD", "CTN", min: 30, perUnit: 301);
			var rateLine31 = AddMinOrPerUnitRateLine(rateEntry, "FRT", "USD", "CTN", min: 0, perUnit: 401);

			var rateLine41 = AddMinOrPerUnitRateLine(rateEntry, "FRT", "USD", "CTN", min: 50, perUnit: 0);

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount()
				+ rateLine30.Calculator.GetDocLineAmount()
				+ rateLine31.Calculator.GetDocLineAmount()
				+ rateLine41.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"Minimum|AUD|10.00|",
					"|AUD|201.00|per KG",
					"Minimum|USD|20.00|",
					"|USD|200.00|per KG",
					"|USD|702.00|per Carton"
				}
			);
		}

		static RateLine AddMinOrPerUnitRateLine(RateEntry rateEntry, ZString chargeCode, string currency, string unit, decimal min, decimal perUnit)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, MinimumOrPerUnitCalculator.Code, unit, currency);
			var calculator = rateLine.GetCalculator<MinimumOrPerUnitCalculator>();
			calculator.Minimum = min;
			calculator.PerUnit = perUnit;

			return rateLine;
		}

		public override void TestQuotationLines()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "", "");

			Line.Parent.TI_Mode = Core.Constants.RateMode.AIR;
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";
			Line.ConversionFactor = new ConversionFactor(333m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.Minimum = 120m;
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
			AssertEquals("Minimum|USD|120.00|", quotationLines[1].ToString());
			AssertEquals("Per Unit|USD|10.00|per KG (1 M3 = 333 KG)", quotationLines[2].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("Minimum|USD|120.00|", quotationLines[1].ToString());
			AssertEquals("Per Unit|USD|10.00|per KG (1 M3 = 333 KG)", quotationLines[2].ToString());

			TestCalculator.Minimum = 0m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|10.00|per KG (1 M3 = 333 KG)", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Charge|USD|10.00|per KG (1 M3 = 333 KG)", quotationLines[0].ToString());
		}

		protected override void TestQuotationLinesWMCore()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";
			Line.ConversionFactor = new ConversionFactor(333m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.Minimum = 120m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|120.00|", quotationLines[0].ToString());

			TestCalculator.PerUnit = 10m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|USD|120.00|", quotationLines[1].ToString());
			AssertEquals("Per Unit|USD|10.00|per W/M", quotationLines[2].ToString());

			TestCalculator.Minimum = 0m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|10.00|per W/M", quotationLines[0].ToString());
		}

		public void TestCalculation()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";
			TestCalculator.Minimum = 50m;
			TestCalculator.PerUnit = 5m;
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(1M, QuantityUnit.KG);
			AssertCalculation(parameters, 50m, "Minimum AUD 50.00");

			parameters.ChargeableAmount = new Quantity(15M, QuantityUnit.KG);
			Line.TL_RX_NKCurrency = "USD";
			AssertCalculation(parameters, 75m, "15 Kilogram(s) @ USD 5.00/KG");

			TestCalculator.Minimum = 100m;
			AssertCalculation(parameters, 100m, "Minimum USD 100.00");

			TestCalculator.Minimum = 0m;
			parameters.ChargeableAmount = new Quantity(100M, QuantityUnit.KG);
			AssertCalculation(parameters, 500m, "100 Kilogram(s) @ USD 5.00/KG");
		}

		public void TestCalculationDescription_TeaChest()
		{
			Line.TL_WeightVolume = Core.Constants.Volume.TeaChest;
			Line.TL_RX_NKCurrency = "USD";
			TestCalculator.PerUnit = 5m;

			// Note the volume amount here is not used for TeaChest, the amount comes from the PackageInformation
			Criteria.RateableMeasures.SetQuantity(MeasureType.Volume, 30m, QuantityUnit.M3);

			var bookGuid = ZGuid.NewZGuid();
			var bikeGuid = ZGuid.NewZGuid();
			Criteria.PackageInformation = new List<PackageInformation>();
			Criteria.PackageInformation.Add(new PackageInformation(bookGuid.ToGuid(), "Book Box", 3, 2.1m, Core.Constants.Volume.CubicMetres, ""));
			Criteria.PackageInformation.Add(new PackageInformation(bikeGuid.ToGuid(), "Bike", 1, 4.3m, Core.Constants.Volume.CubicMetres, ""));

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);

			AssertCalculation(parameters, 250.88m, "50.176008 Tea Chest(s) - 3 Book Box (16.464003 Tea Chest volume) + 1 Bike (33.712005 Tea Chest volume) @ USD 5.00/TE");

			TestCalculator.Minimum = 300m;
			AssertCalculation(parameters, 300m, "Minimum USD 300.00");
		}

		public void TestCalculationDescription_TeaChest_Minimum()
		{
			Line.TL_WeightVolume = Core.Constants.Volume.TeaChest;
			Line.TL_RX_NKCurrency = "USD";
			TestCalculator.PerUnit = 5m;
			TestCalculator.Minimum = 300m;

			// Note the volume amount here is not used for TeaChest, the amount comes from the PackageInformation
			Criteria.RateableMeasures.SetQuantity(MeasureType.Volume, 30m, QuantityUnit.M3);

			var bookGuid = ZGuid.NewZGuid();
			var bikeGuid = ZGuid.NewZGuid();
			Criteria.PackageInformation = new List<PackageInformation>();
			Criteria.PackageInformation.Add(new PackageInformation(bookGuid.ToGuid(), "Book Box", 3, 2.1m, Core.Constants.Volume.CubicMetres, ""));
			Criteria.PackageInformation.Add(new PackageInformation(bikeGuid.ToGuid(), "Bike", 1, 4.3m, Core.Constants.Volume.CubicMetres, ""));

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);

			AssertCalculation(parameters, 300m, "Minimum USD 300.00");
		}

		public override void TestGetCloneCode()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var line = rateEntry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;

			source.BaseRate = 10m;
			AssertEquals(CombinedCalculator.Code, TestCalculator.GetCloneCode(source));
			source.BaseRate = 0m;
			AssertEquals(CalculatorCode, TestCalculator.GetCloneCode(source));
			source["+45"] = (ZDecimal)5m;
			AssertEquals(CombinedCalculator.Code, TestCalculator.GetCloneCode(source));
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.Minimum = 50m;
			TestCalculator.PerUnit = 5m;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;

			source.Minimum = 100m;
			source.PerUnit = 6m;
			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(150m, clonedLine.GetCalculator<MinimumOrPerUnitCalculator>().Minimum);
			AssertEquals(11m, clonedLine.GetCalculator<MinimumOrPerUnitCalculator>().PerUnit);

			source.Minimum = 0m;
			source.BaseRate = 80m;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(50m, clonedLine.GetCalculator<CombinedCalculator>().Minimum);
			AssertEquals(11m, clonedLine.GetCalculator<CombinedCalculator>().PerUnit);
			AssertEquals(80m, clonedLine.GetCalculator<CombinedCalculator>().BaseRate);

			source.PerUnit = 0m;
			source.PerUnitPercent = 25m;
			source.Percent = 25m;
			source.PerUnitPercent = 25m;
			source.Minimum = 10m;
			source.CalculationOrder = Calculator.Items.PercentFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals("50 * 1.25 + 10", 72.5m, clonedLine.GetCalculator<CombinedCalculator>().Minimum);
			AssertEquals(6.25m, clonedLine.GetCalculator<CombinedCalculator>().PerUnit);
			AssertEquals(80m, clonedLine.GetCalculator<CombinedCalculator>().BaseRate);

			source.CalculationOrder = Calculator.Items.IncreaseFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals("(50 + 10) * 1.25", 75m, clonedLine.GetCalculator<CombinedCalculator>().Minimum);
			AssertEquals(6.25m, clonedLine.GetCalculator<CombinedCalculator>().PerUnit);
			AssertEquals(100m, clonedLine.GetCalculator<CombinedCalculator>().BaseRate);

			source.Percent = 0m;
			source.PerUnitPercent = 0m;
			source.BaseRate = 15m;
			source["-45"] = (ZDecimal)4m;
			source["+45"] = (ZDecimal)3m;
			source["+100"] = (ZDecimal)2m;
			source["+250"] = (ZDecimal)1m;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals("50 + 10", 60m, clonedLine.GetCalculator<CombinedCalculator>().Minimum);
			AssertEquals(15m, clonedLine.GetCalculator<CombinedCalculator>().BaseRate);
			AssertEquals(9m, clonedLine.Calculator["-45"]);
			AssertEquals(8m, clonedLine.Calculator["+45"]);
			AssertEquals(7m, clonedLine.Calculator["+100"]);
			AssertEquals(6m, clonedLine.Calculator["+250"]);
		}

		public override void TestGetCostsComparerChargesSummary()
		{
			TestCalculator.Minimum = 100m;
			TestCalculator.PerUnit = 5m;
			var items = TestCalculator.GetCostsComparerChargesSummary(new List<RateLine>());
			AssertEquals(2, items.Count);
			AssertChargesSummaryItem(items[0], "MIN", 100m);
			AssertChargesSummaryItem(items[1], "UNT", 5m);
		}

		public override void TestPricePerSingleChargeable()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateLine = costing.AddRateEntry("AIR", "LSE", "AUSYD", "NZAKL").AddRateLine("FRT", MinimumOrPerUnitCalculator.Code, "KG", "AUD");
			rateLine.GetCalculator<MinimumOrPerUnitCalculator>().Minimum = 100m;
			rateLine.GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 5m;

			AssertEquals("AUD 100.0000", rateLine.ParentRateEntry.AllInCost());
			AssertEquals("AUD 5.0000/KG", rateLine.ParentRateEntry.FreightRatePerChargeableUnit());
		}

		public void TestGetAllInCosts()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "NZAKL");
			var rateLine1 = rateEntry.AddRateLine("FRT", MinimumOrPerUnitCalculator.Code, "KG", "AUD");
			rateLine1.GetCalculator<MinimumOrPerUnitCalculator>().Minimum = 100m;
			rateLine1.GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 5m;

			var rateLine2 = rateEntry.AddRateLine("BAF", MinimumOrPerUnitCalculator.Code, "KG", "USD");
			rateLine2.GetCalculator<MinimumOrPerUnitCalculator>().Minimum = 80m;
			rateLine2.GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 5m;

			AssertEquals("AUD 100.0000 + USD 80.0000", rateEntry.AllInCost());
			AssertEquals("AUD 5.0000/KG", rateEntry.FreightRatePerChargeableUnit());
		}

		#region Implementation

		protected override Type CalculatorType
		{
			get { return typeof(MinimumOrPerUnitCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return MinimumOrPerUnitCalculator.Code; }
		}

		new MinimumOrPerUnitCalculator TestCalculator
		{
			get { return (MinimumOrPerUnitCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
