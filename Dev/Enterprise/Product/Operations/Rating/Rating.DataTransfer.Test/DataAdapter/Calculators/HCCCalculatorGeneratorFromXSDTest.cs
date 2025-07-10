using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer.Testing;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class HCCCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<HighestChargeCalculator, Xsd.HCCCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.HCCCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			CalculatorGenerated.RateLineItems.RemoveAndDeleteAll();
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator should be a HighestChargeCalculator", typeof(HighestChargeCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());
			AssertEquals("Calculator should have 3 RateLineItems", 3, CalculatorGenerated.RateLineItems.Count);

			var chargeCodes = CalculatorGenerated.RateLineItems.Select(rli => rli.ChargeCode.AC_Code);
			AssertContainsExactElementsInAnyOrder(new[] { "ADINS", "ADPOST", "ADTEL" }, chargeCodes);
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Xsd.HCCCalculator calculatorXSD = new Xsd.HCCCalculator();

			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator RateLineItems Count", 3, calculatorXSD.RateItems.Count);

			var chargeCodes = calculatorXSD.RateItems.Cast<Xsd.HCCRateItem>().Select(ri => ri.Chrg);
			AssertContainsExactElementsInAnyOrder(new[] { "ADINS", "ADPOST", "ADTEL" }, chargeCodes);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return HighestChargeCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(HCCCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<HighestChargeCalculator, Xsd.HCCCalculator> CalculatorGenerator
		{
			get { return new HCCCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.HCCCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.HCCCalculator();

			var rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Chrg = "ADINS";

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Chrg = "ADPOST";

			rateItemXSD = calculatorXSD.RateItems.AddNew();
			rateItemXSD.Chrg = "ADTEL";

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(HighestChargeCalculator calculator)
		{
			var charge = RateTestHelper.FindChargeCode("ADINS", Factory);
			var rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Text = "COD";
			rateItem.TM_AC = charge.PK;

			charge = RateTestHelper.FindChargeCode("ADPOST", Factory);
			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Text = "COD";
			rateItem.TM_AC = charge.PK;

			charge = RateTestHelper.FindChargeCode("ADTEL", Factory);
			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Text = "COD";
			rateItem.TM_AC = charge.PK;
		}

		#endregion
	}
}
