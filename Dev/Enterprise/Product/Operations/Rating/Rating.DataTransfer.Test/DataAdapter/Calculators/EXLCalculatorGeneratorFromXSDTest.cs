using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class EXLCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<ExcludeCompanyTariffsCalculator, Xsd.EXLCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.EXLCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is a Exclude from Company Tariffs Caluclator", typeof(ExcludeCompanyTariffsCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());
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
			get { return ExcludeCompanyTariffsCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(EXLCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<ExcludeCompanyTariffsCalculator, Xsd.EXLCalculator> CalculatorGenerator
		{
			get { return new EXLCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.EXLCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.EXLCalculator();

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(ExcludeCompanyTariffsCalculator calculator)
		{
		}

		#endregion
	}
}
