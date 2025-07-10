using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class FRTCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<FreightInclusiveCalculator, Xsd.FRTCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.FRTCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			#region Test (Specified = false)

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is a Freight Inclusive Caluclator", typeof(FreightInclusiveCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());

			#endregion
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Assert("Nothing to test", true);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return FreightInclusiveCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(FRTCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<FreightInclusiveCalculator, Xsd.FRTCalculator> CalculatorGenerator
		{
			get { return new FRTCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.FRTCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.FRTCalculator();

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(FreightInclusiveCalculator calculator)
		{
		}

		#endregion
	}
}
