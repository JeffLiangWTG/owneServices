using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class HRCCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<HighestRateCalculator, Xsd.HRCCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.HRCCalculator calculatorXSD = CalculatorXSDForTest(true);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is an Highest Rate Caluclator", typeof(HighestRateCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());
			AssertEquals("Calculator RateLineItems Count", 3, CalculatorGenerated.RateLineItems.Count);

			AssertRateLineItem(CalculatorGenerated.RateLineItems[0], "MIN", "", 100m, 0m);
			AssertRateLineItem(CalculatorGenerated.RateLineItems[1], "UNT", "KG", 10m, 20m);
			AssertRateLineItem(CalculatorGenerated.RateLineItems[2], "UNT", "M3", 30m, 40m);
		}

		void AssertRateLineItem(RateLineItem item, ZString type, ZString unit, ZDecimal rate, ZDecimal flatAmount)
		{
			AssertEquals("Type", type, item.TM_Type);
			AssertEquals("Unit", unit, item.TM_BreakWeightVolume);
			AssertEquals("Rate", rate, item.TM_RelevantValue);
			AssertEquals("FlatAmount", flatAmount, item.TM_FlatAmount);
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Xsd.HRCCalculator calculatorXSD = new Xsd.HRCCalculator();

			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());
			AssertEquals("Calculator RateLineItems Count", 3, calculatorXSD.RateItems.Count);

			AssertRateLineItemXSD(calculatorXSD.RateItems[0], "MIN", "", 100m, 0m);
			AssertRateLineItemXSD(calculatorXSD.RateItems[1], "UNT", "KG", 10m, 20m);
			AssertRateLineItemXSD(calculatorXSD.RateItems[2], "UNT", "M3", 30m, 40m);
		}

		void AssertRateLineItemXSD(Xsd.RateItemWithOperatorAndBreak rateItemXSD, ZString type, ZString unit, ZDecimal rate, ZDecimal flatAmount)
		{
			AssertEquals("Type", type, rateItemXSD.Operator);
			AssertEquals("Unit", unit, rateItemXSD.Units);
			AssertEquals("Rate", rate, rateItemXSD.PerUnit);
			AssertEquals("FlatAmount", flatAmount, rateItemXSD.FlatAmount);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return HighestRateCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(HRCCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<HighestRateCalculator, Xsd.HRCCalculator> CalculatorGenerator
		{
			get { return new HRCCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.HRCCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.HRCCalculator();

			Xsd.RateItemWithOperatorAndBreak rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("MIN");
			rateItemXSD.Units = "M3";
			if (specified)
			{
				rateItemXSD.PerUnit = 100m;
			}

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("UNT");
			rateItemXSD.Units = "KG";
			if (specified)
			{
				rateItemXSD.PerUnit = 10m;
				rateItemXSD.FlatAmount = 20m;
			}

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("UNT");
			rateItemXSD.Units = "M3";
			if (specified)
			{
				rateItemXSD.PerUnit = 30m;
				rateItemXSD.FlatAmount = 40m;
			}

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(HighestRateCalculator calculator)
		{
			RateLineItem rateItem;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("MIN");
			rateItem.TM_RelevantValue = 100m;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("UNT");
			rateItem.TM_BreakWeightVolume = "KG";
			rateItem.TM_RelevantValue = 10m;
			rateItem.TM_FlatAmount = 20m;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("UNT");
			rateItem.TM_BreakWeightVolume = "M3";
			rateItem.TM_RelevantValue = 30m;
			rateItem.TM_FlatAmount = 40m;
		}

		#endregion
	}
}
