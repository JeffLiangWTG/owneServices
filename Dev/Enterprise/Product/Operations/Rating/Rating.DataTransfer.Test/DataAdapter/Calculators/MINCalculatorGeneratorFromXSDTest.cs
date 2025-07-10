using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class MINCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<MinimumCalculator, Xsd.MINCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.MINCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			#region Test (Specified = false)

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is an Minimum Caluclator", typeof(MinimumCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());

			AssertEquals("Calculator's MinimumValue", 0m, CalculatorGenerated.MinimumValue);
			AssertEquals("Calculator's IsJobMinimum", true, CalculatorGenerated.IsJobMinimum);
			AssertEquals("Calculator's IsChargeCodeMinimum", false, CalculatorGenerated.IsChargeCodeMinimum);

			#endregion

			#region Test (Specified = true)

			calculatorXSD = CalculatorXSDForTest(true);

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator 's MinimumValue", 12.3m, CalculatorGenerated.MinimumValue);
			AssertEquals("Calculator's IsJobMinimum", true, CalculatorGenerated.IsJobMinimum);
			AssertEquals("Calculator's IsChargeCodeMinimum", false, CalculatorGenerated.IsChargeCodeMinimum);

			#endregion
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Xsd.MINCalculator calculatorXSD = new Xsd.MINCalculator();

			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator 's MinimumValue", 12.3m, calculatorXSD.MinimumValue);
			AssertEquals("Calculator 's MinimumValue Specified", true, calculatorXSD.MinimumValueSpecified);

			AssertEquals("Calculator 's IsJobMinimum", Xsd.TrueFalse.@true, calculatorXSD.IsJobMinimum);
			AssertEquals("Calculator 's IsJobMinimum Specified", true, calculatorXSD.IsJobMinimumSpecified);

			AssertEquals("Calculator 's IsChargeCodeMinimum", Xsd.TrueFalse.@false, calculatorXSD.IsChargeCodeMinimum);
			AssertEquals("Calculator 's IsChargeCodeMinimum Specified", true, calculatorXSD.IsChargeCodeMinimumSpecified);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return MinimumCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(MINCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<MinimumCalculator, Xsd.MINCalculator> CalculatorGenerator
		{
			get { return new MINCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.MINCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.MINCalculator();

			if (specified)
			{
				calculatorXSD.MinimumValue = 12.3m;
				calculatorXSD.IsJobMinimum = Xsd.TrueFalse.@true;
				calculatorXSD.IsChargeCodeMinimum = Xsd.TrueFalse.@false;
			}

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(MinimumCalculator calculator)
		{
			calculator.MinimumValue = 12.3;
			calculator.IsJobMinimum = true;
			calculator.IsChargeCodeMinimum = false;
		}

		#endregion
	}
}
