using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class FPUCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<FlatPlusPerUnitCalculator, Xsd.FPUCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.FPUCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			#region Test (Specified = false)

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is an Agency Caluclator", typeof(FlatPlusPerUnitCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());

			AssertEquals("Calculator 's BaseRate", 0m, CalculatorGenerated.BaseRate);
			AssertEquals("Calculator 's PerUnit", 0m, CalculatorGenerated.PerUnit);

			#endregion

			#region Test (Specified = true)

			calculatorXSD = CalculatorXSDForTest(true);

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator 's BaseRate", 12.3m, CalculatorGenerated.BaseRate);
			AssertEquals("Calculator 's PerUnit", 13.2m, CalculatorGenerated.PerUnit);

			#endregion
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Xsd.FPUCalculator calculatorXSD = new Xsd.FPUCalculator();

			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator 's BaseRate", 12.3m, calculatorXSD.BasePrice);
			AssertEquals("Calculator 's BaseRate Specified", true, calculatorXSD.BasePriceSpecified);
			AssertEquals("Calculator 's PerUnit", 13.2m, calculatorXSD.PerUnitPrice);
			AssertEquals("Calculator 's PerUnit Specified", true, calculatorXSD.PerUnitPriceSpecified);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return FlatPlusPerUnitCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(FPUCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<FlatPlusPerUnitCalculator, Xsd.FPUCalculator> CalculatorGenerator
		{
			get { return new FPUCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.FPUCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.FPUCalculator();

			if (specified)
			{
				calculatorXSD.BasePrice = 12.3m;
				calculatorXSD.PerUnitPrice = 13.2m;
			}

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(FlatPlusPerUnitCalculator calculator)
		{
			calculator.BaseRate = 12.3m;
			calculator.PerUnit = 13.2m;
		}

		#endregion
	}
}
