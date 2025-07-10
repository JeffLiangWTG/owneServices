using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer.Testing;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class PEBCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<PercentageBreaksCalculator, Xsd.PEBCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.PEBCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			#region Test (Specified = false)

			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerated.ApplyToRateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is an PercentageBreaksCalculator", typeof(PercentageBreaksCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());

			AssertEquals("Calculator ApplyToRateLineItems Count", 3, CalculatorGenerated.ApplyToRateLineItems.Count);
			AssertEquals("Calculator RateLineItems Count", 3, CalculatorGenerated.RateLineItems.Count);

			AssertEquals("Calculator 's BaseRate", 0m, CalculatorGenerated.BaseRate);
			AssertEquals("Calculator 's Minimum", 0m, CalculatorGenerated.Minimum);
			AssertEquals("Calculator 's Maximum", 0m, CalculatorGenerated.Maximum);
			AssertEquals("Calculator 's UseBreaksBasedOnValues", false, CalculatorGenerated.UseBreaksBasedOnValues);

			#endregion

			#region Test (Specified = true)

			calculatorXSD = CalculatorXSDForTest(true);

			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerated.ApplyToRateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator ApplyToRateLineItems Count", 3, CalculatorGenerated.ApplyToRateLineItems.Count);
			AssertEquals("Calculator RateLineItems Count", 6, CalculatorGenerated.RateLineItems.Count);

			AssertEquals("Calculator 's BaseRate", 1.3m, CalculatorGenerated.BaseRate);
			AssertEquals("Calculator 's Minimum", 1.2m, CalculatorGenerated.Minimum);
			AssertEquals("Calculator 's Maximum", 1.4m, CalculatorGenerated.Maximum);
			AssertEquals("Calculator 's UseBreaksBasedOnValues", true, CalculatorGenerated.UseBreaksBasedOnValues);

			#endregion
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Xsd.PEBCalculator calculatorXSD = new Xsd.PEBCalculator();

			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator ApplyToRateItems Count", 3, calculatorXSD.ApplyToRateItems.Count);
			AssertEquals("Calculator RateLineItems Count", 3, calculatorXSD.RateItems.Count);

			AssertEquals("Calculator 's BaseRate", 1.3m, calculatorXSD.BasePrice);
			AssertEquals("Calculator 's BaseRate Specified", true, calculatorXSD.BasePriceSpecified);
			AssertEquals("Calculator 's Minimum", 1.2m, calculatorXSD.Minimum);
			AssertEquals("Calculator 's Minimum Specified", true, calculatorXSD.MinimumSpecified);
			AssertEquals("Calculator 's Maximum", 1.4m, calculatorXSD.Maximum);
			AssertEquals("Calculator 's Maximum Specified", true, calculatorXSD.MaximumSpecified);
			AssertEquals("Calculator 's UseBreaksBasedOnValues", Xsd.TrueFalse.@true, calculatorXSD.BreaksBasedOnValues);
			AssertEquals("Calculator 's UseBreaksBasedOnValues Specified", true, calculatorXSD.BreaksBasedOnValuesSpecified);
		}

		#endregion

		#region Importing Applies mapps

		public void TestMappingsAppliedCorrectly()
		{
			RateTestHelper.MapChargeCode(Env.Registry.FreightChargeCode, "YYY", null, Factory);
			Factory.Save();

			calculatorXSD = new Xsd.PEBCalculator();

			var rateItemXSDApplyTo = calculatorXSD.ApplyToRateItems.AddNew();
			rateItemXSDApplyTo.Type = "COD";
			rateItemXSDApplyTo.Chrg = "YYY";

			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerated.ApplyToRateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, RateTestHelper.GetContext(Factory));

			AssertEquals(Env.Registry.FreightChargeCode, CalculatorGenerated.ApplyToRateLineItems[0].TM_AC);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return PercentageBreaksCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(PEBCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<PercentageBreaksCalculator, Xsd.PEBCalculator> CalculatorGenerator
		{
			get { return new PEBCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.PEBCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.PEBCalculator();

			if (specified)
			{
				calculatorXSD.Minimum = 1.2m;
				calculatorXSD.BasePrice = 1.3m;
				calculatorXSD.Maximum = 1.4m;
				calculatorXSD.BreaksBasedOnValues = Xsd.TrueFalse.@true;
			}

			Xsd.PERRateItem rateItemXSDApplyTo;

			rateItemXSDApplyTo = calculatorXSD.ApplyToRateItems.AddNew();
			rateItemXSDApplyTo.Type = "COD";
			rateItemXSDApplyTo.Chrg = "ADINS";

			rateItemXSDApplyTo = calculatorXSD.ApplyToRateItems.AddNew();
			rateItemXSDApplyTo.Type = "ALL";
			rateItemXSDApplyTo.Chrg = "ADPOST";

			rateItemXSDApplyTo = calculatorXSD.ApplyToRateItems.AddNew();
			rateItemXSDApplyTo.Type = "FRT";
			rateItemXSDApplyTo.Chrg = "ADTEL";

			Xsd.RateItemWithOperatorAndBreakAndPercent rateItemXSD;

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("-");
			if (specified)
			{
				rateItemXSD.FlatAmount = 10.2m;
				rateItemXSD.BreakAmount = 5.3m;
				rateItemXSD.BreakMinimum = 4.2m;
				rateItemXSD.Percent = 20.3m;
				rateItemXSD.Units = "HR";
			}

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("+");
			if (specified)
			{
				rateItemXSD.FlatAmount = 10.2m;
				rateItemXSD.BreakAmount = 5.3m;
				rateItemXSD.BreakMinimum = 4.2m;
				rateItemXSD.Percent = 20.3m;
				rateItemXSD.Units = "HR";
			}

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Operator = new ZString("+");
			if (specified)
			{
				rateItemXSD.FlatAmount = 11.2m;
				rateItemXSD.BreakAmount = 6.3m;
				rateItemXSD.BreakMinimum = 5.2m;
				rateItemXSD.Percent = 21.3m;
				rateItemXSD.Units = "HR";
			}

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(PercentageBreaksCalculator calculator)
		{
			calculator.Minimum = 1.2m;
			calculator.BaseRate = 1.3m;
			calculator.Maximum = 1.4m;
			calculator.UseBreaksBasedOnValues = true;

			RateLineItem rateItemApplyTo;
			AccChargeCode charge;

			charge = RateTestHelper.FindChargeCode("ADINS", Factory);
			rateItemApplyTo = calculator.ApplyToRateLineItems.AddNew();
			rateItemApplyTo.TM_Text = "COD";
			rateItemApplyTo.TM_AC = charge.PK;

			charge = RateTestHelper.FindChargeCode("ADPOST", Factory);
			rateItemApplyTo = calculator.ApplyToRateLineItems.AddNew();
			rateItemApplyTo.TM_Text = "ALL";
			rateItemApplyTo.TM_AC = charge.PK;

			charge = RateTestHelper.FindChargeCode("ADTEL", Factory);
			rateItemApplyTo = calculator.ApplyToRateLineItems.AddNew();
			rateItemApplyTo.TM_Text = "FRT";
			rateItemApplyTo.TM_AC = charge.PK;

			RateLineItem rateItem;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("-");
			rateItem.TM_FlatAmount = 10.2m;
			rateItem.TM_Break = 5.3m;
			rateItem.TM_BreakMinimum = 4.2m;
			rateItem.TM_FlatAmount = 20.3m;
			rateItem.TM_BreakWeightVolume = "HR";

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_FlatAmount = 10.2m;
			rateItem.TM_Break = 5.3m;
			rateItem.TM_BreakMinimum = 4.2m;
			rateItem.TM_FlatAmount = 20.3m;
			rateItem.TM_BreakWeightVolume = "HR";

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_FlatAmount = 11.2m;
			rateItem.TM_Break = 6.3m;
			rateItem.TM_BreakMinimum = 5.2m;
			rateItem.TM_FlatAmount = 21.3m;
			rateItem.TM_BreakWeightVolume = "HR";
		}

		#endregion
	}
}
