using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class PackageCountCalculatorTest : CalculatorTest
	{
		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS));
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT));
			AssertNull(Line.RateLineItems.FindByTM_Type(PackageCountCalculator.Items.FirstPackageRate));
			AssertNull(Line.RateLineItems.FindByTM_Type(PackageCountCalculator.Items.AddtionalPackageRate));

			InitialiseTestCalculator();

			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(PackageCountCalculator.Items.FirstPackageRate));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(PackageCountCalculator.Items.AddtionalPackageRate));

			AssertEquals(4, Line.RateLineItems.Count);

			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_RelevantValueInfo, TestCalculator.Decimal1Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_RelevantValueInfo, TestCalculator.Decimal2Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(PackageCountCalculator.Items.FirstPackageRate).TM_RelevantValueInfo, TestCalculator.Decimal3Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(PackageCountCalculator.Items.AddtionalPackageRate).TM_RelevantValueInfo, TestCalculator.Decimal4Info);
		}

		public override void TestMapping()
		{
			TestMapping(Calculator.Items.Operator.BAS, "Decimal1");
			TestMapping(Calculator.Items.Operator.UNT, "Decimal2");
			TestMapping(PackageCountCalculator.Items.FirstPackageRate, "Decimal3");
			TestMapping(PackageCountCalculator.Items.AddtionalPackageRate, "Decimal4");
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddPackageCountRateLine(rateEntry, "FRT", "AUD", flat: 100, perUnit: 101, firstPackage: 102, additionalPackage: 103);
			var rateLine11 = AddPackageCountRateLine(rateEntry, "FRT", "AUD", flat: 110, perUnit: 111, firstPackage: 112, additionalPackage: 113);

			var rateLine20 = AddPackageCountRateLine(rateEntry, "FRT", "USD", flat: 200, perUnit: 201, firstPackage: 202, additionalPackage: 203);

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"|AUD|210.00|",
					"|AUD|212.00|KG",
					"First Package|AUD|214.00|",
					"Additional|AUD|216.00|Package",

					"|USD|200.00|",
					"|USD|201.00|KG",
					"First Package|USD|202.00|",
					"Additional|USD|203.00|Package"
				}
			);
		}

		static RateLine AddPackageCountRateLine(RateEntry rateEntry, ZString chargeCode, string currency, decimal flat, decimal perUnit, decimal firstPackage, decimal additionalPackage)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, PackageCountCalculator.Code, currencyCode: currency);
			var calculator = rateLine.GetCalculator<PackageCountCalculator>();
			calculator.BaseRate = flat;
			calculator.PerKG = perUnit;
			calculator.FirstPackageRate = firstPackage;
			calculator.AddtionalPackageRate = additionalPackage;

			return rateLine;
		}

		public override void TestQuotationLines()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			Line.TL_WeightVolume = "KG";
			Line.TL_WeightVolumeMultiple = 100;
			Line.TL_RX_NKCurrency = "EUR";

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("First Package||Not Charged|", quotationLines[1].ToString());
			AssertEquals("Additional Packages||Not Charged|", quotationLines[2].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("First Package||Not Charged|", quotationLines[1].ToString());
			AssertEquals("Additional Packages||Not Charged|", quotationLines[2].ToString());

			TestCalculator.BaseRate = 90.9m;
			TestCalculator.PerKG = 0.95m;
			TestCalculator.FirstPackageRate = 3m;
			TestCalculator.AddtionalPackageRate = 2m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Base Rate|EUR|90.90|", quotationLines[1].ToString());
			AssertEquals("Flat Rate|EUR|0.95|per KG", quotationLines[2].ToString());
			AssertEquals("First Package|EUR|3.00|", quotationLines[3].ToString());
			AssertEquals("Additional Packages|EUR|2.00|per Package", quotationLines[4].ToString());
		}

		public void TestCalculation()
		{
			TestCalculator.BaseRate = 3.87m;
			TestCalculator.PerKG = 0.02m;
			TestCalculator.FirstPackageRate = 0.51m;
			TestCalculator.AddtionalPackageRate = 0.25m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(20, QuantityUnit.KG);
			Criteria.SetMeasure(10m, MeasureType.Package);
			AssertCalculation(parameters, 7.03m, "Base Rate AUD 3.87 + 20 Kilogram(s) @ AUD 0.02/KG + 1 Package @ AUD 0.51/package (for 1st package) + 9 additional package(s) @ AUD 0.25/package");

			parameters.ChargeableAmount = new Quantity(150, QuantityUnit.KG);
			Criteria.SetMeasure(30m, MeasureType.Package);
			AssertCalculation(parameters, 14.63m, "Base Rate AUD 3.87 + 150 Kilogram(s) @ AUD 0.02/KG + 1 Package @ AUD 0.51/package (for 1st package) + 29 additional package(s) @ AUD 0.25/package");

			parameters.ChargeableAmount = new Quantity(1253, QuantityUnit.KG);
			Criteria.SetMeasure(125m, MeasureType.Package);
			AssertCalculation(parameters, 60.44m, "Base Rate AUD 3.87 + 1253 Kilogram(s) @ AUD 0.02/KG + 1 Package @ AUD 0.51/package (for 1st package) + 124 additional package(s) @ AUD 0.25/package");
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.Line.ViewAgentRates = false;
			TestCalculator.BaseRate = 60m;
			TestCalculator.PerKG = 5m;
			TestCalculator.FirstPackageRate = 25m;
			TestCalculator.AddtionalPackageRate = 20m;

			TestCalculator.Line.ViewAgentRates = true;
			TestCalculator.BaseRate = 65m;
			TestCalculator.PerKG = 10m;
			TestCalculator.FirstPackageRate = 30m;
			TestCalculator.AddtionalPackageRate = 25m;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;

			source.PerUnit = 6m;
			source.BaseRate = 80m;
			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.GetCalculator<PackageCountCalculator>().Line.ViewAgentRates = false;
			AssertEquals(140m, clonedLine.GetCalculator<PackageCountCalculator>().BaseRate);
			AssertEquals(11m, clonedLine.GetCalculator<PackageCountCalculator>().PerKG);
			AssertEquals(25m, clonedLine.GetCalculator<PackageCountCalculator>().FirstPackageRate);
			AssertEquals(20m, clonedLine.GetCalculator<PackageCountCalculator>().AddtionalPackageRate);
			clonedLine.GetCalculator<PackageCountCalculator>().Line.ViewAgentRates = true;
			AssertEquals(145m, clonedLine.GetCalculator<PackageCountCalculator>().BaseRate);
			AssertEquals(16m, clonedLine.GetCalculator<PackageCountCalculator>().PerKG);
			AssertEquals(30m, clonedLine.GetCalculator<PackageCountCalculator>().FirstPackageRate);
			AssertEquals(25m, clonedLine.GetCalculator<PackageCountCalculator>().AddtionalPackageRate);

			source.PerUnit = 0m;
			source.Percent = 20m;
			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.PercentFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.GetCalculator<PackageCountCalculator>().Line.ViewAgentRates = false;
			AssertEquals(152m, clonedLine.GetCalculator<PackageCountCalculator>().BaseRate);
			AssertEquals(6m, clonedLine.GetCalculator<PackageCountCalculator>().PerKG);
			AssertEquals(30m, clonedLine.GetCalculator<PackageCountCalculator>().FirstPackageRate);
			AssertEquals(24m, clonedLine.GetCalculator<PackageCountCalculator>().AddtionalPackageRate);
			clonedLine.GetCalculator<PackageCountCalculator>().Line.ViewAgentRates = true;
			AssertEquals(158m, clonedLine.GetCalculator<PackageCountCalculator>().BaseRate);
			AssertEquals(12m, clonedLine.GetCalculator<PackageCountCalculator>().PerKG);
			AssertEquals(36m, clonedLine.GetCalculator<PackageCountCalculator>().FirstPackageRate);
			AssertEquals(30m, clonedLine.GetCalculator<PackageCountCalculator>().AddtionalPackageRate);

			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.GetCalculator<PackageCountCalculator>().Line.ViewAgentRates = false;
			AssertEquals(168m, clonedLine.GetCalculator<PackageCountCalculator>().BaseRate);
			AssertEquals(6m, clonedLine.GetCalculator<PackageCountCalculator>().PerKG);
			AssertEquals(30m, clonedLine.GetCalculator<PackageCountCalculator>().FirstPackageRate);
			AssertEquals(24m, clonedLine.GetCalculator<PackageCountCalculator>().AddtionalPackageRate);
			clonedLine.GetCalculator<PackageCountCalculator>().Line.ViewAgentRates = true;
			AssertEquals(174m, clonedLine.GetCalculator<PackageCountCalculator>().BaseRate);
			AssertEquals(12m, clonedLine.GetCalculator<PackageCountCalculator>().PerKG);
			AssertEquals(36m, clonedLine.GetCalculator<PackageCountCalculator>().FirstPackageRate);
			AssertEquals(30m, clonedLine.GetCalculator<PackageCountCalculator>().AddtionalPackageRate);

			source.Percent = 0m;
			source["-45"] = (ZDecimal)4m;
			source["+45"] = (ZDecimal)3m;
			source["+100"] = (ZDecimal)2m;
			source["+250"] = (ZDecimal)1m;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.GetCalculator<PackageCountCalculator>().Line.ViewAgentRates = false;
			AssertEquals(140m, clonedLine.GetCalculator<PackageCountCalculator>().BaseRate);
			AssertEquals(5m, clonedLine.GetCalculator<PackageCountCalculator>().PerKG);
			AssertEquals(25m, clonedLine.GetCalculator<PackageCountCalculator>().FirstPackageRate);
			AssertEquals(20m, clonedLine.GetCalculator<PackageCountCalculator>().AddtionalPackageRate);
			clonedLine.GetCalculator<PackageCountCalculator>().Line.ViewAgentRates = true;
			AssertEquals(145m, clonedLine.GetCalculator<PackageCountCalculator>().BaseRate);
			AssertEquals(10m, clonedLine.GetCalculator<PackageCountCalculator>().PerKG);
			AssertEquals(30m, clonedLine.GetCalculator<PackageCountCalculator>().FirstPackageRate);
			AssertEquals(25m, clonedLine.GetCalculator<PackageCountCalculator>().AddtionalPackageRate);
		}

		#region Implementation

		protected override Type CalculatorType
		{
			get { return typeof(PackageCountCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return PackageCountCalculator.Code; }
		}

		new PackageCountCalculator TestCalculator
		{
			get { return (PackageCountCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
