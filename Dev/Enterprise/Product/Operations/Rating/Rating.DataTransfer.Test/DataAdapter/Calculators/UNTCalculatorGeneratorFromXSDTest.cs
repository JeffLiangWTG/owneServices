using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class UNTCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<UnitCalculator, Xsd.UNTCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.UNTCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			#region Test (Specified = false)

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is an Agency Caluclator", typeof(UnitCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());

			AssertEquals("Calculator 's PerUnit", 0m, CalculatorGenerated.PerUnit);

			#endregion

			#region Test (Specified = true)

			calculatorXSD = CalculatorXSDForTest(true);

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator 's PerUnit", 12.3m, CalculatorGenerated.PerUnit);

			#endregion
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Xsd.UNTCalculator calculatorXSD = new Xsd.UNTCalculator();

			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator 's PerUnitPrice", 12.3m, calculatorXSD.PerUnitPrice);
			AssertEquals("Calculator 's PerUnitPrice Specified", true, calculatorXSD.PerUnitPriceSpecified);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return UnitCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(UNTCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<UnitCalculator, Xsd.UNTCalculator> CalculatorGenerator
		{
			get { return new UNTCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.UNTCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.UNTCalculator();

			if (specified)
			{
				calculatorXSD.PerUnitPrice = 12.3m;
			}

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(UnitCalculator calculator)
		{
			calculator.PerUnit = 12.3m;
		}

		#endregion
	}
}
