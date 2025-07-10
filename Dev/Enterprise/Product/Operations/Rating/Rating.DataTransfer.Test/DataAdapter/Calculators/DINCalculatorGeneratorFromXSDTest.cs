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
	public class DINCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<DisbursementInterestCalculator, Xsd.DINCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.DINCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			#region Test (Specified = false)

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is an Agency Caluclator", typeof(DisbursementInterestCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());

			AssertEquals("Calculator 's Uplift", 0m, CalculatorGenerated.Uplift);
			AssertEquals("Calculator 's OutstandingDays", false, CalculatorGenerated.OutstandingDays);
			AssertEquals("Calculator 's AdjustmentDays", 0m, CalculatorGenerated.AdjustmentDays);
			AssertEquals("Calculator RateLineItems Count", 3, CalculatorGenerated.RateLineItems.Count);

			#endregion

			#region Test (Specified = true)

			calculatorXSD = CalculatorXSDForTest(true);
			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator 's Uplift", 12.3m, CalculatorGenerated.Uplift);
			AssertEquals("Calculator 's OutstandingDays", true, CalculatorGenerated.OutstandingDays);
			AssertEquals("Calculator 's AdjustmentDays", 12m, CalculatorGenerated.AdjustmentDays);
			AssertEquals("Calculator RateLineItems Count", 3, CalculatorGenerated.RateLineItems.Count);

			#endregion

			calculatorXSD.ApplyTo = "TTT";
			calculatorXSD.RateItems.Clear();
			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator RateLineItems Count", 1, CalculatorGenerated.RateLineItems.Count);
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Xsd.DINCalculator calculatorXSD = new Xsd.DINCalculator();

			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator 's Uplift", 12.3m, calculatorXSD.Uplift);
			AssertEquals("Calculator 's Uplift Specified", true, calculatorXSD.UpliftSpecified);
			AssertEquals("Calculator 's ApplyToOutstandingDaysOnly", Xsd.TrueFalse.@true, calculatorXSD.ApplyToOutstandingDaysOnly);
			AssertEquals("Calculator 's ApplyToOutstandingDaysOnly Specified", true, calculatorXSD.ApplyToOutstandingDaysOnlySpecified);
			AssertEquals("Calculator 's AdjustmentDays", 12m, calculatorXSD.AdjustmentDays);
			AssertEquals("Calculator 's AdjustmentDays Specified", true, calculatorXSD.AdjustmentDaysSpecified);
			AssertEquals("Calculator RateLineItems Count", 3, calculatorXSD.RateItems.Count);
			AssertEquals("Calculator 's ApplyTo", "", calculatorXSD.ApplyTo);
		}

		#endregion

		#region Importing Applies mapps

		public void TestMappingsAppliedCorrectly()
		{
			RateTestHelper.MapChargeCode(Env.Registry.FreightChargeCode, "YYY", null, Factory);
			Factory.Save();

			calculatorXSD = new Xsd.DINCalculator();

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
			get { return DisbursementInterestCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(DINCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<DisbursementInterestCalculator, Xsd.DINCalculator> CalculatorGenerator
		{
			get { return new DINCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.DINCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.DINCalculator();

			if (specified)
			{
				calculatorXSD.Uplift = 12.3m;
				calculatorXSD.ApplyToOutstandingDaysOnly = Xsd.TrueFalse.@true;
				calculatorXSD.AdjustmentDays = 12m;
			}

			Xsd.DINRateItem rateItemXSD;

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

		protected override void SetTestDataCalculatorForTest(DisbursementInterestCalculator calculator)
		{
			calculator.Uplift = 12.3m;
			calculator.OutstandingDays = true;
			calculator.AdjustmentDays = 12;

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
