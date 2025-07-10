using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class MPUCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<MinimumOrPerUnitCalculator, Xsd.MPUCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.MPUCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			#region Test (Specified = false)

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is an Agency Caluclator", typeof(MinimumOrPerUnitCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());

			AssertEquals("Calculator 's Minimum", 0m, CalculatorGenerated.Minimum);
			AssertEquals("Calculator 's PerUnit", 0m, CalculatorGenerated.PerUnit);

			#endregion

			#region Test (Specified = true)

			calculatorXSD = CalculatorXSDForTest(true);

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator 's Minimum", 12.3m, CalculatorGenerated.Minimum);
			AssertEquals("Calculator 's PerUnit", 13.2m, CalculatorGenerated.PerUnit);

			#endregion
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Xsd.MPUCalculator calculatorXSD = new Xsd.MPUCalculator();

			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator 's Minimum", 12.3m, calculatorXSD.Minimum);
			AssertEquals("Calculator 's Minimum Specified", true, calculatorXSD.MinimumSpecified);
			AssertEquals("Calculator 's PerUnitPrice", 13.2m, calculatorXSD.PerUnitPrice);
			AssertEquals("Calculator 's PerUnitPrice Specified", true, calculatorXSD.PerUnitPriceSpecified);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return MinimumOrPerUnitCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(MPUCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<MinimumOrPerUnitCalculator, Xsd.MPUCalculator> CalculatorGenerator
		{
			get { return new MPUCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.MPUCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.MPUCalculator();

			if (specified)
			{
				calculatorXSD.Minimum = 12.3m;
				calculatorXSD.PerUnitPrice = 13.2m;
			}

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(MinimumOrPerUnitCalculator calculator)
		{
			calculator.Minimum = 12.3m;
			calculator.PerUnit = 13.2m;
		}

		#endregion
	}
}
