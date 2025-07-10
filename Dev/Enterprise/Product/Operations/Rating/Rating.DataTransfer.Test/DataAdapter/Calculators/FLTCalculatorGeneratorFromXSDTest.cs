using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class FLTCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<FlatCalculator, Xsd.FLTCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.FLTCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			#region Test (Specified = false)

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is an Agency Caluclator", typeof(FlatCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());

			AssertEquals("Calculator 's BaseRate", 0m, CalculatorGenerated.BaseRate);

			#endregion

			#region Test (Specified = true)

			calculatorXSD = CalculatorXSDForTest(true);

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator 's BaseRate", 12.3m, CalculatorGenerated.BaseRate);

			#endregion
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Xsd.FLTCalculator calculatorXSD = new Xsd.FLTCalculator();

			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator 's BaseRate", 12.3m, calculatorXSD.BasePrice);
			AssertEquals("Calculator 's BaseRate Specified", true, calculatorXSD.BasePriceSpecified);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return FlatCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(FLTCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<FlatCalculator, Xsd.FLTCalculator> CalculatorGenerator
		{
			get { return new FLTCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.FLTCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.FLTCalculator();
			if (specified)
			{
				calculatorXSD.BasePrice = 12.3m;
			}

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(FlatCalculator calculator)
		{
			calculator.BaseRate = 12.3;
		}

		#endregion
	}
}
