using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class SMBCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<SplitMonthBillingCalculator, Xsd.SMBCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.SMBCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			#region Test (Specified = false)

			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is an Split Month Billing Caluclator", typeof(SplitMonthBillingCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());

			AssertEquals("Calculator 's Minimum", 0m, CalculatorGenerated.Minimum);

			AssertEquals("Calculator RateLineItems Count", 3, CalculatorGenerated.RateLineItems.Count);

			#endregion

			#region Test (Specified = true)

			calculatorXSD = CalculatorXSDForTest(true);

			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator 's Minimum", 3.6m, CalculatorGenerated.Minimum);

			AssertEquals("Calculator RateLineItems Count", 4, CalculatorGenerated.RateLineItems.Count);

			#endregion
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Xsd.SMBCalculator calculatorXSD = new Xsd.SMBCalculator();

			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator 's Minimum", 3.6m, calculatorXSD.SimpleRate.Minimum);
			AssertEquals("Calculator 's Minimum Specified", true, calculatorXSD.SimpleRate.MinimumSpecified);

			AssertEquals("Calculator RateLineItems Count", 3, calculatorXSD.RateItems.Count);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return SplitMonthBillingCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(SMBCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<SplitMonthBillingCalculator, Xsd.SMBCalculator> CalculatorGenerator
		{
			get { return new SMBCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.SMBCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.SMBCalculator();

			Xsd.RateItemWithOperatorAndBreak rateItemXSD;

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("-");
			if (specified)
			{
				rateItemXSD.FlatAmount = 10.2m;
				rateItemXSD.BreakAmount = 5.3m;
				rateItemXSD.BreakMinimum = 4.2m;
				rateItemXSD.PerUnit = 20.3m;
				rateItemXSD.Units = "HR";
			}

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("+");
			if (specified)
			{
				rateItemXSD.FlatAmount = 10.2m;
				rateItemXSD.BreakAmount = 5.3m;
				rateItemXSD.BreakMinimum = 4.2m;
				rateItemXSD.PerUnit = 20.3m;
				rateItemXSD.Units = "HR";
			}

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("+");
			if (specified)
			{
				rateItemXSD.FlatAmount = 11.2m;
				rateItemXSD.BreakAmount = 6.3m;
				rateItemXSD.BreakMinimum = 5.2m;
				rateItemXSD.PerUnit = 21.3m;
				rateItemXSD.Units = "HR";
			}

			if (specified)
			{
				calculatorXSD.SimpleRate.Minimum = 3.6m;
			}

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(SplitMonthBillingCalculator calculator)
		{
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

			calculator.Minimum = 3.6m;
		}

		#endregion
	}
}
