using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class CMBCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<CombinedCalculator, Xsd.CMBCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.CMBCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			#region Test (Specified = false)

			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is an Agency Caluclator", typeof(CombinedCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());

			AssertEquals("Calculator 's IsAccumulated", false, CalculatorGenerated.IsAccumulated);
			AssertEquals("Calculator 's UseHigherChargeableLowerRateRule", false, CalculatorGenerated.UseHigherChargeableLowerRateRule);
			AssertEquals("Calculator 's UseInclusiveBreaks", false, CalculatorGenerated.UseInclusiveBreaks);

			AssertEquals("Calculator RateLineItems Count", 3, CalculatorGenerated.RateLineItems.Count);

			AssertEquals("Calculator 's BaseRate", 0m, CalculatorGenerated.BaseRate);
			AssertEquals("Calculator 's Minimum", 0m, CalculatorGenerated.Minimum);
			AssertEquals("Calculator 's Maximum", 0m, CalculatorGenerated.Maximum);
			AssertEquals("Calculator 's PerUnit", 0m, CalculatorGenerated.PerUnit);

			#endregion

			#region Test (Specified = true)

			calculatorXSD = CalculatorXSDForTest(true);
			calculatorXSD.SimpleRate.PerUnitSpecified = false;

			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator 's IsAccumulated", true, CalculatorGenerated.IsAccumulated);
			AssertEquals("Calculator 's UseHigherChargeableLowerRateRule", true, CalculatorGenerated.UseHigherChargeableLowerRateRule);
			AssertEquals("Calculator 's UseInclusiveBreaks", true, CalculatorGenerated.UseInclusiveBreaks);

			AssertEquals("Calculator RateLineItems Count", 6, CalculatorGenerated.RateLineItems.Count);

			AssertEquals("Calculator 's BaseRate", 1.1m, CalculatorGenerated.BaseRate);
			AssertEquals("Calculator 's Minimum", 3.6m, CalculatorGenerated.Minimum);
			AssertEquals("Calculator 's Maximum", 3.6m, CalculatorGenerated.Maximum);
			AssertEquals("Calculator 's PerUnit", 0m, CalculatorGenerated.PerUnit);

			calculatorXSD.SimpleRate.PerUnitSpecified = true;
			calculatorXSD.RateItems = new Xsd.RateItemWithOperatorAndBreakCollection();

			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator 's IsAccumulated", true, CalculatorGenerated.IsAccumulated);
			AssertEquals("Calculator 's UseHigherChargeableLowerRateRule", true, CalculatorGenerated.UseHigherChargeableLowerRateRule);
			AssertEquals("Calculator 's UseInclusiveBreaks", true, CalculatorGenerated.UseInclusiveBreaks);

			AssertEquals("Calculator RateLineItems Count", 4, CalculatorGenerated.RateLineItems.Count);

			AssertEquals("Calculator 's BaseRate", 1.1m, CalculatorGenerated.BaseRate);
			AssertEquals("Calculator 's Minimum", 3.6m, CalculatorGenerated.Minimum);
			AssertEquals("Calculator 's Maximum", 3.6m, CalculatorGenerated.Maximum);
			AssertEquals("Calculator 's PerUnit", 3.6m, CalculatorGenerated.PerUnit);

			#endregion
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Xsd.CMBCalculator calculatorXSD = new Xsd.CMBCalculator();

			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator 's IsAccumulated", Xsd.TrueFalse.@true, calculatorXSD.Accumulation);
			AssertEquals("Calculator 's IsAccumulated Specified", true, calculatorXSD.AccumulationSpecified);
			AssertEquals("Calculator 's UseHigherChargeableLowerRateRule", Xsd.TrueFalse.@true, calculatorXSD.HigherWeightOrUnitLowerRate);
			AssertEquals("Calculator 's UseHigherChargeableLowerRateRule Specified", true, calculatorXSD.HigherWeightOrUnitLowerRateSpecified);
			AssertEquals("Calculator 's UseInclusiveBreaks", Xsd.TrueFalse.@true, calculatorXSD.InclusiveBreaks);
			AssertEquals("Calculator 's UseInclusiveBreaks Specified", true, calculatorXSD.InclusiveBreaksSpecified);

			AssertEquals("Calculator RateLineItems Count", 0, calculatorXSD.RateItems.Count);

			AssertEquals("Calculator 's BaseRate", 1.1m, calculatorXSD.SimpleRate.BaseRate);
			AssertEquals("Calculator 's BaseRate Specified", true, calculatorXSD.SimpleRate.BaseRateSpecified);
			AssertEquals("Calculator 's Minimum", 3.6m, calculatorXSD.SimpleRate.Minimum);
			AssertEquals("Calculator 's Minimum Specified", true, calculatorXSD.SimpleRate.MinimumSpecified);
			AssertEquals("Calculator 's Maximum", 3.6m, calculatorXSD.SimpleRate.Maximum);
			AssertEquals("Calculator 's Maximum Specified", true, calculatorXSD.SimpleRate.MaximumSpecified);
			AssertEquals("Calculator 's PerUnit", 3.6m, calculatorXSD.SimpleRate.PerUnit);
			AssertEquals("Calculator 's PerUnit Specified", true, calculatorXSD.SimpleRate.PerUnitSpecified);

			((CombinedCalculator)RateLineForCalculatorTest.Calculator).PerUnit = 0m;
			calculatorXSD = new Xsd.CMBCalculator();
			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator 's IsAccumulated", Xsd.TrueFalse.@true, calculatorXSD.Accumulation);
			AssertEquals("Calculator 's IsAccumulated Specified", true, calculatorXSD.AccumulationSpecified);
			AssertEquals("Calculator 's UseHigherChargeableLowerRateRule", Xsd.TrueFalse.@true, calculatorXSD.HigherWeightOrUnitLowerRate);
			AssertEquals("Calculator 's UseHigherChargeableLowerRateRule Specified", true, calculatorXSD.HigherWeightOrUnitLowerRateSpecified);
			AssertEquals("Calculator 's UseInclusiveBreaks", Xsd.TrueFalse.@true, calculatorXSD.InclusiveBreaks);
			AssertEquals("Calculator 's UseInclusiveBreaks Specified", true, calculatorXSD.InclusiveBreaksSpecified);

			AssertEquals("Calculator RateLineItems Count", 3, calculatorXSD.RateItems.Count);

			AssertEquals("Calculator 's BaseRate", 1.1m, calculatorXSD.SimpleRate.BaseRate);
			AssertEquals("Calculator 's BaseRate Specified", true, calculatorXSD.SimpleRate.BaseRateSpecified);
			AssertEquals("Calculator 's Minimum", 3.6m, calculatorXSD.SimpleRate.Minimum);
			AssertEquals("Calculator 's Minimum Specified", true, calculatorXSD.SimpleRate.MinimumSpecified);
			AssertEquals("Calculator 's Maximum", 3.6m, calculatorXSD.SimpleRate.Maximum);
			AssertEquals("Calculator 's Maximum Specified", true, calculatorXSD.SimpleRate.MaximumSpecified);
			AssertEquals("Calculator 's PerUnit", 0m, calculatorXSD.SimpleRate.PerUnit);
			AssertEquals("Calculator 's PerUnit Specified", false, calculatorXSD.SimpleRate.PerUnitSpecified);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return CombinedCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(CMBCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<CombinedCalculator, Xsd.CMBCalculator> CalculatorGenerator
		{
			get { return new CMBCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.CMBCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.CMBCalculator();

			if (specified)
			{
				calculatorXSD.Accumulation = Xsd.TrueFalse.@true;
				calculatorXSD.HigherWeightOrUnitLowerRate = Xsd.TrueFalse.@true;
				calculatorXSD.InclusiveBreaks = Xsd.TrueFalse.@true;
			}

			Xsd.RateItemWithOperatorAndBreak rateItemXSD;

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("-");
			rateItemXSD.Units = "HR";
			if (specified)
			{
				rateItemXSD.FlatAmount = 10.2m;
				rateItemXSD.BreakAmount = 5.3m;
				rateItemXSD.BreakMinimum = 4.2m;
				rateItemXSD.PerUnit = 20.3m;
			}

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("+");
			rateItemXSD.Units = "HR";
			if (specified)
			{
				rateItemXSD.FlatAmount = 10.2m;
				rateItemXSD.BreakAmount = 5.3m;
				rateItemXSD.BreakMinimum = 4.2m;
				rateItemXSD.PerUnit = 20.3m;
			}

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("+");
			rateItemXSD.Units = "HR";
			if (specified)
			{
				rateItemXSD.FlatAmount = 11.2m;
				rateItemXSD.BreakAmount = 6.3m;
				rateItemXSD.BreakMinimum = 5.2m;
				rateItemXSD.PerUnit = 21.3m;
			}

			if (specified)
			{
				calculatorXSD.SimpleRate.BaseRate = 1.1m;
				calculatorXSD.SimpleRate.Minimum = 3.6m;
				calculatorXSD.SimpleRate.Maximum = 3.6m;
				calculatorXSD.SimpleRate.PerUnit = 3.6m;
			}

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(CombinedCalculator calculator)
		{
			calculator.IsAccumulated = true;
			calculator.UseHigherChargeableLowerRateRule = true;
			calculator.UseInclusiveBreaks = true;

			RateLineItem rateItem;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("-");
			rateItem.TM_FlatAmount = 10.2m;
			rateItem.TM_Break = 5.3m;
			rateItem.TM_BreakMinimum = 4.2m;
			rateItem.TM_RelevantValue = 20.3m;
			rateItem.TM_BreakWeightVolume = "HR";

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_FlatAmount = 10.2m;
			rateItem.TM_Break = 5.3m;
			rateItem.TM_BreakMinimum = 4.2m;
			rateItem.TM_RelevantValue = 20.3m;
			rateItem.TM_BreakWeightVolume = "HR";

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_FlatAmount = 11.2m;
			rateItem.TM_Break = 6.3m;
			rateItem.TM_BreakMinimum = 5.2m;
			rateItem.TM_RelevantValue = 21.3m;
			rateItem.TM_BreakWeightVolume = "HR";

			calculator.BaseRate = 1.1m;
			calculator.Minimum = 3.6m;
			calculator.Maximum = 3.6m;
			calculator.PerUnit = 3.6m;
		}

		#endregion
	}
}
