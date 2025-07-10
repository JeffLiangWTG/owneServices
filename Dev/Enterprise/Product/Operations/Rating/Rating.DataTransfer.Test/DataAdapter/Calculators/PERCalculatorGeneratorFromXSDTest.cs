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
	public class PERCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<PercentageCalculator, Xsd.PERCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.PERCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			#region Test (Specified = false)

			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is an Agency Caluclator", typeof(PercentageCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());

			AssertEquals("Calculator 's GreaterCharge", false, CalculatorGenerated.GreaterCharge);
			AssertEquals("Calculator 's IncludeGST", false, CalculatorGenerated.IncludeGST);
			AssertEquals("Calculator 's IsPartThereof", false, CalculatorGenerated.IsPartThereof);
			AssertEquals("Calculator 's Rate", 0m, CalculatorGenerated.Rate);
			AssertEquals("Calculator 's ValueOrPartThereOf", 0m, CalculatorGenerated.ValueOrPartThereOf);
			AssertEquals("Calculator 's Percent", 0m, CalculatorGenerated.Percent);

			AssertEquals("Calculator RateLineItems Count", 3, CalculatorGenerated.RateLineItems.Count);

			AssertEquals("Calculator 's BaseRate", 0m, CalculatorGenerated.BaseRate);
			AssertEquals("Calculator 's Minimum", 0m, CalculatorGenerated.Minimum);
			AssertEquals("Calculator 's Maximum", 0m, CalculatorGenerated.Maximum);

			#endregion

			#region Test (Specified = true)

			calculatorXSD = CalculatorXSDForTest(true);

			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator 's GreaterCharge", true, CalculatorGenerated.GreaterCharge);
			AssertEquals("Calculator 's IncludeGST", true, CalculatorGenerated.IncludeGST);
			AssertEquals("Calculator 's IsPartThereof", true, CalculatorGenerated.IsPartThereof);
			AssertEquals("Calculator 's Rate", 1.5m, CalculatorGenerated.Rate);
			AssertEquals("Calculator 's ValueOrPartThereOf", 1.6m, CalculatorGenerated.ValueOrPartThereOf);
			AssertEquals("Calculator 's Percent", 0m, CalculatorGenerated.Percent);

			AssertEquals("Calculator RateLineItems Count", 3, CalculatorGenerated.RateLineItems.Count);

			AssertEquals("Calculator 's BaseRate", 1.3m, CalculatorGenerated.BaseRate);
			AssertEquals("Calculator 's Minimum", 1.2m, CalculatorGenerated.Minimum);
			AssertEquals("Calculator 's Maximum", 1.4m, CalculatorGenerated.Maximum);

			calculatorXSD.PartThereOf.RateSpecified = false;
			calculatorXSD.PartThereOf.ValueOrPartThereOfSpecified = false;

			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerated.Rate = 0m;
			CalculatorGenerated.ValueOrPartThereOf = 0m;
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator 's GreaterCharge", true, CalculatorGenerated.GreaterCharge);
			AssertEquals("Calculator 's IncludeGST", true, CalculatorGenerated.IncludeGST);
			AssertEquals("Calculator 's IsPartThereof", true, CalculatorGenerated.IsPartThereof);
			AssertEquals("Calculator 's Rate", 0m, CalculatorGenerated.Rate);
			AssertEquals("Calculator 's ValueOrPartThereOf", 0m, CalculatorGenerated.ValueOrPartThereOf);
			AssertEquals("Calculator 's Percent", 1.7m, CalculatorGenerated.Percent);

			AssertEquals("Calculator RateLineItems Count", 3, CalculatorGenerated.RateLineItems.Count);

			AssertEquals("Calculator 's BaseRate", 1.3m, CalculatorGenerated.BaseRate);
			AssertEquals("Calculator 's Minimum", 1.2m, CalculatorGenerated.Minimum);
			AssertEquals("Calculator 's Maximum", 1.4m, CalculatorGenerated.Maximum);

			#endregion
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Xsd.PERCalculator calculatorXSD = new Xsd.PERCalculator();

			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator 's GreaterCharge", Xsd.TrueFalse.@true, calculatorXSD.TakeHighestCharge);
			AssertEquals("Calculator 's GreaterCharge Specified", true, calculatorXSD.TakeHighestChargeSpecified);
			AssertEquals("Calculator 's IncludeGST", Xsd.TrueFalse.@true, calculatorXSD.ExcludeGST);
			AssertEquals("Calculator 's IncludeGST Specified", true, calculatorXSD.ExcludeGSTSpecified);
			AssertEquals("Calculator 's Rate", 1.5m, calculatorXSD.PartThereOf.Rate);
			AssertEquals("Calculator 's Rate Specified", true, calculatorXSD.PartThereOf.RateSpecified);
			AssertEquals("Calculator 's ValueOrPartThereOf", 1.6m, calculatorXSD.PartThereOf.ValueOrPartThereOf);
			AssertEquals("Calculator 's ValueOrPartThereOf Specified", true, calculatorXSD.PartThereOf.ValueOrPartThereOfSpecified);
			AssertEquals("Calculator 's Percent", 0m, calculatorXSD.Percentage);
			AssertEquals("Calculator 's Percent Specified", false, calculatorXSD.PercentageSpecified);

			AssertEquals("Calculator RateLineItems Count", 3, calculatorXSD.RateItems.Count);

			AssertEquals("Calculator 's BaseRate", 1.3m, calculatorXSD.BasePrice);
			AssertEquals("Calculator 's BaseRate Specified", true, calculatorXSD.BasePriceSpecified);
			AssertEquals("Calculator 's Minimum", 1.2m, calculatorXSD.Minimum);
			AssertEquals("Calculator 's Minimum Specified", true, calculatorXSD.MinimumSpecified);
			AssertEquals("Calculator 's Maximum", 1.4m, calculatorXSD.Maximum);
			AssertEquals("Calculator 's Maximum Specified", true, calculatorXSD.MaximumSpecified);

			((PercentageCalculator)RateLineForCalculatorTest.Calculator).IsPartThereof = false;
			calculatorXSD = new Xsd.PERCalculator();
			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator 's GreaterCharge", Xsd.TrueFalse.@true, calculatorXSD.TakeHighestCharge);
			AssertEquals("Calculator 's GreaterCharge Specified", true, calculatorXSD.TakeHighestChargeSpecified);
			AssertEquals("Calculator 's IncludeGST", Xsd.TrueFalse.@true, calculatorXSD.ExcludeGST);
			AssertEquals("Calculator 's IncludeGST Specified", true, calculatorXSD.ExcludeGSTSpecified);
			AssertEquals("Calculator 's Rate", 0m, calculatorXSD.PartThereOf.Rate);
			AssertEquals("Calculator 's Rate Specified", false, calculatorXSD.PartThereOf.RateSpecified);
			AssertEquals("Calculator 's ValueOrPartThereOf", 0m, calculatorXSD.PartThereOf.ValueOrPartThereOf);
			AssertEquals("Calculator 's ValueOrPartThereOf Specified", false, calculatorXSD.PartThereOf.ValueOrPartThereOfSpecified);
			AssertEquals("Calculator 's Percent", 1.7m, calculatorXSD.Percentage);
			AssertEquals("Calculator 's Percent Specified", true, calculatorXSD.PercentageSpecified);

			AssertEquals("Calculator RateLineItems Count", 3, calculatorXSD.RateItems.Count);

			AssertEquals("Calculator 's BaseRate", 1.3m, calculatorXSD.BasePrice);
			AssertEquals("Calculator 's BaseRate Specified", true, calculatorXSD.BasePriceSpecified);
			AssertEquals("Calculator 's Minimum", 1.2m, calculatorXSD.Minimum);
			AssertEquals("Calculator 's Minimum Specified", true, calculatorXSD.MinimumSpecified);
			AssertEquals("Calculator 's Maximum", 1.4m, calculatorXSD.Maximum);
			AssertEquals("Calculator 's Maximum Specified", true, calculatorXSD.MaximumSpecified);
		}

		#endregion

		#region Importing Applies mapps

		public void TestMappingsAppliedCorrectly()
		{
			RateTestHelper.MapChargeCode(Env.Registry.FreightChargeCode, "YYY", null, Factory);
			Factory.Save();

			calculatorXSD = new Xsd.PERCalculator();

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
			get { return PercentageCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(PERCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<PercentageCalculator, Xsd.PERCalculator> CalculatorGenerator
		{
			get { return new PERCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.PERCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.PERCalculator();

			if (specified)
			{
				calculatorXSD.Minimum = 1.2m;
				calculatorXSD.BasePrice = 1.3m;
				calculatorXSD.Maximum = 1.4m;

				calculatorXSD.TakeHighestCharge = Xsd.TrueFalse.@true;
				calculatorXSD.ExcludeGST = Xsd.TrueFalse.@true;

				calculatorXSD.PartThereOf.Rate = 1.5m;
				calculatorXSD.PartThereOf.ValueOrPartThereOf = 1.6m;

				calculatorXSD.Percentage = 1.7m;
			}

			Xsd.PERRateItem rateItemXSD;

			rateItemXSD = calculatorXSD.RateItems.AddNew();
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

		protected override void SetTestDataCalculatorForTest(PercentageCalculator calculator)
		{
			calculator.Minimum = 1.2m;
			calculator.BaseRate = 1.3m;
			calculator.Maximum = 1.4m;

			calculator.GreaterCharge = true;
			calculator.IncludeGST = true;

			calculator.IsPartThereof = true;
			calculator.Rate = 1.5m;
			calculator.ValueOrPartThereOf = 1.6m;

			calculator.Percent = 1.7m;

			RateLineItem rateItem;
			AccChargeCode charge;

			charge = RateTestHelper.FindChargeCode("ADINS", Factory);
			rateItem = calculator.RateLineItems.AddNew();
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
