using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer.Testing;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class PSRCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<ProfitShareRebateCalculator, Xsd.PSRCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore()
		{
			Buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, Buffer);

			var calculatorXSD = CalculatorXSDForTest(false);
			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals(typeof(ProfitShareRebateCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());

			CombineAssertions("(Specified = false)", () =>
			{
				AssertEquals("Minimum", 0m, CalculatorGenerated.Minimum);
				AssertEquals("BaseRate", 0m, CalculatorGenerated.BaseRate);
				AssertEquals("Maximum", 0m, CalculatorGenerated.Maximum);
				AssertEquals("Percent", 0m, CalculatorGenerated.Percent);
				AssertEquals("ZeroWhenLoss", false, CalculatorGenerated.ZeroWhenLoss);
				AssertEquals("Calculator RateLineItems Count", 3, CalculatorGenerated.RateLineItems.Count);
			});

			calculatorXSD = CalculatorXSDForTest(true);
			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			CombineAssertions("(Specified = true)", () =>
			{
				AssertEquals("Minimum", 1.2m, CalculatorGenerated.Minimum);
				AssertEquals("BaseRate", 1.3m, CalculatorGenerated.BaseRate);
				AssertEquals("Maximum", 1.4m, CalculatorGenerated.Maximum);
				AssertEquals("Percent", 1.7m, CalculatorGenerated.Percent);
				AssertEquals("ZeroWhenLoss", true, CalculatorGenerated.ZeroWhenLoss);
				AssertEquals("Calculator RateLineItems Count", 3, CalculatorGenerated.RateLineItems.Count);
			});
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore()
		{
			var calculatorXSD = new Xsd.PSRCalculator();
			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Minimum", 1.2m, calculatorXSD.Minimum);
			AssertEquals("Minimum Specified", true, calculatorXSD.MinimumSpecified);

			AssertEquals("BaseRate", 1.3m, calculatorXSD.BasePrice);
			AssertEquals("BaseRate Specified", true, calculatorXSD.BasePriceSpecified);

			AssertEquals("Maximum", 1.4m, calculatorXSD.Maximum);
			AssertEquals("Maximum Specified", true, calculatorXSD.MaximumSpecified);

			AssertEquals("Percent", 1.7m, calculatorXSD.Percentage);
			AssertEquals("Percent Specified", true, calculatorXSD.PercentageSpecified);

			AssertEquals("ZeroWhenLoss", Xsd.TrueFalse.@true, calculatorXSD.ZeroWhenLoss);
			AssertEquals("ZeroWhenLoss Specified", true, calculatorXSD.ZeroWhenLossSpecified);

			AssertEquals("Calculator RateLineItems Count", 3, calculatorXSD.RateItems.Count);
		}

		#endregion

		#region Importing Applies mapps

		public void TestMappingsAppliedCorrectly()
		{
			RateTestHelper.MapChargeCode(Env.Registry.FreightChargeCode, "YYY", null, Factory);
			Factory.Save();

			calculatorXSD = new Xsd.PSRCalculator();

			var rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Type = "COD";
			rateItemXSD.Chrg = "YYY";

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, RateTestHelper.GetContext(Factory));

			AssertEquals(Env.Registry.FreightChargeCode, CalculatorGenerated.RateLineItems[0].TM_AC);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return ProfitShareRebateCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(PSRCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<ProfitShareRebateCalculator, Xsd.PSRCalculator> CalculatorGenerator
		{
			get { return new PSRCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.PSRCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.PSRCalculator();

			if (specified)
			{
				calculatorXSD.Minimum = 1.2m;
				calculatorXSD.BasePrice = 1.3m;
				calculatorXSD.Maximum = 1.4m;
				calculatorXSD.Percentage = 1.7m;
				calculatorXSD.ZeroWhenLoss = Xsd.TrueFalse.@true;
			}

			var rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Type = "COD";
			rateItemXSD.Chrg = "ADINS";

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Type = "ALL";
			rateItemXSD.Chrg = "ADPOST";

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Type = "FRT";
			rateItemXSD.Chrg = "ADTEL";

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(ProfitShareRebateCalculator calculator)
		{
			calculator.Minimum = 1.2m;
			calculator.BaseRate = 1.3m;
			calculator.Maximum = 1.4m;
			calculator.Percent = 1.7m;
			calculator.ZeroWhenLoss = true;

			var charge = RateTestHelper.FindChargeCode("ADINS", Factory);
			var rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Text = "COD";
			rateItem.TM_AC = charge.PK;

			charge = RateTestHelper.FindChargeCode("ADPOST", Factory);
			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Text = "ALL";
			rateItem.TM_AC = charge.PK;

			charge = RateTestHelper.FindChargeCode("ADTEL", Factory);
			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Text = "FRT";
			rateItem.TM_AC = charge.PK;
		}

		#endregion
	}
}
