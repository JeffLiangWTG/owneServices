using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class FPACalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<FirstPlusAdditionalCalculator, Xsd.FPACalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.FPACalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			#region Test (Specified = false)

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is an Agency Caluclator", typeof(FirstPlusAdditionalCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());

			AssertEquals("Calculator 's First", 0m, CalculatorGenerated.First);
			AssertEquals("Calculator 's Additional", 0m, CalculatorGenerated.Additional);

			#endregion

			#region Test (Specified = true)

			calculatorXSD = CalculatorXSDForTest(true);

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator 's First", 12.3m, CalculatorGenerated.First);
			AssertEquals("Calculator 's Additional", 13.2m, CalculatorGenerated.Additional);

			#endregion
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Xsd.FPACalculator calculatorXSD = new Xsd.FPACalculator();

			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator 's First", 12.3m, calculatorXSD.FirstItemPrice);
			AssertEquals("Calculator 's First Specified", true, calculatorXSD.FirstItemPriceSpecified);
			AssertEquals("Calculator 's Additional", 13.2m, calculatorXSD.AddlItemPrice);
			AssertEquals("Calculator 's Additional Specified", true, calculatorXSD.AddlItemPriceSpecified);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return FirstPlusAdditionalCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(FPACalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<FirstPlusAdditionalCalculator, Xsd.FPACalculator> CalculatorGenerator
		{
			get { return new FPACalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.FPACalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.FPACalculator();

			if (specified)
			{
				calculatorXSD.FirstItemPrice = 12.3m;
				calculatorXSD.AddlItemPrice = 13.2m;
			}

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(FirstPlusAdditionalCalculator calculator)
		{
			calculator.First = 12.3m;
			calculator.Additional = 13.2m;
		}

		#endregion
	}
}
