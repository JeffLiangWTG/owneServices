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
	internal sealed class FirstPlusAdditionalCalculatorTest : CalculatorTest
	{
		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(FirstPlusAdditionalCalculator.Items.FST));
			AssertNull(Line.RateLineItems.FindByTM_Type(FirstPlusAdditionalCalculator.Items.ADD));

			InitialiseTestCalculator();

			AssertNotNull(Line.RateLineItems.FindByTM_Type(FirstPlusAdditionalCalculator.Items.FST));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(FirstPlusAdditionalCalculator.Items.ADD));

			AssertEquals(2, Line.RateLineItems.Count);

			AssertEquals(Line.RateLineItems.FindByTM_Type(FirstPlusAdditionalCalculator.Items.FST).TM_RelevantValueInfo, TestCalculator.Decimal1Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(FirstPlusAdditionalCalculator.Items.ADD).TM_RelevantValueInfo, TestCalculator.Decimal2Info);
		}

		public override void TestMapping()
		{
			TestMapping(FirstPlusAdditionalCalculator.Items.FST, "Decimal1");
			TestMapping(FirstPlusAdditionalCalculator.Items.ADD, "Decimal2");
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddFirstPlusAdditionalRateLine(rateEntry, "FRT", "AUD", "KG", first: 100, additional: 101);
			var rateLine11 = AddFirstPlusAdditionalRateLine(rateEntry, "FRT", "AUD", "KG", first: 110, additional: 111);
			var rateLine12 = AddFirstPlusAdditionalRateLine(rateEntry, "FRT", "AUD", "M3", first: 120, additional: 121);

			var rateLine20 = AddFirstPlusAdditionalRateLine(rateEntry, "FRT", "USD", "KG", first: 200, additional: 201);

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine12.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"First KG|AUD|210.00|",
					"Additional|AUD|212.00|KG",
					"First M3|AUD|120.00|",
					"Additional|AUD|121.00|M3",
					"First KG|USD|200.00|",
					"Additional|USD|201.00|KG"
				}
			);
		}

		static RateLine AddFirstPlusAdditionalRateLine(RateEntry rateEntry, ZString chargeCode, string currency, string unit, decimal first, decimal additional)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, FirstPlusAdditionalCalculator.Code, unit, currency);
			var calculator = rateLine.GetCalculator<FirstPlusAdditionalCalculator>();
			calculator.First = first;
			calculator.Additional = additional;

			return rateLine;
		}

		public override void TestQuotationLines()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.AIR);

			Line.Parent.TI_Mode = Core.Constants.RateMode.AIR;
			Line.TL_WeightVolume = "CN";
			Line.TL_RX_NKCurrency = "EUR";

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("First Container||Not Charged|", quotationLines[1].ToString());
			AssertEquals("Thereafter||Not Charged|", quotationLines[2].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("First Container||Not Charged|", quotationLines[1].ToString());
			AssertEquals("Thereafter||Not Charged|", quotationLines[2].ToString());

			TestCalculator.First = 15m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("First Container|EUR|15.00|", quotationLines[1].ToString());
			AssertEquals("Thereafter||Not Charged|", quotationLines[2].ToString());

			TestCalculator.Additional = 10m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("First Container|EUR|15.00|", quotationLines[1].ToString());
			AssertEquals("Thereafter|EUR|10.00|per Container", quotationLines[2].ToString());

			TestCalculator.First = 0m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("First Container||Not Charged|", quotationLines[1].ToString());
			AssertEquals("Thereafter|EUR|10.00|per Container", quotationLines[2].ToString());

			Line.TL_WeightVolume = "KG";
			Line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("First KG (1 M3 = 250 KG)||Not Charged|", quotationLines[1].ToString());
			AssertEquals("Thereafter|EUR|10.00|per KG", quotationLines[2].ToString());
		}

		protected override void TestQuotationLinesWMCore()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			Line.TL_WeightVolume = "CN";
			Line.TL_RX_NKCurrency = "EUR";

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("First Container||Not Charged|", quotationLines[1].ToString());
			AssertEquals("Thereafter||Not Charged|", quotationLines[2].ToString());

			TestCalculator.First = 15m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("First Container|EUR|15.00|", quotationLines[1].ToString());
			AssertEquals("Thereafter||Not Charged|", quotationLines[2].ToString());

			TestCalculator.Additional = 10m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("First Container|EUR|15.00|", quotationLines[1].ToString());
			AssertEquals("Thereafter|EUR|10.00|per Container", quotationLines[2].ToString());

			TestCalculator.First = 0m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("First Container||Not Charged|", quotationLines[1].ToString());
			AssertEquals("Thereafter|EUR|10.00|per Container", quotationLines[2].ToString());

			Line.TL_WeightVolume = "KG";
			Line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("First W/M||Not Charged|", quotationLines[1].ToString());
			AssertEquals("Thereafter|EUR|10.00|per W/M", quotationLines[2].ToString());
		}

		public void TestCalculation()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);

			TestCalculator.First = 14;
			TestCalculator.Additional = 6;
			var (results, error) = TestCalculator.Calculate(parameters);
			AssertEquals("The results should be empty.", 0, results.Count());
			AssertNullOrEmpty(nameof(error), error);

			parameters.ChargeableAmount = new Quantity(1, QuantityUnit.KG);
			AssertCalculation(parameters, 14m, "1 Kilogram(s) @ AUD 14.00/KG");

			parameters.ChargeableAmount = new Quantity(0.5m, QuantityUnit.KG);
			AssertCalculation(parameters, 14m, "1 Kilogram(s) @ AUD 14.00/KG");

			parameters.ChargeableAmount = new Quantity(1.5m, QuantityUnit.KG);
			AssertCalculation(parameters, 20m, "1 Kilogram(s) @ AUD 14.00/KG + 1 Kilogram(s) @ AUD 6.00/KG");

			parameters.ChargeableAmount = new Quantity(10m, QuantityUnit.KG);
			AssertCalculation(parameters, 68m, "1 Kilogram(s) @ AUD 14.00/KG + 9 Kilogram(s) @ AUD 6.00/KG");

			TestCalculator.First = 0;
			AssertCalculation(parameters, 54m, "9 Kilogram(s) @ AUD 6.00/KG");

			TestCalculator.First = 14;
			TestCalculator.Additional = 0;
			AssertCalculation(parameters, 14m, "1 Kilogram(s) @ AUD 14.00/KG");
		}

		public void TestCalculate_UnitIsCN_ShouldCalculatePerContainer()
		{
			Line.TL_WeightVolume = QuantityUnit.CN;
			Line.Parent.TI_RC = Helper.Containers["40GP"].PK;

			TestCalculator.First = 1000;
			TestCalculator.Additional = 500;

			var rateableMeasures = Criteria.RateableMeasures;
			new TestContainers(Factory, "40GP",
				new MeasureInfo.ContainerInfo[]
				{
					new MeasureInfo.ContainerInfo(10000m, "KG", 10m, "M3", 25, 0, "CNT_1"),
					new MeasureInfo.ContainerInfo(4000m, "KG", 3m, "M3", 5, 0, "CNT_2"),
					new MeasureInfo.ContainerInfo(6000m, "KG", 6m, "M3", 2, 0, "CNT_3"),
				}).PopulateContainerList(rateableMeasures);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var result = AssertCalculation(parameters, 2000m, "1 40GP Container(s) @ AUD 1000.00/Container + 2 40GP Container(s) @ AUD 500.00/Container");

			var expectedBases = new[]
			{
				"1000|CN|1|40GP|CNT_1",
				"500|CN|1|40GP|CNT_2",
				"500|CN|1|40GP|CNT_3"
			};

			var actualBases = result.PaymentBases.Select(p =>
				$"{p.RateInfo.PerUnitRate}|{p.RateInfo.Unit}|{p.Chargeable.Amount}|{p.Chargeable.Unit}|{p.Chargeable.Reference}"
			).ToArray();

			AssertContainsExactElementsInAnyOrder(
				"The calculated payment bases should match the expected collection",
				expectedBases,
				actualBases
			);
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.Line.ViewAgentRates = false;
			TestCalculator.First = 25m;
			TestCalculator.Additional = 15m;
			TestCalculator.Line.ViewAgentRates = true;
			TestCalculator.First = 40m;
			TestCalculator.Additional = 20m;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;
			source.PerUnit = 6m;
			source.BaseRate = 80m;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().Line.ViewAgentRates = false;
			AssertEquals(111m, clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().First);
			AssertEquals(21m, clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().Additional);
			clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().Line.ViewAgentRates = true;
			AssertEquals(126m, clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().First);
			AssertEquals(26m, clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().Additional);

			source.PerUnit = 0m;
			source.Percent = 20m;
			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.PercentFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().Line.ViewAgentRates = false;
			AssertEquals(110m, clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().First);
			AssertEquals(18m, clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().Additional);
			clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().Line.ViewAgentRates = true;
			AssertEquals(128m, clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().First);
			AssertEquals(24m, clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().Additional);

			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().Line.ViewAgentRates = false;
			AssertEquals(126m, clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().First);
			AssertEquals(18m, clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().Additional);
			clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().Line.ViewAgentRates = true;
			AssertEquals(144m, clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().First);
			AssertEquals(24m, clonedLine.GetCalculator<FirstPlusAdditionalCalculator>().Additional);
		}

		public override void TestGetCostsComparerChargesSummary()
		{
			TestCalculator.First = 50m;
			TestCalculator.Additional = 10m;
			var items = TestCalculator.GetCostsComparerChargesSummary(new List<RateLine>());
			AssertEquals(2, items.Count);
			AssertChargesSummaryItem(items[0], "BAS", 40m);
			AssertChargesSummaryItem(items[1], "UNT", 10m);
		}

		public override void TestPricePerSingleChargeable()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateLine = costing.AddRateEntry("AIR", "LSE", "AUSYD", "NZAKL").AddRateLine("FRT", FirstPlusAdditionalCalculator.Code, "KG", "AUD");
			rateLine.GetCalculator<FirstPlusAdditionalCalculator>().First = 14m;

			AssertEquals("AUD 14.0000", rateLine.ParentRateEntry.AllInCost());
			AssertEquals("AUD 14.0000/KG", rateLine.ParentRateEntry.FreightRatePerChargeableUnit());

			rateLine.Calculator.PricePerSingleChargeable.Clear();
			rateLine.GetCalculator<FirstPlusAdditionalCalculator>().Additional = 6;
			AssertEquals("AUD 20.0000", rateLine.ParentRateEntry.AllInCost());
			AssertEquals("AUD 20.0000/KG", rateLine.ParentRateEntry.FreightRatePerChargeableUnit());
		}

		#region Implementation

		protected override Type CalculatorType
		{
			get { return typeof(FirstPlusAdditionalCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return FirstPlusAdditionalCalculator.Code; }
		}

		new FirstPlusAdditionalCalculator TestCalculator
		{
			get { return (FirstPlusAdditionalCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
