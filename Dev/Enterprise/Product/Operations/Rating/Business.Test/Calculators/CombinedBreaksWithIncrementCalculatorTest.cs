using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class CombinedBreaksWithIncrementCalculatorTest : BaseCombinedCalculatorTest<CombinedBreaksWithIncrementCalculator>
	{
		protected override int NumberOfRateLineItemsAfterInitialization => 4;

		public void TestMaterialLengthCalculation()
		{
			Line.TL_WeightVolume = QuantityUnit.CM;
			Line.TL_RX_NKCurrency = "AUD";

			TestCalculator.AddRateLineItem("-", 10m, 5m);
			TestCalculator.AddRateLineItem("+", 10m, 6m);
			TestCalculator.AddRateLineItem("+", 20m, 7m);

			var parameters = GetParameters(10, 0, 4);
			AssertCalculation(parameters, 50m, "10 Unit(s) @ AUD 5.00/Unit(s) for 10- CM");

			parameters = GetParameters(10, 0, 12);
			AssertCalculation(parameters, 60m, "10 Unit(s) @ AUD 6.00/Unit(s) for 10+ CM");

			parameters = GetParameters(10, 0, 24);
			AssertCalculation(parameters, 70m, "10 Unit(s) @ AUD 7.00/Unit(s) for 20+ CM");
		}

		public void TestMaterialAreaCalculation()
		{
			Line.TL_WeightVolume = QuantityUnit.CM2;
			Line.TL_RX_NKCurrency = "AUD";

			TestCalculator.AddRateLineItem("-", 10m, 5m);
			TestCalculator.AddRateLineItem("+", 10m, 6m);
			TestCalculator.AddRateLineItem("+", 20m, 7m);

			var parameters = GetParameters(12, 4, 0);
			AssertCalculation(parameters, 60m, "12 Unit(s) @ AUD 5.00/Unit(s) for 10- CM2");

			parameters = GetParameters(12, 14, 0);
			AssertCalculation(parameters, 72m, "12 Unit(s) @ AUD 6.00/Unit(s) for 10+ CM2");

			parameters = GetParameters(12, 24, 0);
			AssertCalculation(parameters, 84m, "12 Unit(s) @ AUD 7.00/Unit(s) for 20+ CM2");
		}

		public void TestMaterialQuantityCalculation()
		{
			Line.TL_WeightVolume = "UNT";
			Line.TL_RX_NKCurrency = "AUD";

			TestCalculator.AddRateLineItem("-", 10m, 5m);
			TestCalculator.AddRateLineItem("+", 10m, 6m);
			TestCalculator.AddRateLineItem("+", 20m, 7m);

			var parameters = GetParameters(4, 0, 0);
			AssertCalculation(parameters, 20m, "4 Unit(s) @ AUD 5.00/Unit(s) for 10- UNT");

			parameters = GetParameters(12, 0, 0);
			AssertCalculation(parameters, 72m, "12 Unit(s) @ AUD 6.00/Unit(s) for 10+ UNT");

			parameters = GetParameters(24, 0, 0);
			AssertCalculation(parameters, 168m, "24 Unit(s) @ AUD 7.00/Unit(s) for 20+ UNT");
		}

		public void TestLabourHourCalculationEnabled()
		{
			Line.TL_WeightVolume = QuantityUnit.CM;
			Line.TL_RX_NKCurrency = "AUD";

			TestCalculator.AddRateLineItem("-", 10m, 5m, breakHour: 7, breakHourRate: 10);
			TestCalculator.AddRateLineItem("+", 10m, 6m, breakHour: 8, breakHourRate: 10);
			TestCalculator.AddRateLineItem("+", 20m, 7m, breakHour: 9, breakHourRate: 10);

			var labourChargeCode = Helper.ChargeCodes.New("LBR", "Test Labour Charge Code", CalculatorCode, ChargeCodeGroupList.Codes.LabourHourRate);
			Line.TL_AC = labourChargeCode.PK;

			Assert("Labour Calculation Enabled", TestCalculator.LabourHourCalculationEnabled);
			Assert("Relevent Value of line items should be reset", !Line.RateLineItems.Any(i => (i as RateLineItem).TM_RelevantValue > 0));
			Assert("Break Hour of line items should not be reset", Line.RateLineItems.Any(i => (i as RateLineItem).TM_BreakHour > 0));
			Assert("Break Hour Rate of line items should not be reset", Line.RateLineItems.Any(i => (i as RateLineItem).TM_BreakHourRate > 0));

			labourChargeCode = Helper.ChargeCodes.New("MNR", "Test MNR Charge Code", CalculatorCode, ChargeCodeGroupList.Codes.MNRWorkOrderHeader);
			Line.TL_AC = labourChargeCode.PK;

			Assert("Labour Calculation should not be Enabled", !TestCalculator.LabourHourCalculationEnabled);
			Assert("Break Hour of line items should be reset", !Line.RateLineItems.Any(i => (i as RateLineItem).TM_BreakHour > 0));
			Assert("Break Hour Rate of line items should be reset", !Line.RateLineItems.Any(i => (i as RateLineItem).TM_BreakHourRate > 0));
		}

		public void TestLabourRateLengthCalculation()
		{
			Line.TL_WeightVolume = QuantityUnit.CM;
			Line.TL_RX_NKCurrency = "AUD";

			TestCalculator.AddRateLineItem("-", 10m, 5m, breakHour: 7, breakHourRate: 10);
			TestCalculator.AddRateLineItem("+", 10m, 6m, breakHour: 8, breakHourRate: 0);
			TestCalculator.AddRateLineItem("+", 20m, 7m, breakHour: 9, breakHourRate: 0);

			var labourChargeCode = Helper.ChargeCodes.New("LBR", "Test Labour Charge Code", CalculatorCode, ChargeCodeGroupList.Codes.LabourHourRate);
			Line.TL_AC = labourChargeCode.PK;

			var parametersWithoutLabourHours = GetParameters(10, 0, 4);
			var result = AssertCalculation(parametersWithoutLabourHours, 700m, "10 Unit(s) @ AUD 70.00/Unit(s) 7 Hour(s)/Unit(s) @ AUD 10.00/Hour(s) for 10- CM");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "7"), result.Attributes.Attributes);

			parametersWithoutLabourHours = GetParameters(10, 0, 12);
			result = AssertCalculation(parametersWithoutLabourHours, 800m, "10 Unit(s) @ AUD 80.00/Unit(s) 8 Hour(s)/Unit(s) @ AUD 10.00/Hour(s) for 10+ CM");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "8"), result.Attributes.Attributes);

			parametersWithoutLabourHours = GetParameters(10, 0, 24);
			result = AssertCalculation(parametersWithoutLabourHours, 900m, "10 Unit(s) @ AUD 90.00/Unit(s) 9 Hour(s)/Unit(s) @ AUD 10.00/Hour(s) for 20+ CM");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "9"), result.Attributes.Attributes);

			var parametersWithLabourHours = GetParameters(10, 0, 4, new TimeSpan(12, 0, 0));
			result = AssertCalculation(parametersWithLabourHours, 1200m, "10 Unit(s) @ AUD 120.00/Unit(s) 12 Hour(s)/Unit(s) @ AUD 10.00/Hour(s)");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "12"), result.Attributes.Attributes);

			parametersWithLabourHours = GetParameters(10, 0, 12, new TimeSpan(12, 0, 0));
			result = AssertCalculation(parametersWithLabourHours, 1200m, "10 Unit(s) @ AUD 120.00/Unit(s) 12 Hour(s)/Unit(s) @ AUD 10.00/Hour(s)");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "12"), result.Attributes.Attributes);

			parametersWithLabourHours = GetParameters(10, 0, 24, new TimeSpan(12, 0, 0));
			result = AssertCalculation(parametersWithLabourHours, 1200m, "10 Unit(s) @ AUD 120.00/Unit(s) 12 Hour(s)/Unit(s) @ AUD 10.00/Hour(s)");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "12"), result.Attributes.Attributes);
		}

		public void TestLabourRateAreaCalculation()
		{
			Line.TL_WeightVolume = QuantityUnit.CM2;
			Line.TL_RX_NKCurrency = "AUD";

			TestCalculator.AddRateLineItem("-", 10m, 5m, breakHour: 7, breakHourRate: 10);
			TestCalculator.AddRateLineItem("+", 10m, 6m, breakHour: 8, breakHourRate: 0);
			TestCalculator.AddRateLineItem("+", 20m, 7m, breakHour: 9, breakHourRate: 0);

			var labourChargeCode = Helper.ChargeCodes.New("LBR", "Test Labour Charge Code", CalculatorCode, ChargeCodeGroupList.Codes.LabourHourRate);
			Line.TL_AC = labourChargeCode.PK;

			var parametersWithoutLabourHours = GetParameters(12, 4, 0);
			var result = AssertCalculation(parametersWithoutLabourHours, 840m, "12 Unit(s) @ AUD 70.00/Unit(s) 7 Hour(s)/Unit(s) @ AUD 10.00/Hour(s) for 10- CM2");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "7"), result.Attributes.Attributes);

			parametersWithoutLabourHours = GetParameters(12, 12, 0);
			result = AssertCalculation(parametersWithoutLabourHours, 960m, "12 Unit(s) @ AUD 80.00/Unit(s) 8 Hour(s)/Unit(s) @ AUD 10.00/Hour(s) for 10+ CM2");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "8"), result.Attributes.Attributes);

			parametersWithoutLabourHours = GetParameters(12, 24, 0);
			result = AssertCalculation(parametersWithoutLabourHours, 1080m, "12 Unit(s) @ AUD 90.00/Unit(s) 9 Hour(s)/Unit(s) @ AUD 10.00/Hour(s) for 20+ CM2");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "9"), result.Attributes.Attributes);

			var parametersWithLabourHours = GetParameters(10, 4, 0, new TimeSpan(12, 0, 0));
			result = AssertCalculation(parametersWithLabourHours, 1200m, "10 Unit(s) @ AUD 120.00/Unit(s) 12 Hour(s)/Unit(s) @ AUD 10.00/Hour(s)");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "12"), result.Attributes.Attributes);

			parametersWithLabourHours = GetParameters(10, 12, 0, new TimeSpan(12, 0, 0));
			result = AssertCalculation(parametersWithLabourHours, 1200m, "10 Unit(s) @ AUD 120.00/Unit(s) 12 Hour(s)/Unit(s) @ AUD 10.00/Hour(s)");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "12"), result.Attributes.Attributes);

			parametersWithLabourHours = GetParameters(10, 24, 0, new TimeSpan(12, 0, 0));
			result = AssertCalculation(parametersWithLabourHours, 1200m, "10 Unit(s) @ AUD 120.00/Unit(s) 12 Hour(s)/Unit(s) @ AUD 10.00/Hour(s)");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "12"), result.Attributes.Attributes);
		}

		public void TestLabourRateQuantityCalculation()
		{
			Line.TL_WeightVolume = "UNT";
			Line.TL_RX_NKCurrency = "AUD";

			TestCalculator.AddRateLineItem("-", 10m, 5m, breakHour: 7, breakHourRate: 10);
			TestCalculator.AddRateLineItem("+", 10m, 6m, breakHour: 8, breakHourRate: 0);
			TestCalculator.AddRateLineItem("+", 20m, 7m, breakHour: 9, breakHourRate: 0);

			var labourChargeCode = Helper.ChargeCodes.New("LBR", "Test Labour Charge Code", CalculatorCode, ChargeCodeGroupList.Codes.LabourHourRate);
			Line.TL_AC = labourChargeCode.PK;

			var parametersWithoutLabourHours = GetParameters(4, 0, 0);
			var result = AssertCalculation(parametersWithoutLabourHours, 280m, "4 Unit(s) @ AUD 70.00/Unit(s) 7 Hour(s)/Unit(s) @ AUD 10.00/Hour(s) for 10- UNT");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "7"), result.Attributes.Attributes);

			parametersWithoutLabourHours = GetParameters(12, 0, 0);
			result = AssertCalculation(parametersWithoutLabourHours, 960m, "12 Unit(s) @ AUD 80.00/Unit(s) 8 Hour(s)/Unit(s) @ AUD 10.00/Hour(s) for 10+ UNT");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "8"), result.Attributes.Attributes);

			parametersWithoutLabourHours = GetParameters(24, 0, 0);
			result = AssertCalculation(parametersWithoutLabourHours, 2160m, "24 Unit(s) @ AUD 90.00/Unit(s) 9 Hour(s)/Unit(s) @ AUD 10.00/Hour(s) for 20+ UNT");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "9"), result.Attributes.Attributes);

			var parametersWithLabourHours = GetParameters(4, 0, 0, new TimeSpan(12, 0, 0));
			result = AssertCalculation(parametersWithLabourHours, 480m, "4 Unit(s) @ AUD 120.00/Unit(s) 12 Hour(s)/Unit(s) @ AUD 10.00/Hour(s)");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "12"), result.Attributes.Attributes);

			parametersWithLabourHours = GetParameters(12, 0, 0, new TimeSpan(12, 0, 0));
			result = AssertCalculation(parametersWithLabourHours, 1440m, "12 Unit(s) @ AUD 120.00/Unit(s) 12 Hour(s)/Unit(s) @ AUD 10.00/Hour(s)");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "12"), result.Attributes.Attributes);

			parametersWithLabourHours = GetParameters(24, 0, 0, new TimeSpan(12, 0, 0));
			result = AssertCalculation(parametersWithLabourHours, 2880m, "24 Unit(s) @ AUD 120.00/Unit(s) 12 Hour(s)/Unit(s) @ AUD 10.00/Hour(s)");
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.BreakHours, "12"), result.Attributes.Attributes);
		}

		AutoRatingCalculatorParametersForTesting GetParameters(int unitCount, decimal area, decimal length, TimeSpan? labourHours = null)
		{
			var part = new RateablePart();
			part.UnitCount = unitCount;
			part.Area = area;
			part.Length = length;

			if (labourHours.HasValue)
			{
				part.Time = new TimeInfo(labourHours.Value);
			}

			var partList = new RateablePartList();
			partList.AddPart(part);

			var rateableMeasures = new RateableMeasureSet();
			rateableMeasures.AddPartList(MeasureType.Unit, partList);
			rateableMeasures.AddPartList(MeasureType.Area, partList);
			rateableMeasures.AddPartList(MeasureType.Length, partList);

			Criteria.RateableMeasures = rateableMeasures;
			return new AutoRatingCalculatorParametersForTesting(Criteria);
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;

			Line.RateLineItems.RemoveAndDelete(Calculator.Items.Operator.MIN, Calculator.Items.Operator.BAS, Calculator.Items.Operator.UNT);
			TestCalculator["-45"] = (ZDecimal)6m;
			TestCalculator["+45"] = (ZDecimal)5m;
			TestCalculator["+100"] = (ZDecimal)4m;
			TestCalculator["+250"] = (ZDecimal)3m;
			TestCalculator.IsAccumulated = true;

			source["-100"] = (ZDecimal)2m;
			source["+100"] = (ZDecimal)1m;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertEquals(8m, clonedLine.Calculator["-45"]);
			AssertEquals(7m, clonedLine.Calculator["+45"]);
			AssertEquals(5m, clonedLine.Calculator["+100"]);
			AssertEquals(4m, clonedLine.Calculator["+250"]);
			Assert(clonedLine.GetCalculator<CombinedBreaksWithIncrementCalculator>().IsAccumulated);
		}

		public override void TestCalculationWithCallForPricingFlag()
		{
			Assert(true);
		}

		public override void TestIsAccumulatedAndUseHigherChargeableLowerRateRuleWhenBreaksPerIsNotEmpty()
		{
			Assert(true);
		}

		public override void TestQuotationLines()
		{
			Line.TL_WeightVolume = "CM";
			Line.TL_RX_NKCurrency = "USD";

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.CYM);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.AddRateLineItem("-", 24m, 10m);
			TestCalculator["+24"] = (ZDecimal)15m;
			TestCalculator["+72"] = (ZDecimal)20m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Less than 24 Centimeter(s)|USD|10.00|per Centimeter", quotationLines[1].ToString());
			AssertEquals("24 Centimeter(s) to less than 72 Centimeter(s)|USD|15.00|per Centimeter", quotationLines[2].ToString());
			AssertEquals("72 Centimeter(s) and above|USD|20.00|per Centimeter", quotationLines[3].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("Less than 24 Centimeter(s)|USD|10.00|per Centimeter", quotationLines[1].ToString());
			AssertEquals("24 Centimeter(s) to less than 72 Centimeter(s)|USD|15.00|per Centimeter", quotationLines[2].ToString());
			AssertEquals("72 Centimeter(s) and above|USD|20.00|per Centimeter", quotationLines[3].ToString());
		}

		#region Implementation

		protected override Type CalculatorType
		{
			get { return typeof(CombinedBreaksWithIncrementCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return CombinedBreaksWithIncrementCalculator.Code; }
		}

		new CombinedBreaksWithIncrementCalculator TestCalculator
		{
			get { return (CombinedBreaksWithIncrementCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
